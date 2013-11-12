using System;
using System.Diagnostics.Contracts;

namespace FlitBit.Data
{
	public struct DbTypeDetails
	{
		private string _bindingName;
		private int? _length;
		private string _name;
		private byte? _scale;

		public DbTypeDetails(string name, string bindingName, int? len,
			byte? scale)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			_name = name;
			_bindingName = bindingName ?? name;
			_length = len;
			_scale = scale;
		}

		public bool IsEmpty
		{
			get { return _name == null; }
		}

		public string BindingName
		{
			get { return _bindingName; }
		}

		public string Name
		{
			get { return _name; }
		}

		public int? Length
		{
			get { return _length; }
		}

		public byte? Scale
		{
			get { return _scale; }
		}
	}
}