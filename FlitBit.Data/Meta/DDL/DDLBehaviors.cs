#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.Data.Meta.DDL
{
    public enum DDLBehaviors
    {
        /// <summary>
        ///     Default behavior; inherits from parent.
        /// </summary>
        Inherit = 0,

        /// <summary>
        ///     Create all database objects.
        /// </summary>
        Create = 1,

        /// <summary>
        ///     Validates database objects.
        /// </summary>
        Validate = 2,

        /// <summary>
        ///     Cascades changes in object structure to the database objects.
        /// </summary>
        Cascade = 3,
    }
}