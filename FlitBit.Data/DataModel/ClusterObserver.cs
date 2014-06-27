using System;
using System.Collections.Generic;
using FlitBit.Core;
using FlitBit.Data.Cluster;

namespace FlitBit.Data.DataModel
{
  public interface IClusterNotificationHandler
  {
    void ClusterNotify(string observation, string identity, IClusteredMemory mem);
  }

  public class ClusterObserver : IClusterObserver
  {
    readonly IClusterNotificationHandler _parent;

    internal ClusterObserver(IClusterNotificationHandler parent, string channel, IEnumerable<string> observations)
    {
      this.Key = Guid.NewGuid();
      this._parent = parent;
      this.Channel = channel;
      this.Observations = observations.ToReadOnly();
    }

    public Guid Key { get; private set; }
    /// <summary>
    /// The observed channel.
    /// </summary>
    public string Channel { get; private set; }

    /// <summary>
    /// Enumeration of observations for which the subscriber should be notified.
    /// </summary>
    public IEnumerable<string> Observations { get; private set; }

    /// <summary>
    /// Callback invoked to notify the subscriber when an observation has occurrred
    /// on the subscribed channel.
    /// </summary>
    /// <param name="channel">name of the channel</param>
    /// <param name="identity">object identifier</param>
    /// <param name="observation">the observation</param>
    /// <param name="mem">clustered memory where the objects reside</param>
    public void Notify(string channel, string identity, string observation, IClusteredMemory mem)
    {
      _parent.ClusterNotify(observation, identity, mem);
    }
  }
}