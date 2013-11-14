#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using FlitBit.Core;
using FlitBit.Core.Collections;
using FlitBit.Data.Meta;
using FlitBit.Data.SPI;
using FlitBit.Emit;
using Inflector;
using Extensions = FlitBit.Core.Extensions;

namespace FlitBit.Data.DataModel
{
	/// <summary>
	///   Utility class for fulfilling the data-model (elsewhere known as Entity) stereotype by emitting implementations.
	/// </summary>
	internal static class DataModelEmitter
	{
		internal static readonly string CDirtyFlagsBackingFieldName = "<DirtyFlags>";

		static readonly Lazy<EmittedModule> LazyModule =
			new Lazy<EmittedModule>(() => RuntimeAssemblies.DynamicAssembly.DefineModule("DataModels", null),
															LazyThreadSafetyMode.ExecutionAndPublication
				);

		static EmittedModule Module { get { return LazyModule.Value; } }

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type ConcreteType<T>(IMapping<T> mapping)
		{
			Contract.Requires<ArgumentNullException>(mapping != null);
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(T);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, "DataModel");

			var module = Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ??
					EmitImplementation<T>.BuildImplementation(module, typeName, mapping);
				return type;
			}
		}

		static class EmitImplementation<TDataModel>
		{
			internal static Type BuildImplementation(EmittedModule module, string typeName, IMapping<TDataModel> mapping)
			{
				Contract.Requires<ArgumentNullException>(module != null);
				Contract.Requires<ArgumentNullException>(typeName != null);
				Contract.Requires<ArgumentException>(typeName.Length > 0);
				Contract.Requires<InvalidOperationException>(mapping.HasBinder);
				Contract.Ensures(Contract.Result<Type>() != null);

				var builder = module.DefineClass(typeName, EmittedClass.DefaultTypeAttributes, typeof(object), null);
				builder.Attributes = TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;

				builder.SetCustomAttribute<SerializableAttribute>();

				var chashCodeSeed = builder.DefineField<int>("CHashCodeSeed");
				chashCodeSeed.IncludeAttributes(FieldAttributes.Static | FieldAttributes.Private | FieldAttributes.InitOnly);
				var cctor = builder.DefineCCtor();
				
				if (mapping.Collections.Any())
				{
					builder.DefineField<object>("_sync").ClearAttributes();
				}
				var dirtyFlags = builder.DefineField<BitVector>(CDirtyFlagsBackingFieldName);
				dirtyFlags.ClearAttributes();

				var ctor = builder.DefineDefaultCtor();

				var propChanged = ImplementINotifyPropertyChanged(builder);
				var props = new List<PropertyRec>();
				foreach (var intf in from type in typeof(TDataModel).GetTypeHierarchyInDeclarationOrder()
														where type.IsInterface
															&& type != typeof(IEquatable<TDataModel>)
															&& type != typeof(IDataModel)
															&& type != typeof(ICloneable)
															&& type != typeof(INotifyPropertyChanged)
														select type)
				{
					builder.AddInterfaceImplementation(intf);
					ImplementPropertiesForInterface(mapping, intf, builder, props, dirtyFlags, propChanged);
					builder.StubMethodsForInterface(intf, true, true);
				}
				ImplementSpecializedEquals(builder);
				ImplementSpecializedGetHashCode(builder, chashCodeSeed);
				ImplementIDataModel(builder, cctor, props, dirtyFlags, propChanged);
				EmitConstructor(builder, ctor, props, dirtyFlags);
				cctor.ContributeInstructions((m, il) =>
				{
					il.LoadType(builder.Builder);
					il.CallVirtual(typeof (Type).GetProperty("AssemblyQualifiedName")
						.GetGetMethod());
					il.CallVirtual<object>("GetHashCode");
					il.StoreField(chashCodeSeed);
					foreach (var prop in props.Where(p => p.IsCollection))
					{
						il.LoadNull();
						il.Emit(OpCodes.Ldftn, prop.CollectionMakeResolve.Builder);
						var funcType = typeof (Func<>).MakeGenericType(prop.CollectionMakeResolve.Builder.ReturnType);
						il.NewObj(funcType.GetConstructors()[0]);
						il.LoadValue(LazyThreadSafetyMode.ExecutionAndPublication);
						var lazyType = prop.CollectionStaticLazyCommand.Builder.FieldType;
						il.NewObj(lazyType.GetConstructor(new [] { funcType, typeof(LazyThreadSafetyMode) }));
						prop.CollectionStaticLazyCommand.StoreValue(il);
					}
				});
				
				builder.Compile();
				return builder.Ref.Target;
			}

			static void EmitConstructor(EmittedClass builder, EmittedMethodBase ctor, ICollection<PropertyRec> props,
				IFieldRef dirtyFlags)
			{
				ctor.ContributeInstructions((m, il) =>
				{
					il.LoadArg_0();
					il.LoadValue(props.Count);
					il.New<BitVector>(typeof(int));
					il.StoreField(dirtyFlags);

					var sync = builder.Fields.FirstOrDefault(f => f.Name == "_sync");
					if (sync != null)
					{
						il.LoadArg_0();
						il.New<Object>();
						il.StoreField(sync);
					}

					foreach (var prop in props)
					{
						if (prop.IsColumn && prop.Column.IsReference)
						{
							il.LoadArg_0();
							il.NewObj(prop.FieldType.GetConstructor(Type.EmptyTypes));
							il.StoreField(prop.EmittedField);
						}
					}
				});
			}

			static void ImplementIDataModelLoadFromDbRecord(EmittedClass builder, ICollection<PropertyRec> props, IFieldRef dirtyFlags)
			{
				var method = builder.DefineMethod("LoadFromDataReader");
				method.ClearAttributes();
				method.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
					MethodAttributes.Virtual | MethodAttributes.Final);
				var reader = method.DefineParameter("reader", typeof(DbDataReader));
				var offsets = method.DefineParameter("offsets", typeof(int[]));
				method.ContributeInstructions((m, il) =>
				{
					var col = m.DefineLocal("col", typeof (int));

					foreach (var prop in props.OrderBy(p => p.Ordinal))
					{
						if (prop.IsColumn)
						{
							var mappedColumn = prop.Column;

							var isnull = il.DefineLabel();
							var store = il.DefineLabel();
							il.LoadArg(offsets);
							il.LoadValue(prop.Ordinal);
							il.Emit(OpCodes.Ldelem_I4);
							col.StoreValue(il);
							il.LoadArg_0();
							if (mappedColumn.IsNullable)
							{
								il.LoadArg(reader);
								col.LoadValue(il);
								il.CallVirtual<DbDataReader>("IsDBNull", typeof (int));
								il.BranchIfTrue(isnull);
							}
							if (prop.Column.IsReference)
							{
								mappedColumn.Emitter.LoadValueFromDbReader(method.Builder, reader, col, mappedColumn.DbTypeDetails);
								il.NewObj(prop.FieldType.GetConstructor(new[] {prop.ReferenceTargetMemberType}));
								il.Branch(store);
								if (mappedColumn.IsNullable)
								{
									il.MarkLabel(isnull);
									il.NewObj(prop.FieldType.GetConstructor(Type.EmptyTypes));
								}
							}
							else
							{
								mappedColumn.Emitter.LoadValueFromDbReader(method.Builder, reader, col, mappedColumn.DbTypeDetails);
								il.Branch(store);
								if (mappedColumn.IsNullable)
								{
									il.MarkLabel(isnull);
									if (prop.FieldType.IsValueType)
									{
										var defa = il.DeclareLocal(prop.FieldType);
										il.LoadLocalAddress(defa);
										il.InitObject(prop.FieldType);
										il.LoadLocal(defa);
									}
									else
									{
										il.LoadDefaultValue(prop.FieldType);
									}
								}
							}
							il.MarkLabel(store);
							il.StoreField(prop.EmittedField);
						}
						else if (prop.IsCollection)
						{
							il.LoadArg_0();
							il.LoadNull();
							prop.EmittedField.StoreValue(il);
						}
					}
					il.LoadArg_0();
					il.LoadValue(props.Count);
					il.New<BitVector>(typeof (int));
					il.StoreField(dirtyFlags);

				});
			}

			static void ImplementICloneableClone(EmittedClass builder, IEnumerable<PropertyRec> props, EmittedField dirtyFlags)
			{
				var method = builder.DefineMethod("Clone");
				method.ClearAttributes();
				method.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
					MethodAttributes.Virtual | MethodAttributes.Final);
				method.ReturnType = TypeRef.FromType<object>();
				method.ContributeInstructions((m, il) =>
				{
					var model = il.DeclareLocal(builder.Builder);
					il.DeclareLocal(typeof(object));
					var flag = il.DeclareLocal(typeof(bool));

					il.LoadArg_0();
					il.Call<Object>("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
					il.CastClass(builder.Builder);
					il.StoreLocal_0();
					il.LoadLocal_0();
					il.LoadArg_0();
					il.LoadFieldAddress(dirtyFlags);
					il.Call<BitVector>("Copy");
					il.StoreField(dirtyFlags);

					il.LoadLocal(model);
					il.LoadNull();
					il.StoreField(builder.Fields.Single(f => f.Name == "<PropertyChanged>")
															.Builder);

					foreach (var prop in props)
					{
						if (prop.IsCollection)
						{
							//	if (this.<collection-field> != null)
							//	{
							//		model.<collection-field> = this.<collection-field>.Clone(new NotifyCollectionChangedEventHandler(model.<collection-field>_CollectionChanged));
							//	}
							//
							var afterColl = il.DefineLabel();
							il.LoadArg_0();
							il.LoadField(prop.EmittedField);
							il.LoadNull();
							il.CompareEqual();
							il.StoreLocal(flag);
							il.LoadLocal(flag);
							il.BranchIfTrue(afterColl);
							il.LoadLocal(model);
							il.LoadArg_0();
							prop.EmittedField.LoadValue(il);
							il.LoadLocal(model);
							il.Emit(OpCodes.Ldftn, prop.CollectionChanged.Builder);
							var ctor = typeof(NotifyCollectionChangedEventHandler).GetConstructors()[0];
							il.NewObj(ctor);
							il.CallVirtual(prop.FieldType.GetMethod("Clone", new [] { typeof (NotifyCollectionChangedEventHandler) } ));
							il.StoreField(prop.EmittedField);
							il.MarkLabel(afterColl);
						}
						else if (prop.FieldType.IsArray)
						{
							// copy the first rank
							var after = il.DefineLabel();
							var len = il.DeclareLocal(typeof(int));
							il.LoadArg_0();
							il.LoadField(prop.EmittedField);
							il.LoadNull();
							il.CompareEqual();
							il.StoreLocal(flag);
							il.LoadLocal(flag);
							il.BranchIfTrue(after);

							il.LoadArg_0();
							il.LoadField(prop.EmittedField);
							il.Emit(OpCodes.Ldlen);
							il.ConvertToInt32();
							il.StoreLocal(len);
							il.LoadLocal(model);
							il.LoadLocal(len);
							il.Emit(OpCodes.Newarr, prop.FieldType.GetElementType());
							il.StoreField(prop.EmittedField);
							il.LoadArg_0();
							il.LoadField(prop.EmittedField);
							il.LoadLocal(model);
							il.LoadField(prop.EmittedField);
							il.LoadLocal(len);
							il.Call<Array>("Copy", BindingFlags.Static | BindingFlags.Public, typeof(Array), typeof(Array),
														typeof(int));

							il.MarkLabel(after);
						}
					}

					il.LoadLocal_0();
					il.StoreLocal_1();
					var exit = il.DefineLabel();
					il.Branch_ShortForm(exit);
					il.MarkLabel(exit);
					il.LoadLocal_1();
				});
			}

			static void ImplementIDataModel(EmittedClass builder, EmittedConstructor cctor, List<PropertyRec> props,
				EmittedField dirtyFlags, EmittedMethod propChanged)
			{
				builder.AddInterfaceImplementation(typeof(IDataModel));

				ImplementIDataModelGetDirtyFlags(builder, dirtyFlags);
				ImplementIDataModelGetReferentID(builder, props);
				ImplementIDataModelSetReferentID(builder, props, dirtyFlags, propChanged);
				ImplementIDataModelResetDirtyFlags(builder, props, dirtyFlags);
				ImplementIDataModelIsDirty(builder, cctor, props, dirtyFlags);
				ImplementICloneableClone(builder, props, dirtyFlags);
				ImplementIValidatableObject(builder, props, dirtyFlags);
				// mapping...
				ImplementIDataModelLoadFromDbRecord(builder, props, dirtyFlags);
			}

			static void ImplementIValidatableObject(EmittedClass builder, List<PropertyRec> props, EmittedField dirtyFlags)
			{
				var method = builder.DefineMethod("Validate");
				method.ClearAttributes();
				method.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
					MethodAttributes.Virtual | MethodAttributes.Final);
				method.ReturnType = TypeRef.FromType<IEnumerable<ValidationResult>>();
				var validationContext = method.DefineParameter("validationContext", typeof(ValidationContext));
				method.ContributeInstructions((m, il) =>
				{
					il.LoadNull();
				});
			}

			static void ImplementIDataModelGetDirtyFlags(EmittedClass builder, EmittedField dirtyFlags)
			{
				var method = builder.DefineMethod("GetDirtyFlags");
				method.ClearAttributes();
				method.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
					MethodAttributes.Virtual | MethodAttributes.Final);
				method.ReturnType = TypeRef.FromType<BitVector>();
				method.ContributeInstructions((m, il) =>
				{
					il.DeclareLocal(typeof(BitVector));

					il.LoadArg_0();
					il.LoadFieldAddress(dirtyFlags);
					il.Call<BitVector>("Clone");
					il.UnboxAny(typeof(BitVector));
					il.StoreLocal_0();
					var exit = il.DefineLabel();
					il.Branch_ShortForm(exit);
					il.MarkLabel(exit);
					il.LoadLocal_0();
				});
			}

			static void ImplementIDataModelGetReferentID(EmittedClass builder, IEnumerable<PropertyRec> props)
			{
				var method = builder.DefineMethod("GetReferentID");
				method.ClearAttributes();
				method.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
					MethodAttributes.Virtual | MethodAttributes.Final);
				var gtpb = method.Builder.DefineGenericParameters("TIdentityKey");
				var tid = gtpb[0];
				method.Builder.SetReturnType(tid);
				method.Builder.SetParameters(typeof(string));
				method.Builder.DefineParameter(1, ParameterAttributes.In, "member");

				method.ContributeInstructions((m, il) =>
				{
					var res = il.DeclareLocal(tid);
					var flag = il.DeclareLocal<bool>();
					var done = il.DefineLabel();
					foreach (var prop in props.Where(p => p.IsColumn && p.Column.IsReference))
					{

						var after = il.DefineLabel();
						il.LoadValue(prop.Source.Name);
						il.LoadArg_1();
						il.Call<string>("Equals", BindingFlags.Public | BindingFlags.Static, typeof(string), typeof(string));
						il.Load_I4_0();
						il.CompareEqual();
						il.BranchIfTrue(after);
						il.LoadArg_0();
						il.LoadField(prop.EmittedField);
						il.CallVirtual(prop.FieldType.GetProperty("IdentityKey")
															.GetGetMethod());
						if (prop.ReferenceTargetMemberType.IsValueType)
						{
							il.Box(prop.ReferenceTargetMemberType);
						}
						il.UnboxAny(tid);
						il.StoreLocal(res);
						il.Branch(done);
						il.MarkLabel(after);
					}
					var cont = il.DefineLabel();
					il.LoadArg_1();
					il.LoadNull();
					il.CompareEqual();
					il.Load_I4_0();
					il.CompareEqual();
					il.StoreLocal(flag);
					il.LoadLocal(flag);
					il.BranchIfTrue(cont);
					il.LoadValue("member");
					il.New<ArgumentNullException>(typeof(string));
					il.Throw();
					il.MarkLabel(cont);
					il.LoadValue("member");
					il.LoadValue(String.Concat(typeof(TDataModel).Name, " does not reference: "));
					il.LoadArg_1();
					il.LoadValue(".");
					il.Call<string>("Concat", BindingFlags.Static | BindingFlags.Public, typeof(string), typeof(string), typeof(string));
					il.New<ArgumentOutOfRangeException>(typeof(string), typeof(string));
					il.Throw();
					il.MarkLabel(done);
					il.LoadLocal(res);
				});
			}

			static void ImplementIDataModelIsDirty(EmittedClass builder, EmittedMethodBase cctor,
				IEnumerable<PropertyRec> props,
				IValueRef dirtyFlags)
			{
				var fieldMap = builder.DefineField<string[]>("__fieldMap");
				fieldMap.ClearAttributes();
				fieldMap.IncludeAttributes(FieldAttributes.Static | FieldAttributes.InitOnly | FieldAttributes.Private);
				cctor.ContributeInstructions((m, il) =>
				{
					var arr = il.DeclareLocal(typeof(String[]));
					var cols = props.OrderBy(p => p.Ordinal).ToArray();

					il.NewArr(typeof(string), cols.Length);
					il.StoreLocal(arr);
					for (var i = 0; i < cols.Length; i++)
					{
						il.LoadLocal(arr);
						il.LoadValue(i);
						il.LoadValue(cols[i].Source.Name);
						il.Emit(OpCodes.Stelem, typeof(string));
					}
					il.LoadLocal(arr);
					il.StoreField(fieldMap);
				});

				var method = builder.DefineMethod("IsDirty");
				method.ClearAttributes();
				method.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
					MethodAttributes.Virtual | MethodAttributes.Final);
				method.ReturnType = TypeRef.FromType<bool>();
				method.DefineParameter("member", typeof(String));
				method.ContributeInstructions((m, il) =>
				{
					il.DeclareLocal(typeof(Int32));
					il.DeclareLocal(typeof(bool));
					il.DeclareLocal(typeof(bool));
					var proceed = il.DefineLabel();
					var yep = il.DefineLabel();

					il.LoadArg_1();
					il.LoadNull();
					il.CompareEqual();
					il.LoadValue(false);
					il.CompareEqual();
					il.StoreLocal_2();
					il.LoadLocal_2();
					il.BranchIfTrue_ShortForm(proceed);
					il.LoadValue("member");
					il.New<ArgumentNullException>(typeof(string));
					il.Throw();
					il.MarkLabel(proceed);
					il.LoadField(fieldMap);
					il.LoadArg_1();
					var indexOf = typeof(Array).MatchGenericMethod("IndexOf", BindingFlags.Public | BindingFlags.Static,
																												1, typeof(int), typeof(string[]), typeof(string));
					il.Call(indexOf.MakeGenericMethod(typeof(string)));
					il.StoreLocal_0();
					il.LoadLocal_0();
					il.LoadValue(0);
					il.CompareLessThan();
					il.LoadValue(0);
					il.CompareEqual();
					il.StoreLocal_2();
					il.LoadLocal_2();
					il.BranchIfTrue(yep);

					il.LoadValue("member");
					il.LoadValue(String.Concat(typeof(TDataModel).GetReadableSimpleName(), " does not define property: `"));
					il.LoadArg_1();
					il.LoadValue("`.");
					il.Call<string>("Concat", BindingFlags.Static | BindingFlags.Public, typeof(string), typeof(string),
													typeof(string));
					il.NewObj(
									 typeof(ArgumentOutOfRangeException).GetConstructor(new[] {typeof(string), typeof(string)}));
					il.Throw();
					il.MarkLabel(yep);
					il.LoadArg_0();
					dirtyFlags.LoadAddress(il);
					il.LoadLocal_0();
					il.Call<BitVector>("get_Item", typeof(int));
					il.StoreLocal_1();
					il.DefineAndMarkLabel();
					il.LoadLocal_1();
				});
			}

			static void ImplementIDataModelResetDirtyFlags(EmittedClass builder, List<PropertyRec> props,
				EmittedField dirtyFlags)
			{
				var method = builder.DefineMethod("ResetDirtyFlags");
				method.ClearAttributes();
				method.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
					MethodAttributes.Virtual | MethodAttributes.Final);
				method.ContributeInstructions((m, il) =>
				{
					il.LoadArg_0();
					il.LoadValue(props.Count);
					il.New<BitVector>(typeof(int));
					il.StoreField(dirtyFlags);
				});
			}

			static void ImplementIDataModelSetReferentID(EmittedClass builder, IEnumerable<PropertyRec> props,
				EmittedField dirtyFlags, EmittedMethod propChanged)
			{
				var method = builder.DefineMethod("SetReferentID");
				method.ClearAttributes();
				method.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
					MethodAttributes.Virtual | MethodAttributes.Final);
				var gtpb = method.Builder.DefineGenericParameters("TIdentityKey");
				var tid = gtpb[0];
				method.Builder.SetParameters(typeof(string), tid);
				method.DefineParameter("member", typeof(string));
				method.DefineParameter("id", tid);

				method.ContributeInstructions((m, il) =>
				{
					var flag = il.DeclareLocal<bool>();
					var done = il.DefineLabel();
					foreach (var prop in props.Where(p => p.IsColumn && p.Column.IsReference))
					{
						var after = il.DefineLabel();
						var brexit = il.DefineLabel();
						il.LoadValue(prop.Source.Name);
						il.LoadArg_1();
						il.Call<string>("Equals", BindingFlags.Public | BindingFlags.Static, typeof(string), typeof(string));
						il.Load_I4_0();
						il.CompareEqual();
						il.BranchIfTrue(after);
						il.LoadArg_0();
						il.LoadField(prop.EmittedField);
						il.LoadArg_2();
						il.Box(tid);
						il.CallVirtual(prop.FieldType.GetMethod("IdentityEquals", new[] {typeof(object)}));
						il.StoreLocal_0();
						il.LoadLocal_0();
						il.BranchIfTrue(brexit);
						il.LoadArg_0();
						il.LoadArg_2();
						il.Box(tid);
						il.UnboxAny(prop.ReferenceTargetMemberType);
						il.NewObj(prop.FieldType.GetConstructor(new[] {prop.ReferenceTargetMemberType}));
						il.StoreField(prop.EmittedField);
						il.LoadArg_0();
						il.LoadFieldAddress(dirtyFlags);
						il.LoadValue(prop.Ordinal);
						il.LoadValue(true);
						il.Call<BitVector>("set_Item");

						il.LoadArg_0();
						il.LoadValue(prop.Source.Name);
						il.Call(propChanged);
						il.MarkLabel(brexit);
						il.Branch(done);
						il.MarkLabel(after);
					}
					var cont = il.DefineLabel();
					il.LoadArg_1();
					il.LoadNull();
					il.CompareEqual();
					il.Load_I4_0();
					il.CompareEqual();
					il.StoreLocal(flag);
					il.LoadLocal(flag);
					il.BranchIfTrue(cont);
					il.LoadValue("member");
					il.New<ArgumentNullException>(typeof(string));
					il.Throw();
					il.MarkLabel(cont);
					il.LoadValue("member");
					il.LoadValue(String.Concat(typeof(TDataModel).Name, " does not reference: "));
					il.LoadArg_1();
					il.LoadValue(".");
					il.Call<string>("Concat", BindingFlags.Static | BindingFlags.Public, typeof(string), typeof(string),
													typeof(string));
					il.New<ArgumentOutOfRangeException>(typeof(string), typeof(string));
					il.Throw();
					il.MarkLabel(done);
				});
			}

			static EmittedMethod ImplementINotifyPropertyChanged(EmittedClass builder)
			{
				var evtType = typeof(PropertyChangedEventHandler);
				builder.AddInterfaceImplementation(typeof(INotifyPropertyChanged));

				var propertyChanged = builder.Builder.DefineEvent("PropertyChanged", EventAttributes.None, evtType);

				var propertyChangedBackingField = builder.DefineField<PropertyChangedEventHandler>("<PropertyChanged>");
				propertyChangedBackingField.ClearAttributes();
				propertyChangedBackingField.IncludeAttributes(FieldAttributes.Private);
				var ctor = typeof(NonSerializedAttribute).GetConstructor(Type.EmptyTypes);
				Debug.Assert(ctor != null, "ctor != null");
				propertyChangedBackingField.Builder.SetCustomAttribute(new CustomAttributeBuilder(ctor, new object[0]));

				// Emit a standard add <event handler> method (similar to what the C# compiler does)...
				/* 				 
				public void add_PropertyChanged(PropertyChangedEventHandler value)
				{
						PropertyChangedEventHandler orig;
						PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
						do
						{
								orig = propertyChanged;
								PropertyChangedEventHandler updated = (PropertyChangedEventHandler) Delegate.Combine(orig, value);
								check = Interlocked.CompareExchange<PropertyChangedEventHandler>(ref this._propertyChanged, updated, orig);
						}
						while (check != orig);
				}
				*/

				var add = builder.DefineMethod("add_PropertyChanged");
				add.ClearAttributes();
				add.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
					MethodAttributes.NewSlot |
					MethodAttributes.Virtual | MethodAttributes.Final);
				add.DefineParameter("value", evtType);
				add.ContributeInstructions((m, il) =>
				{
					var retry = il.DefineLabel();
					il.DeclareLocal(evtType);
					il.DeclareLocal(evtType);
					il.DeclareLocal(evtType);
					il.DeclareLocal(typeof(bool));
					il.LoadArg_0();
					il.LoadField(propertyChangedBackingField);
					il.StoreLocal_0();
					il.MarkLabel(retry);
					il.LoadLocal_0();
					il.StoreLocal_1();
					il.LoadLocal_1();
					il.LoadArg_1();
					il.Call<Delegate>("Combine", BindingFlags.Static | BindingFlags.Public, typeof(Delegate),
														typeof(Delegate));
					il.CastClass<PropertyChangedEventHandler>();
					il.StoreLocal_2();
					il.LoadArg_0();
					il.LoadFieldAddress(propertyChangedBackingField);
					il.LoadLocal_2();
					il.LoadLocal_1();
					var compex = (from c in typeof(Interlocked).GetMethods(BindingFlags.Static | BindingFlags.Public)
												where c.IsGenericMethodDefinition && c.Name == "CompareExchange"
												select c).Single();
					il.Call(compex.MakeGenericMethod(evtType));
					il.StoreLocal_0();
					il.LoadLocal_0();
					il.LoadLocal_1();
					il.CompareEqual();
					il.StoreLocal_3();
					il.LoadLocal_3();
					il.BranchIfTrue_ShortForm(retry);
				});
				var remove = builder.DefineMethod("remove_PropertyChanged");
				remove.ClearAttributes();
				remove.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
					MethodAttributes.NewSlot |
					MethodAttributes.Virtual | MethodAttributes.Final);
				remove.DefineParameter("value", evtType);
				remove.ContributeInstructions((m, il) =>
				{
					var retry = il.DefineLabel();
					il.DeclareLocal(evtType);
					il.DeclareLocal(evtType);
					il.DeclareLocal(evtType);
					il.DeclareLocal(typeof(bool));
					il.LoadArg_0();
					il.LoadField(propertyChangedBackingField);
					il.StoreLocal_0();
					il.MarkLabel(retry);
					il.LoadLocal_0();
					il.StoreLocal_1();
					il.LoadLocal_1();
					il.LoadArg_1();
					il.Call<Delegate>("Remove", BindingFlags.Static | BindingFlags.Public, typeof(Delegate),
														typeof(Delegate));
					il.CastClass<PropertyChangedEventHandler>();
					il.StoreLocal_2();
					il.LoadArg_0();
					il.LoadFieldAddress(propertyChangedBackingField);
					il.LoadLocal_2();
					il.LoadLocal_1();
					var compex = (from c in typeof(Interlocked).GetMethods(BindingFlags.Static | BindingFlags.Public)
												where c.IsGenericMethodDefinition && c.Name == "CompareExchange"
												select c).Single();
					il.Call(compex.MakeGenericMethod(evtType));
					il.StoreLocal_0();
					il.LoadLocal_0();
					il.LoadLocal_1();
					il.CompareEqual();
					il.StoreLocal_3();
					il.LoadLocal_3();
					il.BranchIfTrue_ShortForm(retry);
				});
				propertyChanged.SetAddOnMethod(add.Builder);
				propertyChanged.SetRemoveOnMethod(remove.Builder);

				var onPropertyChanged = builder.DefineMethod("HandlePropertyChanged");
				onPropertyChanged.ClearAttributes();
				onPropertyChanged.IncludeAttributes(MethodAttributes.HideBySig);
				onPropertyChanged.DefineParameter("propName", typeof(String));
				onPropertyChanged.ContributeInstructions((m, il) =>
				{
					var exit = il.DefineLabel();
					il.DeclareLocal(typeof(bool));

					il.LoadArg_0();
					il.LoadField(propertyChangedBackingField);
					il.LoadNull();
					il.CompareEqual();
					il.StoreLocal_0();
					il.LoadLocal_0();
					il.BranchIfTrue_ShortForm(exit);

					il.LoadArg_0();
					il.LoadField(propertyChangedBackingField);
					il.LoadArg_0();
					il.LoadArg_1();
					il.New<PropertyChangedEventArgs>(typeof(string));
					il.CallVirtual<PropertyChangedEventHandler>("Invoke", typeof(object),
																											typeof(PropertyChangedEventArgs));

					il.MarkLabel(exit);
				});

				return onPropertyChanged;
			}

			static void ImplementPropertiesForInterface(IMapping<TDataModel> mapping, Type intf, EmittedClass builder, List<PropertyRec> props,
				EmittedField dirtyFlags, EmittedMethod propChanged)
			{
				var properties = intf.GetProperties();
				foreach (var p in properties)
				{
					ImplementPropertyFor(mapping, intf, builder, props, p, dirtyFlags, propChanged);
				}
			}

			private static void ImplementPropertyFor(IMapping<TDataModel> mapping, Type intf, EmittedClass builder,
				IList<PropertyRec> props, PropertyInfo property,
				EmittedField dirtyFlags, EmittedMethod propChanged)
			{
				var col = (ColumnMapping<TDataModel>) mapping.Columns.SingleOrDefault(c => c.Member == property);
				PropertyRec rec;
				if (col != null)
				{
					props.Add(rec = PropertyRec.CreateOnColumn(intf, builder, col, property));
					if (col.IsReference)
					{
						EmitPropertyForReferenceColumn(rec, dirtyFlags, propChanged);
					}
					else
					{
						EmitPropertyForColumn(rec, dirtyFlags, propChanged);
					}
				}

				var coll = (CollectionMapping<TDataModel>) mapping.Collections.SingleOrDefault(c => c.LocalMember == property);
				if (coll != null)
				{
					props.Add(rec = PropertyRec.CreateOnCollection(mapping, intf, builder, coll, property));
					EmitPropertyForCollection(builder, rec, props, dirtyFlags, propChanged);
				}
			}

			private static void EmitPropertyForCollection(EmittedClass builder, PropertyRec rec, IList<PropertyRec> props, EmittedField dirtyFlags, EmittedMethod propChanged)
			{
				var observer =
					builder.DefineMethod(String.Concat(rec.EmittedField.Name, "_CollectionChanged"));
				observer.ClearAttributes();
				observer.IncludeAttributes(MethodAttributes.Private | MethodAttributes.HideBySig);
				observer.DefineParameter("sender", typeof (object));
				observer.DefineParameter("e", typeof (NotifyCollectionChangedEventArgs));
				observer.ContributeInstructions(
					(m, il) =>
					{
						il.LoadArg_0();
						il.LoadFieldAddress(dirtyFlags);
						il.LoadValue(rec.Ordinal);
						il.LoadValue(true);
						il.Call<BitVector>("set_Item");

						il.LoadArg_0();
						il.LoadValue(rec.Source.Name);
						il.Call(propChanged);
					});

				rec.CollectionChanged = observer;

				EmitMakeResolveCollectionCommand(builder, rec, props);

				rec.EmittedProperty.AddGetter()
					.ContributeInstructions((m, il) =>
					{
						//
						// if (this.<collection-field> == null)
						// {
						//    lock (this._sync)
						//    {
						//        if (this.<collection-field> == null)
						//        {
						//            this.<collection-field> = new DataModelCollectionReference<IMappedType, int>(<collection-name>, <lazy-collection-command-field>.Value, new NotifyCollectionChangedEventHandler(this.<collection-field>_CollectionChanged), this.ID);
						//        }
						//    }
						// }
						// return this.<collection-field>.GetCollection();
						//
						var flag = il.DeclareLocal<bool>();
						var res = il.DeclareLocal(rec.Source.PropertyType);
						var flag2 = il.DeclareLocal<bool>();
						var sync = il.DeclareLocal<object>();
						var outerElse = il.DefineLabel();
						var innerElse = il.DefineLabel();
						var endFinally = il.DefineLabel();
						
						il.LoadArg_0();
						rec.EmittedField.LoadValue(il);
						il.LoadNull();
						il.CompareEqual();
						il.Load_I4_0();
						il.CompareEqual();
						il.StoreLocal(flag2);
						il.LoadLocal(flag2);
						il.BranchIfTrue(outerElse);

						il.BeginExceptionBlock();
						il.Load_I4_0();
						il.StoreLocal(flag);
						il.LoadArg_0();
						il.LoadField(builder.Fields.First(f => f.Name == "_sync"));
						il.Duplicate();
						il.StoreLocal(sync);
						il.LoadLocalAddressShort(flag);
						il.Call(typeof(Monitor).GetMethod("Enter", BindingFlags.Static | BindingFlags.Public, null,
							 new [] { typeof(Object), typeof(bool).MakeByRefType() }, null)
							 );

						il.LoadArg_0();
						rec.EmittedField.LoadValue(il);
						il.LoadNull();
						il.CompareEqual();
						il.Load_I4_0();
						il.CompareEqual();
						il.StoreLocal(flag2);
						il.LoadLocal(flag2);
						il.BranchIfTrue(innerElse);

						il.LoadArg_0();
						il.LoadValue(rec.Source.Name);
						var lazyCmd = rec.CollectionStaticLazyCommand;
						lazyCmd.LoadValue(il);
						il.Call(lazyCmd.Builder.FieldType.GetProperty("Value").GetGetMethod());
						il.LoadArg_0();
						il.Emit(OpCodes.Ldftn, rec.CollectionChanged.Builder);
						var ctor = typeof(NotifyCollectionChangedEventHandler).GetConstructors()[0];
						il.NewObj(ctor);

						// load the param.
						foreach (var joinMem in rec.Collection.LocalJoinProperties)
						{
							il.LoadArg_0();
							if (joinMem.MemberType == MemberTypes.Property)
							{
								var localColumn = props.First(c => c.Source == joinMem);
								if (localColumn.Column.IsReference)
								{
									localColumn.EmittedField.LoadValue(il);
									il.CallVirtual(localColumn.FieldType.GetProperty("IdentityKey").GetGetMethod());
								}
								else
								{
									localColumn.EmittedField.LoadValue(il);
								}
							}
							if (joinMem.MemberType == MemberTypes.Field)
							{
								il.LoadField(((FieldInfo)joinMem));
							}
						}

						il.NewObj(rec.FieldType.GetConstructors()[0]); // assumes only one, matching the parameter types!
						il.StoreField(rec.EmittedField);
						
						il.MarkLabel(innerElse);
						il.Nop();

						il.BeginFinallyBlock();
						il.LoadLocal(flag);
						il.Load_I4_0();
						il.CompareEqual();
						il.StoreLocal(flag2);
						il.LoadLocal(flag2);
						il.BranchIfTrue(endFinally);
						il.LoadLocal(sync);
						il.Call(typeof (Monitor).GetMethod("Exit"));
						il.MarkLabel(endFinally);
						il.EndFinally();
						il.Nop();
						il.EndExceptionBlock();
						il.Nop();
						
						il.MarkLabel(outerElse);
						il.LoadArg_0();
						rec.EmittedField.LoadValue(il);
						il.CallVirtual(rec.EmittedField.FieldType.Target.GetMethod("GetCollection"));
						il.StoreLocal(res);
						il.LoadLocal(res);
					});
			}

			private static void EmitMakeResolveCollectionCommand(EmittedClass builder, PropertyRec rec, IList<PropertyRec> props)
			{
				// This code assumes the DataModelCollectionReference<,,,...> and the IDataModelQueryCommand<,,...>
				// have the same generic arguments other than the reference type's identity key.
				var paramTypes = rec.FieldType.GetGenericArguments();
				var referencedType = paramTypes[0];
				var referencedIdentityKeyType = paramTypes[1];
				var referencedDbConnectionType = paramTypes[2];
				const int parameterOffset = 3;
				var commandType = default(Type);
				var repositoryType = typeof(IDataModelRepository<,,>).MakeGenericType(referencedType, referencedIdentityKeyType, referencedDbConnectionType);
				switch (paramTypes.Length)
				{
					case 4: commandType = typeof(IDataModelQueryCommand<,,>).MakeGenericType(referencedType, referencedDbConnectionType, paramTypes[parameterOffset]); break;
					default:
						throw new NotImplementedException("DataModelEmitter missing implementation for DataModelCollectionReference<...> with " + paramTypes.Length + " type arguments.");
				}

				var lazyCmd = builder.DefineField(String.Concat(rec.EmittedField.Name, "_Command"), typeof(Lazy<>).MakeGenericType(commandType));
				lazyCmd.ClearAttributes();
				lazyCmd.IncludeAttributes(FieldAttributes.Private | FieldAttributes.Static);

				rec.CollectionStaticLazyCommand = lazyCmd;

				var make =
					builder.DefineMethod(String.Concat("MakeResolve", rec.Source.Name, "Command"));
				make.ClearAttributes();
				make.IncludeAttributes(MethodAttributes.Private | MethodAttributes.Static);
				make.ReturnType = TypeRef.FromType(commandType);
				make.ContributeInstructions((m, il) =>
				{
					// Build the Expression tree representing the join, and rely on the referent's repository to build a suitable command...
					//
					//   var repository = (IDataModelRepository<TReferent, TReferentIdentitKey, TDbConnection>) DataModel<TReferent>.GetRepository<TReferentIdentitKey>();
					//   return repository.Where<TParam>("<TReferent_ReferencedProperty[0]>", (model, param) => model.<LocalJoinProperty[0]> == param);
					//

					var repo = il.DeclareLocal(repositoryType);
					var res = il.DeclareLocal(commandType);
					var dataModelT = typeof(DataModel<>).MakeGenericType(referencedType);
					il.Call(dataModelT.GetMethod("GetRepository").MakeGenericMethod(referencedIdentityKeyType));
					il.CastClass(repositoryType);
					il.StoreLocal(repo);

					var selfExpr = il.DeclareLocal<ParameterExpression>();

					il.LoadLocal(repo);
					il.LoadValue(String.Concat(rec.Collection.ReferencedType.Name, "_", rec.Source.Name));

					il.LoadToken(typeof (TDataModel));
					il.Call<Type>("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public, typeof(RuntimeTypeHandle));
					il.LoadValue("self");
					il.Call<Expression>("Parameter", BindingFlags.Static | BindingFlags.Public, typeof(Type), typeof(string));
					il.StoreLocal(selfExpr);

					var exprParams = new List<Tuple<string, Type, LocalBuilder>>();
					for (var i = parameterOffset; i < paramTypes.Length; i++)
					{
						var p = rec.Collection.LocalJoinProperties[i - parameterOffset];
						var name = p.Name.Pascalize();
						var type = paramTypes[i];
						var expr = il.DeclareLocal<ParameterExpression>();
						exprParams.Add(Tuple.Create(name, type, expr));
						il.LoadToken(type);
						il.Call<Type>("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public, typeof(RuntimeTypeHandle));
						il.LoadValue(name);
						il.Call<Expression>("Parameter", BindingFlags.Static | BindingFlags.Public, typeof(Type), typeof(string));
						il.StoreLocal(expr);
					}

					for (var i = 0; i < exprParams.Count; i++)
					{
						var p = (PropertyInfo)rec.Collection.ReferencedProperties[i];
						var c = rec.Collection.ReferencedMapping.Columns.First(ea => ea.Member == p);
						il.LoadLocal(selfExpr);
						il.Emit(OpCodes.Ldtoken, p.GetGetMethod());
						il.Call<MethodBase>("GetMethodFromHandle", BindingFlags.Static | BindingFlags.Public, typeof(RuntimeMethodHandle));
						il.CastClass<MethodInfo>();
						il.Call<Expression>("Property", BindingFlags.Static | BindingFlags.Public, typeof(Expression), typeof(MethodInfo));
						if (c.IsReference)
						{
							var id = c.Mapping.Columns.First(pp => pp.IsIdentity);
							il.Emit(OpCodes.Ldtoken, ((PropertyInfo)id.Member).GetGetMethod());
							il.Call<MethodBase>("GetMethodFromHandle", BindingFlags.Static | BindingFlags.Public, typeof(RuntimeMethodHandle));
							il.CastClass<MethodInfo>();
							il.Call<Expression>("Property", BindingFlags.Static | BindingFlags.Public, typeof(Expression), typeof(MethodInfo));
						}
						il.LoadLocal(exprParams[i].Item3);
						il.Call<Expression>("Equal", BindingFlags.Static | BindingFlags.Public, typeof(Expression), typeof(Expression));
					}
					var arr = il.DeclareLocal<ParameterExpression[]>();
					il.NewArr(typeof(ParameterExpression), exprParams.Count + 1);
					il.StoreLocal(arr);
					il.LoadLocal(arr);
					il.Load_I4_0();
					il.LoadLocal(selfExpr);
					il.StoreElementRef();
					for (var i = 0; i < exprParams.Count; i++)
					{
						il.LoadLocal(arr);
						il.Load_I4(i + 1);
						il.LoadLocal(exprParams[i].Item3);
						il.StoreElementRef();
					}

					il.LoadLocal(arr);
					var funType = default(Type);
					var exprType = default(Type);
					var whereMethod = default(MethodInfo);
					switch (exprParams.Count)
					{
						case 1:
							funType = typeof(Func<,,>).MakeGenericType(referencedType, exprParams[0].Item2, typeof(bool));
							exprType = typeof(Expression<>).MakeGenericType(funType);
							whereMethod = repositoryType.MatchGenericMethod("Where", 1, commandType, typeof(String), exprType)
								.MakeGenericMethod(exprParams[0].Item2);
							break;
					}
					il.Call(typeof(Expression).MatchGenericMethod("Lambda", BindingFlags.Static | BindingFlags.Public, 1, exprType, typeof(Expression), typeof(ParameterExpression[]))
						.MakeGenericMethod(funType));
					il.CallVirtual(whereMethod);
					il.StoreLocal(res);

					var dumb = il.DefineLabel();
					il.Branch_ShortForm(dumb);
					il.MarkLabel(dumb);

					il.LoadLocal(res);
				});
				rec.CollectionMakeResolve = make;
			}

			private static void EmitPropertyForReferenceColumn(PropertyRec rec, EmittedField dirtyFlags, EmittedMethod propChanged)
			{
				rec.EmittedProperty.AddGetter()
					.ContributeInstructions((m, il) =>
					{
						il.LoadArg_0();
						il.LoadField(rec.EmittedField);
						var res = il.DeclareLocal(rec.Source.PropertyType);
						il.CallVirtual(rec.FieldType.GetProperty("Model")
							.GetGetMethod());
						il.StoreLocal(res);
						var exit = il.DefineLabel();
						il.Branch_ShortForm(exit);
						il.MarkLabel(exit);
						il.LoadLocal(res);
					});
				if (rec.Source.CanWrite)
				{
					rec.EmittedProperty.AddSetter()
						.ContributeInstructions((m, il) =>
						{
							var cont = il.DefineLabel();
							var exit = il.DefineLabel();

							il.DeclareLocal(typeof (bool));

							il.LoadArg_0();
							il.LoadField(rec.EmittedField);
							il.LoadArg_1();
							il.CallVirtual(rec.FieldType.GetMethod("Equals", new[] {rec.Source.PropertyType}));

							il.StoreLocal_0();
							il.LoadLocal_0();
							il.BranchIfFalse_ShortForm(cont);
							il.Branch(exit);
							il.MarkLabel(cont);

							// TODO: emit column validation in setter

							il.LoadArg_0();
							il.LoadArg_1();
							il.NewObj(rec.FieldType.GetConstructor(new[] {rec.Source.PropertyType}));
							il.StoreField(rec.EmittedField);

							il.LoadArg_0();
							il.LoadFieldAddress(dirtyFlags);
							il.LoadValue(rec.Ordinal);
							il.LoadValue(true);
							il.Call<BitVector>("set_Item");

							il.LoadArg_0();
							il.LoadValue(rec.Source.Name);
							il.Call(propChanged);

							il.MarkLabel(exit);
						});
				}
			}

			private static void EmitPropertyForColumn(PropertyRec rec, EmittedField dirtyFlags, EmittedMethod propChanged)
			{
				rec.EmittedProperty.AddGetter()
					.ContributeInstructions((m, il) =>
					{
						il.LoadArg_0();
						il.LoadField(rec.EmittedField);
					});
				if (rec.Source.CanWrite)
				{
					rec.EmittedProperty.AddSetter()
						.ContributeInstructions((m, il) =>
						{
							var cont = il.DefineLabel();
							var exit = il.DefineLabel();

							il.DeclareLocal(typeof (bool));


							if (rec.FieldType.IsArray)
							{
								var elmType = rec.FieldType.GetElementType();
								LoadFieldFromThisAndValue(il, rec.EmittedField);
								il.Call(typeof (Extensions).GetMethod("EqualsOrItemsEqual", BindingFlags.Static | BindingFlags.Public)
									.MakeGenericMethod(elmType));
							}
							else if (rec.FieldType.IsClass)
							{
								var opEquality = rec.FieldType.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static);
								if (opEquality != null)
								{
									LoadFieldFromThisAndValue(il, rec.EmittedField);
									il.Call(opEquality);
								}
								else
								{
									il.Call(typeof (EqualityComparer<>).MakeGenericType(rec.FieldType)
										.GetMethod("get_Default", BindingFlags.Static | BindingFlags.Public));
									LoadFieldFromThisAndValue(il, rec.EmittedField);
									il.CallVirtual(typeof (IEqualityComparer<>).MakeGenericType(rec.FieldType)
										.GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance,
											null,
											new[] {rec.FieldType, rec.FieldType},
											null
										));
								}
							}
							else
							{
								LoadFieldFromThisAndValue(il, rec.EmittedField);
								il.CompareEquality(rec.FieldType);
							}
							il.StoreLocal_0();
							il.LoadLocal_0();
							il.BranchIfFalse_ShortForm(cont);
							il.Branch(exit);
							il.MarkLabel(cont);

							// TODO: emit column validation in setter


							il.LoadArg_0();
							il.LoadArg_1();
							il.StoreField(rec.EmittedField);

							il.LoadArg_0();
							il.LoadFieldAddress(dirtyFlags);
							il.LoadValue(rec.Ordinal);
							il.LoadValue(true);
							il.Call<BitVector>("set_Item");

							il.LoadArg_0();
							il.LoadValue(rec.Source.Name);
							il.Call(propChanged);

							il.MarkLabel(exit);
						});
				}
			}

			static EmittedMethod ImplementSpecializedEquals(EmittedClass builder)
			{
				var equatable = typeof(IEquatable<>).MakeGenericType(builder.Builder);
				builder.AddInterfaceImplementation(equatable);

				var specializedEquals = builder.DefineMethod("Equals");
				specializedEquals.ClearAttributes();
				specializedEquals.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
					MethodAttributes.Virtual | MethodAttributes.Final);
				specializedEquals.ReturnType = TypeRef.FromType<bool>();
				var other = specializedEquals.DefineParameter("other", builder.Ref);

				specializedEquals.ContributeInstructions((m, il) =>
				{
					il.DeclareLocal(typeof(bool));
					var exitFalse = il.DefineLabel();

					var fields =
						new List<EmittedField>(
							builder.Fields.Where(f => f.IsStatic == false 
								&& f.FieldType.Target != typeof(PropertyChangedEventHandler)
								&& f.Name != "_sync"
								));
					for (var i = 0; i < fields.Count; i++)
					{
						var field = fields[i];
						var fieldType = field.FieldType.Target;
						if (fieldType.IsArray)
						{
							var elmType = fieldType.GetElementType();
							LoadFieldsFromThisAndParam(il, field, other);
							il.Call(typeof(Extensions).GetMethod("EqualsOrItemsEqual", BindingFlags.Static | BindingFlags.Public)
																				.MakeGenericMethod(elmType));
						}
						else if (fieldType.IsClass)
						{
							if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(ObservableCollection<>))
							{
								// compare observable collections for member equality...
								var genericArg = fieldType.GetGenericArguments()[0];
								var etype = typeof(IEnumerable<>).MakeGenericType(genericArg);
								var sequenceEquals = typeof(Enumerable).MatchGenericMethod("SequenceEqual",
																																					BindingFlags.Static | BindingFlags.Public, 1, typeof(bool), etype, etype);
								LoadFieldsFromThisAndParam(il, field, other);
								il.Call(sequenceEquals.MakeGenericMethod(genericArg));
							}
							else
							{
								var opEquality = fieldType.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static);
								if (opEquality != null)
								{
									LoadFieldsFromThisAndParam(il, field, other);
									il.Call(opEquality);
								}
								else
								{
									il.Call(typeof(EqualityComparer<>).MakeGenericType(fieldType)
																										.GetMethod("get_Default", BindingFlags.Static | BindingFlags.Public));
									LoadFieldsFromThisAndParam(il, field, other);
									il.CallVirtual(typeof(IEqualityComparer<>).MakeGenericType(fieldType)
																														.GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance,
																																			null,
																																			new[] {fieldType, fieldType},
																																			null
																	));
								}
							}
						}
						else
						{
							LoadFieldsFromThisAndParam(il, field, other);
							il.CompareEquality(fieldType);
						}
						if (i < fields.Count - 1)
						{
							il.BranchIfFalse(exitFalse);
						}
					}
					var exit = il.DefineLabel();
					il.Branch(exit);
					il.MarkLabel(exitFalse);
					il.Load_I4_0();
					il.MarkLabel(exit);
					il.StoreLocal_0();
					var fin = il.DefineLabel();
					il.Branch(fin);
					il.MarkLabel(fin);
					il.LoadLocal_0();
				});

				var contributedEquals = new Action<EmittedMethodBase, ILGenerator>((m, il) =>
				{
					var exitFalse2 = il.DefineLabel();
					var exit = il.DefineLabel();
					il.DeclareLocal(typeof(bool));

					il.LoadArg_1();
					il.IsInstance(builder.Builder);
					il.BranchIfFalse(exitFalse2);
					il.LoadArg_0();
					il.LoadArg_1();
					il.CastClass(builder.Builder);
					il.Call(specializedEquals);
					il.Branch(exit);
					il.MarkLabel(exitFalse2);
					il.LoadValue(false);
					il.MarkLabel(exit);
					il.StoreLocal_0();
					var fin = il.DefineLabel();
					il.Branch(fin);
					il.MarkLabel(fin);
					il.LoadLocal_0();
				});

				var equatableT = typeof(IEquatable<>).MakeGenericType(typeof(TDataModel));
				builder.AddInterfaceImplementation(equatableT);
				var equalsT = builder.DefineMethod("Equals");
				equalsT.ClearAttributes();
				equalsT.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
					MethodAttributes.Virtual | MethodAttributes.Final);
				equalsT.ReturnType = TypeRef.FromType<bool>();
				equalsT.DefineParameter("other", typeof(TDataModel));
				equalsT.ContributeInstructions(contributedEquals);

				builder.DefineOverrideMethod(typeof(Object).GetMethod("Equals", BindingFlags.Instance | BindingFlags.Public, null,
																															new[] {typeof(object)}, null))
							.ContributeInstructions(contributedEquals);

				return specializedEquals;
			}

			static void ImplementSpecializedGetHashCode(EmittedClass builder, EmittedField chashCodeSeed)
			{
				Contract.Requires<ArgumentNullException>(builder != null);

				var method = builder.DefineMethod("GetHashCode");
				method.ClearAttributes();
				method.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual);
				method.ReturnType = TypeRef.FromType<int>();
				method.ContributeInstructions((m, il) =>
				{
					var prime = il.DeclareLocal(typeof(Int32));
					var result = il.DeclareLocal(typeof(Int32));
					il.DeclareLocal(typeof(Int32));
					il.DeclareLocal(typeof(bool));

					il.LoadValue(Constants.NotSoRandomPrime);
					il.StoreLocal(prime);
					il.LoadValue(chashCodeSeed);
					il.LoadLocal(prime);
					il.Multiply();
					il.StoreLocal(result);
					var exit = il.DefineLabel();
					var fields =
						new List<EmittedField>(
							builder.Fields.Where(f => f.IsStatic == false 
								&& f.FieldType.Target != typeof(PropertyChangedEventHandler)
								&& f.Name != "_sync"
								));
					var lbl = default(Label);
					foreach (var field in fields)
					{
						var fieldType = field.FieldType.Target;
						var tc = Type.GetTypeCode(fieldType);
						switch (tc)
						{
							case TypeCode.Boolean:
							case TypeCode.Byte:
							case TypeCode.Char:
							case TypeCode.Int16:
							case TypeCode.Int32:
							case TypeCode.SByte:
							case TypeCode.Single:
							case TypeCode.UInt16:
							case TypeCode.UInt32:
								il.LoadLocal(result);
								il.LoadLocal(prime);
								il.LoadArg_0();
								il.LoadField(field);
								il.Multiply();
								il.Xor();
								il.StoreLocal(result);
								break;
							case TypeCode.DateTime:
								il.LoadLocal(result);
								il.LoadLocal(prime);
								il.LoadArg_0();
								il.LoadFieldAddress(field);
								il.Constrained(typeof(DateTime));
								il.CallVirtual<object>("GetHashCode");
								il.Multiply();
								il.Xor();
								il.StoreLocal(result);
								break;
							case TypeCode.Decimal:
								il.LoadLocal(result);
								il.LoadLocal(prime);
								il.LoadArg_0();
								il.LoadFieldAddress(field);
								il.Call<Decimal>("GetHashCode");
								il.Multiply();
								il.Xor();
								il.StoreLocal(result);
								break;
							case TypeCode.Double:
								il.LoadLocal(result);
								il.LoadLocal(prime);
								il.LoadArg_0();
								il.LoadFieldAddress(field);
								il.Call<Double>("GetHashCode");
								il.Multiply();
								il.Xor();
								il.StoreLocal(result);
								break;
							case TypeCode.Int64:
								il.LoadLocal(result);
								il.LoadLocal(prime);
								il.LoadArg_0();
								il.LoadFieldAddress(field);
								il.Constrained(typeof(Int64));
								il.CallVirtual<object>("GetHashCode");
								il.Multiply();
								il.Xor();
								il.StoreLocal(result);
								break;
							case TypeCode.Object:
								if (typeof(Guid).IsAssignableFrom(fieldType))
								{
									il.LoadLocal(result);
									il.LoadLocal(prime);
									il.LoadArg_0();
									il.LoadFieldAddress(field);
									il.Constrained(typeof(Guid));
									il.CallVirtual<object>("GetHashCode");
									il.Multiply();
									il.Xor();
									il.StoreLocal(result);
								}
								else if (fieldType.IsArray)
								{
									var elmType = fieldType.GetElementType();
									il.LoadLocal(result);
									il.LoadLocal(prime);
									il.LoadArg_0();
									il.LoadField(field);
									il.LoadLocal(result);
									il.Call(typeof(Extensions).GetMethod("CalculateCombinedHashcode", BindingFlags.Public | BindingFlags.Static)
																						.MakeGenericMethod(elmType));
									il.Multiply();
									il.Xor();
									il.StoreLocal(result);
								}
								else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
								{
									il.LoadLocal(result);
									il.LoadLocal(prime);
									il.LoadArg_0();
									il.LoadFieldAddress(field);
									il.Constrained(fieldType);
									il.CallVirtual<object>("GetHashCode");
									il.Multiply();
									il.Xor();
									il.StoreLocal(result);
								}
								else if (fieldType.IsValueType)
								{
									il.LoadLocal(result);
									il.LoadLocal(prime);
									il.LoadArg_0();
									il.LoadFieldAddress(field);
									il.Constrained(fieldType);
									il.CallVirtual<object>("GetHashCode");
									il.Multiply();
									il.Xor();
									il.StoreLocal(result);
								}
								else
								{
									il.LoadArg_0();
									il.LoadField(field);
									il.LoadNull();
									il.CompareEqual();
									il.StoreLocal_2();
									il.LoadLocal_2();
									lbl = il.DefineLabel();
									il.BranchIfTrue_ShortForm(lbl);

									il.LoadLocal(result);
									il.LoadLocal(prime);
									il.LoadArg_0();
									il.LoadField(field);
									il.CallVirtual<object>("GetHashCode");
									il.Multiply();
									il.Xor();
									il.StoreLocal(result);
									il.MarkLabel(lbl);
								}
								break;
							case TypeCode.String:
								il.LoadArg_0();
								il.LoadField(field);
								il.LoadNull();
								il.CompareEqual();
								il.StoreLocal_2();
								il.LoadLocal_2();
								lbl = il.DefineLabel();
								il.BranchIfTrue_ShortForm(lbl);

								il.LoadLocal(result);
								il.LoadLocal(prime);
								il.LoadArg_0();
								il.LoadField(field);
								il.CallVirtual<object>("GetHashCode");
								il.Multiply();
								il.Xor();
								il.StoreLocal(result);
								il.MarkLabel(lbl);
								break;
							case TypeCode.UInt64:
								il.LoadLocal(result);
								il.LoadLocal(prime);
								il.LoadArg_0();
								il.LoadFieldAddress(field);
								il.Constrained(typeof(UInt64));
								il.CallVirtual<object>("GetHashCode");
								il.Multiply();
								il.Xor();
								il.StoreLocal(result);
								break;
							default:
								throw new InvalidOperationException(String.Concat("Unable to produce hashcode for type: ",
																																	fieldType.GetReadableFullName()));
						}
					}
					il.LoadLocal(result);
					il.StoreLocal_2();
					il.Branch(exit);
					il.MarkLabel(exit);
					il.LoadLocal_2();
				});
			}

			static void LoadFieldFromThisAndValue(ILGenerator il, EmittedField field)
			{
				Contract.Requires<ArgumentNullException>(il != null);
				Contract.Requires<ArgumentNullException>(field != null);
				il.LoadArg_0();
				il.LoadField(field);
				il.LoadArg_1();
			}

			static void LoadFieldsFromThisAndParam(ILGenerator il, EmittedField field, EmittedParameter parm)
			{
				Contract.Requires<ArgumentNullException>(il != null);
				Contract.Requires<ArgumentNullException>(field != null);
				il.LoadArg_0();
				il.LoadField(field);
				il.LoadArg(parm);
				il.LoadField(field);
			}

			class PropertyRec
			{
				public ColumnMapping<TDataModel> Column { get; private set; }
				public CollectionMapping<TDataModel> Collection { get; private set; }
				public EmittedField EmittedField { get; private set; }
				public EmittedProperty EmittedProperty { get; private set; }
				public Type FieldType { get; private set; }
				public bool IsCollection { get { return Collection != null; } }
				public bool IsColumn { get { return Column != null; } }
				public bool IsLifted { get; private set; }
				public PropertyInfo Source { get; private set; }
				public Type ReferenceTargetMemberType { get; private set; }

				public int Ordinal { get; private set; }

				public static PropertyRec CreateOnColumn(Type intf, EmittedClass builder, ColumnMapping<TDataModel> col, PropertyInfo info)
				{
					var fieldName = String.Concat("<", intf.Name, "_", info.Name, ">_field");
					var res = new PropertyRec
					{
						Column = col,
						Ordinal = col.Ordinal,
						Source = info,
						FieldType = info.PropertyType
					};

					if (col.IsReference)
					{
						res.ReferenceTargetMemberType = col.ReferenceTargetMember.GetTypeOfValue();
						res.FieldType = typeof(DataModelReference<,>).MakeGenericType(res.FieldType, res.ReferenceTargetMemberType);
					}
					
					res.IsLifted = info.IsDefined(typeof(MapLiftedColumnAttribute), true);
					res.EmittedProperty = builder.DefinePropertyFromPropertyInfo(info);
					res.EmittedField = builder.DefineField(fieldName, res.FieldType);
					return res;
				}

				public static PropertyRec CreateOnCollection(IMapping<TDataModel> mapping, Type intf, EmittedClass builder, CollectionMapping<TDataModel> coll, PropertyInfo info)
				{
					var fieldName = String.Concat("<", intf.Name, "_", info.Name, ">");
					var res = new PropertyRec
					{
						Collection = coll,
						Source = info
					};

					var colls = mapping.Collections.ToArray();
					res.Ordinal = mapping.Columns.Count() + Array.IndexOf(colls, coll);

					res.FieldType = coll.MakeCollectionReferenceType(coll.ReferencedMapping);

					res.EmittedProperty = builder.DefinePropertyFromPropertyInfo(info);
					res.EmittedField = builder.DefineField(fieldName, res.FieldType);
					return res;
				}

				public EmittedMethod CollectionChanged { get; set; }

				public EmittedField CollectionStaticLazyCommand { get; set; }

				public EmittedMethod CollectionMakeResolve { get; set; }
			}
		}
	}
}