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
using System.IO;
using System.Linq;
using DbRefactor.Engines.SqlServer;
using DbRefactor.Providers;
using DbRefactor.Providers.Filters;
using DbRefactor.Providers.Model;

namespace DbRefactor.Tools
{
	public class DataDumper
	{
		private readonly IDatabaseEnvironment environment;
		private readonly TransformationProvider provider;
		private readonly SchemaHelper schemaHelper;
		private readonly bool insertGoStatement;

		internal DataDumper(IDatabaseEnvironment environment, TransformationProvider provider, SchemaHelper schemaHelper, bool insertGoStatement)
		{
			this.environment = environment;
			this.provider = provider;
			this.schemaHelper = schemaHelper;
			this.insertGoStatement = insertGoStatement;
		}

		private bool hasIdentity;
		private bool shouldDelete;
		private StringWriter writer;

		private void DeleteStatement(string table)
		{
			if (shouldDelete)
			{
				AddSql("delete from [{0}]", table);
			}
		}

		private void DisableIdentity(string table)
		{
			if (hasIdentity)
			{
				AddSql("set identity_insert [{0}] on", table);
			}
		}

		private void EnableIdentity(string table)
		{
			if (hasIdentity)
			{
				AddSql("set identity_insert [{0}] off", table);
			}
		}

		private void DisableConstraints(string table)
		{
			AddSql("alter table [{0}] nocheck constraint all", table);
		}

		private void EnableConstraints(string table)
		{
			AddSql("alter table [{0}] check constraint all", table);
		}

		private void CheckContraints(string table)
		{
			AddSql("dbcc checkconstraints ('{0}')", table);
		}

		private void EmptyLine()
		{
			writer.WriteLine();
		}

		public string GenerateDropStatement()
		{
			environment.OpenConnection();
			shouldDelete = true;
			writer = new StringWriter();
			var relations = GetRelations();
			List<string> tables = DependencySorter.Sort(schemaHelper.GetTables().ToList(), relations);

			DropConstraints(tables);
			foreach (string table in tables)
			{
				DropStatement(table);
			}
			environment.CloseConnection();
			return writer.ToString();
		}

		private IList<ForeignKey> GetRelations()
		{
			return schemaHelper.GetForeignKeys();
		}

		private void DropConstraints(IEnumerable<string> tables)
		{
			var constraints = tables
				.Select(t => schemaHelper.GetForeignKeys())
				.SelectMany(l => l)
				//.OrderBy(c => c.ConstraintType != ConstraintType.ForeignKey)
				.Select(c => new {c.Name, c.ForeignTable})
				.Distinct()
				.ToList();

			foreach (var constraint in constraints)
			{
				AddSql("alter table {0} drop constraint {1}", constraint.ForeignTable, constraint.Name);
				//AddSql("alter table {0} drop foreign key {1}", constraint.TableName, constraint.Name);
			}
		}

		private void DropStatement(string table)
		{
			AddSql("drop table [{0}]", table);
		}

		private void AddSql(string sql, params string[] arguments)
		{
			if (insertGoStatement)
			{
				sql += Environment.NewLine + "GO";
			}
			writer.WriteLine(sql, arguments);
		}

		public string GenerateDeleteStatement()
		{
			try
			{
				environment.OpenConnection();
				return GenerateDeleteStatementInternal();
			}
			finally
			{
				environment.CloseConnection();
			}
		}

		private string GenerateDeleteStatementInternal()
		{
			shouldDelete = true;
			writer = new StringWriter();
			var relations = GetRelations();
			List<string> tables =
				DependencySorter.Sort(schemaHelper.GetTables().ToList(), relations).Except(new[] {"SchemaInfo"}).ToList();
			//foreach (string table in tables)
			//{
			//    DisableConstraints(table);
			//}
			var nullableRelations = relations.Where(r => r.ForeignNullable).ToList();
			foreach (var relation in nullableRelations)
			{
				AddSql("update [{0}] set [{1}] = null", relation.ForeignTable,
				       relation.ForeignColumn);
			}
			foreach (string table in tables)
			{
				DeleteStatement(table);
			}
			return writer.ToString();
		}

		/// <param name="delete">Generate delete statement for all tables</param>
		public string Dump(bool delete)
		{
			try
			{
				environment.OpenConnection();
				return DumpInternal(delete);
			}
			finally
			{
				environment.CloseConnection();
			}
		}

		private string DumpInternal(bool delete)
		{
			shouldDelete = delete;
			writer = new StringWriter();
			var relations = GetRelations();
			List<string> tables = DependencySorter.Sort(schemaHelper.GetTables(), relations);
			foreach (string table in tables)
			{
				//DisableConstraints(table);
				//DeleteStatement(table);
			}
			tables.Reverse();
			foreach (string table in tables)
			{
				//Column[] columns = provider.GetColumns(table);
				var columnProviders = schemaHelper.GetColumns(new ColumnFilter { TableName = table });

				hasIdentity = schemaHelper.TableHasIdentity(table);
				using (IDataReader reader = provider.ExecuteQuery("select * from [{0}]", table))
				{
					//DisableIdentity(table);
					while (reader.Read())
					{
						var values = columnProviders.ToDictionary(c => c.Name, c => reader[c.Name]); // TODO: convert value from database to column provider data type
						var sql = new SqlServerCrudGenerator().GetInsertStatement(values, table);
						writer.WriteLine(sql);
						//writer.Write("\tinsert into [{0}] (", table);
						//foreach (ColumnProvider columnProvider in columnProviders)
						//{
						//    writer.Write("[{0}]", columnProvider.Name);
						//    if (columnProvider != columnProviders[columnProviders.Count - 1])
						//    {
						//        writer.Write(", ");
						//    }
						//}
						//writer.Write(") values (");
						//foreach (ColumnProvider columnProvider in columnProviders)
						//{
						//    writer.Write(GetValueSql(columnProvider, reader[columnProvider.Name]));

						//    if (columnProvider != columnProviders[columnProviders.Count - 1])
						//    {
						//        writer.Write(", ");
						//    }
						//}
						//writer.WriteLine(");");
					}
					//EnableIdentity(table);
					//EmptyLine();
				}
			}
			//foreach (string table in tables)
			//{
			//    EnableConstraints(table);
			//    CheckContraints(table);
			//}
			//EmptyLine();
			return writer.ToString();
		}

		public string Dump()
		{
			return Dump(false);
		}
	}
}