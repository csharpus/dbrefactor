using System.Collections.Generic;
using System.Data;

namespace DbRefactor.Providers
{
	public interface ICrudProvider
	{
		int Insert(string table, Dictionary<string, object> values);
		int Delete(string tableName, Dictionary<string, object> whereValues);

		int Update(string tableName, Dictionary<string, object> operationalValues,
		           Dictionary<string, object> values);

		IDataReader Select(string tableName, string[] columns, Dictionary<string, object> values);
	}
}