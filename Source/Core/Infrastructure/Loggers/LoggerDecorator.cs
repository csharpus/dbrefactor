using System.Data;
using DbRefactor.Providers;

namespace DbRefactor.Infrastructure.Loggers
{
	public class LoggerDecorator : IDatabaseEnvironment
	{
		private readonly IDatabaseEnvironment environment;
		private readonly ILogger logger;

		public LoggerDecorator(IDatabaseEnvironment environment, ILogger logger)
		{
			this.environment = environment;
			this.logger = logger;
		}

		public int ExecuteNonQuery(string sql)
		{
			logger.Log(sql);
			return environment.ExecuteNonQuery(sql);
		}

		public IDataReader ExecuteQuery(string sql)
		{
			logger.Log(sql);
			return environment.ExecuteQuery(sql);
		}

		public object ExecuteScalar(string sql)
		{
			logger.Log(sql);
			return environment.ExecuteScalar(sql);
		}

		public void BeginTransaction()
		{
			environment.BeginTransaction();
		}

		public void RollbackTransaction()
		{
			environment.RollbackTransaction();
		}

		public void CommitTransaction()
		{
			environment.CommitTransaction();
		}

		public void OpenConnection()
		{
			environment.OpenConnection();
		}

		public void CloseConnection()
		{
			environment.CloseConnection();
		}
	}
}