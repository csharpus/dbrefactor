using System;
using System.Collections.Generic;
using System.Text;

namespace Migrator.Columns
{
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
