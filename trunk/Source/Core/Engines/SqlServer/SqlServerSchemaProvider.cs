using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DbRefactor.Engines.SqlServer.QueryBuilders;
using DbRefactor.Exceptions;
using DbRefactor.Extensions;
using DbRefactor.Providers;
using DbRefactor.Providers.Columns;
using DbRefactor.Providers.Filters;
using DbRefactor.Providers.Model;

namespace DbRefactor.Engines.SqlServer
{
	public class SqlServerSchemaProvider : ISchemaProvider
	{
		private readonly IDatabaseEnvironment databaseEnvironment;

		public SqlServerSchemaProvider(IDatabaseEnvironment databaseEnvironment)
		{
			this.databaseEnvironment = databaseEnvironment;
		}

		protected IDatabaseEnvironment DatabaseEnvironment
		{
			get { return databaseEnvironment; }
		}

		public virtual bool TableHasIdentity(string table)
		{
			return
				Convert.ToInt32(
					databaseEnvironment.ExecuteScalar(string.Format("SELECT OBJECTPROPERTY(object_id('{0}'), 'TableHasIdentity')",
					                                                table))) ==
				1;
		}

		public virtual bool IsNullable(string table, string column)
		{
			object value = databaseEnvironment.ExecuteScalar(
				String.Format(
					@"
select columnproperty(object_id('{0}'),'{1}','AllowsNull')
", table,
					column));
			return Convert.ToBoolean(value);
		}

		public virtual bool IsDefault(string table, string column)
		{
			var filter = new ConstraintFilter
			             	{
			             		TableName = table,
			             		ColumnNames = new[] {column},
			             		ConstraintType = ConstraintType.Default
			             	};
			return GetConstraints(filter).Any();
		}

		public virtual bool IsIdentity(string table, string column)
		{
			return
				Convert.ToBoolean(
					databaseEnvironment.ExecuteScalar(
						String.Format(
							@"
select columnproperty(object_id('{0}'),'{1}','IsIdentity')
", table,
							column)));
		}

		public bool IsUnique(string table, string column)
		{
			return GetUnique(table, column) != null;
		}

		public Unique GetUnique(string table, string column)
		{
			return GetUniques(new UniqueFilter {TableName = table, ColumnNames = new[] {column}}).FirstOrDefault();
		}

		public virtual IList<string> GetTables(TableFilter filter)
		{
			var query = new TableQueryBuilder(filter).BuildQuery();
			return databaseEnvironment
				.ExecuteQuery(query)
				.AsReadable()
				.Select(r => r.GetString(0)).ToList();
		}

		public virtual IList<DatabaseConstraint> GetConstraints(ConstraintFilter filter)
		{
			var query = new ConstraintQueryBuilder(filter).BuildQuery();
			return databaseEnvironment.ExecuteQuery(query).AsReadable()
				.Select(r => new DatabaseConstraint
				             	{
				             		Name = r["ConstraintName"].ToString(),
				             		TableSchema = r["TableSchema"].ToString(),
				             		TableName = r["TableName"].ToString(),
				             		ColumnName = r["ColumnName"].ToString(),
				             		ConstraintType =
				             			GetConstraintType(r["ConstraintType"].ToString())
				             	}).ToList();
		}

		protected virtual ConstraintType GetConstraintType(string typeSql)
		{
			if (!ConstraintQueryBuilder.ConstraintTypeMap.ContainsValue(typeSql))
			{
				throw new DbRefactorException(String.Format("Unsupported constraint type: '{0}'", typeSql));
			}
			return ConstraintQueryBuilder.ConstraintTypeMap.Single(p => p.Value == typeSql).Key;
		}

		public virtual IList<ForeignKey> GetForeignKeys(ForeignKeyFilter filter)
		{
			var query = new ForeignKeyQueryBuilder(filter).BuildQuery();
			return databaseEnvironment.ExecuteQuery(query).AsReadable()
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

		public virtual IList<Unique> GetUniques(UniqueFilter filter)
		{
			return
				GetConstraints(new ConstraintFilter
				               	{
				               		TableName = filter.TableName,
				               		Name = filter.Name,
									ColumnNames = filter.ColumnNames,
				               		ConstraintType = ConstraintType.Unique
				               	})
					.Select(c => new Unique
					             	{
					             		ColumnName = c.ColumnName,
					             		Name = c.Name,
					             		TableName = c.TableName
					             	}).ToList();
		}

		public virtual IList<PrimaryKey> GetPrimaryKeys(PrimaryKeyFilter filter)
		{
			return
				GetConstraints(new ConstraintFilter
				               	{
				               		TableName = filter.TableName,
				               		Name = filter.Name,
									ColumnNames = filter.ColumnNames,
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

		public virtual IList<Index> GetIndexes(IndexFilter filter)
		{
			var query = new IndexQueryBuilder(filter).BuildQuery();
			return databaseEnvironment.ExecuteQuery(query).AsReadable()
				.Select(r => new Index
				             	{
				             		Name = r["Name"].ToString(),
				             		TableName = r["TableName"].ToString(),
				             		ColumnName = r["ColumnName"].ToString()
				             	}).ToList();
		}

		public virtual IList<ColumnProvider> GetColumns(ColumnFilter filter)
		{
			var providers = new List<ColumnProvider>();
			var query = new SqlServerColumnQueryBuilder(filter).BuildQuery();
			using (IDataReader reader = databaseEnvironment.ExecuteQuery(query))
			{
				while (reader.Read())
				{
					ColumnProvider provider = GetProvider(reader);
					providers.Add(provider);
				}
			}
			foreach (var provider in providers)
			{
				AddProviderProperties(filter.TableName, provider);
			}
			return providers;
		}

		private void AddProviderProperties(string table, ColumnProvider provider)
		{
			var primaryKey = GetPrimaryKey(table, provider.Name);
			if (primaryKey != null)
			{
				provider.AddPrimaryKey(primaryKey.Name);
			}
			else if (!IsNullable(table, provider.Name))
			{
				provider.AddNotNull();
			}

			if (IsIdentity(table, provider.Name))
			{
				provider.AddIdentity();
			}

			var unique = GetUnique(table, provider.Name);
			if (unique != null)
			{
				provider.AddUnique(unique.Name);
			}
		}

		// TODO: write test for GetPrimaryKey
		private PrimaryKey GetPrimaryKey(string table, string column)
		{
			return GetPrimaryKeys(new PrimaryKeyFilter
			                      	{
			                      		TableName = table,
			                      		ColumnNames = new[] {column}
			                      	}).FirstOrDefault();
		}

		public ColumnProvider GetProvider(IDataRecord reader)
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
			return CreateColumnProvider(data);
		}

		private ColumnProvider CreateColumnProvider(ColumnData data)
		{
			return new SqlServerTypeHelper().GetColumnProvider(data);
		}

		protected static T? NullSafeGet<T>(IDataRecord reader, string name)
			where T : struct
		{
			object value = reader[name];
			if (value == DBNull.Value)
			{
				return null;
			}
			return (T) Convert.ChangeType(value, typeof (T));
		}

		protected static object GetDefaultValue(object databaseValue)
		{
			return databaseValue == DBNull.Value ? null : databaseValue;
		}
	}
}