#region License

//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.

#endregion

using System;

namespace DbRefactor
{
	[Obsolete("This class will be removed in feature")]
	public class DecimalColumn: Column
	{
		private int _remainder;

		public DecimalColumn(string name, int size, int remainder)
			: base(name, typeof(decimal), size)
		{
			_remainder = remainder;
		}

		public DecimalColumn(string name, int size, int remainder, ColumnProperties property)
			:base(name, typeof(decimal), size, property)
		{
			_remainder = remainder;
		}

		public DecimalColumn(string name, int size, int remainder, decimal defaultValue)
			: base(name, typeof(decimal), size, ColumnProperties.NotNull, defaultValue)
		{
			_remainder = remainder;
		}

		public DecimalColumn(string name, int size, int remainder, ColumnProperties property, decimal defaultValue)
			: base(name, typeof(decimal), size, property, defaultValue)
		{
			_remainder = remainder;
		}

		public int Remainder
		{
			get { return _remainder; }
			set { _remainder = value; }
		}
	
	}
}