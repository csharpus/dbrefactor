using System.Collections.Generic;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Providers
{
	public interface ISchemaProvider
	{
		bool TableHasIdentity(string table);

		bool IsNullable(string table, string column);

		bool IsDefault(string table, string column);

		bool IsIdentity(string table, string column);

		bool IsUnique(string table, string column);

		IList<string> GetTables(TableFilter filter);

		IList<DatabaseConstraint> GetConstraints(ConstraintFilter filter);

		IList<ForeignKey> GetForeignKeys(ForeignKeyFilter filter);

		IList<Unique> GetUniques(UniqueFilter filter);

		IList<PrimaryKey> GetPrimaryKeys(PrimaryKeyFilter filter);

		IList<Index> GetIndexes(IndexFilter filter);

		IList<ColumnProvider> GetColumns(ColumnFilter filter);
	}
}