using DbRefactor.Providers;

namespace DbRefactor.Api
{
	public class UpdateTable : QueryTable
	{
		private readonly TransformationProvider provider;
		private readonly string tableName;
		private readonly object updateObject;

		internal UpdateTable(TransformationProvider provider, string tableName, object updateObject)
		{
			this.provider = provider;
			this.tableName = tableName;
			this.updateObject = updateObject;
		}

		private object whereParameters = new { };

		/// <summary>
		/// This method is a filter for group operations on table records
		/// </summary>
		public UpdateTable Where(object parameters)
		{
			whereParameters = parameters;
			return this;
		}

		public int Execute()
		{
			return provider.Update(tableName, updateObject, whereParameters);
		}
	}
}
