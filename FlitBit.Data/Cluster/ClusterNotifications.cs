using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlitBit.Core;

namespace FlitBit.Data.Cluster
{
  public abstract class ClusterNotifications : Disposable, IClusterNotifications
  {
    readonly static Lazy<IClusterNotifications> Notifications = new Lazy<IClusterNotifications>(() => (FactoryProvider.Factory.CanConstruct<IClusterNotifications>())
                                                                                                        ? FactoryProvider.Factory.CreateInstance<IClusterNotifications>()
                                                                                                        : null, LazyThreadSafetyMode.ExecutionAndPublication);
    public static IClusterNotifications Instance { get { return Notifications.Value; } }

    readonly ConcurrentDictionary<Guid, IClusterObserver> _observers = new ConcurrentDictionary<Guid, IClusterObserver>();
    readonly ConcurrentDictionary<string, Channel> _channels = new ConcurrentDictionary<string, Channel>();

    class Channel
    {
      readonly ConcurrentDictionary<string, Observation> _observations = new ConcurrentDictionary<string, Observation>();

      public Channel(IClusterObserver observer)
      {
        Add(observer);
      }

      public Channel Add(IClusterObserver observer)
      {
        Guid key = observer.Key;
        foreach (var obs in observer.Observations)
        {
          this._observations.AddOrUpdate(obs, o => new Observation(key), (ch, existing) => existing.Add(key));
        }
        return this;
      }

      public void Remove(IClusterObserver observer)
      {
        Guid key = observer.Key;
        foreach (var obs in observer.Observations)
        {
          Observation observation;
          if (this._observations.TryGetValue(obs, out observation))
          {
            observation.Remove(key);
          }
        }
      }

      internal void Notify(ConcurrentDictionary<Guid, IClusterObserver> observers, IClusteredMemory mem, string ch, string id, string observation)
      {
        Observation obs;
        if (this._observations.TryGetValue(observation, out obs))
        {
          obs.Notify(observers, mem, ch, id, observation);
        }
      }
    }

    class Observation
    {
      object _subscribers;

      public Observation(Guid key)
      {
        Add(key);
      }

      public Observation Add(Guid guid)
      {
        while (true)
        {
          var existing = (Guid[])Volatile.Read(ref this._subscribers);
          Guid[] updated;
          if (existing != null)
          {
            updated = existing.Union(new[]
            {
              guid
            }).ToArray();
          }
          else
          {
            updated = new[]
            {
              guid
            };
          }
          if (Interlocked.CompareExchange(ref this._subscribers, updated, existing) == existing)
          {
            return this;
          }
        }
      }

      internal void Remove(Guid key)
      {
        while (true)
        {
          var existing = (Guid[])Volatile.Read(ref this._subscribers);
          if (existing == null) return;

          Guid[] updated = existing.Except(new[] { key }).ToArray();
          if (updated.Length == 0)
          {
            updated = null;
          }

          if (Interlocked.CompareExchange(ref this._subscribers, updated, existing) == existing)
          {
            break;
          }
        }
      }

      public void Notify(ConcurrentDictionary<Guid, IClusterObserver> observers, IClusteredMemory mem, string ch, string id, string observation)
      {
        var existing = (Guid[])Volatile.Read(ref this._subscribers);
        if (existing != null)
        {
          Parallel.ForEach(existing, guid =>
          {
            IClusterObserver observer;
            if (observers.TryGetValue(guid, out observer))
            {
              observer.Notify(ch, id, observation, mem);
            }
          });
        }
      }
    }

    public static string FormatNotification(string channel, string identity, string observation)
    {
      Contract.Requires<ArgumentNullException>(channel != null);
      Contract.Requires<ArgumentNullException>(identity != null);
      Contract.Requires<ArgumentNullException>(observation != null);
      return String.Concat(channel, ':', identity, '|', observation);
    }

    protected void PerformNotification(IClusteredMemory mem, string notification)
    {
      // topic := channel ':' identity '|' observation
      var idofs = notification.IndexOf(':');
      var obofs = notification.IndexOf('|');
      if (idofs >= 0
          && obofs >= idofs)
      {
        var channel = notification.Substring(0, idofs);
        var id = notification.Substring(idofs + 1, (obofs - idofs) - 1);
        var observation = notification.Substring(obofs + 1);
        PerformNotification(mem, channel, id, observation);
      }
    }

    protected void PerformNotification(IClusteredMemory mem, string channel, string identity, string observation)
    {
      Channel ch;
      if (this._channels.TryGetValue(channel, out ch))
      {
        ch.Notify(this._observers, mem, channel, identity, observation);
      }
    }

    public void Subscribe(IClusterObserver observer)
    {
      Contract.Requires<ArgumentNullException>(observer != null);

      var key = observer.Key;
      if (this._observers.TryAdd(key, observer))
      {
        this._channels.AddOrUpdate(observer.Channel, ch => new Channel(observer), (ch, existing) => existing.Add(observer));
      }
    }

    public void Cancel(Guid key)
    {
      IClusterObserver removed;
      if (this._observers.TryRemove(key, out removed))
      {
        Channel channel;
        if (this._channels.TryGetValue(removed.Channel, out channel))
        {
          channel.Remove(removed);
        }
      }
    }
  }
}