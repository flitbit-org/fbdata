#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using FlitBit.Core;
using FlitBit.Core.Log;
using FlitBit.Data.Meta;
using FlitBit.Data.Meta.DDL;

namespace FlitBit.Data.DataModel
{
  public static class IMappingExtensions
  {
    static readonly ILogSink LogSink = typeof(IMappingExtensions).GetLogSink();

    public static DDLBatch GetDdlBatch(this IMapping mapping, DDLBatch batch, Stack<IMapping> stack)
    {
      Contract.Requires<ArgumentNullException>(mapping != null);
      Contract.Requires<ArgumentNullException>(batch != null);
      Contract.Requires<ArgumentNullException>(stack != null);
      Contract.Ensures(Contract.Result<DDLBatch>() != null);

      stack.Push(mapping);
      try
      {
        var cn = DbExtensions.CreateAndOpenConnection(mapping.ConnectionName);
        var helper = DbProviderHelpers.GetDbProviderHelperForDbConnection(mapping.ConnectionName);

        DDLNode scope = batch;
        if (!String.IsNullOrWhiteSpace(mapping.TargetCatalog))
        {
          scope = scope.UseCatalog(mapping.TargetCatalog);
        }
        if (!String.IsNullOrWhiteSpace(mapping.TargetSchema))
        {
          scope = scope.UseSchema(mapping.TargetSchema);
        }

        foreach (var dep in mapping.Dependencies)
        {
          var target = dep.Target;
          if (!stack.Contains(target))
          {
            target.GetDdlBatch(batch, stack);
          }
        }

        var hasLcgColumns = false;
        var table = scope.DefineTable(mapping.TargetObject);
        foreach (var col in mapping.Columns)
        {
          hasLcgColumns = hasLcgColumns || col.Behaviors.HasFlag(ColumnBehaviors.LinearCongruentGenerated);
          var column = table.DefineColumn(col, batch.Behaviors);
        }
      }
      finally
      {
        var bottom = stack.Pop();
        if (!ReferenceEquals(bottom, mapping))
        {
          LogSink.Warning(
            String.Concat(
              "GetDdlBatch encountered an error in the stack while generating DDL; stack contained {0} instead of {1} on exit.",
              bottom.GetType()
                    .GetReadableFullName(),
              mapping.GetType()
                     .GetReadableFullName())
            );
        }
      }
      return batch;
    }

    public static DDLBatch GetDdlBatch(this IMapping mapping, DDLBehaviors behaviors)
    {
      Contract.Requires<ArgumentNullException>(mapping != null);
      Contract.Requires<InvalidOperationException>(!string.IsNullOrEmpty(mapping.ConnectionName),
        "ConnectionName must be set before creating SQL commands for an entity");
      Contract.Ensures(Contract.Result<DDLBatch>() != null);

      return GetDdlBatch(mapping, new DDLBatch(behaviors), new Stack<IMapping>());
    }

    public static ColumnMapping GetPreferredReferenceColumn(this IMapping mapping)
    {
      if (mapping.IsEnum)
      {
        var idcol = mapping.Columns.FirstOrDefault(c => c.RuntimeType.IsEnum && c.IsIdentity);
        if (idcol == null)
        {
          throw new MappingException(String.Concat("Entity type '", mapping.RuntimeType.GetReadableFullName(),
            "' declares behavior EntityBehaviors.MapEnum but the enum type cannot be determined. Specify an identity column of enum type."));
        }
        var namecol = mapping.Columns.FirstOrDefault(c => c.RuntimeType == typeof(String) && c.IsAlternateKey);
        if (namecol == null)
        {
          throw new MappingException(String.Concat("Entity type '", mapping.RuntimeType.GetReadableFullName(),
            "' declares behavior EntityBehaviors.MapEnum but a column to hold the enum name cannot be determined. Specify a string column with alternate key behavior."));
        }
        var mapEnum =
          (MapEnumAttribute)idcol.RuntimeType.GetCustomAttributes(typeof(MapEnumAttribute), false)
                                 .SingleOrDefault();
        return (mapEnum == null || mapEnum.Behavior == EnumBehavior.ReferenceValue)
                 ? idcol
                 : namecol;
      }

      var candidates = mapping.Columns.Where(c => c.IsIdentity).ToArray();
      var candidateCount = candidates.Count();
      if (candidateCount == 0)
      {
        // Select any property called "ID" that is either an SyntheticID, int,
        // or long.
        return (from col in candidates
                where col.Member.Name == "ID"
                      && (col.RuntimeType == typeof(SyntheticID)
                          || col.RuntimeType == typeof(int)
                          || col.RuntimeType == typeof(long))
                select col).SingleOrDefault();
      }
      if (candidateCount == 1)
      {
        return candidates.Single();
      }
      // Try for a column called ID...
      var candidate = (from col in candidates
                       where col.Member.Name == "ID"
                       select col).SingleOrDefault();
      if (candidate == null)
      {
        // Try for first column ending in "ID", such as "MyTypeID"
        candidate = (from col in candidates
                     where col.Member.Name.EndsWith("ID")
                     select col).FirstOrDefault();
      }
      return candidate;
    }
  }
}