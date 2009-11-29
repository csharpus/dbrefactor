using System;
using DbRefactor.Providers;

namespace DbRefactor.Engines.MySql
{
	internal class MySqlObjectNameService : ObjectNameService
	{
		public override string EncodeTable(string table)
		{
			return FormatObjectName(table);
		}

		private static string FormatObjectName(string table)
		{
			return String.Format("`{0}`", table);
		}

		public override string EncodeColumn(string name)
		{
			return FormatObjectName(name);
		}
	}
}
