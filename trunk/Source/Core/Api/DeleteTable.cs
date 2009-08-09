using DbRefactor.Providers;

namespace DbRefactor.Api
{
	public class DeleteTable : QueryTable
	{
		private readonly TransformationProvider provider;
		private readonly string tableName;

		public DeleteTable(TransformationProvider provider, string tableName)
		{
			this.provider = provider;
			this.tableName = tableName;
		}

		private object whereParameters = new { };

		/// <summary>
		/// This method is a filter for group operations on table records
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public DeleteTable Where(object parameters)
		{
			whereParameters = parameters;
			return this;
		}

		public int Execute()
		{
			return provider.Delete(tableName, whereParameters);
		}
	}
}
