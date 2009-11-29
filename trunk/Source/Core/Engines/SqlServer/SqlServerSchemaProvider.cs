using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DbRefactor.Exceptions;
using DbRefactor.Extensions;
using DbRefactor.Providers;
using DbRefactor.Providers.Columns;
using DbRefactor.Tools.DesignByContract;

namespace DbRefactor.Engines.SqlServer
{
	internal class SqlServerSchemaProvider : SchemaProvider
	{
		public SqlServerSchemaProvider(IDatabaseEnvironment databaseEnvironment,
		                               ObjectNameService objectNameService,
		                               SqlServerColumnMapper sqlServerColumnMapper)
			: base(databaseEnvironment, objectNameService, sqlServerColumnMapper)
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

		private List<DatabaseConstraint> GetConstraints(ConstraintFilter filter)
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

		public override List<PrimaryKey> GetPrimaryKeys(PrimaryKeyFilter filter)
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

		public override List<Unique> GetUniques(UniqueFilter filter)
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
			object value = DatabaseEnvironment.ExecuteScalar(
				String.Format(
@"
select columnproperty(object_id('{0}'),'{1}','AllowsNull')
", table,
					column));
			return Convert.ToBoolean(value);
		}

		public override bool IsIdentity(string table, string column)
		{
			return
				Convert.ToBoolean(
					DatabaseEnvironment.ExecuteScalar(
						String.Format(
@"
select columnproperty(object_id('{0}'),'{1}','IsIdentity')
", table,
						              column)));
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
			return GetTypesMap()[data.DataType](data);
		}

		public override void RenameColumn(string table, string oldColumnName, string newColumnName)
		{
			Check.RequireNonEmpty(table, "table");
			Check.RequireNonEmpty(oldColumnName, "oldColumnName");
			Check.RequireNonEmpty(newColumnName, "newColumnName");
			DatabaseEnvironment.ExecuteNonQuery(String.Format("EXEC sp_rename '{0}.{1}', '{2}', 'COLUMN'", table,
			                                                  oldColumnName, newColumnName));
		}

		public override void RenameTable(string oldName, string newName)
		{
			Check.RequireNonEmpty(oldName, "oldName");
			Check.RequireNonEmpty(newName, "newName");
			DatabaseEnvironment.ExecuteNonQuery(String.Format("EXEC sp_rename '{0}', '{1}', 'OBJECT'", oldName,
			                                                  newName));
		}

		public override bool IsDefault(string table, string column)
		{
			var filter = new ConstraintFilter
			             	{
			             		TableName = table,
			             		ColumnNames = new[] {column},
			             		ConstraintType = ConstraintType.Default
			             	};
			return GetConstraints(filter).Any();
		}

		//public override string[] GetTables()
		//{
		//    const string query = "select [TABLE_NAME] as [name] from information_schema.tables";
		//    return DatabaseEnvironment.ExecuteQuery(query).AsReadable().Select(r => r.GetString(0)).ToArray();
		//}

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