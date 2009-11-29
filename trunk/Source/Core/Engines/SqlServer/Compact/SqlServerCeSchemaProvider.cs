using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DbRefactor.Exceptions;
using DbRefactor.Extensions;
using DbRefactor.Providers;
using DbRefactor.Providers.Columns;
using DbRefactor.Tools.DesignByContract;

namespace DbRefactor.Engines.SqlServer.Compact
{
	internal class SqlServerCeSchemaProvider : SchemaProvider
	{
		private object increment;

		public SqlServerCeSchemaProvider(IDatabaseEnvironment databaseEnvironment,
		                                 ObjectNameService objectNameService,
		                                 SqlServerColumnMapper sqlServerColumnMapper)
			: base(databaseEnvironment, objectNameService, sqlServerColumnMapper)
		{
		}

		public override List<ForeignKey> GetForeignKeys(ForeignKeyFilter filter)
		{
			var query = new CeForeignKeyQueryBuilder(filter).BuildQuery();
			return DatabaseEnvironment.ExecuteQuery(query).AsReadable()
				.Select(r => new ForeignKey
				             	{
				             		Name = r["Name"].ToString(),
				             		ForeignTable = r["ForeignTable"].ToString(),
				             		ForeignColumn = r["ForeignColumn"].ToString(),
				             		PrimaryTable = r["PrimaryTable"].ToString(),
				             		PrimaryColumn = r["PrimaryColumn"].ToString(),
				             		ForeignNullable = r["ForeignNullable"].ToString() == "YES"
				             	}).ToList();
		}

		public override List<DatabaseConstraint> GetConstraints(ConstraintFilter filter)
		{
			var query = new CeConstraintQueryBuilder(filter).BuildQuery();
			return DatabaseEnvironment.ExecuteQuery(query).AsReadable()
				.Select(r => new DatabaseConstraint
				             	{
				             		Name = r["ConstraintName"].ToString(),
				             		TableSchema = r["TableSchema"].ToString(),
				             		TableName = r["TableName"].ToString(),
				             		ColumnName = r["ColumnName"].ToString(),
				             		ConstraintType = GetConstraintType(r["ConstraintType"].ToString())
				             	}).ToList();
		}

		public override List<Index> GetIndexes(IndexFilter filter)
		{
			var query = new CeIndexQueryBuilder(filter).BuildQuery();
			return DatabaseEnvironment.ExecuteQuery(query).AsReadable()
				.Select(r => new Index
				             	{
				             		Name = r["Name"].ToString(),
				             		TableName = r["TableName"].ToString(),
				             		ColumnName = r["ColumnName"].ToString()
				             	}).ToList();
		}

		public override bool IsNullable(string table, string column)
		{
			string value = DatabaseEnvironment.ExecuteScalar(
				String.Format(
					@"select IS_NULLABLE from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '{0}' and COLUMN_NAME = '{1}'",
					table, column)).ToString();
			return value == "YES";
		}

		public override bool IsIdentity(string table, string column)
		{
			increment = DatabaseEnvironment.ExecuteScalar(
				String.Format(
					@"select AUTOINC_INCREMENT from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '{0}' and COLUMN_NAME = '{1}'",
					table,
					column));
			int result;
			return Int32.TryParse(increment.ToString(), out result);
		}

		public override bool TableExists(string table)
		{
			return Convert.ToInt32(DatabaseEnvironment.ExecuteScalar(
				String.Format(
					@"
select count(*)
from INFORMATION_SCHEMA.TABLES 
where TABLE_NAME = '{0}' 
	and TABLE_TYPE = 'TABLE'
",
					table)
				)) > 0;
		}

		public override bool ColumnExists(string table, string column)
		{
			return Convert.ToInt32(DatabaseEnvironment.ExecuteScalar(
				String.Format(
					@"
select count(*)
from INFORMATION_SCHEMA.COLUMNS 
where TABLE_NAME = '{0}' 
	and COLUMN_NAME = '{1}'
",
					table, column)
				)) > 0;
		}

		protected override ColumnProvider GetProvider(IDataRecord reader)
		{
			var data = new ColumnData
			{
				Name = reader["COLUMN_NAME"].ToString(),
				DataType = reader["DATA_TYPE"].ToString(),
				Length = NullSafeGet<int>(reader, "CHARACTER_MAXIMUM_LENGTH"),
				Precision = NullSafeGet<int>(reader, "NUMERIC_PRECISION"),
				Scale = NullSafeGet<int>(reader, "NUMERIC_SCALE"),
				DefaultValue = GetDefaultValue(reader["COLUMN_DEFAULT"])
			};
			var typesMap = GetTypesMap();
			if (!typesMap.ContainsKey(data.DataType))
			{
				throw new DbRefactorException(String.Format("Could not find data type in map: '{0}'", data.DataType));
			}
			return typesMap[data.DataType](data);
		}

		public override void RenameColumn(string table, string oldColumnName, string newColumnName)
		{
			// Currently, sp_rename support in SQL Server Compact 3.5 is limited to tables.
			// http://technet.microsoft.com/en-us/library/bb726044.aspx
			throw new NotSupportedException();
		}

		public override void RenameTable(string oldName, string newName)
		{
			Check.RequireNonEmpty(oldName, "oldName");
			Check.RequireNonEmpty(newName, "newName");
			DatabaseEnvironment.ExecuteNonQuery(String.Format("EXEC sp_rename '{0}', '{1}', 'OBJECT'", oldName, newName));
		}

		public override bool IsDefault(string table, string column)
		{
			return Convert.ToBoolean(DatabaseEnvironment.ExecuteScalar(
				String.Format(
					@"
select COLUMN_HASDEFAULT
from INFORMATION_SCHEMA.COLUMNS 
where TABLE_NAME = '{0}' 
	and COLUMN_NAME = '{1}'
",
					table, column)
				));
		}

		public override string[] GetTables()
		{
			const string query = "select [TABLE_NAME] as [name] from information_schema.tables";
			return DatabaseEnvironment.ExecuteQuery(query).AsReadable().Select(r => r.GetString(0)).ToArray();
		}

		private static ConstraintType GetConstraintType(string typeSql)
		{
			if (!CeConstraintQueryBuilder.ConstraintTypeMap.ContainsValue(typeSql))
			{
				throw new DbRefactorException(String.Format("Unsupported constraint type: '{0}'", typeSql));
			}
			return CeConstraintQueryBuilder.ConstraintTypeMap.Single(p => p.Value == typeSql).Key;
		}
	}
}