using System;
using System.IO;
using System.Text;
using FlitBit.Core;
using FlitBit.Data.SPI;

namespace FlitBit.Data.Cluster
{
  /// <summary>
  /// Utility class for writing data models to binary.
  /// </summary>
  public static class BinaryFormat
  {
    const byte IsNull = 0;
    const byte IsPresent = 1;

    static readonly Encoding Enc = Encoding.UTF8;

    public static byte[] CaptureBufferView<T>(this T item)
    {
      Type type = item.GetType();
      if (!typeof(IDataModel).IsAssignableFrom(type))
      {
        throw new InvalidOperationException("Type must implement IDataModel: " + type.GetReadableFullName());
      }
      using (var mem = new MemoryStream(400))
      using (var writer = new BinaryWriter(mem))
      {
        // Support polymorphism...
        var typeName = item.GetType().AssemblyQualifiedName;
        Write(writer, typeName);
        ((IDataModel)item).CaptureBufferView(writer);
        writer.Flush();
        return mem.ToArray();
      }
    }

    public static T RestoreBufferView<T>(byte[] buffer)
    {
      using (var mem = new MemoryStream(buffer))
      using (var reader = new BinaryReader(mem))
      {
        string typeName = null;
        Read(reader, ref typeName);
        Type type = null;
        if (typeName != null
            && (type = Type.GetType(typeName)) != null
            && typeof(IDataModel).IsAssignableFrom(type))
        {
          var res = (T)FactoryProvider.Factory.CreateInstance(type);
          ((IDataModel)res).RestoreBufferView(reader);
          return res;
        }
      }
      return default(T);
    }

    public static void Read(BinaryReader reader, ref string value)
    {
      var mark = reader.ReadByte();
      if (mark == IsNull)
      {
        value = null;
        return;
      }
      if (mark == IsPresent)
      {
        var len = reader.ReadInt32();
        var bytes = reader.ReadBytes(len);
        value = Enc.GetString(bytes);
      }
      else
        throw new FormatException(String.Concat("Encountered an invalid mark in the data model's binary formatted data: ", mark));
    }

    public static void Read(BinaryReader reader, ref Type value)
    {
      string typeName = null;
      Read(reader, ref typeName);
      if (typeName != null)
      {
        value = Type.GetType(typeName);
      }
    }

    public static void Read(BinaryReader reader, ref byte[] value)
    {
      var mark = reader.ReadByte();
      if (mark == IsNull)
      {
        value = null;
        return;
      }
      if (mark == IsPresent)
      {
        var len = reader.ReadInt32();
        value = reader.ReadBytes(len);
      }
      else
        throw new FormatException(String.Concat("Encountered an invalid mark in the data model's binary formatted data: ", mark));
    }

    public static void Read(BinaryReader reader, ref bool value) { value = reader.ReadBoolean(); }
    public static void Read(BinaryReader reader, ref byte value) { value = reader.ReadByte(); }
    public static void Read(BinaryReader reader, ref sbyte value) { value = reader.ReadSByte(); }
    public static void Read(BinaryReader reader, ref short value) { value = reader.ReadInt16(); }
    public static void Read(BinaryReader reader, ref int value) { value = reader.ReadInt32(); }
    public static void Read(BinaryReader reader, ref long value) { value = reader.ReadInt64(); }
    public static void Read(BinaryReader reader, ref float value) { value = reader.ReadSingle(); }
    public static void Read(BinaryReader reader, ref double value) { value = reader.ReadDouble(); }
    public static void Read(BinaryReader reader, ref DateTime value) { value = new DateTime(reader.ReadInt64()); }
    public static void Read(BinaryReader reader, ref decimal value) { value = reader.ReadDecimal(); }
    public static void Read(BinaryReader reader, ref Guid value) { value = new Guid(reader.ReadBytes(16)); }
    public static void Read(BinaryReader reader, ref ushort value) { value = reader.ReadUInt16(); }
    public static void Read(BinaryReader reader, ref uint value) { value = reader.ReadUInt32(); }
    public static void Read(BinaryReader reader, ref ulong value) { value = reader.ReadUInt64(); }

    public static void Read(BinaryReader reader, ref bool? value)
    {
      var mark = reader.ReadByte();
      if (mark == IsNull)
      {
        value = null;
        return;
      }
      if (mark == IsPresent)
      {
        value = reader.ReadBoolean();
      }
      else
        throw new FormatException(String.Concat("Encountered an invalid mark in the data model's binary formatted data: ", mark));
    }

    public static void Read(BinaryReader reader, ref byte? value)
    {
      var mark = reader.ReadByte();
      if (mark == IsNull)
      {
        value = null;
        return;
      }
      if (mark == IsPresent)
      {
        value = reader.ReadByte();
      }
      else
        throw new FormatException(String.Concat("Encountered an invalid mark in the data model's binary formatted data: ", mark));
    }

    public static void Read(BinaryReader reader, ref sbyte? value)
    {
      var mark = reader.ReadByte();
      if (mark == IsNull)
      {
        value = null;
        return;
      }
      if (mark == IsPresent)
      {
        value = reader.ReadSByte();
      }
      else
        throw new FormatException(String.Concat("Encountered an invalid mark in the data model's binary formatted data: ", mark));
    }

    public static void Read(BinaryReader reader, ref short? value)
    {
      var mark = reader.ReadByte();
      if (mark == IsNull)
      {
        value = null;
        return;
      }
      if (mark == IsPresent)
      {
        value = reader.ReadInt16();
      }
      else
        throw new FormatException(String.Concat("Encountered an invalid mark in the data model's binary formatted data: ", mark));
    }

    public static void Read(BinaryReader reader, ref int? value)
    {
      var mark = reader.ReadByte();
      if (mark == IsNull)
      {
        value = null;
        return;
      }
      if (mark == IsPresent)
      {
        value = reader.ReadInt32();
      }
      else
        throw new FormatException(String.Concat("Encountered an invalid mark in the data model's binary formatted data: ", mark));
    }

    public static void Read(BinaryReader reader, ref long? value)
    {
      var mark = reader.ReadByte();
      if (mark == IsNull)
      {
        value = null;
        return;
      }
      if (mark == IsPresent)
      {
        value = reader.ReadInt64();
      }
      else
        throw new FormatException(String.Concat("Encountered an invalid mark in the data model's binary formatted data: ", mark));
    }

    public static void Read(BinaryReader reader, ref float? value)
    {
      var mark = reader.ReadByte();
      if (mark == IsNull)
      {
        value = null;
        return;
      }
      if (mark == IsPresent)
      {
        value = reader.ReadSingle();
      }
      else
        throw new FormatException(String.Concat("Encountered an invalid mark in the data model's binary formatted data: ", mark));
    }

    public static void Read(BinaryReader reader, ref DateTime? value)
    {
      var mark = reader.ReadByte();
      if (mark == IsNull)
      {
        value = null;
        return;
      }
      if (mark == IsPresent)
      {
        var ticks = reader.ReadInt64();
        value = new DateTime(ticks);
      }
      else
        throw new FormatException(String.Concat("Encountered an invalid mark in the data model's binary formatted data: ", mark));
    }

    public static void Read(BinaryReader reader, ref double? value)
    {
      var mark = reader.ReadByte();
      if (mark == IsNull)
      {
        value = null;
        return;
      }
      if (mark == IsPresent)
      {
        value = reader.ReadDouble();
      }
      else
        throw new FormatException(String.Concat("Encountered an invalid mark in the data model's binary formatted data: ", mark));
    }

    public static void Read(BinaryReader reader, ref decimal? value)
    {
      var mark = reader.ReadByte();
      if (mark == IsNull)
      {
        value = null;
        return;
      }
      if (mark == IsPresent)
      {
        value = reader.ReadDecimal();
      }
      else
        throw new FormatException(String.Concat("Encountered an invalid mark in the data model's binary formatted data: ", mark));
    }

    public static void Read(BinaryReader reader, ref Guid? value)
    {
      var mark = reader.ReadByte();
      if (mark == IsNull)
      {
        value = null;
        return;
      }
      if (mark == IsPresent)
      {
        value = new Guid(reader.ReadBytes(16));
      }
      else
        throw new FormatException(String.Concat("Encountered an invalid mark in the data model's binary formatted data: ", mark));
    }

    public static void Read(BinaryReader reader, ref ushort? value)
    {
      var mark = reader.ReadByte();
      if (mark == IsNull)
      {
        value = null;
        return;
      }
      if (mark == IsPresent)
      {
        value = reader.ReadUInt16();
      }
      else
        throw new FormatException(String.Concat("Encountered an invalid mark in the data model's binary formatted data: ", mark));
    }

    public static void Read(BinaryReader reader, ref uint? value)
    {
      var mark = reader.ReadByte();
      if (mark == IsNull)
      {
        value = null;
        return;
      }
      if (mark == IsPresent)
      {
        value = reader.ReadUInt32();
      }
      else
        throw new FormatException(String.Concat("Encountered an invalid mark in the data model's binary formatted data: ", mark));
    }

    public static void Read(BinaryReader reader, ref ulong? value)
    {
      var mark = reader.ReadByte();
      if (mark == IsNull)
      {
        value = null;
        return;
      }
      if (mark == IsPresent)
      {
        value = reader.ReadUInt64();
      }
      else
        throw new FormatException(String.Concat("Encountered an invalid mark in the data model's binary formatted data: ", mark));
    }

    public static void Write(BinaryWriter writer, string value)
    {
      if (value == null)
      {
        writer.Write(IsNull);
      }
      else
      {
        writer.Write(IsPresent);
        var bytes = Enc.GetBytes(value);
        writer.Write(bytes.Length);
        writer.Write(bytes);
      }
    }

    public static void Write(BinaryWriter writer, byte[] value)
    {
      if (value == null)
      {
        writer.Write(IsNull);
      }
      else
      {
        writer.Write(IsPresent);
        writer.Write(value.Length);
        writer.Write(value);
      }
    }

    public static void Write(BinaryWriter writer, Type value)
    {
      if (value == null)
      {
        writer.Write(IsNull);
      }
      else
      {
        writer.Write(IsPresent);
        var bytes = Enc.GetBytes(value.FullName);
        writer.Write(bytes.Length);
        writer.Write(bytes);
      }
    }

    public static void Write(BinaryWriter writer, bool value) { writer.Write(value); }

    public static void Write(BinaryWriter writer, bool? value)
    {
      if (value.HasValue)
      {
        writer.Write(IsPresent);
        writer.Write(value.Value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }

    public static void Write(BinaryWriter writer, float value) { writer.Write(value); }

    public static void Write(BinaryWriter writer, float? value)
    {
      if (value.HasValue)
      {
        writer.Write(IsPresent);
        writer.Write(value.Value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }

    public static void Write(BinaryWriter writer, double value) { writer.Write(value); }

    public static void Write(BinaryWriter writer, double? value)
    {
      if (value.HasValue)
      {
        writer.Write(IsPresent);
        writer.Write(value.Value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }

    public static void Write(BinaryWriter writer, decimal value) { writer.Write(value); }

    public static void Write(BinaryWriter writer, decimal? value)
    {
      if (value.HasValue)
      {
        writer.Write(IsPresent);
        writer.Write(value.Value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }

    public static void Write(BinaryWriter writer, DateTime value) { writer.Write(value.Ticks); }

    public static void Write(BinaryWriter writer, DateTime? value)
    {
      if (value.HasValue)
      {
        writer.Write(IsPresent);
        Write(writer, value.Value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }

    public static void Write(BinaryWriter writer, byte value) { writer.Write(value); }

    public static void Write(BinaryWriter writer, byte? value)
    {
      if (value.HasValue)
      {
        writer.Write(IsPresent);
        writer.Write(value.Value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }

    public static void Write(BinaryWriter writer, short value) { writer.Write(value); }

    public static void Write(BinaryWriter writer, short? value)
    {
      if (value.HasValue)
      {
        writer.Write(IsPresent);
        writer.Write(value.Value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }

    public static void Write(BinaryWriter writer, int value) { writer.Write(value); }

    public static void Write(BinaryWriter writer, int? value)
    {
      if (value.HasValue)
      {
        writer.Write(IsPresent);
        writer.Write(value.Value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }

    public static void Write(BinaryWriter writer, long value) { writer.Write(value); }

    public static void Write(BinaryWriter writer, long? value)
    {
      if (value.HasValue)
      {
        writer.Write(IsPresent);
        writer.Write(value.Value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }

    public static void Write(BinaryWriter writer, sbyte value) { writer.Write(value); }

    public static void Write(BinaryWriter writer, sbyte? value)
    {
      if (value.HasValue)
      {
        writer.Write(IsPresent);
        writer.Write(value.Value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }

    public static void Write(BinaryWriter writer, ushort value) { writer.Write(value); }

    public static void Write(BinaryWriter writer, ushort? value)
    {
      if (value.HasValue)
      {
        writer.Write(IsPresent);
        writer.Write(value.Value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }

    public static void Write(BinaryWriter writer, uint value) { writer.Write(value); }

    public static void Write(BinaryWriter writer, uint? value)
    {
      if (value.HasValue)
      {
        writer.Write(IsPresent);
        writer.Write(value.Value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }

    public static void Write(BinaryWriter writer, ulong value) { writer.Write(value); }

    public static void Write(BinaryWriter writer, ulong? value)
    {
      if (value.HasValue)
      {
        writer.Write(IsPresent);
        writer.Write(value.Value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }


    public static void Write(BinaryWriter writer, Guid value) { writer.Write(value.ToByteArray()); }

    public static void Write(BinaryWriter writer, Guid? value)
    {
      if (value.HasValue)
      {
        writer.Write(IsPresent);
        Write(writer, value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }

   
    public static void Write(BinaryWriter writer, bool hasValue, byte value)
    {
      if (hasValue)
      {
        writer.Write(IsPresent);
        writer.Write(value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }

    public static void Write(BinaryWriter writer, bool hasValue, short value)
    {
      if (hasValue)
      {
        writer.Write(IsPresent);
        writer.Write(value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }

    public static void Write(BinaryWriter writer, bool hasValue, int value)
    {
      if (hasValue)
      {
        writer.Write(IsPresent);
        writer.Write(value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }

    public static void Write(BinaryWriter writer, bool hasValue, long value)
    {
      if (hasValue)
      {
        writer.Write(IsPresent);
        writer.Write(value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }

    public static void Write(BinaryWriter writer, bool hasValue, Guid value)
    {
      if (hasValue)
      {
        writer.Write(IsPresent);
        Write(writer, value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }

    public static void Write(BinaryWriter writer, bool hasValue, string value)
    {
      if (hasValue)
      {
        writer.Write(IsPresent);
        writer.Write(value);
      }
      else
      {
        writer.Write(IsNull);
      }
    }
  }
}
