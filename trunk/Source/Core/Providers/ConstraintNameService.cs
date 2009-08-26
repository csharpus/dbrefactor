using System;
using System.Linq;

namespace DbRefactor.Providers
{
	internal class ConstraintNameService
	{
		public string UniqueName(string table, string column)
		{
			return String.Format("UQ_{0}_{1}", table, column);	
		}

		public string PrimaryKeyName(string table, string column)
		{
			return String.Format("PK_{0}_{1}", table, column);
		}

		public string PrimaryKeyName(string table, string[] columns)
		{
			string names = String.Join("_", columns.ToArray());
			return PrimaryKeyName(table, names);
		}

		public string UniqueName(string table, string[] columns)
		{
			return GenerateConstraintName("UQ", table, columns);
		}

		private string GenerateConstraintName(string prefix, string table, string[] columns)
		{
			string names = String.Join("_", columns);
			return String.Format("{0}_{1}_{2}", prefix, table, names);
		}

		//private string GenerateDefaultName()
		//{
		//    return GenerateConstraintName("DF");
		//}

		public string IndexName(string table, string[] columns)
		{
			return GenerateConstraintName("IX", table, columns);
		}

		public string DefaultName(string table, string[] columns)
		{
			return GenerateConstraintName("DF", table, columns);
		}

		public string ForeignKeyName(string foreignKeyTable, string primaryKeyTable)
		{
			return String.Format("FK_{0}_{1}", foreignKeyTable, primaryKeyTable);
		}
	}
}
