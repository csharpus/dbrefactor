using System;
using System.Linq;
using DbRefactor.Engines.SqlServer;
using DbRefactor.Extensions;
using DbRefactor.Tools.DesignByContract;

namespace DbRefactor.Providers
{
	public sealed partial class TransformationProvider
	{
		public bool UniqueExists(string name)
		{
			var filter = new ConstraintFilter { Name = name, ConstraintType = "UQ" };
			return GetConstraints(filter).Any();
		}

		public bool ForeignKeyExists(string name)
		{
			var filter = new ForeignKeyFilter { Name = name };
			return GetForeignKeys(filter).Any();
		}

		public bool PrimaryKeyExists(string name)
		{
			var filter = new ConstraintFilter { Name = name, ConstraintType = "PK" };
			return GetConstraints(filter).Any();
		}

		public bool IndexExists(string name)
		{
			var filter = new IndexFilter { Name = name };
			return GetIndexes(filter).Any();
		}

		/// <summary>
		/// Determines if a table exists.
		/// </summary>
		/// <param name="table">Table name</param>
		/// <returns><c>true</c> if the constraint exists.</returns>
		public bool TableExists(string table)
		{
			return ObjectExists(table);
		}

		/// <summary>
		/// Determines if a constraint exists.
		/// </summary>
		/// <param name="name">Constraint name</param>
		/// <returns><c>true</c> if the constraint exists.</returns>
		public bool ConstraintExists(string name)
		{
			return ObjectExists(name);
		}

		public bool ColumnExists(string table, string column)
		{
			Check.RequireNonEmpty(table, "table");
			Check.RequireNonEmpty(column, "column");
			if (!TableExists(table))
			{
				Logger.Warn("Table {0} does not exists", table);
				return false;
			}
			var query = String.Format("SELECT TOP 1 * FROM syscolumns WHERE id = object_id('{0}') AND name = '{1}'",
									  table,
									  column);
			return ExecuteQuery(query).AsReadable().Any();
		}

		private bool ObjectExists(string name)
		{
			Check.RequireNonEmpty(name, "name");
			var query = String.Format("SELECT TOP 1 * FROM sysobjects WHERE id = object_id('{0}')", name);
			return ExecuteQuery(query).AsReadable().Any();
		}

		public bool IsIdentity(string table, string column)
		{
			return Convert.ToBoolean(ExecuteScalar(@"SELECT COLUMNPROPERTY(OBJECT_ID('{0}'),'{1}','IsIdentity')", table, column));
		}

		public bool IsNullable(string table, string column)
		{
			return Convert.ToBoolean(ExecuteScalar(@"SELECT COLUMNPROPERTY(OBJECT_ID('{0}'),'{1}','AllowsNull')", table, column));
		}

		public bool TableHasIdentity(string table)
		{
			Check.RequireNonEmpty(table, "table");
			return Convert.ToInt32(ExecuteScalar("SELECT OBJECTPROPERTY(object_id('{0}'), 'TableHasIdentity')", table)) == 1;
		}

		private bool IsPrimaryKey(string table, string column)
		{
			var filter = new ConstraintFilter { TableName = table, ColumnNames = new[] { column }, ConstraintType = "PK" };
			return GetConstraints(filter).Any();
		}

		private bool IsUnique(string table, string column)
		{
			var filter = new ConstraintFilter { TableName = table, ColumnNames = new[] { column }, ConstraintType = "UQ" };
			return GetConstraints(filter).Any();
		}

		public bool IsDefault(string table, string column)
		{
			var filter = new ConstraintFilter {TableName = table, ColumnNames = new[] {column}, ConstraintType = "DF"};
			return GetConstraints(filter).Any();
		}
	}
}
