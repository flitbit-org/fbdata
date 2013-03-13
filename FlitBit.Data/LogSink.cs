using System.Diagnostics;

namespace FlitBit.Data
{
	public class LogSink
	{
		internal static void OnTraceEvent(TraceEventType eventType, string message) { }
		internal static bool ShouldTrace(TraceEventType eventType) { return false; }
	}
}