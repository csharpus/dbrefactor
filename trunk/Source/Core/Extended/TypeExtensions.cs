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

		public static NewTable Time(this NewTable newTable, string name)
		{
			newTable.AddColumn(new TimeProvider(name, null));
			return newTable;
		}

		public static NewTable Time(this NewTable newTable, string name, TimeSpan timeSpan)
		{
			newTable.AddColumn(new TimeProvider(name, timeSpan));
			return newTable;
		}
	}
}