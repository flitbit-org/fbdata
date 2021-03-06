﻿using System;

namespace FlitBit.Data
{
	/// <summary>
	/// Indicates behaviors for the current query.
	/// </summary>
	[Flags]
	public enum QueryBehaviors
	{
		/// <summary>
		///   Indicates the default behavior.
		/// </summary>
		Default = 0,

		/// <summary>
		///   Indicates the query should not consider cached data or cache its results.
		/// </summary>
		NoCache = 1,

		/// <summary>
		///   Indicates the number of results should be limited.
		/// </summary>
		Limited = 1 << 1,

		/// <summary>
		///   Indicates the results should be paged.
		/// </summary>
		Paged = 1 << 2,
	}
}