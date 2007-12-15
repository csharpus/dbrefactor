using System.Data;

namespace DbRefactor.Providers
{
	public interface IDatabaseEnvironment
	{
		int ExecuteNonQuery(string sql);

		IDataReader ExecuteQuery(string sql);

		object ExecuteScalar(string sql);

		void BeginTransaction();

		void RollbackTransaction();

		void CommitTransaction();
	}
}