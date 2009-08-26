using System.Data;
using DbRefactor.Providers;

namespace DbRefactor.Api
{
	public class SelectTable : QueryTable
	{
		private readonly TransformationProvider provider;
		private readonly string tableName;
		private readonly string[] selectColumns;

		internal SelectTable(TransformationProvider provider, string tableName, string[] selectColumns)
		{
			this.provider = provider;
			this.tableName = tableName;
			this.selectColumns = selectColumns;
		}

		private object whereParameters = new {};

		/// <summary>
		/// This method is a filter for group operations on table records
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public SelectTable Where(object parameters)
		{
			whereParameters = parameters;
			return this;
		}

		public IDataReader Execute()
		{
			return provider.Select(tableName, selectColumns, whereParameters);
		}
	}
}
