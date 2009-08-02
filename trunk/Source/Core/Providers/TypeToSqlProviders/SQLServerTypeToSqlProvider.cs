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
using DbRefactor.Providers.ColumnPropertiesMappers;

namespace DbRefactor.Providers.TypeToSqlProviders
{
	sealed class SqlServerTypeToSqlProvider
	{
		public ColumnPropertiesMapper Char(byte size)
		{
			return new ColumnPropertiesMapper(string.Format("nchar({0})", size));
		}

		public ColumnPropertiesMapper String(ushort size)
		{
			return new ColumnPropertiesMapper(string.Format("nvarchar({0})", size));
		}

		public ColumnPropertiesMapper Text
		{
			get { return new ColumnPropertiesMapper("ntext"); }
		}

		public ColumnPropertiesMapper LongText
		{
			get { return new ColumnPropertiesMapper("nvarchar(max)"); }
		}

		public ColumnPropertiesMapper Binary(byte size)
		{
			return new ColumnPropertiesMapper(string.Format("VARBINARY({0})", size));
		}

		public ColumnPropertiesMapper Blob
		{
			get { return new ColumnPropertiesMapper("image"); }
		}

		public ColumnPropertiesMapper LongBlob
		{
			get { return new ColumnPropertiesMapper("image"); }
		}

		public ColumnPropertiesMapper Integer
		{
			get { return new ColumnPropertiesMapper("int"); }
		}

		public ColumnPropertiesMapper Long
		{
			get { return new ColumnPropertiesMapper("bigint"); }
		}

		public ColumnPropertiesMapper Float
		{
			get { return new ColumnPropertiesMapper("real"); }
		}

		public ColumnPropertiesMapper Double
		{
			get { return new ColumnPropertiesMapper("float"); }
		}

		public ColumnPropertiesMapper Decimal(int whole)
		{
			return new ColumnPropertiesMapper(string.Format("numeric({0})", whole));
		}

		public ColumnPropertiesMapper Decimal(int whole, int part)
		{
			return new ColumnPropertiesMapper(string.Format("numeric({0}, {1})", whole, part));
		}

		public ColumnPropertiesMapper Bool
		{
			get
			{
				return new ColumnPropertiesMapper("bit");
			}
		}

		public ColumnPropertiesMapper DateTime
		{
			get { return new ColumnPropertiesMapper("datetime"); }
		}
	}

	public interface ISqlTypes
	{
		string Binary();
		string Boolean();
		string DateTime();
		string Decimal(int precision, int radix);
		string Double();
		string Float();
		string Int();
		string Long();
		string String(int size);
		string Text();
	}

	sealed class SqlServerTypes : ISqlTypes
	{
		public string Binary()
		{
			return "varbinary(max)";
		}

		public string Boolean()
		{
			return "bit";
		}

		public string DateTime()
		{
			return "datetime";
		}

		public string Decimal(int precision, int radix)
		{
			return string.Format("decimal({0},{1})", precision, radix);
		}

		public string Double()
		{
			return "double";
		}

		public string Float()
		{
			return "real";
		}

		public string Int()
		{
			return "integer";
		}

		public string Long()
		{
			return "long";
		}

		public string String(int size)
		{
			return string.Format("varchar({0})", size);
		}

		public string Text()
		{
			return "text";
		}
	}
}

