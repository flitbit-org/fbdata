#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Text;
using FlitBit.Data.DataModel;
using FlitBit.Data.Expressions;

namespace FlitBit.Data.Meta
{
    public class MappedSortColumn
    {
        public MappedSortColumn(ColumnMapping column, SortOrderKind kind, int ordinal)
        {
            Column = column;
            Kind = kind;
            Ordinal = ordinal;
        }

        public SortOrderKind Kind { get; set; }
        public ColumnMapping Column { get; set; }
        public int Ordinal { get; set; }

        public virtual void WriteConditions(IMapping mapping, SqlWriter writer, string refName, bool inverse)
        {
            if (!String.IsNullOrEmpty(refName))
            {
                writer.Append(refName).Append('.');
            }
            writer.Append(mapping.QuoteObjectName(Column.TargetName));
            if (inverse)
            {
                writer.Append(Kind == SortOrderKind.Asc ? " DESC" : " ASC");
            }
            else
            {
                writer.Append(Kind == SortOrderKind.Asc ? " ASC" : " DESC");
            }
        }

        public override string ToString()
        {
            var writer = new StringBuilder(40);
            writer.Append(Column.TargetName);
            writer.Append(Kind == SortOrderKind.Asc ? " ASC" : " DESC");
            return writer.ToString();
        }
    }
}