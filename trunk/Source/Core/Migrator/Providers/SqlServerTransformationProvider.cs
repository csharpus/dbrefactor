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
using System.Collections;
using System.Data;

namespace Migrator.Providers
{
	/// <summary>
	/// Migration transformations provider for Microsoft SQL Server.
	/// </summary>
	public class SqlServerTransformationProvider : TransformationProvider
	{
		public SqlServerTransformationProvider(IDbConnection connection)
		{
			_connection = connection;

			//_connectionString = connectionString;
			//_connection = new SqlConnection();
			//_connection.ConnectionString = _connectionString;
			//_connection.Open();
		}
		
		public override void AddTable(string name, params Column[] columns)
		{
			if (TableExists(name))
			{
				Logger.Warn("The table {0} already exists", name);
				return;
			}
			
			string[] sqlColumns = new string[columns.Length];
			int i = 0;
			ArrayList pk = new ArrayList();
			
			foreach (Column col in columns)
			{
				sqlColumns[i] = GetSQLForColumn(col);
				
				if (col.ColumnProperty == ColumnProperties.PrimaryKey
				    || col.ColumnProperty == ColumnProperties.PrimaryKeyWithIdentity
				   )
				{
					pk.Add(col.Name);
				}
				
				i++;
			}
			
			ExecuteNonQuery( string.Format("CREATE TABLE {0} ({1})", name, string.Join(", ", sqlColumns)));
			
			if (pk.Count > 0)
				AddPrimaryKey(string.Format("PK_{0}", name), name, (string[]) pk.ToArray(typeof(string)));
		}
		
		public override void RemoveTable(string name)
		{
			if (TableExists(name))
				ExecuteNonQuery( string.Format("DROP TABLE {0}", name));
		}
		
		public override void AddPrimaryKey(string name, string table, params string[] columns)
		{
			if (ConstraintExists(name, table))
			{
				Logger.Warn("The primary key {0} already exists", name);
				return;
			}
			ExecuteNonQuery( string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY ({2}) ", table, name, string.Join(",",columns)));
		}
		
		public override void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable, string[] refColumns)
		{
			if (ConstraintExists(name, primaryTable))
			{
				Logger.Warn("The contraint {0} already exists", name);
				return;
			}
			ExecuteNonQuery( string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4})",
			        primaryTable, name, string.Join(",", primaryColumns),
			        refTable, string.Join(",", refColumns)));
		}

		public override void AddUnique(string name, string table, string[] columns)
		{
			if (ConstraintExists(name, table))
			{
				Logger.Warn("The contraint {0} already exists", name);
				return;
			}
			ExecuteNonQuery(string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} UNIQUE ({2})",
					table, name, string.Join(",", columns)));
		}
		
		public override void RemoveForeignKey(string name, string table)
		{
			if (TableExists(table) && ConstraintExists(name, table))
				ExecuteNonQuery( string.Format("ALTER TABLE {0} DROP CONSTRAINT {1}", table, name));
		}

		public override void RemoveUnique(string name, string table)
		{
			if (TableExists(table) && ConstraintExists(name, table))
				ExecuteNonQuery(string.Format("ALTER TABLE {0} DROP CONSTRAINT {1}", table, name));
		}
		
		public override bool ConstraintExists(string name, string table)
		{
			using (IDataReader reader =
			       ExecuteQuery( string.Format("SELECT TOP 1 * FROM sysobjects WHERE id = object_id('{0}')",
			                   name)))
			{
				return reader.Read();
			}
		}


						
		public override void AddColumn(string table, string column, Type type, int size, ColumnProperties property, object defaultValue)
		{
			if (ColumnExists(table, column))
			{
				Logger.Warn("The column {0}.{1} already exists", table, column);
				return;
			}
			
			string sqlColumn = GetSQLForColumn(new Column(column, type, size, property, defaultValue));
			string sqlPrimaryKey = String.Empty;

			if (property == ColumnProperties.PrimaryKeyWithIdentity)
			{
				sqlPrimaryKey = String.Format(" CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED ([{1}] ASC )", table, column);
			}
			ExecuteNonQuery( string.Format("ALTER TABLE {0} ADD {1}{2}", table, sqlColumn, sqlPrimaryKey));
		}

		public override void AddColumn(string table, Column column)
		{
			if (ColumnExists(table, column.Name))
			{
				Logger.Warn("The column {0}.{1} already exists", table, column.Name);
				return;
			}

			string sqlColumn = GetSQLForColumn(column);

			ExecuteNonQuery(string.Format("ALTER TABLE {0} ADD {1}", table, sqlColumn));
		}
		
		public override void RemoveColumn(string table, string column)
		{
			if (ColumnExists(table, column))
			{
				DeleteColumnConstraints(table, column);
				ExecuteNonQuery( string.Format("ALTER TABLE {0} DROP COLUMN {1} ", table, column));
			}
		}
		
		public override bool ColumnExists(string table, string column)
		{
			if (!TableExists(table))
				return false;
			
			using (IDataReader reader =
			       ExecuteQuery( string.Format("SELECT TOP 1 * FROM syscolumns WHERE id=object_id('{0}') and name='{1}'",
			                   table, column)))
			{
				return reader.Read();
			}
		}
		
		// Deletes all constraints linked to a column. Sql Server
		// doesn't seems to do this.
		public override void DeleteColumnConstraints(string table, string column)
		{
			string sqlContrainte = String.Format(@"with constraint_depends
as 
(
select c.TABLE_SCHEMA, c.TABLE_NAME, c.COLUMN_NAME, c.CONSTRAINT_NAME
  from INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE as c
 union all
select s.name, o.name, c.name, d.name
  from sys.default_constraints as d
  join sys.objects as o
    on o.object_id = d.parent_object_id
  join sys.columns as c
    on c.object_id = o.object_id and c.column_id = d.parent_column_id
  join sys.schemas as s
    on s.schema_id = o.schema_id
)
select c.CONSTRAINT_NAME
  from constraint_depends as c
 where c.TABLE_NAME = '{0}' and c.COLUMN_NAME = '{1}';",
			table, column);
			ExecuteNonQuery(sqlContrainte);

			ArrayList constraints = new ArrayList();

			using (IDataReader reader = ExecuteQuery(sqlContrainte))
			{
				while (reader.Read())
				{
					constraints.Add(reader.GetString(0));
				}
			}
			// Can't share the connection so two phase modif
			foreach (string constraint in constraints) {
			    RemoveForeignKey(constraint, table);
			}
			
		}
		
		public override bool TableExists(string table)
		{
			using (IDataReader reader =
			       ExecuteQuery(string.Format("SELECT TOP 1 * FROM syscolumns WHERE id=object_id('{0}')",
			                   table)))
			{
				return reader.Read();
			}
		}
		
		public override string[] GetTables()
		{
			ArrayList tables = new ArrayList();
		
			using (IDataReader reader =
				ExecuteQuery("SELECT name FROM sysobjects WHERE xtype = 'U'"))
			{
				while (reader.Read())
				{
					tables.Add(reader[0]);
				}
			}
		
			return (string[]) tables.ToArray(typeof (string));
		}
		
		public override Column[] GetColumns(string table)
		{
			ArrayList columns = new ArrayList();
			
			using (IDataReader reader = ExecuteQuery(string.Format("select COLUMN_NAME from information_schema.columns where table_name = '{0}';", table)))
			{
				while(reader.Read())
				{
					columns.Add(new Column(reader[0].ToString(), typeof(string)));
				}
			}
			
			return (Column[])columns.ToArray(typeof(Column));
		}
		
		#region Helper methods
		private string GetSQLForColumn(Column col)
		{
			string sqlType = ToSqlType(col.Type, col.Size, col.Scale);
			string sqlNull = col.ColumnProperty == ColumnProperties.Null ? "NULL" : "NOT NULL";
			string sqlDefault = "";
			string sqlIdentity = "";
			string sqlUnique = String.Empty;
			
			if (col.DefaultValue != null)
			{
				string sep = col.Type == typeof(string) ? "'" : "";
				sqlDefault = string.Format("DEFAULT {0}{1}{0}", sep, col.DefaultValue);
			}
			else if (col.Type == typeof(bool)) // Boolean must always have default value
			{
				sqlDefault = "DEFAULT (0)";
			}
			
			if (col.ColumnProperty == ColumnProperties.PrimaryKeyWithIdentity
			    || col.ColumnProperty == ColumnProperties.Identity)
			{
				sqlIdentity = string.Format("IDENTITY (1, 1)");
			}

			if (col.ColumnProperty == ColumnProperties.Unique)
			{
				sqlUnique = "CONSTRAINT ";
			}
			
			return string.Join(" ", new string[] { col.Name, sqlType, sqlIdentity, sqlNull, sqlDefault });
		}
				
		private string ToSqlType(Type type, int size, int scale)
		{
			if (type == typeof(string))
			{
				if (size <= 255)
					return string.Format("nvarchar({0})", size);
				else
					return "ntext";
			}
			else if (type == typeof(int))
			{
				if(size >= 8)
					return "bigint";
				else 
					return "int";
			}
			else if (type == typeof(float) || type == typeof(double))
			{
				if (size == 0)
					return "real";
				else
					return string.Format("float({0})", size);
			}
			else if (type == typeof(bool))
			{
				return "bit";
			}
			else if (type == typeof(DateTime))
			{
				return "datetime";
			}
			else if (type == typeof(char))
			{
				return string.Format("char({0})", size);
			}
			else if (type == typeof(Guid))
			{
				return "uniqueidentifier";
			}
			else if (type == typeof(TypeHelper._Decimal))
			{
				return String.Format("decimal({0}, {1})", size, scale);
			}
			else if (type == typeof(TypeHelper._Money))
			{
				return "money";
			}
			else if (type == typeof(TypeHelper._Text))
			{
				return "ntext";
			}
			else
			{
				throw new NotSupportedException("Type not supported : " + type.Name);
			}
		}
		#endregion
	}
}
