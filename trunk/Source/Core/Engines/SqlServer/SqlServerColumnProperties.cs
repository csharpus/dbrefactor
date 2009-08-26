using System;

namespace DbRefactor.Engines.SqlServer
{
	internal class SqlServerColumnProperties : IColumnProperties
	{
		public string NotNull()
		{
			return "NOT NULL";
		}

		public string PrimaryKey(string name)
		{
			return String.Format("CONSTRAINT {0} PRIMARY KEY", name);
		}

		public string Unique(string name)
		{
			return String.Format("CONSTRAINT {0} UNIQUE", name);
		}

		public string Identity()
		{
			return "IDENTITY";
		}

		public string Default(string value)
		{
			return string.Format("DEFAULT {0}", value);
		}
	}
}