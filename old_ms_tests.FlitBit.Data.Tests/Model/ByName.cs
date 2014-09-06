using System;

namespace FlitBit.Data.Tests.Model
{
  class ByName
  {
    public ByName(string name) { Name = name; }
    public string Name { get; set; }

    public override string ToString() { return String.Concat("Name=", Name); }
  }
}
