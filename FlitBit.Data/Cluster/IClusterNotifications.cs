using System;

namespace FlitBit.Data.Cluster
{
  /// <summary>
  /// Coordinates subscriptions to cluster notifications.
  /// </summary>
  public interface IClusterNotifications
  {
    /// <summary>
    /// Registers the specified observer for notifications.
    /// </summary>
    /// <param name="observer"></param>
    void Subscribe(IClusterObserver observer);
    /// <summary>
    /// Cancels a previously registered subscription.
    /// </summary>
    /// <param name="key"></param>
    void Cancel(Guid key);
  }
}