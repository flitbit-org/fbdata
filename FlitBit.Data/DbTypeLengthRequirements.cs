using System;
using System.Diagnostics.Contracts;

namespace FlitBit.Data
{
	[Flags]
	public enum DbTypeLengthRequirements
	{
		None = 0,
		Length = 1,
		Precision = 1 << 1,
		Scale = 1 << 2,
		OptionalScale = Scale | 1 << 3,
		IndicatedByBrackets = 1 << 4,
		IndicatedByParenthesis = 1 << 5,
		ApproximationMapping = 0x08000000,
		LengthSpecifierMask = Length | Precision | OptionalScale,
	}

	public struct DbTypeDetails
	{
		string _name;
		string _bindingName;
		DbTypeLengthRequirements _lengthRequirements;
		int? _length;
		int? _scale;

		public DbTypeDetails(string name, string bindingName, DbTypeLengthRequirements lengthRequirements, int? len,
			int? scale)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(!lengthRequirements.HasFlag(DbTypeLengthRequirements.Length | DbTypeLengthRequirements.Scale) || len.HasValue);
			Contract.Requires<ArgumentNullException>(!lengthRequirements.HasFlag(DbTypeLengthRequirements.Scale) || (scale.HasValue || lengthRequirements.HasFlag(DbTypeLengthRequirements.OptionalScale)));
			_name = name;
			_bindingName = bindingName ?? name;
			_lengthRequirements = lengthRequirements;
			_length = len;
			_scale = scale;
		}

		public string BindingName { get { return _bindingName; } set { _bindingName = value; } }

		public string Name	{ get { return _name; } set { _name = value; } }

		public DbTypeLengthRequirements LengthRequirements { get { return this._lengthRequirements; } set { this._lengthRequirements = value; } }

		public int? Length { get { return this._length; } set { this._length = value; } }

		public int? Scale { get { return this._scale; } set { this._scale = value; } }
	}
}