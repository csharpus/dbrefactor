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

using System.Data;
using System.Data.SqlClient;
using DbRefactor.Exceptions;
using DbRefactor.Providers;

namespace DbRefactor.Engines.SqlServer
{
	internal sealed class SqlServerEnvironment : IDatabaseEnvironment
	{
		private readonly string connectionString;
		private IDbConnection connection;
		private IDbTransaction transaction;

		public SqlServerEnvironment(string connectionString)
		{
			this.connectionString = connectionString;
		}

		int IDatabaseEnvironment.ExecuteNonQuery(string sql)
		{
			IDbCommand cmd = BuildCommand(sql);
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
			if (connection == null)
			{
				throw new DbRefactorException("There is no open connection, can not perform query");
			}
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
			if (connection == null)
			{
				throw new DbRefactorException("There is no open connection");
			}
			transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
		}

		/// <summary>
		/// Rollback the current migration. Called by the migration mediator.
		/// </summary>
		public void RollbackTransaction()
		{
			if (transaction == null)
			{
				throw new DbRefactorException("There is no active transaction");
			}
			transaction.Rollback();
			transaction.Dispose();
			transaction = null;
		}

		/// <summary>
		/// Commit the current transaction. Called by the migrations mediator.
		/// </summary>
		public void CommitTransaction()
		{
			if (transaction == null)
			{
				throw new DbRefactorException("There is not active transaction");
			}
			transaction.Commit();
			transaction.Dispose();
			transaction = null;
		}

		public void OpenConnection()
		{
			if (connection != null)
			{
				throw new DbRefactorException("Connection is already open or broken");
			}
			connection = new SqlConnection { ConnectionString = connectionString };
			connection.Open();
		}

		public void CloseConnection()
		{
			if (connection == null)
			{
				throw new DbRefactorException("Connection is not open");
			}
			connection.Close();
			connection.Dispose();
			connection = null;
		}
	}
}