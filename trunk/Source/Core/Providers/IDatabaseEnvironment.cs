using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Migrator.Providers
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
