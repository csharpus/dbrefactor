using System;
using System.Collections.Generic;
using System.Text;
using Migrator.Providers.ColumnPropertiesMappers;

namespace Migrator.Providers.TypeToSqlProviders
{
	public class SQLServerTypeToSqlProvider: ITypeToSqlProvider
	{

		#region ITypeToSqlProvider Members

		public IColumnPropertiesMapper PrimaryKey
		{
			get { return Integer; }
		}

		public IColumnPropertiesMapper Char(byte size)
		{
			return new ColumnPropertiesMapper(string.Format("nchar({0})", size));
		}

		public IColumnPropertiesMapper String(ushort size)
		{
			return new ColumnPropertiesMapper(string.Format("nvarchar({0})", size));
		}

		public IColumnPropertiesMapper Text
		{
			get { return new ColumnPropertiesMapper("ntext"); }
		}

		public IColumnPropertiesMapper LongText
		{
			get { return new ColumnPropertiesMapper("nvarchar(max)"); }
		}

		public IColumnPropertiesMapper Binary(byte size)
		{
			return new ColumnPropertiesMapper(string.Format("VARBINARY({0})", size));
		}

		public IColumnPropertiesMapper Blob
		{
			get { return new ColumnPropertiesMapper("image"); }
		}

		public IColumnPropertiesMapper LongBlob
		{
			get { return new ColumnPropertiesMapper("image"); }
		}

		public IColumnPropertiesMapper Integer
		{
			get { return new ColumnPropertiesMapper("int"); }
		}

		public IColumnPropertiesMapper Long
		{
			get { return new ColumnPropertiesMapper("bigint"); }
		}

		public IColumnPropertiesMapper Float
		{
			get { return new ColumnPropertiesMapper("real"); }
		}

		public IColumnPropertiesMapper Double
		{
			get { return new ColumnPropertiesMapper("float"); }
		}

		public IColumnPropertiesMapper Decimal(int whole)
		{
			return new ColumnPropertiesMapper(string.Format("numeric({0})", whole));
		}

		public IColumnPropertiesMapper Decimal(int whole, int part)
		{
			return new ColumnPropertiesMapper(string.Format("numeric({0}, {1})", whole, part));
		}

		public IColumnPropertiesMapper Bool
		{
			get
			{
				IColumnPropertiesMapper mapper = new ColumnPropertiesMapper("bit");
				mapper.Default("0");
				return mapper;
			}
		}

		public IColumnPropertiesMapper DateTime
		{
			get { return new ColumnPropertiesMapper("datetime"); }
		}

		#endregion

	}
}
