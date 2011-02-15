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
using DbRefactor.Api;
using DbRefactor.Engines.SqlServer.Columns;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Extended
{
	public static class TypeExtensions
	{
		public static NewTable Bigint(this NewTable newTable, string name)
		{
			return newTable.Long(name);
		}

		public static NewTable Bigint(this NewTable newTable, string name, long defaultValue)
		{
			return newTable.Long(name, defaultValue);
		}

		public static NewTable Binary(this NewTable newTable, string name)
		{
			newTable.AddColumn(new BinaryProvider(name, null));
			return newTable;
		}

		public static NewTable Binary(this NewTable newTable, string name, byte[] defaultValue)
		{
			newTable.AddColumn(new BinaryProvider(name, defaultValue));
			return newTable;
		}

		public static NewTable Float(this NewTable newTable, string name)
		{
			newTable.AddColumn(new FloatProvider(name, null));
			return newTable;
		}

		public static NewTable Float(this NewTable newTable, string name, float defaultValue)
		{
			newTable.AddColumn(new FloatProvider(name, defaultValue));
			return newTable;
		}

		public static NewTable Guid(this NewTable newTable, string name)
		{
			newTable.AddColumn(new GuidProvider(name, null));
			return newTable;
		}

		public static NewTable Guid(this NewTable newTable, string name, Guid defaultValue)
		{
			newTable.AddColumn(new GuidProvider(name, defaultValue));
			return newTable;
		}

		public static NewTable Smallint(this NewTable newTable, string name)
		{
			newTable.AddColumn(new SmallintProvider(name, null));
			return newTable;
		}

		public static NewTable Smallint(this NewTable newTable, string name, Int16 defaultValue)
		{
			newTable.AddColumn(new SmallintProvider(name, defaultValue));
			return newTable;
		}

		public static NewTable Money(this NewTable newTable, string name)
		{
			newTable.AddColumn(new MoneyProvider(name, null));
			return newTable;
		}

		public static NewTable Money(this NewTable newTable, string name, decimal defaultValue)
		{
			newTable.AddColumn(new MoneyProvider(name, defaultValue));
			return newTable;
		}

		public static NewTable Double(this NewTable newTable, string name)
		{
			newTable.AddColumn(new DoubleProvider(name, null));
			return newTable;
		}

		public static NewTable Double(this NewTable newTable, string name, double defaultValue)
		{
			newTable.AddColumn(new DoubleProvider(name, defaultValue));
			return newTable;
		}

		public static NewTable DateTimeOffset(this NewTable newTable, string name)
		{
			newTable.AddColumn(new DateTimeOffsetProvider(name, null));
			return newTable;
		}

		public static NewTable DateTimeOffset(this NewTable newTable, string name, DateTimeOffset offset)
		{
			newTable.AddColumn(new DateTimeOffsetProvider(name, offset));
			return newTable;
		}

		public static NewTable Time(this NewTable newTable, string name, int? scale = null, TimeSpan? defaultValue = null)
		{
			newTable.AddColumn(new TimeProvider(name, scale, defaultValue != null ? defaultValue.Value : (TimeSpan?)null));
			return newTable;
		}

		public static NewTable Char(this NewTable newTable, string name, int size, string value = null, string collation = null)
		{
			newTable.AddColumn(new CharProvider(name, value, size, collation));
			return newTable;
		}

		public static NewTable Date(this NewTable newTable, string name, DateTime? defaultValue = null)
		{
			newTable.AddColumn(new DateProvider(name, defaultValue != null ? defaultValue.Value : (DateTime?)null));
			return newTable;
		}

		public static NewTable Smalldatetime(this NewTable newTable, string name, DateTime? defaultValue = null)
		{
			newTable.AddColumn(new SmalldatetimeProvider(name, defaultValue != null ? defaultValue.Value : (DateTime?)null));
			return newTable;
		}

		public static NewTable Image(this NewTable newTable, string name, byte[] defaultValue)
		{
			newTable.AddColumn(new ImageProvider(name, defaultValue));
			return newTable;
		}

		public static NewTable NChar(this NewTable newTable, string name, int size, string value = null)
		{
			newTable.AddColumn(new NCharProvider(name, value, size));
			return newTable;
		}

		public static NewTable Varchar(this NewTable newTable, string name, int size, string value = null, string collation = null)
		{
			newTable.AddColumn(new VarcharProvider(name, value, size, collation));
			return newTable;
		}

		public static NewTable NVarchar(this NewTable newTable, string name, int size, string value = null)
		{
			newTable.AddColumn(new NVarcharProvider(name, value, size));
			return newTable;
		}

		public static NewTable NText(this NewTable newTable, string name, string value = null)
		{
			newTable.AddColumn(new NTextProvider(name, value));
			return newTable;
		}

		public static NewTable Varbinary(this NewTable newTable, string name, int size, string value = null)
		{
			newTable.AddColumn(new VarbinaryProvider(name, value, size));
			return newTable;
		}

		public static NewTable Numeric(this NewTable newTable, string name, int precision, int scale, decimal? defaultValue = null)
		{
			newTable.AddColumn(new NumericProvider(name, defaultValue != null ? defaultValue.Value : (decimal?)null, precision, scale));
			return newTable;
		}

		public static NewTable Smallmoney(this NewTable newTable, string name, decimal? defaultValue = null)
		{
			newTable.AddColumn(new SmallmoneyProvider(name, defaultValue != null ? defaultValue.Value : (decimal?)null));
			return newTable;
		}

		public static NewTable Tinyint(this NewTable newTable, string name, byte? defaultValue = null)
		{
			newTable.AddColumn(new TinyintProvider(name, defaultValue != null ? defaultValue.Value : (byte?)null));
			return newTable;
		}

		public static NewTable Geography(this NewTable newTable, string name, string defaultValue = null)
		{
			newTable.AddColumn(new GeographyProvider(name, defaultValue));
			return newTable;
		}

		public static NewTable Geometry(this NewTable newTable, string name, string defaultValue = null)
		{
			newTable.AddColumn(new GeometryProvider(name, defaultValue));
			return newTable;
		}

		public static NewTable Xml(this NewTable newTable, string name, string defaultValue = null)
		{
			newTable.AddColumn(new XmlProvider(name, defaultValue));
			return newTable;
		}

		public static NewTable Timestamp(this NewTable newTable, string name)
		{
			newTable.AddColumn(new TimestampProvider(name));
			return newTable;
		}

		public static NewTable Datetime2(this NewTable newTable, string name, DateTime? defaultValue = null)
		{
			newTable.AddColumn(new Datetime2Provider(name, defaultValue));
			return newTable;
		}
	}
}