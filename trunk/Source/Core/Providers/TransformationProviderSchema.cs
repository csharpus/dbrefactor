﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DbRefactor.Engines.SqlServer;
using DbRefactor.Exceptions;
using DbRefactor.Extensions;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Providers
{
	internal sealed partial class TransformationProvider
	{
		private ColumnProvider GetColumnProvider(string tableName, string columnName)
		{
			ColumnProvider provider;
			using (
				IDataReader reader =
					ExecuteQuery(
						"SELECT DATA_TYPE, COLUMN_NAME, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_PRECISION_RADIX, COLUMN_DEFAULT FROM information_schema.columns WHERE table_name = '{0}' and column_name = '{1}'",
						tableName, columnName))
			{
				if (!reader.Read())
				{
					throw new DbRefactorException(String.Format("Couldn't find column '{0}' in table '{1}'", columnName, tableName));
				}
				provider = GetProvider(reader);
			}
			AddProviderProperties(tableName, provider);
			return provider;
		}

		public string[] GetTables()
		{
			//const string query = "SELECT [name] FROM sysobjects WHERE xtype = 'U'";
			const string query = "SELECT [TABLE_NAME] AS [name] FROM information_schema.tables";
			return ExecuteQuery(query).AsReadable().Select(r => r.GetString(0)).ToArray();
		}

		private List<string> GetIndexes(string tableName, string columnName)
		{
			var filter = new IndexFilter { TableName = tableName, ColumnName = columnName };
			return GetIndexes(filter).Select(i => i.Name).ToList();
		}

		internal List<ColumnProvider> GetColumnProviders(string table)
		{
			var providers = new List<ColumnProvider>();

			using (
				IDataReader reader =
					ExecuteQuery(
						"SELECT DATA_TYPE, COLUMN_NAME, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_PRECISION_RADIX, COLUMN_DEFAULT FROM information_schema.columns WHERE table_name = '{0}';",
						table))
			{
				while (reader.Read())
				{
					ColumnProvider provider = GetProvider(reader);
					providers.Add(provider);
				}
			}
			foreach (var provider in providers)
			{
				AddProviderProperties(table, provider);
			}
			return providers;
		}

		private List<string> GetConstraintsByType(string table, string[] columns, string type)
		{
			var filter = new ConstraintFilter { TableName = table, ColumnNames = columns, ConstraintType = type };
			return GetConstraints(filter).Select(c => c.Name).ToList();
		}

		private List<DatabaseConstraint> GetConstraints(ConstraintFilter filter)
		{
			var query = new ConstraintQueryBuilder(filter).BuildQuery();
			return ExecuteQuery(query).AsReadable()
				.Select(r => new DatabaseConstraint
				{
					Name = r["ConstraintName"].ToString(),
					TableSchema = r["TableSchema"].ToString(),
					TableName = r["TableName"].ToString(),
					ColumnName = r["ColumnName"].ToString(),
					ConstraintType = r["ConstraintType"].ToString()
				}).ToList();
		}


		public List<string> GetConstraints(string table, string[] columns)
		{
			var filter = new ConstraintFilter { TableName = table, ColumnNames = columns };
			return GetConstraints(filter).Select(c => c.Name).Distinct().ToList();
		}

		public List<string> GetConstraintNames(string table)
		{
			var filter = new ConstraintFilter { TableName = table };
			return GetConstraints(filter).Select(c => c.Name).Distinct().ToList();
		}

		public List<DatabaseConstraint> GetConstraints(string table)
		{
			var filter = new ConstraintFilter { TableName = table };
			return GetConstraints(filter);
		}

		internal List<DatabaseConstraint> GetUniqueConstraints(string table, string[] columns)
		{
			var filter = new ConstraintFilter { TableName = table, ColumnNames = columns, ConstraintType = "UQ" };
			return GetConstraints(filter).ToList();
		}

		private List<ForeignKey> GetForeignKeys(ForeignKeyFilter filter)
		{
			var query = new ForeignKeyQueryBuilder(filter).BuildQuery();
			return ExecuteQuery(query).AsReadable()
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

		private List<Index> GetIndexes(IndexFilter filter)
		{
			var query = new IndexQueryBuilder(filter).BuildQuery();
			return ExecuteQuery(query).AsReadable()
				.Select(r => new Index
				{
					Name = r["Name"].ToString(),
					TableName = r["TableName"].ToString(),
					ColumnName = r["ColumnName"].ToString()
				}).ToList();
		}

		public List<ForeignKey> GetForeignKeys()
		{
			return GetForeignKeys(new ForeignKeyFilter());
		}

		//public List<string> SortTablesByDependency(List<string> tables, List<Relation> relations)
		//{
		//    return DependencySorter.Sort(tables, relations);
		//}
	}
}
