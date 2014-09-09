#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.Data.DataModel
{
    /// <summary>
    ///     Interface for classes that can resolve references to models of type M
    /// </summary>
    /// <typeparam name="M">model type M</typeparam>
    public interface IDataModelResolver<out M, IK>
    {
        /// <summary>
        ///     Gets the identity key this resolver can resolve.
        /// </summary>
        IK IdentityKey { get; }

        /// <summary>
        ///     Resolves a referenced model instance.
        /// </summary>
        /// <returns>the referenced model</returns>
        M Resolve();
    }
}