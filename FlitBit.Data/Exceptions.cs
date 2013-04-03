using System;
using System.Data;
using System.Runtime.Serialization;

namespace FlitBit.Data
{
	/// <summary>
	///   Base object data exceptions.
	/// </summary>
	[Serializable]
	public class DataModelException : ApplicationException
	{
		/// <summary>
		///   Default constructor; creates a new instance.
		/// </summary>
		public DataModelException()
		{}

		/// <summary>
		///   Creates a new instance using the error message given.
		/// </summary>
		/// <param name="errorMessage">An error message describing the exception.</param>
		public DataModelException(string errorMessage)
			: base(errorMessage)
		{}

		/// <summary>
		///   Creates a new instance using the error message and cuase given.
		/// </summary>
		/// <param name="errorMessage">An error message describing the exception.</param>
		/// <param name="cause">An inner exception that caused this exception</param>
		public DataModelException(string errorMessage, Exception cause)
			: base(errorMessage, cause)
		{}

		/// <summary>
		///   Used during serialization.
		/// </summary>
		/// <param name="si">SerializationInfo</param>
		/// <param name="sc">StreamingContext</param>
		protected DataModelException(SerializationInfo si, StreamingContext sc)
			: base(si, sc)
		{}
	}

	/// <summary>
	///   Exception raised by repositories when one instance is expected
	///   but none were found.
	/// </summary>
	[Serializable]
	public class ObjectNotFoundException : DataException
	{
		/// <summary>
		///   Default constructor; creates a new instance.
		/// </summary>
		public ObjectNotFoundException()
		{}

		/// <summary>
		///   Creates a new instance using the error message given.
		/// </summary>
		/// <param name="errorMessage">An error message describing the exception.</param>
		public ObjectNotFoundException(string errorMessage)
			: base(errorMessage)
		{}

		/// <summary>
		///   Creates a new instance using the error message and cuase given.
		/// </summary>
		/// <param name="errorMessage">An error message describing the exception.</param>
		/// <param name="cause">An inner exception that caused this exception</param>
		public ObjectNotFoundException(string errorMessage, Exception cause)
			: base(errorMessage, cause)
		{}

		/// <summary>
		///   Used during serialization.
		/// </summary>
		/// <param name="si">SerializationInfo</param>
		/// <param name="sc">StreamingContext</param>
		protected ObjectNotFoundException(SerializationInfo si, StreamingContext sc)
			: base(si, sc)
		{}
	}

	/// <summary>
	///   Exception raised by repositories when only one instance is expected
	///   but many were found.
	/// </summary>
	[Serializable]
	public class DuplicateObjectException : DataException
	{
		/// <summary>
		///   Default constructor; creates a new instance.
		/// </summary>
		public DuplicateObjectException()
		{}

		/// <summary>
		///   Creates a new instance using the error message given.
		/// </summary>
		/// <param name="errorMessage">An error message describing the exception.</param>
		public DuplicateObjectException(string errorMessage)
			: base(errorMessage)
		{}

		/// <summary>
		///   Creates a new instance using the error message and cuase given.
		/// </summary>
		/// <param name="errorMessage">An error message describing the exception.</param>
		/// <param name="cause">An inner exception that caused this exception</param>
		public DuplicateObjectException(string errorMessage, Exception cause)
			: base(errorMessage, cause)
		{}

		/// <summary>
		///   Used during serialization.
		/// </summary>
		/// <param name="si">SerializationInfo</param>
		/// <param name="sc">StreamingContext</param>
		protected DuplicateObjectException(SerializationInfo si, StreamingContext sc)
			: base(si, sc)
		{}
	}

	/// <summary>
	///   Exception raised when resolving references fails.
	/// </summary>
	[Serializable]
	public class DataModelReferenceException : DataException
	{
		/// <summary>
		///   Default constructor; creates a new instance.
		/// </summary>
		public DataModelReferenceException()
		{ }

		/// <summary>
		///   Creates a new instance using the error message given.
		/// </summary>
		/// <param name="errorMessage">An error message describing the exception.</param>
		public DataModelReferenceException(string errorMessage)
			: base(errorMessage)
		{ }

		/// <summary>
		///   Creates a new instance using the error message and cuase given.
		/// </summary>
		/// <param name="errorMessage">An error message describing the exception.</param>
		/// <param name="cause">An inner exception that caused this exception</param>
		public DataModelReferenceException(string errorMessage, Exception cause)
			: base(errorMessage, cause)
		{ }

		/// <summary>
		///   Used during serialization.
		/// </summary>
		/// <param name="si">SerializationInfo</param>
		/// <param name="sc">StreamingContext</param>
		protected DataModelReferenceException(SerializationInfo si, StreamingContext sc)
			: base(si, sc)
		{ }
	}
}