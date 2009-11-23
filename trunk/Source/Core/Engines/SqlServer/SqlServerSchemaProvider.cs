using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DbRefactor.Exceptions;
using DbRefactor.Extensions;
using DbRefactor.Providers;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Engines.SqlServer
{
	internal class SqlServerSchemaProvider : SchemaProvider
	{
		public SqlServerSchemaProvider(IDatabaseEnvironment databaseEnvironment,
		                               ConstraintNameService constraintNameService,
		                               SqlServerColumnMapper sqlServerColumnMapper)
			: base(databaseEnvironment, constraintNameService, sqlServerColumnMapper)
		{
		}

		public override List<ForeignKey> GetForeignKeys(ForeignKeyFilter filter)
		{
			var query = new ForeignKeyQueryBuilder(filter).BuildQuery();
			return DatabaseEnvironment.ExecuteQuery(query).AsReadable()
				.Select(r => new ForeignKey
				             	{
				             		Name = r["Name"].ToString(),
				             		ForeignTable = r["ForeignTable"].ToString(),
				             		ForeignColumn = r["ForeignColumn"].ToString(),
				             		PrimaryTable = r["PrimaryTable"].ToString(),
				             		PrimaryColumn = r["PrimaryColumn"].ToString(),
				             		ForeignNullable = Convert.ToBoolean(r["ForeignNullable"])
				             	}).ToList();
		}

		public override List<DatabaseConstraint> GetConstraints(ConstraintFilter filter)
		{
			var query = new ConstraintQueryBuilder(filter).BuildQuery();
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
			var query = new IndexQueryBuilder(filter).BuildQuery();
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
			return Convert.ToBoolean(
				DatabaseEnvironment.ExecuteScalar(
					String.Format(
						@"
select columnproperty(object_id('{0}'),'{1}','AllowsNull')
", table,
						column)));
		}

		public override bool IsIdentity(string table, string column)
		{
			return
				Convert.ToBoolean(
					DatabaseEnvironment.ExecuteScalar(
						String.Format(@"
select columnproperty(object_id('{0}'),'{1}','IsIdentity')
", table,
						              column)));
		}

		public override bool TableExists(string table)
		{
			return
				Convert.ToBoolean(
					DatabaseEnvironment.ExecuteScalar(
						String.Format(
							@"
select exists(select top 1 * from dbo.sysobjects where id = object_id(N'{0}') and columnproperty(id, N'IsUserTable'))
",
							table)));
		}

		public override bool ColumnExists(string table, string column)
		{
			return
				Convert.ToBoolean(
					DatabaseEnvironment.ExecuteScalar(
						String.Format(
							@"
select exists(select top 1 * from syscolumns where id = object_id(N'{0}') and name = '{1}')
",
							table, column)));
		}

		protected override ColumnProvider GetProvider(IDataRecord reader)
		{
			var data = new ColumnData
			{
				Name = reader["COLUMN_NAME"].ToString(),
				DataType = reader["DATA_TYPE"].ToString(),
				Length = NullSafeGet<int>(reader, "CHARACTER_MAXIMUM_LENGTH"),
				Precision = NullSafeGet<byte>(reader, "NUMERIC_PRECISION"),
				Scale = NullSafeGet<byte>(reader, "NUMERIC_SCALE"),
				DefaultValue = GetDefaultValue(reader["COLUMN_DEFAULT"])
			};
			return GetTypesMap()[data.DataType](data);
		}

		private static ConstraintType GetConstraintType(string typeSql)
		{
			if (!ConstraintQueryBuilder.ConstraintTypeMap.ContainsValue(typeSql))
			{
				throw new DbRefactorException(String.Format("Unsupported constraint type: '{0}'", typeSql));
			}
			return ConstraintQueryBuilder.ConstraintTypeMap.Single(p => p.Value == typeSql).Key;
		}
	}
}