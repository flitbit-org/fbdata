using System;
using System.Collections.Generic;

namespace FlitBit.Data.Cluster
{
    /// <summary>
    ///     Interface for classes that subscribe to clustered activity notifications.
    /// </summary>
    public interface IClusterObserver
    {
        /// <summary>
        ///     Gets the observer's key.
        /// </summary>
        Guid Key { get; }

        /// <summary>
        ///     The observed channel.
        /// </summary>
        string Channel { get; }

        /// <summary>
        ///     Enumeration of observations for which the subscriber should be notified.
        /// </summary>
        IEnumerable<string> Observations { get; }

        /// <summary>
        ///     Callback invoked to notify the subscriber when an observation has occurrred
        ///     on the subscribed channel.
        /// </summary>
        /// <param name="channel">name of the channel</param>
        /// <param name="identity">object identifier</param>
        /// <param name="observation">the observation</param>
        /// <param name="mem">clustered memory where the objects reside</param>
        void Notify(string channel, string identity, string observation, IClusteredMemory mem);
    }
}