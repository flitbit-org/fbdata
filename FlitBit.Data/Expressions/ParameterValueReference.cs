using System;

namespace FlitBit.Data.Expressions
{
  public class ParameterValueReference : ValueReference
  {
    public ParameterValueReference(string name, int ordinal, Type type)
      : base(ValueReferenceKind.Parameter)
    {
      this.Name = name;
      this.Ordinal = ordinal;
      this.RuntimeType = type;
    }

    public Type RuntimeType { get; set; }

    public int Ordinal { get; set; }

    public string Name { get; set; }
  }
}