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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DbRefactor.Core;
using DbRefactor.Engines.SqlServer;
using DbRefactor.Exceptions;
using DbRefactor.Extensions;
using DbRefactor.Infrastructure;
using DbRefactor.Providers.Columns;
using DbRefactor.Tools.DesignByContract;
using System.Text.RegularExpressions;


namespace DbRefactor.Providers
{
	internal sealed partial class TransformationProvider
	{
		private readonly IDatabaseEnvironment environment;
		private readonly SchemaProvider schemaProvider;
		private readonly ObjectNameService objectNameService;

		internal TransformationProvider(IDatabaseEnvironment environment, SchemaProvider schemaProvider,
		                                ObjectNameService objectNameService)
		{
			this.environment = environment;
			this.schemaProvider = schemaProvider;
			this.objectNameService = objectNameService;
		}

		#region Table and column transformations

		public void CreateTable(string name, params ColumnProvider[] columns)
		{
			Check.RequireNonEmpty(name, "name");
			Check.Require(columns.Length > 0, "At least one column should be passed");
			var columnsSql = GetCreateColumnsSql(columns);
			ExecuteNonQuery("create table [{0}] ({1})", objectNameService.EncodeTable(name), columnsSql);
		}

		private static string GetCreateColumnsSql(IEnumerable<ColumnProvider> columns)
		{
			return columns.Select(col => col.GetCreateColumnSql())
				.WithTabsOnStart(2)
				.WithNewLinesOnStart()
				.ComaSeparated();
		}

		public void DropTable(string name)
		{
			Check.RequireNonEmpty(name, "name");
			ExecuteNonQuery("drop table [{0}]", 
				objectNameService.EncodeTable(name));
		}

		public void AddColumn(string table, ColumnProvider columnProvider)
		{
			ExecuteNonQuery("alter table [{0}] add {1}", 
				objectNameService.EncodeTable(table), columnProvider.GetAddColumnSql());
		}

		public void DropColumn(string table, string column)
		{
			Check.RequireNonEmpty(table, "table");
			Check.RequireNonEmpty(column, "column");
			DropColumnConstraints(table, column);
			ExecuteNonQuery("alter table [{0}] drop column {1}", 
				objectNameService.EncodeTable(table), 
				objectNameService.EncodeColumn(column));
		}

		private void AlterColumn(string table, string sqlColumn)
		{
			Check.RequireNonEmpty(table, "table");
			Check.RequireNonEmpty(sqlColumn, "sqlColumn");
			ExecuteNonQuery("alter table [{0}] alter column {1}", 
				objectNameService.EncodeTable(table),
				sqlColumn);
		}

		public void AlterColumn(string tableName, ColumnProvider columnProvider)
		{
			var provider = GetColumnProvider(tableName, columnProvider.Name);
			// TODO: IS it correct just copy properties?
			columnProvider.CopyPropertiesFrom(provider);
			AlterColumn(tableName, columnProvider.GetAlterColumnSql());
		}

		public void RenameColumn(string table, string oldColumnName, string newColumnName)
		{
			schemaProvider.RenameColumn(table, oldColumnName, newColumnName);
		}

		public void RenameTable(string oldName, string newName)
		{
			schemaProvider.RenameTable(oldName, newName);
		}

		#endregion

		#region Column properties transformation
		public void SetNull(string tableName, string columnName)
		{
			var provider = GetColumnProvider(tableName, columnName);
			provider.RemoveNotNull();
			AlterColumn(tableName, provider.GetAlterColumnSql());
		}

		public void SetNotNull(string tableName, string columnName)
		{
			var provider = GetColumnProvider(tableName, columnName);
			provider.AddNotNull();
			AlterColumn(tableName, provider.GetAlterColumnSql());
		}

		public void SetDefault(string constraintName, string tableName, string columnName, object value)
		{
			var provider = GetColumnProvider(tableName, columnName);
			provider.DefaultValue = value;
			var query = String.Format("alter table {0} add constraint {1} default {2} for [{3}]",
				objectNameService.EncodeTable(tableName),
				constraintName,
				provider.GetDefaultValueSql(), columnName);
			ExecuteNonQuery(query);
		}

		public void DropDefault(string tableName, string columnName)
		{
			var query = String.Format("alter table {0} alter column {1} drop default", 
				objectNameService.EncodeTable(tableName), columnName);
			ExecuteQuery(query);
			//List<string> defaultConstraints = GetConstraintsByType(tableName, new[] {columnName},
			//                                                       ConstraintType.Default);
			//foreach (var constraint in defaultConstraints)
			//{
			//    DropConstraint(tableName, constraint);
			//}
		}		

		public void DropForeignKey(string foreignKeyTable, string[] foreignKeyColumns, string primaryKeyTable,
								   string[] primaryKeyColumns)
		{
			if (foreignKeyColumns.Length != primaryKeyColumns.Length)
				throw new DbRefactorException(
					"The number of foreign key columns should be the same as the number of primary key columns");
			var filter = new ForeignKeyFilter
			{
				ForeignKeyTable = foreignKeyTable,
				ForeignKeyColumns = foreignKeyColumns,
				PrimaryKeyTable = primaryKeyTable,
				PrimaryKeyColumns = primaryKeyColumns
			};
			var foreignKeys = schemaProvider.GetForeignKeys(filter);
			var keysSharedBetweenAllColumns = foreignKeys.GroupBy(key => key.Name)
				.Where(group => !foreignKeyColumns.Except(group.Select(k => k.ForeignColumn)).Any())
				.Select(g => g.Key).ToList();
			foreach (var key in keysSharedBetweenAllColumns)
			{
				DropConstraint(foreignKeyTable, key);
			}
		}

		public void AddUnique(string name, string table, params string[] columns)
		{
			Check.RequireNonEmpty(name, "name");
			Check.RequireNonEmpty(table, "table");
			Check.Require(columns.Length > 0, "You have to pass at least one column");
			ExecuteNonQuery("alter table {0} add constraint {1} unique ({2}) ",
							objectNameService.EncodeTable(table), name, String.Join(",", columns));
		}

		public void AddIndex(string name, string table, params string[] columns)
		{
			Check.RequireNonEmpty(name, "name");
			Check.RequireNonEmpty(table, "table");
			Check.Require(columns.Length > 0, "You have to pass at least one column");
			ExecuteNonQuery("create nonclustered index {0} on {1} ({2}) ",
							name, objectNameService.EncodeTable(table), String.Join(",", columns));
		}

		public void DropIndex(string table, params string[] columns)
		{
			Check.RequireNonEmpty(table, "table");
			Check.Require(columns.Length > 0, "You have to pass at least one column");
			var indexesList = new List<List<string>>();
			foreach (var column in columns)
			{
				var indexes = GetIndexes(table, column);
				indexesList.Add(indexes);
			}
			var indexesPresentInAllColumns = GetIndexesPresentInAllColumns(indexesList);
			if (indexesPresentInAllColumns.Count == 0)
			{
				throw new DbRefactorException("Couldn't find any indexes mutual for all passed columns");
			}
			foreach (var index in indexesPresentInAllColumns)
			{
				ExecuteNonQuery("drop index {0}.{1}", table, index);
			}
		}

		private static List<string> GetIndexesPresentInAllColumns(IList<List<string>> indexesList)
		{
			var startIndexes = indexesList[0];
			foreach (var indexList in indexesList)
			{
				var indexesThatExists = indexList.Intersect(startIndexes).ToList();
				startIndexes = indexesThatExists;
			}
			return startIndexes;
		}

		public void AddPrimaryKey(string name, string table, params string[] columns)
		{
			Check.RequireNonEmpty(name, "name");
			Check.RequireNonEmpty(table, "table");
			Check.Require(columns.Length > 0, "You have to pass at least one column");
			ExecuteNonQuery("alter table {0} add constraint {1} primary key ({2}) ",
							objectNameService.EncodeTable(table), name, String.Join(",", columns));
		}

		public void AddForeignKey(string name, string primaryTable, string[] primaryColumns,
								  string refTable, string[] refColumns, OnDelete constraint)
		{
			Check.RequireNonEmpty(name, "name");
			Check.RequireNonEmpty(primaryTable, "primaryTable");
			Check.RequireNonEmpty(refTable, "refTable");
			Check.Require(primaryColumns.Length > 0, "You have to pass at least one primary column");
			Check.Require(refColumns.Length > 0, "You have to pass at least one ref column");
			ExecuteNonQuery(
@"
alter table {0} 
add constraint [{1}] 
foreign key ({2}) 
references {3} ({4}) 
	on delete {5}
",
				objectNameService.EncodeTable(primaryTable), name, String.Join(",", primaryColumns),
				objectNameService.EncodeTable(refTable), String.Join(",", refColumns),
				new SqlServerForeignKeyConstraintMapper().Resolve(constraint));
		}

		public void DropUnique(string table, params string[] columnNames)
		{
			Check.RequireNonEmpty(table, "table");

			var uniqueConstraints = GetUniqueConstraints(table, columnNames);
			var constraintsSharedBetweenAllColumns = uniqueConstraints.GroupBy(c => c.Name)
				.Where(group => !columnNames.Except(group.Select(c => c.ColumnName)).Any())
				.Select(g => g.Key).ToList();
			if (constraintsSharedBetweenAllColumns.Count == 0)
			{
				string message = columnNames.Length == 1
				                 	? String.Format(
				                 	  	"Could not find any unique constraints for column '{0}' in table '{1}'",
				                 	  	columnNames[0], table)
				                 	: String.Format(
				                 	  	"Could not find any mutual unique constraints for columns '{0}' in table '{1}'",
				                 	  	String.Join("', '", columnNames), table);
				throw new DbRefactorException(message);
			}
			foreach (var constraint in constraintsSharedBetweenAllColumns)
			{
				DropConstraint(table, constraint);
			}
		}

		public void DropPrimaryKey(string table)
		{
			Check.RequireNonEmpty(table, "table");

			string constraintName = GetPrimaryKeyConstraintName(table);

			DropConstraint(table, constraintName);
		}

		private string GetPrimaryKeyConstraintName(string table)
		{
			var keys = schemaProvider.GetPrimaryKeys(new PrimaryKeyFilter {TableName = table});
			if (keys.Count == 0)
				throw new DbRefactorException(String.Format("Could not find primary key constraint on table '{0}'",
				                                            table));
			return keys[0].Name;
		}

		#endregion

		private void DropColumnConstraints(string table, string column)
		{
			Check.RequireNonEmpty(table, "table");
			Check.RequireNonEmpty(column, "column");
			//var constraints = GetConstraints(table, new[] {column});
			//foreach (string constraint in constraints)
			//{
			//	DropConstraint(table, constraint);
			//}

			foreach (var key in schemaProvider.GetForeignKeys(new ForeignKeyFilter{ForeignKeyTable = table, ForeignKeyColumns = new [] {column}}))
			{
				DropForeignKey(key.ForeignTable, key.Name);
			}
		}

		private void DropConstraint(string table, string name)
		{
			Check.RequireNonEmpty(table, "table");
			Check.RequireNonEmpty(name, "name");

			ExecuteNonQuery("alter table [{0}] drop constraint [{1}]", objectNameService.EncodeTable(table), name);
		}

		private void DropForeignKey(string table, string name)
		{
			Check.RequireNonEmpty(table, "table");
			Check.RequireNonEmpty(name, "name");

			ExecuteNonQuery("alter table [{0}] drop constraint [{1}]", objectNameService.EncodeTable(table), name);
		}


		private const int GuidLength = 38; // 36 symbols in guid + 2 curly brackets


		public int ExecuteNonQuery(string sql, params string[] values)
		{
			Check.RequireNonEmpty(sql, "sql");
            if (values.Length > 0)
            {
                sql = String.Format(sql, values);
            }

            var commands = GetCommands(sql);
            foreach (var command in commands)
            {
                try
                {
                    environment.ExecuteNonQuery(command);
                }
                catch (Exception ex)
                {
                    environment.ExecuteNonQuery(command);
                    throw new Exception(String.Format("Command: {0}", command), ex);
                }
            }
            return 1;
		}

        /// <summary>
        /// Gets the sql commands.
        /// </summary>
        /// <param name="dataSql">The data SQL.</param>
        /// <returns>The list of SQL commands</returns>
        private static List<string> GetCommands(string dataSql)
        {
			var splitcommands = Regex.Split(dataSql, @"GO([\n\s]|$)");
            List<string> commandList = new List<string>(splitcommands);
            return commandList;
        }

        #region CRUD

		/// <summary>
		/// Execute an SQL query returning results.
		/// </summary>
		/// <param name="sql">The SQL command.</param>
		/// <param name="values">Replacements for {d} pattern.</param>
		/// <returns>A data iterator, <see cref="System.Data.IDataReader">IDataReader</see>.</returns>
		public IDataReader ExecuteQuery(string sql, params string[] values)
		{
			Check.RequireNonEmpty(sql, "sql");
			return environment.ExecuteQuery(String.Format(sql, values));
		}


		public object ExecuteScalar(string sql, params string[] values)
		{
			return environment.ExecuteScalar(String.Format(sql, values));
		}

		public int Insert(string table, object insertObject)
		{
			var providers = GetColumnProviders(table);
			string operation = GetValues(providers, insertObject);
			var columns = String.Join("], [", GetColumnNames(insertObject));

			return ExecuteNonQuery(
				"insert into {0} ([{1}]) values ({2})",
				objectNameService.EncodeTable(table),
				columns,
				operation);
		}

		public IDataReader Select(string tableName, string[] columns, object whereParameters)
		{
			string query = String.Format("select {0} from {1}", columns.ComaSeparated(), objectNameService.EncodeTable(tableName));
			var providers = GetColumnProviders(tableName);
			string whereClause = GetWhereClauseValues(providers, whereParameters);
			if (whereClause != String.Empty)
			{
				query += String.Format(" where {0}", whereClause);
			}
			return ExecuteQuery(query);
		}

		public int Update(string tableName, object updateObject, object whereParameters)
		{
			var providers = GetColumnProviders(tableName);
			string operation = GetOperationValues(providers, updateObject);
			var query = String.Format("update {0} set {1}", objectNameService.EncodeTable(tableName), operation);

			string whereClause = GetWhereClauseValues(providers, whereParameters);
			if (whereClause != String.Empty)
			{
				query += String.Format(" where {0}", whereClause);
			}
			return ExecuteNonQuery(query);
		}

		private static string[] GetColumnNames(object insertObject)
		{
			return ParametersHelper.GetPropertyValues(insertObject).Select(v => v.Key).ToArray();
		}

		private static string GetValues(IEnumerable<ColumnProvider> providers, object updateObject)
		{
			var updateValues = ParametersHelper.GetPropertyValues(updateObject);
			var sqlUpdatePairs = from p in providers
								 join v in updateValues on p.Name equals v.Key
								 select String.Format("{0}", p.GetValueSql(v.Value));
			return String.Join(", ", sqlUpdatePairs.ToArray());
		}

		private static string GetOperationValues(IEnumerable<ColumnProvider> providers, object updateObject)
		{
			var updateValues = ParametersHelper.GetPropertyValues(updateObject);
			var sqlUpdatePairs = from p in providers
								 join v in updateValues on p.Name equals v.Key
								 select String.Format("[{0}] = {1}", p.Name, p.GetValueSql(v.Value));
			return String.Join(", ", sqlUpdatePairs.ToArray());
		}

		private static string GetWhereClauseValues(IEnumerable<ColumnProvider> providers,
												   object whereParameters)
		{
			var whereValues = ParametersHelper.GetPropertyValues(whereParameters);
			var sqlWherePairs = from p in providers
								join v in whereValues on p.Name equals v.Key
								select EqualitySql(p.Name, p.GetValueSql(v.Value));
			return String.Join(" and ", sqlWherePairs.ToArray());
		}

		private static string EqualitySql(string name, string valueSql)
		{
			string equalitySign = valueSql != "null" ? "=" : "is";
			return String.Format("[{0}] {1} {2}", name, equalitySign, valueSql);
		}

		public int Delete(string tableName, object whereParameters)
		{
			var providers = GetColumnProviders(tableName);
			string whereClause = GetWhereClauseValues(providers, whereParameters);
			if (whereClause == String.Empty)
			{
				throw new DbRefactorException("Couldn't execute delete without where clause");
			}
			var query = String.Format("delete from {0} where {1}", objectNameService.EncodeTable(tableName), whereClause);
			return ExecuteNonQuery(query);
		}

		public object SelectScalar(string column, string tableName, object whereParameters)
		{
			var providers = GetColumnProviders(tableName);
			string whereClause = GetWhereClauseValues(providers, whereParameters);
			return ExecuteScalar("select {0} from {1} where {2}", column, objectNameService.EncodeTable(tableName), whereClause);
		}

		#endregion
	}
}