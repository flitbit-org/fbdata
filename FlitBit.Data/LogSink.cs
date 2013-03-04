using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using FlitBit.Core;

namespace FlitBit.Data
{
	public class LogSink
	{
		internal static bool ShouldTrace(TraceEventType eventType)
		{
			return false;
		}

		internal static void OnTraceEvent(TraceEventType eventType, string message)
		{	
		}		
	}
}
