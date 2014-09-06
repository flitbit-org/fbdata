#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using FlitBit.Data.Meta;
using FlitBit.Wireup.Meta;

// Declare a direct dependency on FlitBit.Data being wired up.
[assembly: WireupDependency(typeof(FlitBit.Data.AssemblyWireup))]
[module: MapConnection("test-data")]

