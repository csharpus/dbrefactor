using System;
using System.Collections.Generic;
using System.Linq;
using DbRefactor.Engines.SqlServer;
using DbRefactor.Extensions;
using StringListExtensions = DbRefactor.Extensions.StringListExtensions;

namespace DbRefactor.Providers
{
	public class SqlServerCrudGenerator : ICrudGenerator
	{
		public string GetInsertStatement(Dictionary<string, object> values, string table)
		{
			string valuesExpression = StringListExtensions.ComaSeparated(values.Select(p => ConvertToSql(p.Value)));
			string columnList = StringListExtensions.ComaSeparated(values.Select(v => Name(v.Key)));

			return string.Format("insert into {0} ({1}) values ({2})",
			                     Name(table),
			                     columnList,
			                     valuesExpression);
		}

		public string GetDeleteStatement(Dictionary<string, object> whereValues, string tableName)
		{
			var sqlWherePairs = whereValues.Select(v => EqualitySql(v.Key, ConvertToSql(v.Value)));

			string whereClause = String.Join(" and ", sqlWherePairs.ToArray());

			return String.Format("delete from {0} where {1}", Name(tableName), whereClause);
		}

		public string GetUpdateStatement(Dictionary<string, object> operationalValues, string tableName, Dictionary<string, object> whereValues)
		{
			var sqlUpdatePairs =
				operationalValues.Select(p => String.Format("[{0}] = {1}", p.Key, ConvertToSql(p.Value)));
			string operation = String.Join(", ", sqlUpdatePairs.ToArray());
			var sql = String.Format("update {0} set {1}", Name(tableName), operation);
			var sqlWherePairs = whereValues.Select(v => EqualitySql(v.Key, ConvertToSql(v.Value)));
			string whereClause = String.Join(" and ", sqlWherePairs.ToArray());
			if (whereClause != String.Empty)
			{
				sql += String.Format(" where {0}", whereClause);
			}
			return sql;
		}

		public string GetSelectStatement(string[] columns, string tableName, Dictionary<string, object> whereValues)
		{
			string query = String.Format("select {0} from {1}", columns.ComaSeparated(),
			                             Name(tableName));
			var sqlWherePairs = whereValues.Select(v => EqualitySql(v.Key, ConvertToSql(v.Value)));
			string whereClause = String.Join(" and ", sqlWherePairs.ToArray());
			if (whereClause != String.Empty)
			{
				query += String.Format(" where {0}", whereClause);
			}
			return query;
		}

		private string ConvertToSql(object value)
		{
			return new SqlServerTypeHelper().GetValueSql(value);
		}

		private string Name(string objectName)
		{
			return NameEncoderHelper.Encode(objectName);
		}

		private static string EqualitySql(string name, string valueSql)
		{
			string equalitySign = valueSql != "null" ? "=" : "is";
			return String.Format("[{0}] {1} {2}", name, equalitySign, valueSql);
		}
	}
}