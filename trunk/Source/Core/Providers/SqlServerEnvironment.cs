using System.Data;
using System.Data.SqlClient;
using DbRefactor.Providers;

namespace DbRefactor.Providers
{
	sealed class SqlServerEnvironment : IDatabaseEnvironment
	{
		private readonly string _connectionString;
		private readonly IDbConnection _connection;
		private IDbTransaction _transaction;

		public SqlServerEnvironment(string connectionString)
		{
			_connectionString = connectionString;
			_connection = new SqlConnection();
			_connection.ConnectionString = _connectionString;
			_connection.Open();
		}

		#region IDatabaseEnvironment Members

		int IDatabaseEnvironment.ExecuteNonQuery(string sql)
		{
			IDbCommand cmd = BuildCommand(sql);
			return cmd.ExecuteNonQuery();
		}

		private IDbCommand BuildCommand(string sql)
		{
			IDbCommand cmd = _connection.CreateCommand();
			cmd.CommandText = sql;
			cmd.CommandType = CommandType.Text;
			if (_transaction != null)
			{
				cmd.Transaction = _transaction;
			}
			return cmd;
		}

		IDataReader IDatabaseEnvironment.ExecuteQuery(string sql)
		{
			IDbCommand cmd = BuildCommand(sql);
			return cmd.ExecuteReader();
		}

		object IDatabaseEnvironment.ExecuteScalar(string sql)
		{
			IDbCommand cmd = BuildCommand(sql);
			return cmd.ExecuteScalar();
		}

		/// <summary>
		/// Starts a transaction. Called by the migration mediator.
		/// </summary>
		public void BeginTransaction()
		{
			if (_transaction == null && _connection != null)
			{
				EnsureHasConnection();
				_transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
			}
		}

		private void EnsureHasConnection()
		{
			if (_connection.State != ConnectionState.Open)
			{
				_connection.Open();
			}
		}

		/// <summary>
		/// Rollback the current migration. Called by the migration mediator.
		/// </summary>
		public void RollbackTransaction()
		{
			if (_transaction != null && _connection != null && _connection.State == ConnectionState.Open)
			{
				try
				{
					_transaction.Rollback();
				}
				finally
				{
					_connection.Close();
				}
			}
			_transaction = null;
		}

		/// <summary>
		/// Commit the current transaction. Called by the migrations mediator.
		/// </summary>
		public void CommitTransaction()
		{
			if (_transaction != null && _connection != null && _connection.State == ConnectionState.Open)
			{
				try
				{
					_transaction.Commit();
				}
				finally
				{
					_connection.Close();
				}
			}
			_transaction = null;
		}
		#endregion
	}
}