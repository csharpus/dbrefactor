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
using System.IO;
using DbRefactor.Providers;

namespace DbRefactor.Tools
{
//TODO: this class should be changed according to new changes
	internal class SchemaDumper
	{
		readonly TransformationProvider _provider;

		public SchemaDumper(string connectionString)
		{
			_provider = new ProviderFactory().Create(connectionString);
		}

		public string Dump()
		{
			var writer = new StringWriter();

			writer.WriteLine("using DbRefactor;");
			writer.WriteLine();
			writer.WriteLine("[Migration(1)]");
			writer.WriteLine("public class SchemaDump : Migration");
			writer.WriteLine("{");
			writer.WriteLine("\tpublic override void Up()");
			writer.WriteLine("\t{");

			foreach (string table in _provider.GetTables())
			{
				writer.WriteLine("\t\tCreateTable(\"{0}\")", table);
				foreach (var column in _provider.GetColumns(table))
				{
					writer.WriteLine("\t\t\t." + GenerateColumnCode(column));
				}
				writer.WriteLine("\t\t\t.Execute();");
				writer.WriteLine();
			}

			writer.WriteLine("\t}");
			writer.WriteLine();
			writer.WriteLine("\tpublic override void Down()");
			writer.WriteLine("\t{");

			foreach (string table in _provider.GetTables())
			{
				writer.WriteLine("\t\tDropTable(\"{0}\");", table);
			}

			writer.WriteLine("\t}");
			writer.WriteLine("}");

			return writer.ToString();
		}

		private string GenerateColumnCode(Column column)
		{
			throw new NotImplementedException();
		}

		public void DumpTo(string file)
		{
			using (var writer = new StreamWriter(file))
			{
				DumpTo(writer);
			}
		}

		public void DumpTo(StreamWriter writer)
		{
			writer.Write(Dump());
		}
	}
}