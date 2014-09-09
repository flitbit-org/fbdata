#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Runtime.Serialization;

namespace FlitBit.Data.Meta
{
    /// <summary>
    ///     Base object mapping exception.
    /// </summary>
    [Serializable]
    public class MappingException : ApplicationException
    {
        /// <summary>
        ///     Default constructor; creates a new instance.
        /// </summary>
        public MappingException() { }

        /// <summary>
        ///     Creates a new instance using the error message given.
        /// </summary>
        /// <param name="errorMessage">An error message describing the exception.</param>
        public MappingException(string errorMessage)
            : base(errorMessage) { }

        /// <summary>
        ///     Creates a new instance using the error message and cuase given.
        /// </summary>
        /// <param name="errorMessage">An error message describing the exception.</param>
        /// <param name="cause">An inner exception that caused this exception</param>
        public MappingException(string errorMessage, Exception cause)
            : base(errorMessage, cause) { }

        /// <summary>
        ///     Used during serialization.
        /// </summary>
        /// <param name="si">SerializationInfo</param>
        /// <param name="sc">StreamingContext</param>
        protected MappingException(SerializationInfo si, StreamingContext sc)
            : base(si, sc) { }
    }

    /// <summary>
    ///     Indicates that an identity type is not supported for an object.
    /// </summary>
    [Serializable]
    public class IdentityTypeNotSupportedException : MappingException
    {
        /// <summary>
        ///     Default constructor; creates a new instance.
        /// </summary>
        public IdentityTypeNotSupportedException() { }

        /// <summary>
        ///     Creates a new instance using the error message given.
        /// </summary>
        /// <param name="errorMessage">An error message describing the exception.</param>
        public IdentityTypeNotSupportedException(string errorMessage)
            : base(errorMessage) { }

        /// <summary>
        ///     Creates a new instance using the error message and cuase given.
        /// </summary>
        /// <param name="errorMessage">An error message describing the exception.</param>
        /// <param name="cause">An inner exception that caused this exception</param>
        public IdentityTypeNotSupportedException(string errorMessage, Exception cause)
            : base(errorMessage, cause) { }

        /// <summary>
        ///     Used during serialization.
        /// </summary>
        /// <param name="si">SerializationInfo</param>
        /// <param name="sc">StreamingContext</param>
        protected IdentityTypeNotSupportedException(SerializationInfo si, StreamingContext sc)
            : base(si, sc) { }
    }
}