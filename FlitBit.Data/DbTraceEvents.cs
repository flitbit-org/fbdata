#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics;

namespace FlitBit.Data
{
	public static class DbTraceEvents
	{
		internal static void OnTraceEvent(Object sender, TraceEventType eventType, string description) { }
		internal static bool ShouldTrace(TraceEventType traceEventType) { return false; }
	}
}