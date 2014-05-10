#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using FlitBit.Data.DataModel;

namespace FlitBit.Data.Meta
{
  public abstract class CollectionMapping
  {
    internal CollectionMapping(IMapping mapping, MemberInfo member, string name)
    {
      Contract.Requires<ArgumentNullException>(mapping != null);
      Contract.Requires<ArgumentNullException>(member != null);
      Contract.Requires<ArgumentNullException>(name != null);

      LocalMapping = mapping;
      LocalMember = member;
      Name = name;
    }

    /// <summary>
    ///   The collection's name.
    /// </summary>
    public string Name { get; private set; }

    public MemberInfo LocalMember { get; internal set; }

    public IMapping LocalMapping { get; private set; }

    public IList<MemberInfo> LocalProperties { get; internal set; }

    public ReferenceBehaviors ReferenceBehaviors { get; internal set; }

    public Type ReferencedType { get; internal set; }

    public IMapping ReferencedMapping { get; internal set; }

    public IList<MemberInfo> ReferencedProperties { get; internal set; }

    public Type JoinType { get; set; }

    public IList<MemberInfo> JoinProperties { get; set; }

    public IMapping JoinMapping { get; internal set; }

    internal Type MakeCollectionReferenceType(IMapping mapping)
    {
      Contract.Requires<InvalidOperationException>(mapping.HasBinder);

      var paramTypes = new List<Type>();
      var arity = ReferencedProperties.Count;
      for (var i = 0; i < arity; i++)
      {
        paramTypes.Add(CalculateColumnTypeFromJoinProperties(this.LocalProperties[i], ReferencedProperties[i]));
      }

      switch (arity)
      {
        case 1:
          return typeof(DataModelCollectionReference<,,,>).MakeGenericType(ReferencedType,
            mapping.IdentityKeyType,
            mapping.GetDbProviderHelper().DbConnectionType,
            paramTypes[0]);
        default:
          throw new NotImplementedException();
      }
    }

    Type CalculateColumnTypeFromJoinProperties(MemberInfo local, MemberInfo reference)
    {
      Contract.Requires<ArgumentNullException>(local != null);
      Contract.Requires<ArgumentNullException>(reference != null);

      var lcolumn = LocalMapping.Columns.SingleOrDefault(c => c.Member == local);
      if (lcolumn == null)
      {
        throw new MappingException(String.Concat("The mapped collection on ", LocalMapping.RuntimeType.Name, ".",
          LocalMember.Name,
          " uses a local property that is not mapped to a column: ", LocalMapping.RuntimeType.Name, ".", local.Name)
          );
      }
      var rmapping = JoinMapping ?? ReferencedMapping;
      var rcolumn = rmapping.Columns.SingleOrDefault(c => c.Member == reference);
      if (rcolumn == null)
      {
        throw new MappingException(String.Concat("The mapped collection on ", LocalMapping.RuntimeType.Name, ".",
          LocalMember.Name,
          " references an property that is not mapped to a column: ", ReferencedType.Name, ".", reference.Name)
          );
      }
      if (lcolumn.UnderlyingType != rcolumn.UnderlyingType)
      {
        throw new MappingException(String.Concat("The mapped collection on ", LocalMapping.RuntimeType.Name, ".",
          LocalMember.Name,
          " references incompatible properties: ", LocalMapping.RuntimeType.Name, ".", local.Name, " cannot equal ",
          ReferencedType.Name, ".", reference.Name)
          );
      }
      return lcolumn.UnderlyingType;
    }
  }
}