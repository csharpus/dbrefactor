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

namespace Migrator.Loggers
{
	/// <summary>
	/// Text logger for the migration mediator
	/// </summary>
	public class Logger : IAttachableLogger
	{
		private int _widthFirstColumn = 5;
		private bool _trace = false;
		private List<ILogWriter> _writers = new List<ILogWriter>();

		public Logger(bool trace)
		{
			_trace = trace;
		}

		public Logger(bool trace, params ILogWriter[] writers)
			: this(trace)
		{
			_writers.AddRange(writers);
		}

		public void Attach(ILogWriter writer)
		{
			_writers.Add(writer);
		}

		public void Detach(ILogWriter writer)
		{
			_writers.Remove(writer);
		}

		public void Started(int currentVersion, int finalVersion)
		{
			WriteLine("Current version : {0}", currentVersion);
		}

		public void MigrateUp(int version, string migrationName)
		{
			WriteLine("{0} {1}", version.ToString().PadLeft(_widthFirstColumn), migrationName);
		}

		public void MigrateDown(int version, string migrationName)
		{
			MigrateUp(version, migrationName);
		}

		public void Skipping(int version)
		{
			WriteLine("{0} {1}", version.ToString().PadLeft(_widthFirstColumn), "<Migration not found>");
		}

		public void RollingBack(int originalVersion)
		{
			WriteLine("Rolling back to migration {0}", originalVersion);
		}

		public void Exception(int version, string migrationName, Exception ex)
		{
			WriteLine("{0} Error in migration {1} : {2}", "".PadLeft(_widthFirstColumn), version, ex.Message);
			if (_trace)
			{
				WriteLine("========= Error detail =========");
				WriteLine(ex.ToString());
				WriteLine(ex.StackTrace);
				Exception iex = ex.InnerException;
				while (ex.InnerException != null)
				{
					WriteLine("Caused by: {0}", ex.InnerException);
					WriteLine(ex.InnerException.StackTrace);
					iex = iex.InnerException;
				}
				WriteLine("======================================");
			}
		}

		public void Finished(int originalVersion, int currentVersion)
		{
			WriteLine("Migrated to version {0}", currentVersion);
		}

		public void Log(string format, params object[] args)
		{
			Write("{0} ", "".PadLeft(_widthFirstColumn));
			WriteLine(format, args);
		}

		public void Warn(string format, params object[] args)
		{
			Write("{0} Warning! : ", "".PadLeft(_widthFirstColumn));
			WriteLine(format, args);
		}

		public void Trace(string format, params object[] args)
		{
			if (_trace)
			{
				Log(format, args);
			}
		}

		private void Write(string message, params object[] args)
		{
			foreach (ILogWriter writer in _writers)
			{
				writer.Write(message, args);
			}
		}

		private void WriteLine(string message, params object[] args)
		{
			foreach (ILogWriter writer in _writers)
			{
				writer.WriteLine(message, args);
			}
		}

		#region Static Logger Helpers

		public static ILogger ConsoleLogger()
		{
			return new Logger(false, new ConsoleWriter());
		}
		#endregion
	}
}
