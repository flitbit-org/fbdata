﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FlitBit.Core.Collections;
using FlitBit.Emit;
using FlitBit.Core;

namespace FlitBit.Data.Meta
{
	[Flags]
	public enum DependencyKind
	{
		/// <summary>
		/// No known dependency
		/// </summary>
		None = 0,
		/// <summary>
		/// Indicates a direct dependency via a column reference.
		/// </summary>
		Direct = 1,
		/// <summary>
		/// Indicates an indirect dependency >= 1 degree of separation via
		/// direct dependency.
		/// </summary>
		Indirect = 2,
		/// <summary>
		/// Indicates a soft dependency supporting a query path or
		/// collection criteria.
		/// </summary>
		Soft = 4,
		/// <summary>
		/// Indicates the dependency path results in a circular dependency.
		/// </summary>
		Circular = 8,

		DirectCircular = Direct | Circular,
		IndirectCircular = Indirect | Circular,
		SoftCircular = Soft | Circular,
		/// <summary>
		/// Indicates a dependency on self.
		/// </summary>
		Self = 16 | Circular
	}

	public sealed class Dependency
	{
		static readonly int CHashCodeSeed = typeof(Dependency).AssemblyQualifiedName.GetHashCode();
		
		readonly Object _lock = new Object();
		Status<DependencyKind> _kind = new Status<DependencyKind>();
		IEnumerable<IEnumerable<MemberInfo>> _inderdependencyPaths = new IEnumerable<MemberInfo>[0].ToReadOnly();

		public Dependency(DependencyKind kind, IMapping origin, MemberInfo member, IMapping target)
		{
			_kind.ChangeState(kind);
			Origin = origin;
			Member = member;
			Target = target;
		}

		public DependencyKind Kind { get { return _kind.CurrentState; } }
		public IMapping Origin { get; private set; }
		public MemberInfo Member { get; private set; }
		public IMapping Target { get; private set; }
		public IEnumerable<IEnumerable<MemberInfo>> InterdependencyPaths
		{
			get
			{
				return _inderdependencyPaths;
			}
		}

		internal Dependency CalculateDependencyKind()
		{
			if (Target == Origin)
			{
				_kind.ChangeState(_kind.CurrentState | DependencyKind.Self);
			}
			else
			{
				var self = this;
				var path = new MemberInfo[] { Member };
				Target.Completed(() =>
				{
					var refs = Target.ParticipatingMembers.Where(info => Mapping.ExistsFor(info.GetTypeOfValue().FindElementType()));
					foreach (var mbr in refs)
					{
						ExhaustiveSeekCircularDependency(self, mbr, path);
					}		
				});
			}
			return this;
		}

		void AddInterdependencyPath(IEnumerable<MemberInfo> path)
		{
			lock (this)
			{
				_kind.ChangeState(_kind.CurrentState | DependencyKind.Circular);
			
				_inderdependencyPaths = _inderdependencyPaths
					.Concat(new IEnumerable<MemberInfo>[] { path })
					.OrderBy(e => e.Count()).ToReadOnly();
			}
		}

		static void ExhaustiveSeekCircularDependency(Dependency origin, MemberInfo member, IEnumerable<MemberInfo> path)
		{
			var mtype = member.GetTypeOfValue().FindElementType();
			path = path.Concat(new MemberInfo[] { member }).ToArray();				
			if (origin.Origin.RuntimeType == mtype)
			{
				origin.AddInterdependencyPath(path);
				
			}
			else if (!path.Contains(member))
			{
				IMapping m = Mapping.AccessMappingFor(mtype);
				m.Completed(() => 
				{
					var refs = m.ParticipatingMembers.Where(info => Mapping.ExistsFor(info.GetTypeOfValue().FindElementType()));
					foreach (var mbr in refs)
					{
						ExhaustiveSeekCircularDependency(origin, mbr, path);
					}					
				});
			}
		}

		#region Object overrides

		public bool Equals(Dependency other)
		{
			return other != null
				&& Kind == other.Kind
				&& Origin == other.Origin
				&& Member == other.Member;
		}

		public override bool Equals(object obj)
		{
			return obj is Dependency
				&& Equals((Dependency)obj);
		}

		public override int GetHashCode()
		{
			int prime = 999067; // a random prime

			int result = CHashCodeSeed * prime;
			result *= base.GetHashCode(); // TODO: Fix this!
			return result;
		}

		public override string ToString()
		{
			var buffer = new StringBuilder(400)
				.Append("{ Kind: ").Append(_kind.CurrentState)
				.Append(", Origin: ").Append('"').Append(Origin.RuntimeType.FullName).Append('"')
				.Append(", Member: ").Append('"').Append(Member.Name).Append('"')
				.Append(", Target: ").Append('"').Append(Target.RuntimeType.FullName).Append('"')
				;

			if (_inderdependencyPaths.Count() > 0)
			{
				buffer.Append(", InterdependencyPaths: [");
				var i = 0;
				foreach (var e in _inderdependencyPaths)
				{
					if (i++ > 0) buffer.Append(", ");
					buffer.Append('"').Append(e.Describe()).Append('"');
				}
				buffer.Append(" ]");
			}
			return buffer.Append(
				" }").ToString();		
		}
		#endregion
	}

	internal static class DepExt
	{
		internal static string Describe(this IEnumerable<MemberInfo> self)
		{
			var buf = new StringBuilder(200);
			var i = 0;
			foreach (var m in self)
			{
				if (i++ > 0) buf.Append(" -> ");
				buf.Append(m.ReflectedType.Name).Append('.').Append(m.Name);
			}
			return buf.ToString();
		}
	}
}