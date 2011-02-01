using System;

namespace DbRefactor.Infrastructure.Loggers
{
	public class NullLogger : ILogger
	{
		public void MigrateTo(int version, string migrationName)
		{
			
		}

		public void Skipping(int version)
		{
			
		}

		public void RollingBack(int originalVersion)
		{
			
		}

		public void Exception(int version, string migrationName, Exception ex)
		{
			
		}

		public void Log(string format, params object[] args)
		{
			
		}

		public void Modify(string query)
		{
			
		}

		public void Query(string query)
		{
			
		}
	}
}