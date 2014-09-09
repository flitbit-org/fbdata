#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

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