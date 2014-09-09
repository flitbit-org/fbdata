#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.Data.Meta
{
    public enum EnumBehavior
    {
        /// <summary>
        ///     Indicates the value of the enum is mapped to the underlying DB.
        /// </summary>
        ReferenceValue = 0,

        /// <summary>
        ///     Indicates the name of the enum is mapped to the underlying DB.
        /// </summary>
        ReferenceName = 1
    }

    [AttributeUsage(AttributeTargets.Enum)]
    public sealed class MapEnumAttribute : Attribute
    {
        static readonly int CDefaultAnticipatedNameLength = 100;
        int _maxAnticipatedNameLength = CDefaultAnticipatedNameLength;

        public MapEnumAttribute() { }

        public MapEnumAttribute(EnumBehavior behavior) { this.Behavior = behavior; }

        public MapEnumAttribute(string targetSchema)
            : this(null, targetSchema, null, default(EnumBehavior))
        {}

        public MapEnumAttribute(string targetSchema, EnumBehavior behavior)
            : this(null, targetSchema, null, behavior)
        {}

        public MapEnumAttribute(string targetSchema, string targetName)
            : this(targetSchema, targetName, null, default(EnumBehavior))
        {}

        public MapEnumAttribute(string targetSchema, string targetName, EnumBehavior behavior)
            : this(targetSchema, targetName, null, behavior)
        {}

        public MapEnumAttribute(string targetSchema, string targetName,
            string connectionName, EnumBehavior behavior)
        {
            this.TargetName = targetName;
            this.TargetSchema = targetSchema;
            this.ConnectionName = connectionName;
            this.Behavior = behavior;
        }

        public EnumBehavior Behavior { get; private set; }
        public string ConnectionName { get; private set; }

        /// <summary>
        ///     Indicates the maximum anticipated name length for the string representation of the enum.
        ///     Used to define the column length during database mapping.
        /// </summary>
        public int MaxAnticipatedNameLength
        {
            get { return _maxAnticipatedNameLength; }
            set { _maxAnticipatedNameLength = value; }
        }

        public string TargetName { get; set; }
        public string TargetSchema { get; private set; }
    }
}