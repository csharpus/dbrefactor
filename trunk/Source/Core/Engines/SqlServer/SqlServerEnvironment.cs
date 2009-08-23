#region License

//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.

#endregion

using System;
using System.Data;
using System.Data.SqlClient;
using DbRefactor.Exceptions;
using DbRefactor.Infrastructure.Loggers;
using DbRefactor.Providers;

namespace DbRefactor.Engines.SqlServer
{
	internal sealed class SqlServerEnvironment : IDatabaseEnvironment
	{
		private readonly string connectionString;
		private readonly ILogger logger;
		private readonly IDbConnection connection;
		private IDbTransaction transaction;

		public SqlServerEnvironment(string connectionString, ILogger logger)
		{
			this.connectionString = connectionString;
			this.logger = logger;
			connection = new SqlConnection {ConnectionString = this.connectionString};
			connection.Open();
		}

		internal IDbConnection Connection
		{
			get { return connection; }
		}

		#region IDatabaseEnvironment Members

		int IDatabaseEnvironment.ExecuteNonQuery(string sql)
		{
			logger.Log(sql);
			IDbCommand cmd = BuildCommand(sql);
			cmd.CommandTimeout = 120;
			try
			{
				return cmd.ExecuteNonQuery();
			}
			catch (SqlException e)
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

		IDataReader IDatabaseEnvironment.ExecuteQuery(string sql)
		{
			logger.Log(sql);
			IDbCommand cmd = BuildCommand(sql);
			try
			{
				return cmd.ExecuteReader();
			}
			catch (SqlException e)
			{
				throw new IncorrectQueryException(sql, e);
			}
		}

		object IDatabaseEnvironment.ExecuteScalar(string sql)
		{
			logger.Log(sql);
			IDbCommand cmd = BuildCommand(sql);
			try
			{
				return cmd.ExecuteScalar();
			}
			catch (SqlException e)
			{
				throw new IncorrectQueryException(sql, e);
			}
		}

		/// <summary>
		/// Starts a transaction. Called by the migration mediator.
		/// </summary>
		public void BeginTransaction()
		{
			if (transaction == null && connection != null)
			{
				EnsureHasConnection();
				transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
			}
		}

		internal IDbTransaction Transaction
		{
			get { return transaction; }
		}

		private void EnsureHasConnection()
		{
			if (connection.State != ConnectionState.Open)
			{
				connection.Open();
			}
		}

		/// <summary>
		/// Rollback the current migration. Called by the migration mediator.
		/// </summary>
		public void RollbackTransaction()
		{
			if (transaction != null && connection != null && connection.State == ConnectionState.Open)
			{
				try
				{
					transaction.Rollback();
				}
				finally
				{
					// connection.Close();
				}
			}
			transaction = null;
		}

		/// <summary>
		/// Commit the current transaction. Called by the migrations mediator.
		/// </summary>
		public void CommitTransaction()
		{
			if (transaction != null && connection != null && connection.State == ConnectionState.Open)
			{
				try
				{
					transaction.Commit();
				}
				finally
				{
					// connection.Close();
				}
			}
			transaction = null;
		}

		public void CloseConnection()
		{
			connection.Close();
		}

		#endregion
	}
}