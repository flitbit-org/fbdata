using System;
using System.Diagnostics.Contracts;

namespace FlitBit.Data
{

	public struct DbTypeDetails
	{
		string _name;
		string _bindingName;
		int? _length;
		int? _scale;

		public DbTypeDetails(string name, string bindingName, int? len,
			int? scale)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			this._name = name;
			this._bindingName = bindingName ?? name;
			this._length = len;
			this._scale = scale;
		}

		public bool IsEmpty { get { return _name == null; } }

		public string BindingName { get { return this._bindingName; } set { this._bindingName = value; } }

		public string Name	{ get { return this._name; } set { this._name = value; } }

		public int? Length { get { return this._length; } set { this._length = value; } }

		public int? Scale { get { return this._scale; } set { this._scale = value; } }
	}
}