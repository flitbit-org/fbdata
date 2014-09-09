#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using FlitBit.Core;
using FlitBit.Emit;

namespace FlitBit.Data
{
    public static class IDataParameterBinderExtensions
    {
        public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name,
            bool value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(bool));
            }
            binder.SetParameterValue(name, value);
            return binder;
        }

        public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name,
            byte value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(byte));
            }
            binder.SetParameterValue(name, value);
            return binder;
        }

        public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name,
            int length,
            byte[] value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(byte[]), length, ParameterDirection.Input);
            }
            binder.SetParameterValue(name, value);
            return binder;
        }

        public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name,
            byte[] value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(byte[]));
            }
            binder.SetParameterValue(name, value);
            return binder;
        }

        public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name,
            DateTime value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(DateTime));
            }
            binder.SetParameterValue(name, value);
            return binder;
        }

        public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name,
            decimal value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(decimal));
            }
            binder.SetParameterValue(name, value);
            return binder;
        }

        public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name,
            double value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(double));
            }
            binder.SetParameterValue(name, value);
            return binder;
        }

        public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name,
            Guid value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(Guid));
            }
            binder.SetParameterValue(name, value);
            return binder;
        }

        public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name,
            float value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(float));
            }
            binder.SetParameterValue(name, value);
            return binder;
        }

        public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name,
            sbyte value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(sbyte));
            }
            binder.SetParameterValue(name, value);
            return binder;
        }

        public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name,
            int length,
            string value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(string), length, ParameterDirection.Input);
            }
            binder.SetParameterValue(name, value);
            return binder;
        }

        public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name,
            string value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(string));
            }
            binder.SetParameterValue(name, value);
            return binder;
        }

        public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name,
            short value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(short));
            }
            binder.SetParameterValue(name, value);
            return binder;
        }

        public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name,
            int value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(int));
            }
            binder.SetParameterValue(name, value);
            return binder;
        }

        public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name,
            long value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(long));
            }
            binder.SetParameterValue(name, value);
            return binder;
        }

        public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name,
            ushort value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(ushort));
            }
            binder.SetParameterValue(name, value);
            return binder;
        }

        public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name,
            uint value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(uint));
            }
            binder.SetParameterValue(name, value);
            return binder;
        }

        public static IDataParameterBinder DefineAndBindParameter(this IDataParameterBinder binder, string name,
            ulong value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(ulong));
            }
            binder.SetParameterValue(name, value);
            return binder;
        }

        public static IDataParameterBinder DefineAndBindParameter<T>(this IDataParameterBinder binder, string name,
            T value)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(T));
            }
            binder.SetParameterValue(name, value);
            return binder;
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "By design."
            )]
        public static IDataParameterBinder DefineAndSetDbNull<T>(this IDataParameterBinder binder, string name)
        {
            Contract.Requires<ArgumentNullException>(binder != null);
            Contract.Requires<ArgumentNullException>(name != null);

            if (!binder.ContainsParameter(name))
            {
                binder.DefineParameter(name, typeof(T));
            }
            binder.SetParameterDbNull(name);
            return binder;
        }

        [SuppressMessage("Microsoft.Design", "CA1006", Justification = "By design.")]
        public static IDataParameterBinder DefineParameter<T>(this IDataParameterBinder exe,
            Expression<Func<T, object>> expression)
        {
            Contract.Requires<ArgumentNullException>(exe != null);
            Contract.Requires<ArgumentNullException>(expression != null);
            Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

            return DefineParameter(exe, expression, ParameterDirection.Input);
        }

        [SuppressMessage("Microsoft.Design", "CA1006", Justification = "By design.")]
        public static IDataParameterBinder DefineParameter<T>(this IDataParameterBinder exe,
            Expression<Func<T, object>> expression, ParameterDirection direction)
        {
            Contract.Requires<ArgumentNullException>(exe != null);
            Contract.Requires<ArgumentNullException>(expression != null);
            Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

            var member = expression.GetMemberFromExpression();
            Contract.Assert(member != null, "Expression must reference a field or property member");

            var memberType = member.MemberType;
            Contract.Assert(memberType == MemberTypes.Property, "Expression must reference a property member");

            return exe.DefineParameter(member.Name, member.GetTypeOfValue(), direction);
        }

        public static IDataParameterBinder DefineParameter(this IDataParameterBinder exe, PropertyInfo property)
        {
            Contract.Requires<ArgumentNullException>(exe != null);
            Contract.Requires<ArgumentNullException>(property != null);
            Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

            return DefineParameter(exe, property, ParameterDirection.Input);
        }

        public static IDataParameterBinder DefineParameter(this IDataParameterBinder exe, PropertyInfo property,
            ParameterDirection direction)
        {
            Contract.Requires<ArgumentNullException>(exe != null);
            Contract.Requires<ArgumentNullException>(property != null);
            Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

            return exe.DefineParameter(property.Name, property.PropertyType, direction);
        }

        public static IDataParameterBinder DefineParameter(this IDataParameterBinder exe, string bindName,
            PropertyInfo property, ParameterDirection direction)
        {
            Contract.Requires<ArgumentNullException>(exe != null);
            Contract.Requires<ArgumentNullException>(bindName != null);
            Contract.Requires<ArgumentException>(bindName.Length > 0);
            Contract.Requires<ArgumentNullException>(property != null);
            Contract.Requires<ArgumentNullException>(property.Name != null);
            Contract.Requires<ArgumentNullException>(property.Name.Length > 0);
            Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

            return
                exe.DefineParameter(
                    () => new DbParamDefinition(property.Name, bindName, property.PropertyType, direction));
        }
    }
}