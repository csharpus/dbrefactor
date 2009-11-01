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
using System.Collections.Generic;

namespace DbRefactor.Infrastructure.Loggers
{
	/// <summary>
	/// Text logger for the migration mediator
	/// </summary>
	public class Logger : IAttachableLogger
	{
		private const int WidthFirstColumn = 5;
		private readonly List<ILogWriter> writers = new List<ILogWriter>();
		private static readonly ILogger nullLogger = new Logger();

		public static ILogger NullLogger
		{
			get
			{
				return nullLogger;
			}
		}

		public Logger()
		{
		}

		public Logger(params ILogWriter[] writers)
		{
			this.writers.AddRange(writers);
		}

		public void Attach(ILogWriter writer)
		{
			writers.Add(writer);
		}

		public void Detach(ILogWriter writer)
		{
			writers.Remove(writer);
		}

		public void MigrateTo(int version, string migrationName)
		{
			WriteLine("Migration {0} {1}", version.ToString().PadLeft(WidthFirstColumn), migrationName);
		}

		public void Skipping(int version)
		{
			WriteLine("{0} {1}", version.ToString().PadLeft(WidthFirstColumn), "<Migration not found>");
		}

		public void RollingBack(int originalVersion)
		{
			WriteLine("Rolling back to migration {0}", originalVersion);
		}

		public void Exception(int version, string migrationName, Exception ex)
		{
			WriteLine("{0} Error in migration {1} : {2}", "".PadLeft(WidthFirstColumn), version, ex.Message);
			WriteLine("========= Error detail =========");
			WriteLine(ex.ToString());
			WriteLine(ex.StackTrace);
			//Exception innerException = ex.InnerException;
			//while (innerException != null)
			//{
			//    WriteLine("Caused by: {0}", innerException);
			//    WriteLine(innerException.StackTrace);
			//    innerException = innerException.InnerException;
			//}
			WriteLine("======================================");
		}

		public void Log(string format, params object[] args)
		{
			Write("{0} ", "".PadLeft(WidthFirstColumn));
			WriteLine(format, args);
		}

		public void Modify(string query)
		{
			Log(query);
		}

		public void Query(string query)
		{
			
		}

		private void Write(string message, params object[] args)
		{
			foreach (ILogWriter writer in writers)
			{
				writer.Write(message, args);
			}
		}

		private void WriteLine(string message, params object[] args)
		{
			foreach (ILogWriter writer in writers)
			{
				writer.WriteLine(message, args);
			}
		}

		public static ILogger ConsoleLogger()
		{
			return new Logger(new ConsoleWriter());
		}
	}
}