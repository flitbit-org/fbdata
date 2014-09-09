using System;
using FlitBit.Data.Expressions;

namespace FlitBit.Data.DataModel
{
    internal class CacheKeyFormatter
    {
        public static Delegate EmitFormatSingleCacheKey<TDataModel>(IMapping<TDataModel> mapping,
            DataModelSqlExpression<TDataModel> sql)
        {
            Type[] argTypes;
            var parms = sql.ValueParameters;
            Type dlgType;
            switch (parms.Count)
            {
                case 1:
                    return ((KeyCapture<TDataModel>)
                            Activator.CreateInstance(typeof(KeyCapture<,>).MakeGenericType(
                                typeof(TDataModel),
                                parms[0].Type
                                ))).MakeFormatKey(mapping, sql);
                case 2:
                    return ((KeyCapture<TDataModel>)
                            Activator.CreateInstance(typeof(KeyCapture<,,>).MakeGenericType(
                                typeof(TDataModel),
                                parms[0].Type,
                                parms[1].Type
                                ))).MakeFormatKey(mapping, sql);
                case 3:
                    return ((KeyCapture<TDataModel>)
                            Activator.CreateInstance(typeof(KeyCapture<,,,>).MakeGenericType(
                                typeof(TDataModel),
                                parms[0].Type,
                                parms[1].Type,
                                parms[2].Type
                                ))).MakeFormatKey(mapping, sql);
                case 4:
                    return ((KeyCapture<TDataModel>)
                            Activator.CreateInstance(typeof(KeyCapture<,,,,>).MakeGenericType(
                                typeof(TDataModel),
                                parms[0].Type,
                                parms[1].Type,
                                parms[2].Type,
                                parms[3].Type
                                ))).MakeFormatKey(mapping, sql);
                case 5:
                    return ((KeyCapture<TDataModel>)
                            Activator.CreateInstance(typeof(KeyCapture<,,,,,>).MakeGenericType(
                                typeof(TDataModel),
                                parms[0].Type,
                                parms[1].Type,
                                parms[2].Type,
                                parms[3].Type,
                                parms[4].Type
                                ))).MakeFormatKey(mapping, sql);
                case 6:
                    return ((KeyCapture<TDataModel>)
                            Activator.CreateInstance(typeof(KeyCapture<,,,,,,>).MakeGenericType(
                                typeof(TDataModel),
                                parms[0].Type,
                                parms[1].Type,
                                parms[2].Type,
                                parms[3].Type,
                                parms[4].Type,
                                parms[5].Type
                                ))).MakeFormatKey(mapping, sql);
                case 7:
                    return ((KeyCapture<TDataModel>)
                            Activator.CreateInstance(typeof(KeyCapture<,,,,,,,>).MakeGenericType(
                                typeof(TDataModel),
                                parms[0].Type,
                                parms[1].Type,
                                parms[2].Type,
                                parms[3].Type,
                                parms[4].Type,
                                parms[5].Type,
                                parms[6].Type
                                ))).MakeFormatKey(mapping, sql);

                case 8:
                    return ((KeyCapture<TDataModel>)
                            Activator.CreateInstance(typeof(KeyCapture<,,,,,,,,>).MakeGenericType(
                                typeof(TDataModel),
                                parms[0].Type,
                                parms[1].Type,
                                parms[2].Type,
                                parms[3].Type,
                                parms[4].Type,
                                parms[5].Type,
                                parms[6].Type,
                                parms[7].Type
                                ))).MakeFormatKey(mapping, sql);
                case 9:
                    return ((KeyCapture<TDataModel>)
                            Activator.CreateInstance(typeof(KeyCapture<,,,,,,,,,>).MakeGenericType(
                                typeof(TDataModel),
                                parms[0].Type,
                                parms[1].Type,
                                parms[2].Type,
                                parms[3].Type,
                                parms[4].Type,
                                parms[5].Type,
                                parms[6].Type,
                                parms[7].Type,
                                parms[8].Type
                                ))).MakeFormatKey(mapping, sql);
                case 10:
                    return ((KeyCapture<TDataModel>)
                            Activator.CreateInstance(typeof(KeyCapture<,,,,,,,,,,>).MakeGenericType(
                                typeof(TDataModel),
                                parms[0].Type,
                                parms[1].Type,
                                parms[2].Type,
                                parms[3].Type,
                                parms[4].Type,
                                parms[5].Type,
                                parms[6].Type,
                                parms[7].Type,
                                parms[8].Type,
                                parms[9].Type
                                ))).MakeFormatKey(mapping, sql);
                default:
                    return null;
            }
        }

        abstract class KeyCapture<TDataModel>
        {
            public abstract Delegate MakeFormatKey(IMapping<TDataModel> mapping, DataModelSqlExpression<TDataModel> sql);
        }

        class KeyCapture<TDataModel, TParam> : KeyCapture<TDataModel>
        {
            public override Delegate MakeFormatKey(IMapping<TDataModel> mapping, DataModelSqlExpression<TDataModel> sql)
            {
                var parms = sql.ValueParameters;
                var keyWriter = new SqlWriter(400, "", "");
                sql.Write(keyWriter);
                var fmt = mapping.FormatClusteredMemoryKey(keyWriter.Text);
                fmt = fmt.Replace(parms[0].Text, "{0}");
                return new Func<TParam, string>(
                    param =>
                    String.Format(fmt, param));
            }
        }

        class KeyCapture<TDataModel, TParam, TParam1> : KeyCapture<TDataModel>
        {
            public override Delegate MakeFormatKey(IMapping<TDataModel> mapping, DataModelSqlExpression<TDataModel> sql)
            {
                var parms = sql.ValueParameters;
                var keyWriter = new SqlWriter(400, "", "");
                sql.Write(keyWriter);
                var fmt = mapping.FormatClusteredMemoryKey(keyWriter.Text);
                fmt = fmt.Replace(parms[0].Text, "{0}");
                fmt = fmt.Replace(parms[1].Text, "{1}");
                return new Func<TParam, TParam1, string>(
                    (param, param1) =>
                    String.Format(fmt, param, param1));
            }
        }

        class KeyCapture<TDataModel, TParam, TParam1, TParam2> : KeyCapture<TDataModel>
        {
            public override Delegate MakeFormatKey(IMapping<TDataModel> mapping, DataModelSqlExpression<TDataModel> sql)
            {
                var parms = sql.ValueParameters;
                var keyWriter = new SqlWriter(400, "", "");
                sql.Write(keyWriter);
                var fmt = mapping.FormatClusteredMemoryKey(keyWriter.Text);
                fmt = fmt.Replace(parms[0].Text, "{0}");
                fmt = fmt.Replace(parms[1].Text, "{1}");
                fmt = fmt.Replace(parms[2].Text, "{2}");
                return new Func<TParam, TParam1, TParam2, string>(
                    (param, param1, param2) =>
                    String.Format(fmt, param, param1, param2));
            }
        }

        class KeyCapture<TDataModel, TParam, TParam1, TParam2, TParam3> : KeyCapture<TDataModel>
        {
            public override Delegate MakeFormatKey(IMapping<TDataModel> mapping, DataModelSqlExpression<TDataModel> sql)
            {
                var parms = sql.ValueParameters;
                var keyWriter = new SqlWriter(400, "", "");
                sql.Write(keyWriter);
                var fmt = mapping.FormatClusteredMemoryKey(keyWriter.Text);
                fmt = fmt.Replace(parms[0].Text, "{0}");
                fmt = fmt.Replace(parms[1].Text, "{1}");
                fmt = fmt.Replace(parms[2].Text, "{2}");
                fmt = fmt.Replace(parms[3].Text, "{3}");
                return new Func<TParam, TParam1, TParam2, TParam3, string>(
                    (param, param1, param2, param3) =>
                    String.Format(fmt, param, param1, param2, param3));
            }
        }

        class KeyCapture<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4> : KeyCapture<TDataModel>
        {
            public override Delegate MakeFormatKey(IMapping<TDataModel> mapping, DataModelSqlExpression<TDataModel> sql)
            {
                var parms = sql.ValueParameters;
                var keyWriter = new SqlWriter(400, "", "");
                sql.Write(keyWriter);
                var fmt = mapping.FormatClusteredMemoryKey(keyWriter.Text);
                fmt = fmt.Replace(parms[0].Text, "{0}");
                fmt = fmt.Replace(parms[1].Text, "{1}");
                fmt = fmt.Replace(parms[2].Text, "{2}");
                fmt = fmt.Replace(parms[3].Text, "{3}");
                fmt = fmt.Replace(parms[4].Text, "{4}");
                return new Func<TParam, TParam1, TParam2, TParam3, TParam4, string>(
                    (param, param1, param2, param3, param4) =>
                    String.Format(fmt, param, param1, param2, param3, param4));
            }
        }

        class KeyCapture<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> : KeyCapture<TDataModel>
        {
            public override Delegate MakeFormatKey(IMapping<TDataModel> mapping, DataModelSqlExpression<TDataModel> sql)
            {
                var parms = sql.ValueParameters;
                var keyWriter = new SqlWriter(400, "", "");
                sql.Write(keyWriter);
                var fmt = mapping.FormatClusteredMemoryKey(keyWriter.Text);
                fmt = fmt.Replace(parms[0].Text, "{0}");
                fmt = fmt.Replace(parms[1].Text, "{1}");
                fmt = fmt.Replace(parms[2].Text, "{2}");
                fmt = fmt.Replace(parms[3].Text, "{3}");
                fmt = fmt.Replace(parms[4].Text, "{4}");
                fmt = fmt.Replace(parms[5].Text, "{5}");
                return new Func<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, string>(
                    (param, param1, param2, param3, param4, param5) =>
                    String.Format(fmt, param, param1, param2, param3, param4, param5));
            }
        }

        class KeyCapture<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> :
            KeyCapture<TDataModel>
        {
            public override Delegate MakeFormatKey(IMapping<TDataModel> mapping, DataModelSqlExpression<TDataModel> sql)
            {
                var parms = sql.ValueParameters;
                var keyWriter = new SqlWriter(400, "", "");
                sql.Write(keyWriter);
                var fmt = mapping.FormatClusteredMemoryKey(keyWriter.Text);
                fmt = fmt.Replace(parms[0].Text, "{0}");
                fmt = fmt.Replace(parms[1].Text, "{1}");
                fmt = fmt.Replace(parms[2].Text, "{2}");
                fmt = fmt.Replace(parms[3].Text, "{3}");
                fmt = fmt.Replace(parms[4].Text, "{4}");
                fmt = fmt.Replace(parms[5].Text, "{5}");
                fmt = fmt.Replace(parms[6].Text, "{6}");
                return new Func<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, string>(
                    (param, param1, param2, param3, param4, param5, param6) =>
                    String.Format(fmt, param, param1, param2, param3, param4, param5, param6));
            }
        }

        class KeyCapture<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> :
            KeyCapture<TDataModel>
        {
            public override Delegate MakeFormatKey(IMapping<TDataModel> mapping, DataModelSqlExpression<TDataModel> sql)
            {
                var parms = sql.ValueParameters;
                var keyWriter = new SqlWriter(400, "", "");
                sql.Write(keyWriter);
                var fmt = mapping.FormatClusteredMemoryKey(keyWriter.Text);
                fmt = fmt.Replace(parms[0].Text, "{0}");
                fmt = fmt.Replace(parms[1].Text, "{1}");
                fmt = fmt.Replace(parms[2].Text, "{2}");
                fmt = fmt.Replace(parms[3].Text, "{3}");
                fmt = fmt.Replace(parms[4].Text, "{4}");
                fmt = fmt.Replace(parms[5].Text, "{5}");
                fmt = fmt.Replace(parms[6].Text, "{6}");
                fmt = fmt.Replace(parms[7].Text, "{7}");
                return new Func<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, string>(
                    (param, param1, param2, param3, param4, param5, param6, param7) =>
                    String.Format(fmt, param, param1, param2, param3, param4, param5, param6, param7));
            }
        }

        class KeyCapture<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> :
            KeyCapture<TDataModel>
        {
            public override Delegate MakeFormatKey(IMapping<TDataModel> mapping, DataModelSqlExpression<TDataModel> sql)
            {
                var parms = sql.ValueParameters;
                var keyWriter = new SqlWriter(400, "", "");
                sql.Write(keyWriter);
                var fmt = mapping.FormatClusteredMemoryKey(keyWriter.Text);
                fmt = fmt.Replace(parms[0].Text, "{0}");
                fmt = fmt.Replace(parms[1].Text, "{1}");
                fmt = fmt.Replace(parms[2].Text, "{2}");
                fmt = fmt.Replace(parms[3].Text, "{3}");
                fmt = fmt.Replace(parms[4].Text, "{4}");
                fmt = fmt.Replace(parms[5].Text, "{5}");
                fmt = fmt.Replace(parms[6].Text, "{6}");
                fmt = fmt.Replace(parms[7].Text, "{7}");
                fmt = fmt.Replace(parms[8].Text, "{8}");
                return new Func<TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, string>(
                    (param, param1, param2, param3, param4, param5, param6, param7, param8) =>
                    String.Format(fmt, param, param1, param2, param3, param4, param5, param6, param7, param8));
            }
        }

        class KeyCapture<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8,
                         TParam9> :
                             KeyCapture<TDataModel>
        {
            public override Delegate MakeFormatKey(IMapping<TDataModel> mapping, DataModelSqlExpression<TDataModel> sql)
            {
                var parms = sql.ValueParameters;
                var keyWriter = new SqlWriter(400, "", "");
                sql.Write(keyWriter);
                var fmt = mapping.FormatClusteredMemoryKey(keyWriter.Text);
                fmt = fmt.Replace(parms[0].Text, "{0}");
                fmt = fmt.Replace(parms[1].Text, "{1}");
                fmt = fmt.Replace(parms[2].Text, "{2}");
                fmt = fmt.Replace(parms[3].Text, "{3}");
                fmt = fmt.Replace(parms[4].Text, "{4}");
                fmt = fmt.Replace(parms[5].Text, "{5}");
                fmt = fmt.Replace(parms[6].Text, "{6}");
                fmt = fmt.Replace(parms[7].Text, "{7}");
                fmt = fmt.Replace(parms[8].Text, "{8}");
                fmt = fmt.Replace(parms[9].Text, "{9}");
                return new Func
                    <TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, string>
                    (
                    (param, param1, param2, param3, param4, param5, param6, param7, param8, param9) =>
                    String.Format(fmt, param, param1, param2, param3, param4, param5, param6, param7, param8, param9));
            }
        }
    }
}