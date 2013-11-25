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