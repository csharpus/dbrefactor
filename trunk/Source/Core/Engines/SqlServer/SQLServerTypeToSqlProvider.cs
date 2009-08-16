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


namespace DbRefactor.Engines.SqlServer
{
	public interface ISqlTypes
	{
		string Binary();
		string BinaryValue(byte[] value);
		string Boolean();
		string BooleanValue(bool value);
		string DateTime();
		string DateTimeValue(DateTime dateTime);
		string Decimal(int precision, int radix);
		string DecimalValue(decimal value);
		string Double();
		string DoubleValue(double value);
		string Float();
		string FloatValue(float value);
		string Int();
		string IntValue(int value);
		string Long();
		string LongValue(long value);
		string String(int size);
		string StringValue(string value);
		string Text();
		string TextValue(string value);
		string NullValue();
	}

	internal sealed class SqlServerTypes : ISqlTypes
	{
		public string Binary()
		{
			return "varbinary(max)";
		}

		public string BinaryValue(byte[] value)
		{
			throw new NotSupportedException("Couldn't set default value for binary");
		}

		public string Boolean()
		{
			return "bit";
		}

		public string BooleanValue(bool value)
		{
			return value ? "1" : "0";
		}

		public string DateTime()
		{
			return "datetime";
		}

		public string DateTimeValue(DateTime dateTime)
		{
			return string.Format("'{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}'", dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour,
			                     dateTime.Minute, dateTime.Second);
		}

		public string Decimal(int precision, int radix)
		{
			return string.Format("decimal({0},{1})", precision, radix);
		}

		public string DecimalValue(decimal value)
		{
			return value.ToString();
		}

		public string Double()
		{
			return "float";
		}

		public string DoubleValue(double value)
		{
			return value.ToString();
		}

		public string Float()
		{
			return "real";
		}

		public string FloatValue(float value)
		{
			return value.ToString();
		}

		public string Int()
		{
			return "integer";
		}

		public string IntValue(int value)
		{
			return value.ToString();
		}

		public string Long()
		{
			return "bigint";
		}

		public string LongValue(long value)
		{
			return value.ToString();
		}

		public string String(int size)
		{
			return string.Format("varchar({0})", size);
		}

		public string StringValue(string value)
		{
			return string.Format("'{0}'", value.Replace("'", "''"));
		}

		public string Text()
		{
			return "text";
		}

		public string TextValue(string value)
		{
			return StringValue(value);
		}

		public string NullValue()
		{
			return "null";
		}
	}

	public interface IColumnProperties
	{
		string NotNull();
		string PrimaryKey(string name);
		string Unique(string name);
		string Identity();
		string Default(string value);
	}

	public class SqlServerColumnProperties : IColumnProperties
	{
		public string NotNull()
		{
			return "NOT NULL";
		}

		public string PrimaryKey(string name)
		{
			return String.Format("CONSTRAINT {0} PRIMARY KEY", name);
		}

		public string Unique(string name)
		{
			return String.Format("CONSTRAINT {0} UNIQUE", name);
		}

		public string Identity()
		{
			return "IDENTITY";
		}

		public string Default(string value)
		{
			return string.Format("DEFAULT {0}", value);
		}
	}
}