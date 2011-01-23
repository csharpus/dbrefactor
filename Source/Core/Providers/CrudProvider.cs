using System.Collections.Generic;
using System.Data;

namespace DbRefactor.Providers
{
	public class CrudProvider : ICrudProvider
	{
		private readonly IDatabaseEnvironment environment;
		private readonly ICrudGenerator crudGenerator;

		public CrudProvider(IDatabaseEnvironment environment, ICrudGenerator crudGenerator)
		{
			this.environment = environment;
			this.crudGenerator = crudGenerator;
		}

		public int Insert(string table, Dictionary<string, object> values)
		{
			string sql = crudGenerator.GetInsertStatement(values, table);
			return environment.ExecuteNonQuery(sql);
		}

		public int Delete(string tableName, Dictionary<string, object> whereValues)
		{
			string sql = crudGenerator.GetDeleteStatement(whereValues, tableName);
			return environment.ExecuteNonQuery(sql);
		}

		public int Update(string tableName, Dictionary<string, object> operationalValues,
		                  Dictionary<string, object> whereValues)
		{
			string sql = crudGenerator.GetUpdateStatement(operationalValues, tableName, whereValues);
			return environment.ExecuteNonQuery(sql);
		}

		public IDataReader Select(string tableName, string[] columns, Dictionary<string, object> whereValues)
		{
			string query = crudGenerator.GetSelectStatement(columns, tableName, whereValues);
			return environment.ExecuteQuery(query);
		}
	}
}