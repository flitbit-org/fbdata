using System;
using System.Configuration;
using System.Data.Common;

namespace FlitBit.Data
{
    /// <summary>
    ///     Connection provider implementation that creates connections from the configuration file.
    /// </summary>
    public class ConfigurationFileConnectionProvider : IConnectionProvider
    {
        /// <summary>
        ///     Indicates whether the provider can create a connection for the specified name.
        /// </summary>
        /// <param name="name">The connection's name.</param>
        /// <returns><em>true</em> if the provider can create the specified connection; otherwise <em>false</em></returns>
        public bool CanCreate(string name)
        {
            var css = ConfigurationManager.ConnectionStrings[name];
            return (css != null);
        }

        /// <summary>
        ///     Gets a connection for the specified name.
        /// </summary>
        /// <param name="name">The connection's name.</param>
        /// <returns>A connection for the specified name.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     thrown if the provider cannot provide a connection for the specified
        ///     name.
        /// </exception>
        public IConnection GetConnection(string name)
        {
            try
            {
                return new DefaultConnection(name, DbExtensions.CreateConnection(name), false);
            }
            catch (ConfigurationErrorsException)
            {
                throw new ArgumentOutOfRangeException("name");
            }
        }

        /// <summary>
        ///     Gets a connection for the specified name, of type TDbConnection.
        /// </summary>
        /// <param name="name">the connection's name</param>
        /// <typeparam name="TDbConnection">the connection's type</typeparam>
        /// <returns>A connection for the specified name.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     thrown if the provider cannot provide a connection for the specified
        ///     name.
        /// </exception>
        /// <exception cref="InvalidCastException">thrown if the connection's type is not a TDbConnection.</exception>
        public IConnection<TDbConnection> GetConnection<TDbConnection>(string name) where TDbConnection : DbConnection
        {
            try
            {
                return new DefaultConnection<TDbConnection>(name, DbExtensions.CreateConnection<TDbConnection>(name),
                    false);
            }
            catch (ConfigurationErrorsException)
            {
                throw new ArgumentOutOfRangeException("name");
            }
        }
    }
}