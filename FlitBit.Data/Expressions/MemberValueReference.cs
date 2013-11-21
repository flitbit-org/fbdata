using System.Reflection;

namespace FlitBit.Data.Expressions
{
  public class MemberValueReference : ValueReference
  {
    public MemberValueReference(MemberInfo member)
      : this(ValueReferenceKind.Member, member)
    {
    }

    public MemberValueReference(ValueReferenceKind kind, MemberInfo member)
      : base(kind)
    {
      this.Member = member;
    }

    public MemberInfo Member { get; private set; }
  }
}