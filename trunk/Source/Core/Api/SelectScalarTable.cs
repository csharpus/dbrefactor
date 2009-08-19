using DbRefactor.Providers;

namespace DbRefactor.Api
{
	public class SelectScalarTable<T>
	{
		private readonly TransformationProvider provider;
		private readonly string tableName;
		private readonly string column;

		public SelectScalarTable(TransformationProvider provider, string tableName, string column)
		{
			this.provider = provider;
			this.tableName = tableName;
			this.column = column;
		}

		private object whereParameters = new { };

		/// <summary>
		/// This method is a filter for group operations on table records
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public SelectScalarTable<T> Where(object parameters)
		{
			whereParameters = parameters;
			return this;
		}

		public T Execute()
		{
			return (T)provider.SelectScalar(column, tableName, whereParameters);
		}
	}
}
