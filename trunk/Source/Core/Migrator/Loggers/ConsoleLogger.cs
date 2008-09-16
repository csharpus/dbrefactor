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

namespace Migrator.Loggers
{
	/// <summary>
	/// Console logger for the migration mediator
	/// </summary>
	public class ConsoleLogger : ILogger
	{
		private int _widthFirstColumn = 5;
		private bool _trace = false;
		
		public ConsoleLogger(bool trace)
		{
			_trace = trace;
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
			//Console.Write("{0} Warning! : ", "".PadLeft(_widthFirstColumn));
			//Console.WriteLine(format, args);
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
			System.Diagnostics.Trace.Write(String.Format(message, args));
		}

		private void WriteLine(string message, params object[] args)
		{
			System.Diagnostics.Trace.WriteLine(String.Format(message, args));
		}
	}
}
