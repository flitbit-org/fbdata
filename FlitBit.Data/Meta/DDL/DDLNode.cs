#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace FlitBit.Data.Meta.DDL
{
	public abstract class DDLNode
	{
		Dictionary<string, DDLNode> _children = new Dictionary<string, DDLNode>();

		public DDLNode(DDLNodeKind kind, DDLNode parent, string name, DDLBehaviors behaviors)
		{
			this.Kind = kind;
			this.Name = name;
			this.Parent = parent;
			this.Behaviors = (behaviors == DDLBehaviors.Inherit) ? parent.Behaviors : behaviors;
		}

		public DDLNodeKind Kind { get; private set; }
		public DDLNode Parent { get; private set; }
		public int Ordinal { get; private set; }
		public string Name { get; private set; }
		public DDLBehaviors Behaviors { get; private set; }

		public DDLTable DefineTable(string name) { return DefineTable(name, Behaviors); }

		public DDLTable DefineTable(string name, DDLBehaviors behaviors)
		{
			DDLNode n;
			if (!_children.TryGetValue(name, out n))
			{
				AddChild(n = new DDLTable(this, name, behaviors));
			}
			else if (n.Kind != DDLNodeKind.Table)
			{
				throw new InvalidOperationException(String.Concat("Table `", name, "` is obstructed another node: ", n.Kind, "."));
			}
			return (DDLTable) n;
		}

		public DDLCatalog UseCatalog(string name) { return UseCatalog(name, Behaviors); }

		public DDLCatalog UseCatalog(string name, DDLBehaviors behaviors)
		{
			DDLNode n;
			if (!_children.TryGetValue(name, out n))
			{
				AddChild(n = new DDLCatalog(this, name, behaviors));
			}
			else if (n.Kind != DDLNodeKind.Catalog)
			{
				throw new InvalidOperationException(String.Concat("Catalog `", name, "` is obstructed another node: ", n.Kind, "."));
			}
			return (DDLCatalog) n;
		}

		public DDLSchema UseSchema(string name) { return UseSchema(name, Behaviors); }

		public DDLSchema UseSchema(string name, DDLBehaviors behaviors)
		{
			DDLNode n;
			if (!_children.TryGetValue(name, out n))
			{
				AddChild(n = new DDLSchema(this, name, behaviors));
			}
			else if (n.Kind != DDLNodeKind.Schema)
			{
				throw new InvalidOperationException(String.Concat("Schema `", name, "` is obstructed another node: ", n.Kind, "."));
			}
			return (DDLSchema) n;
		}

		protected void AddChild<TNode>(TNode child)
			where TNode : DDLNode
		{
			DDLNode n;
			if (_children.TryGetValue(child.Name, out n))
			{
				throw new InvalidOperationException(String.Concat("Catalog `", child.Name, "` is obstructed another node: ", n.Kind,
																													"."));
			}
			else
			{
				child.Ordinal = _children.Count;
				_children.Add(child.Name, child);
			}
		}

		protected TNode GetChild<TNode>(DDLNodeKind kind, string name)
			where TNode : DDLNode
		{
			DDLNode n;
			if (_children.TryGetValue(name, out n) && n.Kind == kind)
			{
				return (TNode) n;
			}
			return default(TNode);
		}

		protected IEnumerable<TNode> GetChildren<TNode>(DDLNodeKind kind)
			where TNode : DDLNode
		{
			return (from n in _children.Values
							where n.Kind == kind
							orderby n.Ordinal
							select n).Cast<TNode>();
		}

		protected void SpecializeKind(DDLNodeKind kind) { this.Kind = kind; }
	}
}