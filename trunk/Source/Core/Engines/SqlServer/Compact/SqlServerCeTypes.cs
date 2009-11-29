namespace DbRefactor.Engines.SqlServer.Compact
{
	class SqlServerCeTypes : SqlServerTypes
	{
		public override string Text()
		{
			return String(Max.Value);
		}

		protected override string GetMaxValueString()
		{
			const int maxAllowedNvarcharLengthInSqlServerCe35 = 4000;
			return maxAllowedNvarcharLengthInSqlServerCe35.ToString();
		}
	}
}
