using System.Data;
using DbRefactor.Exceptions;
using DbRefactor.Infrastructure.Loggers;
using DbRefactor.Providers;
using MySql.Data.MySqlClient;

namespace DbRefactor.Engines.MySql
{
	public class MySqlEnvironment : IDatabaseEnvironment
	{
		private readonly ILogger logger;
		private readonly IDbConnection connection;
		private IDbTransaction transaction;

		public MySqlEnvironment(string connectionString, ILogger logger)
		{
			this.logger = logger;
			connection = new MySqlConnection(connectionString);
			connection.Open();
		}

		int IDatabaseEnvironment.ExecuteNonQuery(string sql)
		{
			logger.Log(sql);
			IDbCommand cmd = BuildCommand(sql);
			cmd.CommandTimeout = 120;
			try
			{
				return cmd.ExecuteNonQuery();
			}
			catch (MySqlException e)
			{
				throw new IncorrectQueryException(sql, e);
			}
		}

		IDataReader IDatabaseEnvironment.ExecuteQuery(string sql)
		{
			logger.Query(sql);
			IDbCommand cmd = BuildCommand(sql);
			try
			{
				return cmd.ExecuteReader();
			}
			catch (MySqlException e)
			{
				throw new IncorrectQueryException(sql, e);
			}
		}

		object IDatabaseEnvironment.ExecuteScalar(string sql)
		{
			logger.Query(sql);
			IDbCommand cmd = BuildCommand(sql);
			try
			{
				return cmd.ExecuteScalar();
			}
			catch (MySqlException e)
			{
				throw new IncorrectQueryException(sql, e);
			}
		}

		private IDbCommand BuildCommand(string sql)
		{
			IDbCommand cmd = connection.CreateCommand();
			cmd.CommandText = sql;
			cmd.CommandType = CommandType.Text;
			if (transaction != null)
			{
				cmd.Transaction = transaction;
			}
			return cmd;
		}

		public void BeginTransaction()
		{
			if (transaction == null && connection != null)
			{
				EnsureHasConnection();
				transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
			}
		}

		private void EnsureHasConnection()
		{
			if (connection.State != ConnectionState.Open)
			{
				connection.Open();
			}
		}

		public void RollbackTransaction()
		{
			if (transaction != null && connection != null && connection.State == ConnectionState.Open)
			{
				transaction.Rollback();
			}
			transaction = null;
		}

		public void CommitTransaction()
		{
			if (transaction != null && connection != null && connection.State == ConnectionState.Open)
			{
				transaction.Commit();
			}
			transaction = null;
		}

		public void CloseConnection()
		{
			connection.Close();
		}
	}
}
