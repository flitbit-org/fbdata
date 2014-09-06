using System;
using System.Data.Common;
using System.Diagnostics.Contracts;
using FlitBit.Core;
using FlitBit.Emit;

namespace FlitBit.Data
{
    /// <summary>
    /// Default implementation of IConnection.
    /// </summary>
    public class DefaultConnection : IConnection, IEquatable<DefaultConnection>, IEquatable<IConnection>
    {
        static readonly int __HashCodeSeed = typeof(DefaultConnection).AssemblyQualifiedName.GetHashCode();

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="name">The connection's name</param>
        /// <param name="cn">The connection</param>
        public DefaultConnection(string name, DbConnection cn)
            : this(name, cn, false)
        {
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Requires<ArgumentNullException>(cn != null);
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="name">The connection's name</param>
        /// <param name="cn">The connection</param>
        /// <param name="canShare">Indicates whether the connection can be shared.</param>
        public DefaultConnection(string name, DbConnection cn, bool canShare)
        {
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Requires<ArgumentNullException>(cn != null);

            this.Name = name;
            this.UntypedDbConnection = cn;
            this.CanShareConnection = canShare;
        }

        /// <summary>
        /// The connection's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The db connection.
        /// </summary>
        public DbConnection UntypedDbConnection { get; private set; }

        /// <summary>
        /// Determins if this connection is equal to another.
        /// </summary>
        /// <param name="other">the other connection</param>
        /// <returns></returns>
        public bool Equals(DefaultConnection other)
        {
            return other != null
                   && String.Equals(this.Name, other.Name, StringComparison.InvariantCulture)
                   && ReferenceEquals(this.UntypedDbConnection, other.UntypedDbConnection);
        }

        /// <summary>
        /// Determins if this connection is equal to another.
        /// </summary>
        /// <param name="other">the other connection</param>
        /// <returns></returns>
        public bool Equals(IConnection other)
        {
            return other is DefaultConnection
                   && Equals((DefaultConnection)other);
        }

        /// <summary>
        /// Determins if this connection is equal to another object.
        /// </summary>
        /// <param name="obj">the other object</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is DefaultConnection
                   && Equals((DefaultConnection)obj);
        }

        /// <summary>
        /// Calculates the connection's hash code. 
        /// </summary>
        /// <returns>
        /// the connection's hash code
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            const int prime = 999067; // a random prime

            var res = __HashCodeSeed * prime;
            res ^= this.Name.GetHashCode() * prime;
            res ^= this.UntypedDbConnection.GetHashCode() * prime;
            res ^= Convert.ToInt32(CanShareConnection) * prime;
            return res;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return String.Concat("{ Name: ", this.Name, 
                ", Type: ", this.UntypedDbConnection.GetType().GetReadableSimpleName(), " }");
        }


        public bool CanShareConnection { get; private set; }
    }

    /// <summary>
    /// Default implementation of strongly typed connection.
    /// </summary>
    /// <typeparam name="TDbConnection"></typeparam>
    public class DefaultConnection<TDbConnection> : IConnection<TDbConnection>, IEquatable<DefaultConnection<TDbConnection>>, IEquatable<IConnection>
        where TDbConnection: DbConnection
    {
        static readonly int __HashCodeSeed = typeof(DefaultConnection<TDbConnection>).AssemblyQualifiedName.GetHashCode();

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="name">The connection's name</param>
        /// <param name="cn">The connection</param>
        public DefaultConnection(string name, TDbConnection cn)
            : this(name, cn, false)
        {
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Requires<ArgumentNullException>(cn != null);
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="name">The connection's name</param>
        /// <param name="cn">The connection</param>
        /// <param name="canShare">Indicates whether the connection can be shared.</param>
        public DefaultConnection(string name, TDbConnection cn, bool canShare)
        {
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Requires<ArgumentNullException>(cn != null);

            this.Name = name;
            this.DbConnection = cn;
            this.CanShareConnection = canShare;
        }

        /// <summary>
        /// The connection's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Indicates whether the connection can be shared.
        /// </summary>
        public bool CanShareConnection { get; private set; }

        /// <summary>
        /// The db connection.
        /// </summary>
        public DbConnection UntypedDbConnection { get { return DbConnection; } }

        public TDbConnection DbConnection { get; private set; }

        /// <summary>
        /// Determins if this connection is equal to another.
        /// </summary>
        /// <param name="other">the other connection</param>
        /// <returns></returns>
        public bool Equals(DefaultConnection<TDbConnection> other)
        {
            return other != null
                   && String.Equals(this.Name, other.Name, StringComparison.InvariantCulture)
                   && ReferenceEquals(this.UntypedDbConnection, other.UntypedDbConnection);
        }

        /// <summary>
        /// Determins if this connection is equal to another.
        /// </summary>
        /// <param name="other">the other connection</param>
        /// <returns></returns>
        public bool Equals(IConnection other)
        {
            return other is DefaultConnection<TDbConnection>
                   && Equals((DefaultConnection<TDbConnection>)other);
        }

        /// <summary>
        /// Determins if this connection is equal to another object.
        /// </summary>
        /// <param name="obj">the other object</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is DefaultConnection<TDbConnection>
                   && Equals((DefaultConnection<TDbConnection>)obj);
        }

        /// <summary>
        /// Calculates the connection's hash code. 
        /// </summary>
        /// <returns>
        /// the connection's hash code
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            const int prime = 999067; // a random prime

            var res = __HashCodeSeed * prime;
            res ^= this.Name.GetHashCode() * prime;
            res ^= this.DbConnection.GetHashCode() * prime;
            res ^= Convert.ToInt32(CanShareConnection) * prime;
            return res;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return String.Concat("{ Name: ", this.Name,
                ", Type: ", typeof(TDbConnection).GetReadableSimpleName(), " }");
        }
    }

}