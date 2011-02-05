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
using System.Diagnostics.Contracts;
using DbRefactor.Exceptions;
using DbRefactor.Infrastructure;
using DbRefactor.Providers.Columns;


namespace DbRefactor.Providers
{
	public sealed class TransformationProvider
	{
		private readonly IDatabaseEnvironment environment;
		private readonly IDatabaseProvider databaseProvider;
		private readonly ICrudProvider crudProvider;

		internal TransformationProvider(IDatabaseEnvironment environment, IDatabaseProvider databaseProvider,
		                                ICrudProvider crudProvider)
		{
			this.environment = environment;
			this.databaseProvider = databaseProvider;
			this.crudProvider = crudProvider;
		}

		#region Table and column transformations

		public void CreateTable(string name, params ColumnProvider[] columns)
		{
			Contract.Requires(name != null);
			Contract.Requires(columns.Length > 0, "At least one column should be passed");
			databaseProvider.CreateTable(name, columns);
		}

		public void DropTable(string name)
		{
			Contract.Requires(name != null);
			databaseProvider.DropTable(name);
		}

		public void AddColumn(string table, ColumnProvider columnProvider)
		{
			databaseProvider.AddColumn(table, columnProvider);
		}

		public void DropColumn(string table, string column)
		{
			Contract.Requires(table != null);
			Contract.Requires(column != null);
			databaseProvider.DropColumn(table, column);
		}

		public void AlterColumn(string tableName, ColumnProvider columnProvider)
		{
			databaseProvider.AlterColumn(tableName, columnProvider);
		}

		public void RenameColumn(string table, string oldColumnName, string newColumnName)
		{
			Contract.Requires(table != null);
			Contract.Requires(oldColumnName != null);
			Contract.Requires(newColumnName != null);
			databaseProvider.RenameColumn(table, oldColumnName, newColumnName);
		}

		public void RenameTable(string oldName, string newName)
		{
			databaseProvider.RenameTable(oldName, newName);
		}

		#endregion

		#region Column properties transformation

		public void SetNull(string tableName, string columnName)
		{
			databaseProvider.SetNull(tableName, columnName);
		}

		public void SetNotNull(string tableName, string columnName)
		{
			databaseProvider.SetNotNull(tableName, columnName);
		}

		public void SetDefault(string constraintName, string tableName, string columnName, object value)
		{
			databaseProvider.SetDefault(constraintName, tableName, columnName, value);
		}

		public void DropDefault(string tableName, string columnName)
		{
			//var query = String.Format("alter table {0} alter column {1} drop default", 
			//	objectNameService.EncodeTable(tableName), columnName);
			//ExecuteQuery(query);

			databaseProvider.DropDefault(tableName, columnName);
			//throw new NotImplementedException();
		}

		public void DropForeignKey(string foreignKeyTable, string[] foreignKeyColumns, string primaryKeyTable,
		                           string[] primaryKeyColumns)
		{
			databaseProvider.DropForeignKey(foreignKeyTable, foreignKeyColumns, primaryKeyTable, primaryKeyColumns);
		}

		public void AddUnique(string name, string table, params string[] columns)
		{
			Contract.Requires(name != null);
			Contract.Requires(table != null);
			Contract.Requires(columns.Length > 0, "You have to pass at least one column");
			databaseProvider.AddUnique(name, table, columns);
		}

		public void AddIndex(string name, string table, params string[] columns)
		{
			Contract.Requires(name != null);
			Contract.Requires(table != null);
			Contract.Requires(columns.Length > 0, "You have to pass at least one column");
			databaseProvider.AddIndex(name, table, columns);
		}

		public void DropIndex(string table, params string[] columns)
		{
			Contract.Requires(table != null);
			Contract.Requires(columns.Length > 0, "You have to pass at least one column");
			databaseProvider.DropIndex(table, columns);
		}

		public void AddPrimaryKey(string name, string table, params string[] columns)
		{
			Contract.Requires(name != null);
			Contract.Requires(table != null);
			Contract.Requires(columns.Length > 0, "You have to pass at least one column");
			databaseProvider.AddPrimaryKey(name, table, columns);
		}

		public void AddForeignKey(string name, string primaryTable, string[] primaryColumns,
		                          string refTable, string[] refColumns, OnDelete constraint)
		{
			Contract.Requires(name != null);
			Contract.Requires(primaryTable != null);
			Contract.Requires(refTable != null);
			Contract.Requires(primaryColumns.Length > 0, "You have to pass at least one primary column");
			Contract.Requires(refColumns.Length > 0, "You have to pass at least one primary column");
			databaseProvider.AddForeignKey(name, primaryTable, primaryColumns, refTable, refColumns, constraint);
		}

		public void DropUnique(string table, params string[] columnNames)
		{
			Contract.Requires(table != null);

			databaseProvider.DropUnique(table, columnNames);
		}

		public void DropPrimaryKey(string table)
		{
			Contract.Requires(table != null);

			databaseProvider.DropPrimaryKey(table);
		}

		#endregion

		public int ExecuteNonQuery(string sql, params string[] values)
		{
			Contract.Requires(sql != null);
			return environment.ExecuteNonQuery(String.Format(sql, values));
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
			Contract.Requires(sql != null);
			return environment.ExecuteQuery(String.Format(sql, values));
		}

		public object ExecuteScalar(string sql, params string[] values)
		{
			return environment.ExecuteScalar(String.Format(sql, values));
		}

		public void Insert(string table, object insertObject)
		{
			crudProvider.Insert(table, ParametersHelper.GetPropertyValues(insertObject));
		}

		public IDataReader Select(string tableName, string[] columns, object whereParameters)
		{
			Dictionary<string, object> whereValues = ParametersHelper.GetPropertyValues(whereParameters);

			return crudProvider.Select(tableName, columns, whereValues);
		}

		public int Update(string tableName, object updateObject, object whereParameters)
		{
			var updateValues = ParametersHelper.GetPropertyValues(updateObject);
			Dictionary<string, object> whereValues = ParametersHelper.GetPropertyValues(whereParameters);
			return crudProvider.Update(tableName, updateValues, whereValues);
		}

		public int Delete(string tableName, object whereParameters)
		{
			Dictionary<string, object> whereValues = ParametersHelper.GetPropertyValues(whereParameters);
			if (whereValues.Count == 0)
			{
				throw new DbRefactorException("Couldn't execute delete without where clause");
			}
			return crudProvider.Delete(tableName, whereValues);
		}

		public object SelectScalar(string column, string tableName, object whereParameters)
		{
			Dictionary<string, object> whereValues = ParametersHelper.GetPropertyValues(whereParameters);
			var reader = crudProvider.Select(tableName, new[] {column}, whereValues);
			using (reader)
			{
				reader.Read();
				return reader[0];
			}
		}

		#endregion
	}
}