using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using FlitBit.Core;
using FlitBit.Core.Collections;
using FlitBit.Emit;
using FlitBit.Data.SPI;

namespace FlitBit.Data
{	
	/// <summary>
	/// Utility class for fulfilling the DataModel stereotype.
	/// </summary>
	internal static class DataModels
	{
		internal static readonly string CDirtyFlagsBackingFieldName = "$DirtyFlags";

		static readonly Lazy<EmittedModule> __module = new Lazy<EmittedModule>(() =>
		{ return RuntimeAssemblies.DynamicAssembly.DefineModule("DataModels", null); },
			LazyThreadSafetyMode.ExecutionAndPublication
			);

		static EmittedModule Module { get { return __module.Value; } }
		
		#region Emit ConcreteType<T>

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type ConcreteType<T>()
		{
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(T);
			string typeName = RuntimeAssemblies.PrepareTypeName(targetType, "DataModel");

			var module = DataModels.Module;
			lock (module)
			{
				Type type = module.Builder.GetType(typeName, false, false);
				if (type == null)
				{
					type = BuildDataModel<T>(module, typeName);
				}
				return type;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		static Type BuildDataModel<T>(EmittedModule module, string typeName)
		{
			Contract.Requires(module != null);
			Contract.Requires(typeName != null);
			Contract.Requires(typeName.Length > 0);
			Contract.Ensures(Contract.Result<Type>() != null);

			var builder = module.DefineClass(typeName, EmittedClass.DefaultTypeAttributes, typeof(object), null);
			builder.Attributes = TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;

			builder.Builder.SetCustomAttribute(new CustomAttributeBuilder(
				typeof(SerializableAttribute).GetConstructor(Type.EmptyTypes), new object[0])
				);

			var chashCodeSeed = builder.DefineField<int>("CHashCodeSeed");
			chashCodeSeed.IncludeAttributes(FieldAttributes.Static | FieldAttributes.Private | FieldAttributes.InitOnly);
			var cctor = builder.DefineCCtor();
			cctor.ContributeInstructions((m, il) =>
			{
				il.LoadType(builder.Builder);
				il.CallVirtual(typeof(Type).GetProperty("AssemblyQualifiedName").GetGetMethod());
				il.CallVirtual<object>("GetHashCode");
				il.StoreField(chashCodeSeed);
			});
			var dataType = BackingDataType<T>();
			var data = builder.DefineField("_data", dataType);

			var ctor = builder.DefineDefaultCtor();
			ctor.ContributeInstructions((m, il) =>
			{
				il.LoadArg_0();
				il.Call(dataType.GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic));
				il.StoreField(data);
				il.Nop();
			});

			List<PropertyInfo> props = new List<PropertyInfo>();
			foreach (var intf in from type in typeof(T).GetTypeHierarchyInDeclarationOrder()
													 where type.IsInterface
															&& type != typeof(IEquatable<T>)
															&& type != typeof(IDataModel)
															&& type != typeof(ICloneable)
													 select type)
			{
				builder.AddInterfaceImplementation(intf);
				ImplementPropertiesForInterface(intf, builder, data, dataType, props);
				builder.StubMethodsForInterface(intf, true, true);
			}
			ImplementIEquatable<T>(builder, data, dataType);
			ImplementIDataModelSPI<T>(builder, data, dataType, props, cctor);
			ImplementICloneable<T>(builder, ctor, data, dataType);
			ImplementSpecializedGetHashCode(builder, data, dataType, chashCodeSeed);
			builder.Compile();
			return builder.Ref.Target;
		}

		static void ImplementICloneable<T>(EmittedClass builder, EmittedConstructor ctor, EmittedField data, Type dataType)
		{
			Contract.Requires(builder != null);
			Contract.Requires(ctor != null);
			Contract.Requires(data != null);
			Contract.Requires(dataType != null);

			builder.AddInterfaceImplementation(typeof(ICloneable));
			var clone = builder.DefineMethod("Clone");
			clone.ReturnType = TypeRef.FromType<object>();
			clone.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final);
			clone.ContributeInstructions((m, il) =>
			{
				il.DeclareLocal(builder.Builder);
				il.DeclareLocal(typeof(object));
				il.Nop();
				il.NewObj(ctor.Builder);
				il.StoreLocal_0();
				il.LoadLocal_0();
				il.LoadArg_0();
				il.LoadFieldAddress(data);
				il.Call(dataType.GetMethod("Copy", BindingFlags.Instance | BindingFlags.NonPublic));
				il.StoreField(data);
				il.LoadLocal_0();
				il.StoreLocal_1();
				il.DefineAndMarkLabel();
				il.LoadLocal_1();
			});	
		}

		static void ImplementIDataModelSPI<T>(EmittedClass builder, EmittedField data, Type dataType, List<PropertyInfo> props, EmittedConstructor cctor)
		{
			builder.AddInterfaceImplementation(typeof(IDataModel));
			ImplementIDataModelSPI_GetDirtyFlags<T>(builder, data, dataType);
			ImplementIDataModelSPI_IsDirty<T>(builder, data, dataType, props, cctor);
		}

		private static void ImplementIDataModelSPI_IsDirty<T>(EmittedClass builder, EmittedField data, Type dataType, List<PropertyInfo> props, EmittedConstructor cctor)
		{
			var fieldMap = builder.DefineField<List<string>>("__fieldMap");
			fieldMap.ClearAttributes();
			fieldMap.IncludeAttributes(FieldAttributes.Static | FieldAttributes.InitOnly | FieldAttributes.Private);
			cctor.ContributeInstructions((m, il) =>
			{
				var arr = il.DeclareLocal(typeof(String[]));
				il.Nop();
				il.LoadValue(props.Count);
				il.New<List<string>>(typeof(int));
				il.StoreField(fieldMap);
				il.LoadField(fieldMap);
				il.NewArr(typeof(String), props.Count);
				il.StoreLocal(arr);
				for (var i = 0; i < props.Count; i++)
				{
					il.LoadLocal(arr);
					il.LoadValue(i);
					il.LoadValue(props[i].Name);
					il.Emit(OpCodes.Stelem, typeof(string));
				}
				il.LoadLocal(arr);
				il.CallVirtual<List<string>>("AddRange", typeof(IEnumerable<string>));
				il.Nop();
				il.Nop();
			});

			var method = builder.DefineMethod("IsDirty");
			method.DefineParameter("member", typeof(String));
			method.ReturnType = TypeRef.FromType<bool>();
			method.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final);
			method.ContributeInstructions((m, il) =>
			{
				il.DeclareLocal(typeof(Int32));
				il.DeclareLocal(typeof(bool));
				il.DeclareLocal(typeof(bool));
				var yep = il.DefineLabel();
				il.Nop();
				il.LoadField(fieldMap);
				il.LoadArg_1();
				il.CallVirtual<List<string>>("IndexOf", typeof(string));
				il.StoreLocal_0();
				il.LoadLocal_0();
				il.LoadValue(0);
				il.CompareLessThan();
				il.LoadValue(0);
				il.CompareEqual();
				il.StoreLocal_2();
				il.LoadLocal_2();
				il.BranchIfTrue(yep);
				il.Nop();
				il.LoadValue("member");
				il.LoadValue(String.Concat(typeof(T).GetReadableSimpleName(), " does not define property: `"));
				il.LoadArg_1();
				il.LoadValue("`.");
				il.Call<string>("Concat", BindingFlags.Static | BindingFlags.Public, typeof(string), typeof(string), typeof(string));
				il.NewObj(typeof(ArgumentOutOfRangeException).GetConstructor(new Type[] { typeof(string), typeof(string) }));
				il.Throw();
				il.MarkLabel(yep);
				il.LoadArg_0();
				il.LoadFieldAddress(data);
				il.LoadFieldAddress(dataType.GetField(CDirtyFlagsBackingFieldName));
				il.LoadLocal_0();
				il.Call<BitVector>("get_Item", typeof(int));
				il.StoreLocal_1();
				il.DefineAndMarkLabel();
				il.LoadLocal_1();
			});
		}

		static void ImplementIEquatable<T>(EmittedClass builder, EmittedField data, Type dataType)
		{

			builder.AddInterfaceImplementation(typeof(IEquatable<T>)); 
			var specialized_equals = builder.DefineMethodFromInfo(typeof(IEquatable<T>).GetMethod("Equals", new Type[] { typeof(T) }));
			specialized_equals.ContributeInstructions((m, il) =>
			{
				var exitFalse = il.DefineLabel();
				var exit = il.DefineLabel();

				il.DeclareLocal(builder.Builder);
				il.DeclareLocal(typeof(bool));
				il.LoadArg_1();
				il.IsInstance(builder.Builder);
				il.StoreLocal_0();
				il.LoadArg_1();
				il.BranchIfFalse_ShortForm(exitFalse);

				il.LoadArg_0();
				il.LoadFieldAddress(data);
				il.LoadLocal_0();
				il.LoadField(data);
				il.Call(dataType.GetMethod("Equals", new Type[] { dataType }));
				il.Branch_ShortForm(exit);

				il.MarkLabel(exitFalse);
				il.LoadValue(false);
				il.MarkLabel(exit);
				il.StoreLocal_1();
				il.DefineAndMarkLabel();
				il.LoadLocal_1();
			});

			//
			// public override bool Equals(object obj)
			// {
			//    return ((obj is T) && this.Equals((T)obj)));
			// }
			var equals = builder.DefineOverrideMethod(typeof(Object).GetMethod("Equals", new Type[] { typeof(Object) }));
			equals.ContributeInstructions((m, il) =>
			{
				il.DeclareLocal(typeof(bool));
				var exitfalse = il.DefineLabel();
				var res = il.DefineLabel();
				il.Nop();
				il.LoadArg_1();
				il.IsInstance(typeof(T));
				il.BranchIfFalse(exitfalse);
				il.LoadArg_0();
				il.LoadArg_1();
				il.CastClass(typeof(T));
				il.Call(specialized_equals.Builder);
				il.Branch(res);
				il.MarkLabel(exitfalse);
				il.LoadValue(false);
				il.MarkLabel(res);
				il.StoreLocal_0();
				il.DefineAndMarkLabel();
				il.LoadLocal_0();
			});
		}

		static void ImplementPropertiesForInterface(Type intf, EmittedClass builder, EmittedField data, Type dataType, List<PropertyInfo> props)
		{
			var properties = intf.GetProperties();
			foreach (var p in properties)
			{
				ImplementPropertyFor(builder, p, data, dataType);
				props.Add(p);
			}
		}

		static void ImplementPropertyFor(EmittedClass builder, PropertyInfo property, EmittedField data, Type dataType)
		{
			var prop = builder.DefinePropertyFromPropertyInfo(property);
			var fieldName = property.FormatBackingFieldName();
			var backingField = dataType.GetField(fieldName);

			prop.AddGetter().ContributeInstructions((m, il) =>
			{
				il.LoadArg_0();
				il.LoadFieldAddress(data);
				il.LoadField(backingField);
			});
			if (property.CanWrite)
			{
				prop.AddSetter().ContributeInstructions((m, il) =>
				{
					il.Nop();
					il.LoadArg_0();
					il.LoadFieldAddress(data);
					il.LoadArg_1();
					il.Call(dataType.GetMethod(String.Concat("Write", fieldName), BindingFlags.Instance | BindingFlags.Public, null, new Type[] { prop.TargetType }, null));
					il.Pop();
				});
			}
		}

		static void ImplementIDataModelSPI_GetDirtyFlags<T>(EmittedClass builder, EmittedField data, Type dataType)
		{
			var baseMethod = typeof(IDataModel).GetMethod("GetDirtyFlags", BindingFlags.Public | BindingFlags.Instance);
			var method = builder.DefineMethodFromInfo(baseMethod);

			method.DefineLocal("res", typeof(BitVector));
			method.ContributeInstructions((m, il) =>
			{
				var exit = il.DefineLabel();
				il.Nop();
				il.LoadArg_0();
				il.LoadFieldAddress(data);
				il.LoadField(dataType.GetField(CDirtyFlagsBackingFieldName));
				il.StoreLocal_0();
				il.DefineAndMarkLabel();
				il.LoadLocal_0();
			});
		}
				
		static EmittedMethod ImplementSpecializedGetHashCode(EmittedClass builder, EmittedField data, Type dataType, EmittedField chashCodeSeed)
		{
			Contract.Requires<ArgumentNullException>(builder != null);
			Contract.Requires<ArgumentNullException>(data != null);
			Contract.Requires<ArgumentNullException>(chashCodeSeed != null);
			Contract.Requires<ArgumentNullException>(dataType != null);

			var baseMethod = builder.Builder.BaseType.GetMethod("GetHashCode", BindingFlags.Instance | BindingFlags.Public);
			var method = builder.DefineOverrideMethod(baseMethod);
			method.ContributeInstructions((m, il) =>
			{
				var prime = il.DeclareLocal(typeof(Int32));
				var result = il.DeclareLocal(typeof(Int32));
				var exit = il.DefineLabel();
				il.DeclareLocal(typeof(Int32));
				il.Nop();
				il.LoadValue(0xf3e9b);
				il.StoreLocal(prime);
				il.LoadValue(chashCodeSeed);
				il.LoadLocal(prime);
				il.Multiply();
				il.StoreLocal(result);
				il.LoadLocal(result);
				il.LoadArg_0();
				il.Call(baseMethod);
				il.LoadLocal(prime);
				il.Multiply();
				il.Xor();
				il.StoreLocal(result);
				il.LoadLocal(result);
				il.LoadArg_0();
				il.LoadFieldAddress(data);
				il.Constrained(dataType);
				il.CallVirtual<object>("GetHashCode");
				il.LoadLocal(prime);
				il.Multiply();
				il.Xor();
				il.StoreLocal(result);
				il.LoadLocal(result);
				il.StoreLocal_2();
				il.Branch(exit);
				il.MarkLabel(exit);
				il.LoadLocal_2();
			});
			return method;
		}

		#endregion

		#region Emit BackingDataType<T>

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
		internal static Type BackingDataType<T>()
		{
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(T);
			string typeName = RuntimeAssemblies.PrepareTypeName(targetType, "DataModel$Data");

			var module = DataModels.Module;
			lock (module)
			{
				Type type = module.Builder.GetType(typeName, false, false);
				if (type == null)
				{
					type = BuildBackingDataType<T>(module, typeName, t => true);
				}
				return type;
			}
		}

		static Type BuildBackingDataType<T>(EmittedModule module, string typeName, Func<Type, bool> interfaceFilter)
		{
			Contract.Requires(module != null);
			Contract.Requires(typeName != null);
			Contract.Requires(typeName.Length > 0);
			Contract.Ensures(Contract.Result<Type>() != null);

			var builder = module.DefineClass(
				typeName,
				EmittedClass.DefaultTypeAttributes,
				typeof(ValueType),
				null
				);
			builder.Attributes = TypeAttributes.SequentialLayout | TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;

			builder.Builder.SetCustomAttribute(new CustomAttributeBuilder(
				typeof(SerializableAttribute).GetConstructor(Type.EmptyTypes), new object[0])
				);

			var chashCodeSeed = builder.DefineField<int>("CHashCodeSeed");
			chashCodeSeed.IncludeAttributes(FieldAttributes.Static | FieldAttributes.Private | FieldAttributes.InitOnly);
			var cctor = builder.DefineCCtor();
			cctor.ContributeInstructions((m, il) =>
			{
				il.LoadType(builder.Builder);
				il.CallVirtual(typeof(Type).GetProperty("AssemblyQualifiedName").GetGetMethod());
				il.CallVirtual<object>("GetHashCode");
				il.StoreField(chashCodeSeed);
			});

			var dirtyFlagsField = builder.DefineField<BitVector>(CDirtyFlagsBackingFieldName);
			dirtyFlagsField.ClearAttributes();
			dirtyFlagsField.IncludeAttributes(FieldAttributes.Public);

			int fieldIndex = 0;
			foreach (var intf in from type in typeof(T).GetTypeHierarchyInDeclarationOrder()
													 where type.IsInterface && interfaceFilter(type)
													 select type)
			{
				AddFieldsForPropertyValues(builder, intf, dirtyFlagsField, ref fieldIndex);
			}
			var equality = ImplementSpecializedDataTypeEquals(builder, dirtyFlagsField);
			ImplementSpecializedDataTypeGetHashCode(builder, chashCodeSeed, dirtyFlagsField);
			ImplementStaticCreate(builder, dirtyFlagsField);
			ImplementCopy(builder, dirtyFlagsField);
			ImplementEqualityOperators(builder, equality);
			ImplementInequalityOperators(builder, equality);

			builder.Compile();
			return builder.Ref.Target;
		}

		private static void ImplementCopy(EmittedClass builder, EmittedField dirtyFlagsField)
		{
			var copy = builder.DefineMethod("Copy");
			copy.ReturnType = builder.Ref;
			copy.ClearAttributes();
			copy.IncludeAttributes(MethodAttributes.HideBySig | MethodAttributes.Assembly);
			copy.ContributeInstructions((m, il) =>
			{
				il.DeclareLocal(builder.Builder);
				il.DeclareLocal(builder.Builder);
				il.Nop();
				il.LoadArg_0();
				il.LoadValueType(builder.Builder);
				il.StoreLocal_0();
				il.LoadLocalAddressShort(0);
				il.LoadArg_0();
				il.LoadFieldAddress(dirtyFlagsField);
				il.Call<BitVector>("Copy");
				il.StoreField(dirtyFlagsField);
				il.LoadLocal_0();
				il.StoreLocal_1();
				il.DefineAndMarkLabel();
				il.LoadLocal_1();
			});
		}

		private static void ImplementStaticCreate(EmittedClass builder, EmittedField dirtyFlagsField)
		{
			var copy = builder.DefineMethod("Create");
			copy.ReturnType = builder.Ref;
			copy.ClearAttributes();
			copy.IncludeAttributes(MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.Assembly);
			copy.ContributeInstructions((m, il) =>
			{
				il.DeclareLocal(builder.Builder);
				il.DeclareLocal(builder.Builder);
				il.Nop();
				il.LoadLocalAddressShort(0);
				il.InitObject(builder.Builder);
				il.LoadLocalAddressShort(0);
				il.LoadValue(builder.Fields.Count() - 1);
				il.New<BitVector>(typeof(int));
				il.StoreField(dirtyFlagsField);
				il.LoadLocal_0();
				il.StoreLocal_1();
				il.DefineAndMarkLabel();
				il.LoadLocal_1();
			});
		}

		static void AddFieldsForPropertyValues(EmittedClass builder, Type intf, EmittedField dirtyFlags, ref int fieldIndex)
		{
			foreach (var p in intf.GetReadableProperties())
			{
				EmittedField field;
				var fieldName = p.FormatBackingFieldName();
				field = builder.DefineField(fieldName, p.PropertyType);
				field.ClearAttributes();
				field.IncludeAttributes(FieldAttributes.Public);

				var indexCapture = fieldIndex;
				var writeField = builder.DefineMethod(String.Concat("Write", fieldName));
				writeField.ClearAttributes();
				writeField.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig);
				writeField.ReturnType = TypeRef.FromType<bool>();
				writeField.DefineParameter("value", p.PropertyType);

				var exitFalse = default(Label);

				writeField.ContributeInstructions((m, il) =>
				{
					il.DeclareLocal(typeof(bool));
					il.DeclareLocal(typeof(bool));
					exitFalse = il.DefineLabel();
					il.Nop();

					var fieldType = field.FieldType.Target;
					if (fieldType.IsArray)
					{
						LoadFieldAndArg1(il, field, fieldType);
						il.Call(typeof(FlitBit.Core.Extensions).GetMethod("EqualsOrItemsEqual", BindingFlags.Public | BindingFlags.Static)
							.MakeGenericMethod(fieldType));
					}
					else if (fieldType.IsClass)
					{
						var op_Equality = fieldType.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static);
						if (op_Equality != null)
						{
							LoadFieldAndArg1(il, field, fieldType);
							il.Call(op_Equality);
						}
						else
						{
							il.Call(typeof(EqualityComparer<>).MakeGenericType(fieldType).GetMethod("get_Default", BindingFlags.Static | BindingFlags.Public));
							LoadFieldAndArg1(il, field, fieldType);
							il.CallVirtual(typeof(IEqualityComparer<>).MakeGenericType(fieldType).GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance,
								null,
								new Type[] { fieldType, fieldType },
								null
								));
						}
					}
					else
					{
						LoadFieldAndArg1(il, field, fieldType);
						il.CompareEquality(fieldType);
					}
					il.StoreLocal_1();
					il.LoadLocal_1();
					il.BranchIfTrue_ShortForm(exitFalse);
					il.Nop();
					il.LoadArg_0();
					il.LoadArg_1();
					il.StoreField(field);
					il.LoadArg_0();
					il.LoadFieldAddress(dirtyFlags);
					il.LoadValue(indexCapture);
					il.LoadValue(true);
					il.Call<BitVector>("set_Item");
					il.Nop();
					il.LoadValue(true);
					il.StoreLocal_0();

					var exit = il.DefineLabel();
					il.Branch_ShortForm(exit);
					il.MarkLabel(exitFalse);
					il.LoadValue(false);
					il.StoreLocal_0();
					il.Branch_ShortForm(exit);
					il.MarkLabel(exit);
					il.LoadLocal_0();
				});
				fieldIndex++;
			}
		}

		static void LoadFieldAndArg1(ILGenerator il, EmittedField field, Type fieldType)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(field != null);
			Contract.Requires<ArgumentNullException>(fieldType != null);
			il.LoadArg_0();
			il.LoadField(field);
			il.LoadArg_1();
		}

		static EmittedMethod ImplementSpecializedDataTypeGetHashCode(EmittedClass builder, EmittedField chashCodeSeed, EmittedField dirtyFlagsField)
		{
			Contract.Requires<ArgumentNullException>(builder != null);

			var method = builder.DefineOverrideMethod(typeof(ValueType).GetMethod("GetHashCode", BindingFlags.Instance | BindingFlags.Public));
			method.ContributeInstructions((m, il) =>
			{
				var prime = il.DeclareLocal(typeof(Int32));
				var result = il.DeclareLocal(typeof(Int32));
				il.DeclareLocal(typeof(Int32));
				il.DeclareLocal(typeof(bool));
				il.Nop();
				il.LoadValue(Constants.NotSoRandomPrime);
				il.StoreLocal(prime);
				il.LoadValue(chashCodeSeed);
				il.LoadLocal(prime);
				il.Multiply();
				il.StoreLocal(result);
				var exit = il.DefineLabel();
				var fields = new List<EmittedField>(builder.Fields.Where(f => f.IsStatic == false && f.Name != dirtyFlagsField.Name));
				for (int i = 0; i < fields.Count; i++)
				{
					var field = fields[i];
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
								il.Call(typeof(FlitBit.Core.Extensions).GetMethod("CalculateCombinedHashcode", BindingFlags.Public | BindingFlags.Static)
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
							else
							{
								il.LoadLocal(result);
								il.LoadLocal(prime);
								il.LoadArg_0();
								if (fieldType.IsValueType)
								{
									il.LoadFieldAddress(field);
									il.Constrained(fieldType);
								}
								else
								{
									il.LoadField(field);
								}
								il.CallVirtual<object>("GetHashCode");
								il.Multiply();
								il.Xor();
								il.StoreLocal(result);
							}
							break;
						case TypeCode.String:
							il.LoadArg_0();
							il.LoadField(field);
							il.LoadNull();
							il.CompareEqual();
							il.StoreLocal_2();
							il.LoadLocal_2();
							var lbl = il.DefineLabel();
							il.BranchIfTrue_ShortForm(lbl);
							il.Nop();
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
							throw new InvalidOperationException(String.Concat("Unable to produce hashcode for type: ", fieldType.GetReadableFullName()));
					}
				}
				il.LoadLocal(result);
				il.StoreLocal_2();
				il.Branch(exit);
				il.MarkLabel(exit);
				il.LoadLocal_2();
			});
			return method;
		}

		static EmittedMethod ImplementSpecializedDataTypeEquals(EmittedClass builder, EmittedField dirtyFlagsField)
		{
			Contract.Requires<ArgumentNullException>(builder != null);

			var equatable = typeof(IEquatable<>).MakeGenericType(builder.Builder);
			builder.AddInterfaceImplementation(equatable);

			var specialized_equals = builder.DefineMethod("Equals");
			specialized_equals.ClearAttributes();
			specialized_equals.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual);
			specialized_equals.ReturnType = TypeRef.FromType<bool>();
			specialized_equals.DefineParameter("other", builder.Ref);

			var exitFalse = default(Label);

			specialized_equals.ContributeInstructions((m, il) =>
			{
				il.DeclareLocal(typeof(bool));
				exitFalse = il.DefineLabel();
				il.Nop();

				var fields = new List<EmittedField>(builder.Fields.Where(f => f.IsStatic == false && f.Name != dirtyFlagsField.Name));
				for (int i = 0; i < fields.Count; i++)
				{
					var field = fields[i];
					var fieldType = field.FieldType.Target;
					if (fieldType.IsArray)
					{
						LoadFieldFromTwoObjects(il, field, fieldType, true);
						il.Call(typeof(FlitBit.Core.Extensions).GetMethod("EqualsOrItemsEqual", BindingFlags.Public | BindingFlags.Static)
							.MakeGenericMethod(fieldType));
					}
					else if (fieldType.IsClass)
					{
						var op_Equality = fieldType.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static);
						if (op_Equality != null)
						{
							LoadFieldFromTwoObjects(il, field, fieldType, true);
							il.Call(op_Equality);
						}
						else
						{
							il.Call(typeof(EqualityComparer<>).MakeGenericType(fieldType).GetMethod("get_Default", BindingFlags.Static | BindingFlags.Public));
							LoadFieldFromTwoObjects(il, field, fieldType, true);
							il.CallVirtual(typeof(IEqualityComparer<>).MakeGenericType(fieldType).GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance,
								null,
								new Type[] { fieldType, fieldType },
								null
								));
						}
					}
					else
					{
						LoadFieldFromTwoObjects(il, field, fieldType, false);
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

			builder.DefineOverrideMethod(typeof(ValueType).GetMethod("Equals", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(object) }, null))
				.ContributeInstructions((m, il) =>
				{
					var exitFalse2 = il.DefineLabel();
					var exit = il.DefineLabel();
					il.DeclareLocal(typeof(bool));
					il.LoadType(builder.Builder);
					il.LoadArg_1();
					il.CallVirtual<Type>("IsInstanceOfType");
					il.BranchIfFalse(exitFalse2);
					il.LoadArg_0();
					il.LoadArg_1();
					il.UnboxAny(builder.Builder);
					il.Call(specialized_equals);
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

			return specialized_equals;
		}

		static void LoadFieldFromTwoObjects(ILGenerator il, EmittedField field, Type fieldType, bool isStatic)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(field != null);
			Contract.Requires<ArgumentNullException>(fieldType != null);
			il.LoadArg_0();
			il.LoadField(field);
			il.LoadArgAddress(1);
			il.LoadField(field);
		}

		static void ImplementInequalityOperators(EmittedClass builder, EmittedMethod equals)
		{
			Contract.Requires<ArgumentNullException>(builder != null);
			Contract.Requires<ArgumentNullException>(equals != null);

			var op_Inequality = builder.DefineMethod("op_Inequality");
			op_Inequality.ClearAttributes();
			op_Inequality.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static);
			op_Inequality.ReturnType = TypeRef.FromType<bool>();
			op_Inequality.DefineParameter("lhs", TypeRef.FromEmittedClass(builder));
			op_Inequality.DefineParameter("rhs", TypeRef.FromEmittedClass(builder));

			op_Inequality.ContributeInstructions((m, il) =>
			{
				il.Nop();
				il.LoadArg_0();
				il.LoadArg_1();
				il.Call(equals);
				il.Load_I4_0(); // load false
				il.CompareEqual();
			});
		}

		static void ImplementEqualityOperators(EmittedClass builder, EmittedMethod equals)
		{
			Contract.Requires<ArgumentNullException>(builder != null);
			Contract.Requires<ArgumentNullException>(equals != null);

			var op_Equality = builder.DefineMethod("op_Equality");
			op_Equality.ClearAttributes();
			op_Equality.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static);
			op_Equality.ReturnType = TypeRef.FromType<bool>();
			op_Equality.DefineParameter("lhs", TypeRef.FromEmittedClass(builder));
			op_Equality.DefineParameter("rhs", TypeRef.FromEmittedClass(builder));

			op_Equality.ContributeInstructions((m, il) =>
			{
				il.Nop();
				il.LoadArg_0();
				il.LoadArg_1();
				il.Call(equals);
			});
		}

		#endregion
	}

}
