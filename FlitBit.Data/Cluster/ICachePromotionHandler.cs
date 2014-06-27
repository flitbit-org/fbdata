using System;

namespace FlitBit.Data.Cluster
{
  public interface ICachePromotionHandler
  {
    void PromoteCacheItem<T>(string key, TimeSpan ttl, T item, bool created);

    void PromoteCacheItem<T>(string key, T item, bool created);

    void PromoteCacheDeletion(string key);
  }
}
