#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.Data
{
    /// <summary>
    ///     Length requirements for a DbType
    /// </summary>
    [Flags]
    public enum DbTypeLengthRequirements
    {
        /// <summary>
        ///     Indicates there are no requirements.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Indicates that a length is required.
        /// </summary>
        Length = 1,

        /// <summary>
        ///     Indicates that a precision is required.
        /// </summary>
        Precision = 1 << 1,

        /// <summary>
        ///     Indicates that a scale is required.
        /// </summary>
        Scale = 1 << 2,

        /// <summary>
        ///     Indicates that a scale is optional.
        /// </summary>
        OptionalScale = Scale | 1 << 3,

        /// <summary>
        ///     Mask of all length related flags; used by the framework to prune
        ///     extended (smuggled) values that may be contributed by overrides.
        /// </summary>
        LengthSpecifierMask = Length | Precision | OptionalScale,
    }
}