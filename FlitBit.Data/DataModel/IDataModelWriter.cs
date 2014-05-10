#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.Data.DataModel
{
  public interface IDataModelWriter<TDataModel>
  {
    string SelfName { get; }
    string SelfRef { get; }
    int[] ColumnOffsets { get; }
    string[] QuotedColumnNames { get; }
    DynamicSql Select { get; }
  }
}