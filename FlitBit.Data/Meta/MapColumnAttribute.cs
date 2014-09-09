#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using FlitBit.Data.DataModel;

namespace FlitBit.Data.Meta
{
    /// <summary>
    ///     Maps a field or property to a database column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class MapColumnAttribute : Attribute
    {
        int? _size;
        short? _precision;
        byte? _scale;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public MapColumnAttribute()
        {}

        /// <summary>
        ///     Creates a new instance that references a specified target.
        /// </summary>
        /// <param name="targetName">the column's name</param>
        /// <param name="referenceTarget">the referenced type</param>
        /// <param name="behaviors">reference behaviors</param>
        /// <param name="referencePropertyNames">the reference target's referenced properties</param>
        public MapColumnAttribute(string targetName, Type referenceTarget, ReferenceBehaviors behaviors,
            params string[] referencePropertyNames)
        {
            TargetName = targetName;
            ReferenceTarget = referenceTarget;
            ReferenceBehaviors = behaviors;
            References = referencePropertyNames.ToArray();
        }

        /// <summary>
        ///     Creates a new instance that references a specified target.
        /// </summary>
        /// <param name="targetName">the column's name</param>
        /// <param name="behaviors">reference behaviors</param>
        /// <param name="referencePropertyNames">the reference target's referenced properties</param>
        public MapColumnAttribute(string targetName, ReferenceBehaviors behaviors,
            params string[] referencePropertyNames)
        {
            TargetName = targetName;
            ReferenceBehaviors = behaviors;
            References = referencePropertyNames.ToArray();
        }

        /// <summary>
        ///     Creates a new instance that references a specified target.
        /// </summary>
        /// <param name="behaviors">reference behaviors</param>
        /// <param name="referencePropertyNames">the reference target's referenced properties</param>
        public MapColumnAttribute(ReferenceBehaviors behaviors, params string[] referencePropertyNames)
        {
            ReferenceBehaviors = behaviors;
            References = referencePropertyNames.ToArray();
        }

        /// <summary>
        ///     Creates a new instance with the specified target name.
        /// </summary>
        /// <param name="targetName">the target column's name</param>
        public MapColumnAttribute(string targetName)
            : this(targetName, default(ColumnBehaviors), default(int), default(byte))
        {}

        /// <summary>
        ///     Creates a new instance with the specified target name and behavior.
        /// </summary>
        /// <param name="targetName">the target column's name</param>
        /// <param name="behaviors"></param>
        public MapColumnAttribute(string targetName, ColumnBehaviors behaviors)
            : this(targetName, behaviors, default(int), default(byte))
        {}

        /// <summary>
        ///     Creates a new instance with the specified length.
        /// </summary>
        /// <param name="length"></param>
        public MapColumnAttribute(int length)
            : this(default(String), default(ColumnBehaviors), length, default(byte))
        {}

        /// <summary>
        ///     Creates a new instance with the specified target name and length.
        /// </summary>
        /// <param name="targetName">the target column's name</param>
        /// <param name="length"></param>
        public MapColumnAttribute(string targetName, int length)
            : this(targetName, default(ColumnBehaviors), length, default(byte))
        {}

        /// <summary>
        ///     Creates a new instance with the specified target name, behavior, and length.
        /// </summary>
        /// <param name="targetName">the target column's name</param>
        /// <param name="behaviors"></param>
        /// <param name="length"></param>
        public MapColumnAttribute(string targetName, ColumnBehaviors behaviors, int length)
            : this(targetName, behaviors, length, default(byte))
        {}

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="targetName"></param>
        /// <param name="behaviors"></param>
        /// <param name="size"></param>
        /// <param name="scale"></param>
        public MapColumnAttribute(string targetName, ColumnBehaviors behaviors, int size, byte scale)
        {
            TargetName = targetName;
            Behaviors = behaviors;
            Length = size;
            Scale = scale;
        }

        /// <summary>
        ///     Creates a new instance with the specified behavior.
        /// </summary>
        /// <param name="behaviors"></param>
        public MapColumnAttribute(ColumnBehaviors behaviors)
            : this(behaviors, default(int), default(byte))
        {}

        /// <summary>
        ///     Creates a new instance with the specified behavior and length.
        /// </summary>
        /// <param name="behaviors"></param>
        /// <param name="length"></param>
        public MapColumnAttribute(ColumnBehaviors behaviors, int length)
            : this(behaviors, length, default(byte))
        {}

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="behaviors"></param>
        /// <param name="size"></param>
        /// <param name="scale"></param>
        public MapColumnAttribute(ColumnBehaviors behaviors, int size, byte scale)
        {
            Behaviors = behaviors;
            Length = size;
            Scale = scale;
        }

        /// <summary>
        ///     Gets and sets a column's behaviors.
        /// </summary>
        public ColumnBehaviors Behaviors { get; set; }

        /// <summary>
        ///     Gets and sets a column's database translator type (reserved).
        /// </summary>
        public Type DbTypeTranslator { get; set; }

        /// <summary>
        ///     Declared DBType; providers use this as a suggestion when mapping the type.
        /// </summary>
        public DbType SuggestedDbType { get; set; }

        /// <summary>
        ///     Gets and sets the mapped column's length/size. When length is set scale is ignored.
        /// </summary>
        public int Length { get { return _size.HasValue ? _size.Value : 0; } set { _size = value; } }

        /// <summary>
        ///     Gets and sets the column's reference behaviors.
        /// </summary>
        public ReferenceBehaviors ReferenceBehaviors { get; set; }

        /// <summary>
        ///     Name of property or properties on the target object
        ///     that are used for reference.
        /// </summary>
        public IEnumerable<string> References { get; set; }

        /// <summary>
        ///     Gets and sets the column's precision.
        /// </summary>
        public short Precision
        {
            get { return _precision.HasValue ? _precision.Value : (short)0; }
            set { _precision = value; }
        }

        /// <summary>
        ///     Gets and sets the column's scale.
        /// </summary>
        public byte Scale { get { return _scale.HasValue ? _scale.Value : (byte)0; } set { _scale = value; } }

        /// <summary>
        ///     Gets and sets the column's size/length.
        /// </summary>
        public int Size { get { return _size.HasValue ? _size.Value : 0; } set { _size = value; } }

        /// <summary>
        ///     Gets and sets the column's target name.
        /// </summary>
        public string TargetName { get; set; }

        /// <summary>
        ///     Gets and sets the column's reference target.
        /// </summary>
        public Type ReferenceTarget { get; set; }

        internal void PrepareMapping<T>(Mapping<T> mapping, PropertyInfo p)
        {
            var column = mapping.DefineColumn(p);
            if (!String.IsNullOrEmpty(TargetName))
            {
                column.WithTargetName(TargetName);
            }
            var behaviors = Behaviors;
            if (p.PropertyType.IsGenericType
                && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                behaviors |= ColumnBehaviors.Nullable;
            }
            column.WithBehaviors(behaviors);
            if (_size.HasValue)
            {
                column.WithVariableLength(_size.Value);
            }
            else if (p.PropertyType.IsEnum)
            {
                var mapEnum =
                    (MapEnumAttribute)p.PropertyType.GetCustomAttributes(typeof(MapEnumAttribute), false)
                                       .FirstOrDefault();
                if (mapEnum != null)
                {
                    column.WithVariableLength(mapEnum.MaxAnticipatedNameLength);
                }
            }
            if (_precision.HasValue)
            {
                column.WithPrecision(_precision.Value);
            }
            if (_scale.HasValue)
            {
                column.WithScale(_scale.Value);
            }
            if (column.Behaviors.HasFlag(ColumnBehaviors.Identity))
            {
                mapping.Identity.AddColumn(column, SortOrderKind.Asc);
            }
            if (p.PropertyType == mapping.RuntimeType)
            {
                MakeReferenceTo(mapping, mapping, p, column);
            }
            else if (DbTypeTranslator == null
                     && Mappings.ExistsFor(p.PropertyType))
            {
                MakeReferenceTo(mapping, Mappings.AccessMappingFor(p.PropertyType), p, column);
            }
        }

        void MakeReferenceTo<T>(Mapping<T> mapping, IMapping foreign, PropertyInfo p, ColumnMapping<T> column)
        {
            ColumnMapping foreignColumn;

            if (References == null
                || !References.Any())
            {
                foreignColumn = foreign.GetPreferredReferenceColumn();
                if (foreignColumn == null)
                {
                    throw new MappingException(String.Concat("Relationship not defined between ", typeof(T).Name, ".",
                        p.Name,
                        " and the referenced type: ", p.PropertyType.Name));
                }

                References = new[]
                {
                    foreignColumn.Member.Name
                };
            }
            else
            {
                // Only 1 reference column for now.
                foreignColumn = foreign.Columns.FirstOrDefault(c => c.Member.Name == References.First());
            }

            if (foreignColumn == null)
            {
                throw new InvalidOperationException(String.Concat("Property '", p.Name,
                    "' references an entity but a relationship cannot be determined."));
            }
            column.DefineReference(foreignColumn, ReferenceBehaviors);
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