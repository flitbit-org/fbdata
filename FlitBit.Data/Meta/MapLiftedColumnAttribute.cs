using System;

namespace FlitBit.Data.Meta
{
	public enum LiftedColumnBehaviors
	{
		Default = 0,
		FullMatch = 0,
		Identity = 1
	}

	public class MapLiftedColumnAttribute : Attribute
	{
		public MapLiftedColumnAttribute()
			: this(LiftedColumnBehaviors.FullMatch) { }

		public MapLiftedColumnAttribute(LiftedColumnBehaviors behaviors) { this.Behaviors = behaviors; }

		public LiftedColumnBehaviors Behaviors { get; set; }
	}
}