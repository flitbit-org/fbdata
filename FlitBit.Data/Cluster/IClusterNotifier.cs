using System;
using System.Threading;
using FlitBit.Core;

namespace FlitBit.Data.Cluster
{
  /// <summary>
  /// Interface for classes that publish clustered activity notifications.
  /// </summary>
  public interface IClusterNotifier
  {
    /// <summary>
    /// Publishes an activity notification.
    /// </summary>
    /// <param name="channel">name of the channel</param>
    /// <param name="identity">object identifier</param>
    /// <param name="observation">the observation</param>
    void PublishNotification(string channel, string identity, string observation);
  }

  public static class ClusterNotifier
  {
    readonly static Lazy<IClusterNotifier> Notifications = new Lazy<IClusterNotifier>(() => (FactoryProvider.Factory.CanConstruct<IClusterNotifier>())
                                                                                                        ? FactoryProvider.Factory.CreateInstance<IClusterNotifier>()
                                                                                                        : null, LazyThreadSafetyMode.ExecutionAndPublication);
    public static IClusterNotifier Instance { get { return Notifications.Value; } }

   
    public static void PublishNotification(string channel, string identity, string observation)
    {
      var notifier = Instance;
      if (notifier != null)
      {
        notifier.PublishNotification(channel, identity, observation);
      }
    }
  }
}
