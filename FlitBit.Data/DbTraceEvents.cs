using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace FlitBit.Data
{
	public static class DbTraceEvents
	{
		internal static bool ShouldTrace(TraceEventType traceEventType)
		{
			return false;
		}

		internal static void OnTraceEvent(Object sender, TraceEventType eventType, string description)
		{	
			
		}
	}
}
