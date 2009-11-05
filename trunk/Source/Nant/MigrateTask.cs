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

using System.IO;
using System.Reflection;
using DbRefactor.Factories;
using DbRefactor.NAnt.Loggers;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace DbRefactor.NAnt
{
	/// <summary>
	/// Runs migrations on a database
	/// </summary>
	[TaskName("migrate")]
	public class MigrateTask : Task
	{
		private int to = -1; // To last revision
		private string provider;
		private string connectionString;
		private FileInfo migrationsAssembly;
		private bool trace;
		
		#region Attribute properties
		[TaskAttribute("provider", Required=true)]
		public string Provider
		{
			set
			{
				provider = value;
			}
			get
			{
				return provider;
			}
		}
		
		[TaskAttribute("connectionstring", Required=true)]
		public string ConnectionString
		{
			set
			{
				connectionString = value;
			}
			get
			{
				return connectionString;
			}
		}
		
		[TaskAttribute("migrations", Required=true)]
		public FileInfo MigrationsAssembly
		{
			set
			{
				migrationsAssembly = value;
			}
			get
			{
				return migrationsAssembly;
			}
		}
		
		[TaskAttribute("to")]
		public int To
		{
			set
			{
				to = value;
			}
			get
			{
				return to;
			}
		}
		
		[TaskAttribute("trace")]
		public bool Trace
		{
			set
			{
				trace = value;
			}
			get
			{
				return trace;
			}
		}
		#endregion
		
		protected override void ExecuteTask()
		{
			Assembly asm = Assembly.LoadFrom(migrationsAssembly.FullName);

			var mig = new ProviderFactory().CreateMigrator(provider, connectionString, null, trace);
			//mig.Logger = new TaskLogger(this);
			
			if (to == -1)
				mig.MigrateToLastVersion(asm);
			else
				mig.MigrateTo(asm, to);
		}
	}
}