using System;

namespace DbRefactor.Engines.SqlServer
{
	internal class SqlServerTypes : ISqlTypes
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
			return string.Format("'{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}'", dateTime.Year, dateTime.Month,
			                     dateTime.Day, dateTime.Hour,
			                     dateTime.Minute, dateTime.Second);
		}

		public string Decimal(int precision, int scale)
		{
			return string.Format("decimal({0},{1})", precision, scale);
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

		public virtual string String(int size)
		{
			string sizeString = size != Max.Value ? size.ToString() : GetMaxValueString();
			return string.Format("nvarchar({0})", sizeString);
		}

		protected virtual string GetMaxValueString()
		{
			return "max";
		}

		public string StringValue(string value)
		{
			return string.Format("'{0}'", value.Replace("'", "''"));
		}

		public virtual string Text()
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
}