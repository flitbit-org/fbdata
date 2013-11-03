#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using FlitBit.Data.DataModel;

namespace FlitBit.Data.Meta
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class MapColumnAttribute : Attribute
	{
		int _size;

		public MapColumnAttribute()
		{}

		public MapColumnAttribute(string name, ReferenceBehaviors behaviors, params string[] referencePropertyNames)
		{
			this.TargetName = name;
			this.ReferenceBehaviors = behaviors;
			this.References = referencePropertyNames;
		}

		public MapColumnAttribute(ReferenceBehaviors behaviors, params string[] referencePropertyNames)
		{
			this.ReferenceBehaviors = behaviors;
			this.References = referencePropertyNames;
		}

		public MapColumnAttribute(string name)
			: this(name, default(ColumnBehaviors), default(int), default(byte))
		{}

		public MapColumnAttribute(string name, ColumnBehaviors behaviors)
			: this(name, behaviors, default(int), default(byte))
		{}

		public MapColumnAttribute(int length)
			: this(default(String), default(ColumnBehaviors), length, default(byte))
		{}

		public MapColumnAttribute(string name, int length)
			: this(name, default(ColumnBehaviors), length, default(byte))
		{}

		public MapColumnAttribute(string name, ColumnBehaviors behaviors, int length)
			: this(name, behaviors, length, default(byte))
		{}

		public MapColumnAttribute(string name, ColumnBehaviors behaviors, int size, byte scale)
		{
			this.TargetName = name;
			this.Behaviors = behaviors;
			this.Length = size;
			this.Scale = scale;
		}

		public MapColumnAttribute(ColumnBehaviors behaviors)
			: this(behaviors, default(int), default(byte))
		{}

		public MapColumnAttribute(ColumnBehaviors behaviors, int length)
			: this(behaviors, length, default(byte))
		{}

		public MapColumnAttribute(ColumnBehaviors behaviors, int size, byte scale)
		{
			this.Behaviors = behaviors;
			this.Length = size;
			this.Scale = scale;
		}

		public ColumnBehaviors Behaviors { get; set; }
		public Type DbTypeTranslator { get; set; }

		public int Length { get { return _size; } set { _size = value; } }

		public ReferenceBehaviors ReferenceBehaviors { get; set; }

		/// <summary>
		///   Name of property or properties on the target object
		///   that are used for reference.
		/// </summary>
		public IEnumerable<string> References { get; set; }

		public byte Scale { get; set; }
		public int Size { get { return _size; } set { _size = value; } }
		public string TargetName { get; set; }

		internal void PrepareMapping<T>(Mapping<T> mapping, PropertyInfo p)
		{
			var column = mapping.DefineColumn(p);
			if (!String.IsNullOrEmpty(TargetName))
			{
				column.WithTargetName(TargetName);
			}
			var behaviors = this.Behaviors;
			if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				behaviors |= ColumnBehaviors.Nullable;
			}
			column.WithBehaviors(behaviors);
			if (this.Length > 0)
			{
				column.WithVariableLength(this.Length);
			}
			else if (p.PropertyType.IsEnum)
			{
				var mapEnum =
					(MapEnumAttribute) p.PropertyType.GetCustomAttributes(typeof(MapEnumAttribute), false)
															.FirstOrDefault();
				if (mapEnum != null)
				{
					column.WithVariableLength(mapEnum.MaxAnticipatedNameLength);
				}
			}
			if (column.Behaviors.HasFlag(ColumnBehaviors.Identity))
			{
				mapping.Identity.AddColumn(column, SortOrderKind.Asc);
			}
			if (p.PropertyType == mapping.RuntimeType)
			{
				MakeReferenceTo(mapping, mapping, p, column);
			}
			else if (this.DbTypeTranslator == null && Mappings.ExistsFor(p.PropertyType))
			{
				MakeReferenceTo(mapping, Mappings.AccessMappingFor(p.PropertyType), p, column);
			}
		}

		private void MakeReferenceTo<T>(Mapping<T> mapping, IMapping foreign, PropertyInfo p, ColumnMapping<T> column)
		{
			var foreignColumn = default(ColumnMapping);

			if (References == null || References.Count() == 0)
			{
				foreignColumn = foreign.GetPreferredReferenceColumn();
				if (foreignColumn == null)
				{
					throw new MappingException(String.Concat("Relationship not defined between ", typeof(T).Name, ".", p.Name,
																									" and the referenced type: ", p.PropertyType.Name));
				}

				References = new string[] { foreignColumn.Member.Name };
			}
			else
			{
				// Only 1 reference column for now.
				foreignColumn = foreign.Columns.Where(c => c.Member.Name == this.References.First())
																			.FirstOrDefault();
			}

			if (foreignColumn == null)
			{
				throw new InvalidOperationException(String.Concat("Property '", p.Name,
																													"' references an entity but a relationship cannot be determined."));
			}
			column.DefineReference(foreignColumn, this.ReferenceBehaviors);
			mapping.AddDependency(foreign, DependencyKind.Direct, p);
		}

		internal static MapColumnAttribute DefineOnProperty<T>(PropertyInfo property)
		{
			Contract.Requires(property != null);

			var attr = new MapColumnAttribute(property.Name);
			return attr;
		}
	}
}