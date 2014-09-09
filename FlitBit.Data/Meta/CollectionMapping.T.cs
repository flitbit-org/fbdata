#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FlitBit.Core;
using FlitBit.Data.DataModel;
using FlitBit.Emit;

namespace FlitBit.Data.Meta
{
    public class CollectionMapping<T> : CollectionMapping
    {
        internal CollectionMapping(Mapping<T> mapping, MemberInfo member, string name)
            : base(mapping, member, name)
        {}

        public IMapping<T> End() { return (Mapping<T>)LocalMapping; }

        public CollectionMapping<T> JoinReference<U>(Expression<Func<U, object>> expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);

            var mapping = LocalMapping;

            var member = expression.GetMemberFromExpression();
            Contract.Assert(member != null, "Expression must reference a field or property member");

            var memberType = member.MemberType;
            Contract.Assert(memberType == MemberTypes.Field
                            || memberType == MemberTypes.Property,
                "Expression must reference a field or property member");

            var refType = member.GetTypeOfValue();
            var elmType = refType.GetElementType();

            IMapping elmMapping;

            if (elmType == typeof(T))
            {
                elmMapping = LocalMapping;
            }
            else if (Mappings.ExistsFor(elmType))
            {
                elmMapping = Mappings.AccessMappingFor(elmType);
            }
            else
            {
                throw new MappingException(String.Concat("Unable to fulfill collection mapping on ", typeof(T).Name, ".",
                    member.Name,
                    " because the property must reference a mapped type.")
                    );
            }
            var localProps = mapping.Identity.Columns.Select(c => c.Column.Member.Name).ToArray();
            if (localProps.Length > 1)
            {
                throw new MappingException(String.Concat("The mapped collection on ", typeof(T).Name, ".", member.Name,
                    " must identify the same number of join properties on both sides of the reference.")
                    );
            }
            var locals = new List<MemberInfo>();
            foreach (var name in localProps)
            {
                var pp = typeof(T).GetProperty(name);
                if (pp == null)
                {
                    throw new MappingException(String.Concat("The mapped collection on ", typeof(T).Name, ".",
                        member.Name,
                        " names a local property that does not exist: ", name, ".")
                        );
                }
                locals.Add(pp);
            }
            var referenced = new List<MemberInfo>(new[]
            {
                member
            });
            ReferencedType = elmType;
            ReferencedProperties = referenced;
            ReferencedMapping = elmMapping;
            this.LocalProperties = locals;

            return this;
        }

        public CollectionMapping Where<U>(Expression<Func<T, U, bool>> expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);

            throw new NotImplementedException();

            return this;
        }
    }
}