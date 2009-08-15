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
		private readonly bool trace;
		private readonly List<ILogWriter> writers = new List<ILogWriter>();
		private static readonly ILogger nullLogger = new Logger(false);

		public static ILogger NullLogger
		{
			get
			{
				return nullLogger;
			}
		}

		public Logger(bool trace)
		{
			this.trace = trace;
		}

		public Logger(bool trace, params ILogWriter[] writers)
			: this(trace)
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

		public void Started(int currentVersion, int finalVersion)
		{
			WriteLine("Current version : {0}", currentVersion);
		}

		public void MigrateUp(int version, string migrationName)
		{
			WriteLine("{0} {1}", version.ToString().PadLeft(WidthFirstColumn), migrationName);
		}

		public void MigrateDown(int version, string migrationName)
		{
			MigrateUp(version, migrationName);
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
			UntracedWriteLine("{0} Error in migration {1} : {2}", "".PadLeft(WidthFirstColumn), version, ex.Message);
			UntracedWriteLine("========= Error detail =========");
			UntracedWriteLine(ex.ToString());
			UntracedWriteLine(ex.StackTrace);
			Exception iex = ex.InnerException;
			while (ex.InnerException != null)
			{
				UntracedWriteLine("Caused by: {0}", ex.InnerException);
				UntracedWriteLine(ex.InnerException.StackTrace);
				iex = iex.InnerException;
			}
			UntracedWriteLine("======================================");
		}

		public void Finished(int originalVersion, int currentVersion)
		{
			WriteLine("Migrated to version {0}", currentVersion);
		}

		public void Log(string format, params object[] args)
		{
			Write("{0} ", "".PadLeft(WidthFirstColumn));
			WriteLine(format, args);
		}

		public void Warn(string format, params object[] args)
		{
			Write("{0} Warning! : ", "".PadLeft(WidthFirstColumn));
			WriteLine(format, args);
		}

		public void Trace(string format, params object[] args)
		{
			Log(format, args);
		}

		private void UntracedWrite(string message, params object[] args)
		{
			foreach (ILogWriter writer in writers)
			{
				writer.Write(message, args);
			}
		}

		private void UntracedWriteLine(string message, params object[] args)
		{
			foreach (ILogWriter writer in writers)
			{
				writer.WriteLine(message, args);
			}
		}

		private void Write(string message, params object[] args)
		{
			if (!trace) return;
			UntracedWrite(message, args);
		}

		private void WriteLine(string message, params object[] args)
		{
			if (!trace) return;
			UntracedWriteLine(message, args);
		}

		#region Static Logger Helpers

		public static ILogger ConsoleLogger()
		{
			return new Logger(false, new ConsoleWriter());
		}
		#endregion
	}
}