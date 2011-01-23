using System.Collections.Generic;

namespace DbRefactor.Providers
{
	public interface ICrudGenerator
	{
		string GetInsertStatement(Dictionary<string, object> values, string table);
		string GetDeleteStatement(Dictionary<string, object> whereValues, string tableName);
		string GetUpdateStatement(Dictionary<string, object> operationalValues, string tableName, Dictionary<string, object> whereValues);
		string GetSelectStatement(string[] columns, string tableName, Dictionary<string, object> whereValues);
	}
}