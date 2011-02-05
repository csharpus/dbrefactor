using System;
using System.Collections.Generic;
using System.Linq;
using DbRefactor.Exceptions;
using DbRefactor.Extensions;
using DbRefactor.Providers;
using DbRefactor.Providers.Filters;
using DbRefactor.Providers.Model;

namespace DbRefactor.Engines.SqlServer.Compact
{
	public class SqlServerCeSchemaProvider : SqlServerSchemaProvider
	{
		public SqlServerCeSchemaProvider(IDatabaseEnvironment databaseEnvironment)
			: base(databaseEnvironment)
		{
		}

		public override IList<ForeignKey> GetForeignKeys(ForeignKeyFilter filter)
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

		public override IList<Unique> GetUniques(UniqueFilter filter)
		{
			return
				GetConstraints(new ConstraintFilter
				               	{
				               		TableName = filter.TableName,
				               		Name = filter.Name,
				               		ConstraintType = ConstraintType.Unique
				               	})
					.Select(c => new Unique
					             	{
					             		ColumnName = c.ColumnName,
					             		Name = c.Name,
					             		TableName = c.TableName
					             	}).ToList();
		}

		public override IList<DatabaseConstraint> GetConstraints(ConstraintFilter filter)
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

		public override IList<Index> GetIndexes(IndexFilter filter)
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
					@"
select IS_NULLABLE 
from INFORMATION_SCHEMA.COLUMNS 
where TABLE_NAME = '{0}' 
	and COLUMN_NAME = '{1}'
",
					table, column)).ToString();
			return value == "YES";
		}

		public override bool IsIdentity(string table, string column)
		{
			var increment = DatabaseEnvironment.ExecuteScalar(
				String.Format(
					@"
select AUTOINC_INCREMENT 
from INFORMATION_SCHEMA.COLUMNS 
where TABLE_NAME = '{0}' 
	and COLUMN_NAME = '{1}'
",
					table,
					column));
			int result;
			return Int32.TryParse(increment.ToString(), out result);
		}

		public override IList<PrimaryKey> GetPrimaryKeys(PrimaryKeyFilter filter)
		{
			return
				GetConstraints(new ConstraintFilter
				               	{
				               		TableName = filter.TableName,
				               		Name = filter.Name,
				               		ConstraintType = ConstraintType.PrimaryKey
				               	})
					.GroupBy(c => c.TableName)
					.Select(c => new PrimaryKey
					             	{
					             		ColumnNames = c.Select(g => g.ColumnName).ToArray(),
					             		Name = c.First().Name,
					             		TableName = c.Key
					             	}).ToList();
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

		protected override ConstraintType GetConstraintType(string typeSql)
		{
			if (!CeConstraintQueryBuilder.ConstraintTypeMap.ContainsValue(typeSql))
			{
				throw new DbRefactorException(String.Format("Unsupported constraint type: '{0}'", typeSql));
			}
			return CeConstraintQueryBuilder.ConstraintTypeMap.Single(p => p.Value == typeSql).Key;
		}
	}
}