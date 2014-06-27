
using System;

namespace FlitBit.Data.Cluster
{
  /// <summary>
  /// Interface for clustered memory access (caching).
  /// </summary>
  public interface IClusteredMemory
  {
    /// <summary>
    /// Tries to get and item of type TItem from clustered memory by the specified string key.
    /// </summary>
    /// <param name="key">the item's string key</param>
    /// <param name="value">upon success, holds the resulting data</param>
    /// <returns><em>true</em> if the item is retrieved from clustered memory; otherwise <em>false</em>.</returns>
    bool TryGet(string key, out byte[] value);

    /// <summary>
    /// Puts an item in clustered memory with the specified string key.
    /// </summary>
    /// <param name="key">the item's string key</param>
    /// <param name="value">the item's value</param>
    void Put(string key, byte[] value);

    /// <summary>
    /// Deletes the item in clustered memory with the specified string key.
    /// </summary>
    /// <param name="key">the item's string key</param>
    void Delete(string key);

    /// <summary>
    /// Puts an item into clustered memory with the specified string key and expiration.
    /// </summary>
    /// <param name="key">the item's string key</param>
    /// <param name="maxAge">the item's maximum age - a timespan after which the item is expired</param>
    /// <param name="value">the item's value</param>
    void PutWithExpiration(string key, TimeSpan maxAge, byte[] value);
  }
}
