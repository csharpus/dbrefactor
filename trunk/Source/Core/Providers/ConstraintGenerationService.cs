using System;

namespace DbRefactor.Providers
{
	public class ConstraintNameService
	{
		public string UniqueName(string table, string column)
		{
			return String.Format("UQ_{0}_{1}", table, column);	
		}

		public string PrimaryKeyName(string table, string column)
		{
			return String.Format("PK_{0}_{1}", table, column);
		}
	}
}
