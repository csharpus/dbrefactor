using System.Data;
using DbRefactor.Exceptions;
using DbRefactor.Providers;

namespace DbRefactor.Api
{
	public class WhereTable
	{
		private readonly TransformationProvider provider;
		private readonly string tableName;
		private readonly object whereParameters;

		internal WhereTable(TransformationProvider provider, string tableName, object whereParameters)
		{
			this.provider = provider;
			this.tableName = tableName;
			this.whereParameters = whereParameters;
		}

		public int Update(object updateObject)
		{
			return provider.Update(tableName, updateObject, whereParameters);
		}

		public int Delete()
		{
			return provider.Delete(tableName, whereParameters);
		}

		public IDataReader Select(params string[] columns)
		{
			return provider.Select(tableName, columns, whereParameters);
		}

		public T SelectScalar<T>(string column)
		{
			var value = provider.SelectScalar(column, tableName, whereParameters);
			if (value == null)
			{
				throw new DbRefactorException("Could not select scalar value because no data was found");
			}
			return (T)value;
		}
	}
}
