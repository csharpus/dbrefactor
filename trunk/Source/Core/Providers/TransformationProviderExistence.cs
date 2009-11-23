using System;
using System.Linq;
using DbRefactor.Extensions;
using DbRefactor.Tools.DesignByContract;

namespace DbRefactor.Providers
{
	internal sealed partial class TransformationProvider
	{
		public bool UniqueExists(string name)
		{
			var filter = new ConstraintFilter {Name = name, ConstraintType = ConstraintType.Unique};
			return GetConstraints(filter).Any();
		}

		public bool ForeignKeyExists(string name)
		{
			var filter = new ForeignKeyFilter {Name = name};
			return GetForeignKeys(filter).Any();
		}

		public bool PrimaryKeyExists(string name)
		{
			var filter = new ConstraintFilter {Name = name, ConstraintType = ConstraintType.PrimaryKey};
			return GetConstraints(filter).Any();
		}

		public bool IndexExists(string name)
		{
			var filter = new IndexFilter {Name = name};
			return GetIndexes(filter).Any();
		}

		/// <summary>
		/// Determines if a table exists.
		/// </summary>
		/// <param name="table">Table name</param>
		/// <returns><c>true</c> if the constraint exists.</returns>
		public bool TableExists(string table)
		{
			return schemaProvider.TableExists(table);
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
			return schemaProvider.ColumnExists(table, column);
		}

		private bool ObjectExists(string name)
		{
			Check.RequireNonEmpty(name, "name");
			var query = String.Format("SELECT TOP 1 * FROM sysobjects WHERE id = object_id('{0}')", name);
			return ExecuteQuery(query).AsReadable().Any();
		}

		public bool IsIdentity(string table, string column)
		{
			return schemaProvider.IsIdentity(table, column);
		}

		public bool IsNullable(string table, string column)
		{
			return schemaProvider.IsNullable(table, column);
		}

		public bool TableHasIdentity(string table)
		{
			Check.RequireNonEmpty(table, "table");
			return
				Convert.ToInt32(ExecuteScalar("SELECT OBJECTPROPERTY(object_id('{0}'), 'TableHasIdentity')", table)) ==
				1;
		}

		

		public bool IsDefault(string table, string column)
		{
			var filter = new ConstraintFilter
			             	{
			             		TableName = table,
			             		ColumnNames = new[] {column},
			             		ConstraintType = ConstraintType.Default
			             	};
			return GetConstraints(filter).Any();
		}
	}
}