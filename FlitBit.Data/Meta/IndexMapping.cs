#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.Data.Meta
{
	public class IndexMapping
	{
		public IndexBehaviors Behaviors { get; set; }

		public IndexMapping SetBehavior(IndexBehaviors behaviors)
		{
			this.Behaviors = behaviors;
			return this;
		}
	}
}