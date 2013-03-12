using System.Collections.Generic;

namespace FlitBit.Data
{
	public interface IHierarchyMapping<in M>
	{
		void NotifySubtype<MM>(IMapping<MM> mapping)
			where MM: M;
	}

	public interface IHierarchyMappings<out M>
	{
		IEnumerable<IMapping<M>> KnownSubtypes { get; }
	}

	public class HierarchyMapping<M>: IHierarchyMapping<M>, IHierarchyMappings<M>
	{
		readonly List<IMapping<M>> __knownSubtypes = new List<IMapping<M>>();

		public void NotifySubtype<MM>(IMapping<MM> mapping) where MM : M
		{
			__knownSubtypes.Add((IMapping<M>)mapping);
		}

		public IEnumerable<IMapping<M>> KnownSubtypes
		{
			get { return __knownSubtypes.AsReadOnly(); }
		}
	}
}
