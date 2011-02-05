using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DbRefactor.Providers;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Engines.SqlServer
{
	public class SqlServerTypeHelper
	{
		private static IEnumerable<Map> GetTypeMap()
		{
			return new List<Map>
			       	{
			       		new Map
			       			{
			       				Provider = typeof (BinaryProvider),
			       				GetSqlValue = v => { throw new NotImplementedException(); },
			       				GetSqlType = p => "varbinary(max)",
			       				SqlType = "varbinary",
			       				CreateProvider = d => new BinaryProvider(d.Name, d.DefaultValue),
								ClrType = typeof(byte[])
			       			},
			       		new Map
			       			{
			       				Provider = typeof (BooleanProvider),
			       				GetSqlValue = v => (bool) v ? "1" : "0",
			       				GetSqlType = p => "bit",
			       				SqlType = "bit",
			       				CreateProvider = d => new BooleanProvider(d.Name, d.DefaultValue),
								ClrType = typeof(bool)
			       			},
			       		new Map
			       			{
			       				Provider = typeof (DateTimeProvider),
			       				GetSqlValue = v =>
			       					{
			       						var dateTime = (DateTime) v;
			       						return string.Format("'{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}'", dateTime.Year, dateTime.Month,
			       						                     dateTime.Day, dateTime.Hour,
			       						                     dateTime.Minute, dateTime.Second);
			       					},
			       				GetSqlType = p => "datetime",
			       				SqlType = "datetime",
			       				CreateProvider = d => new DateTimeProvider(d.Name, d.DefaultValue),
								ClrType = typeof(DateTime)
			       			},
						new Map
			       			{
			       				Provider = typeof (DateTimeProvider),
			       				GetSqlValue = v =>
			       					{
			       						var dateTime = (DateTime) v;
			       						return string.Format("'{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}'", dateTime.Year, dateTime.Month,
			       						                     dateTime.Day, dateTime.Hour,
			       						                     dateTime.Minute, dateTime.Second);
			       					},
			       				GetSqlType = p => "datetime2",
			       				SqlType = "datetime2",
			       				CreateProvider = d => new DateTimeProvider(d.Name, d.DefaultValue),
								ClrType = typeof(DateTime)
			       			},
			       		new Map
			       			{
			       				Provider = typeof (StringProvider),
			       				GetSqlValue = v => string.Format("'{0}'", ((string) v).Replace("'", "''")),
			       				GetSqlType = p =>
			       					{
			       						var stringProvider = (StringProvider) p;
			       						string sizeString = stringProvider.Size != Max.Value ? stringProvider.Size.ToString() : "max";
			       						return string.Format("nvarchar({0})", sizeString);
			       					},
			       				SqlType = "nvarchar",
			       				CreateProvider = d =>
			       					{
			       						int length = d.Length == null ? 10 : d.Length.Value;
			       						return new StringProvider(d.Name, d.DefaultValue, length);
			       					},
								ClrType = typeof(string)
			       			},
			       		new Map
			       			{
			       				Provider = typeof (TextProvider),
			       				GetSqlValue = v => string.Format("'{0}'", ((string) v).Replace("'", "''")),
			       				GetSqlType = p => "text",
			       				SqlType = "text",
			       				CreateProvider = d => new TextProvider(d.Name, d.DefaultValue),
								ClrType = typeof(string)
			       			},
			       		new Map
			       			{
			       				Provider = typeof (DecimalProvider),
			       				GetSqlValue = v => Convert.ToString(v, CultureInfo.InvariantCulture),
			       				GetSqlType = p =>
			       					{
			       						var decimalProvider = (DecimalProvider) p;
			       						return string.Format("decimal({0},{1})", decimalProvider.Precision, decimalProvider.Scale);
			       					},
			       				SqlType = "decimal",
			       				CreateProvider = d => new DecimalProvider(d.Name, d.DefaultValue, d.Precision.Value, d.Scale.Value),
								ClrType = typeof(decimal)
			       			},
			       		new Map
			       			{
			       				Provider = typeof (IntProvider),
			       				GetSqlValue = v => Convert.ToString(v, CultureInfo.InvariantCulture),
			       				GetSqlType = p => "integer",
			       				SqlType = "int",
			       				CreateProvider = d => new IntProvider(d.Name, d.DefaultValue),
								ClrType = typeof(int)
			       			},
			       		new Map
			       			{
			       				Provider = typeof (LongProvider),
			       				GetSqlValue = v => Convert.ToString(v, CultureInfo.InvariantCulture),
			       				GetSqlType = p => "bigint",
			       				SqlType = "bigint",
			       				CreateProvider = d => new LongProvider(d.Name, d.DefaultValue),
								ClrType = typeof(long)
			       			},
			       		new Map
			       			{
			       				Provider = typeof (DoubleProvider),
			       				GetSqlValue = v => Convert.ToString(v, CultureInfo.InvariantCulture),
			       				GetSqlType = p => "float",
			       				SqlType = "float",
			       				CreateProvider = d => new DoubleProvider(d.Name, d.DefaultValue),
								ClrType = typeof(double)
			       			},
			       		new Map
			       			{
			       				Provider = typeof (FloatProvider),
			       				GetSqlValue = v => Convert.ToString(v, CultureInfo.InvariantCulture),
			       				GetSqlType = p => "real",
			       				SqlType = "real",
			       				CreateProvider = d => new FloatProvider(d.Name, d.DefaultValue),
								ClrType = typeof(float)
			       			},
						new Map
			       			{
			       				Provider = typeof (GuidProvider),
			       				GetSqlValue = v => Convert.ToString(v, CultureInfo.InvariantCulture),
			       				GetSqlType = p => "uniqueidentifier",
			       				SqlType = "uniqueidentifier",
			       				CreateProvider = d => new GuidProvider(d.Name, d.DefaultValue),
								ClrType = typeof(Guid)
			       			},
						new Map
			       			{
			       				Provider = typeof (MoneyProvider),
			       				GetSqlValue = v => Convert.ToString(v, CultureInfo.InvariantCulture),
			       				GetSqlType = p => "money",
			       				SqlType = "money",
			       				CreateProvider = d => new MoneyProvider(d.Name, d.DefaultValue),
								ClrType = typeof(decimal)
			       			},
					new Map
			       			{
			       				Provider = typeof (SmallintProvider),
			       				GetSqlValue = v => Convert.ToString(v, CultureInfo.InvariantCulture),
			       				GetSqlType = p => "smallint",
			       				SqlType = "smallint",
			       				CreateProvider = d => new SmallintProvider(d.Name, d.DefaultValue),
								ClrType = typeof(Int16)
			       			}
			       	};
		}

		public string GetValueSql(object value)
		{
			if (value == DBNull.Value) return "null";
			if (value == null) return "null";
			var map = GetTypeMap().FirstOrDefault(m => m.ClrType == value.GetType());
			if (map == null)
			{
				throw new NotSupportedException(String.Format("Can not find map for type: {0}. This type is not supported", value.GetType()));
			}
			return map.GetSqlValue(value);
		}

		public string GetValueSql(ColumnProvider provider, object value)
		{
			var map = GetTypeMap()
				.FirstOrDefault(m => m.Provider == provider.GetType());
			if (map == null)
			{
				throw new NotSupportedException(String.Format("Can not find map for column: {0}", provider));
			}
			return map.GetSqlValue(value);
		}

		public string GetSqlType(ColumnProvider provider)
		{
			var map = GetTypeMap().FirstOrDefault(m => m.Provider == provider.GetType());
			if (map == null)
			{
				throw new NotSupportedException(String.Format("Can not find map for column: {0}", provider));
			}
			return map.GetSqlType(provider);
		}

		public ColumnProvider GetColumnProvider(ColumnData columnData)
		{
			var map = GetTypeMap().FirstOrDefault(m => m.SqlType == columnData.DataType);
			if (map == null)
			{
				throw new NotSupportedException(String.Format("Can not find map for type: {0}", columnData.DataType));
			}
			return map.CreateProvider(columnData);
		}
	}


	//class SqlServerCeTypes : SqlServerTypes
	//{
	//    public override string Text()
	//    {
	//        return String(Max.Value);
	//    }

	//    protected override string GetMaxValueString()
	//    {
	//        const int maxAllowedNvarcharLengthInSqlServerCe35 = 4000;
	//        return maxAllowedNvarcharLengthInSqlServerCe35.ToString();
	//    }
	//}


	//return new Dictionary<string, Func<ColumnData, ColumnProvider>>
	//                {
	//                    {"bigint", sqlServerColumnMapper.CreateLong},
	//                    {"binary", sqlServerColumnMapper.CreateBinary},
	//                    {"bit", sqlServerColumnMapper.CreateBoolean},
	//                    {"char", sqlServerColumnMapper.CreateString},
	//                    {"datetime", sqlServerColumnMapper.CreateDateTime},
	//                    {"decimal", sqlServerColumnMapper.CreateDecimal},
	//                    {"float", sqlServerColumnMapper.CreateFloat},
	//                    {"image", sqlServerColumnMapper.CreateBinary},
	//                    {"int", sqlServerColumnMapper.CreateInt},
	//                    {"money", sqlServerColumnMapper.CreateDecimal},
	//                    {"nchar", sqlServerColumnMapper.CreateString},
	//                    {"ntext", sqlServerColumnMapper.CreateText},
	//                    {"numeric", sqlServerColumnMapper.CreateDecimal},
	//                    {"nvarchar", sqlServerColumnMapper.CreateString},
	//                    {"real", sqlServerColumnMapper.CreateFloat},
	//                    {"smalldatetime", sqlServerColumnMapper.CreateDateTime},
	//                    {"smallint", sqlServerColumnMapper.CreateInt},
	//                    {"smallmoney", sqlServerColumnMapper.CreateDecimal},
	//                    {"sql_variant", sqlServerColumnMapper.CreateBinary},
	//                    {"text", sqlServerColumnMapper.CreateText},
	//                    {"timestamp", sqlServerColumnMapper.CreateDateTime},
	//                    {"tinyint", sqlServerColumnMapper.CreateInt},
	//                    {"uniqueidentifier", sqlServerColumnMapper.CreateString},
	//                    {"varbinary", sqlServerColumnMapper.CreateBinary},
	//                    {"varchar", sqlServerColumnMapper.CreateString},
	//                    {"xml", sqlServerColumnMapper.CreateString}
	//                };


	// private const int GuidLength = 38; // 36 symbols in guid + 2 curly brackets

	public class Map
	{
		public Type Provider { get; set; }

		public Func<object, string> GetSqlValue { get; set; }

		public Func<ColumnProvider, string> GetSqlType { get; set; }

		public string SqlType { get; set; }

		public Func<ColumnData, ColumnProvider> CreateProvider { get; set; }

		public Type ClrType { get; set; }
	}
}