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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Infrastructure;
using DbRefactor.Providers;
using DbRefactor.Providers.Columns;
using DbRefactor.Providers.Filters;

namespace DbRefactor.Tools
{
	public class SchemaDumper
	{
		// Future options:
		// Generate several files instead of one
		// Add a letter to the migration names
		// Add a number to the migration names
		// use three symbols for migration name numbers
		// Override all constraint names
		// Override only generated constraint names
		// Use namespace for migrations
		private readonly IDatabaseEnvironment environment;
		private readonly SchemaHelper schemaHelper;

		internal SchemaDumper(IDatabaseEnvironment environment, SchemaHelper schemaHelper)
		{
			this.environment = environment;
			this.schemaHelper = schemaHelper;
		}

		public string Dump()
		{
			environment.OpenConnection();
			var writer = new StringWriter();

			writer.WriteLine("using DbRefactor;");
			writer.WriteLine();
			writer.WriteLine("[Migration(1)]");
			writer.WriteLine("public class M001_SchemaDump : Migration");
			writer.WriteLine("{");
			writer.WriteLine("\tpublic override void Up()");
			writer.WriteLine("\t{");

			foreach (string table in schemaHelper.GetTables())
			{
				writer.WriteLine(GetTableCode(table));
			}

			writer.WriteLine(GetAddForeignKeysCode());

			writer.WriteLine("\t}");
			writer.WriteLine();
			writer.WriteLine("\tpublic override void Down()");
			writer.WriteLine("\t{");

			writer.WriteLine(GetDropForeignKeysCode());

			foreach (string table in schemaHelper.GetTables())
			{
				writer.WriteLine("\t\tDropTable(\"{0}\");", table);
			}

			writer.WriteLine("\t}");
			writer.WriteLine("}");
			environment.CloseConnection();
			return writer.ToString();
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

		private string GetTableCode(string tableName)
		{
			var providers = schemaHelper.GetColumns(new ColumnFilter { TableName = tableName });

			string values = CreateTableMethodCode(tableName);
			values = providers.Aggregate(values, (current, columnProvider) => current + GetColumnCode(columnProvider));
			values += ExecuteMethodCode();
			return values;
		}

		private static string ExecuteMethodCode()
		{
			return Environment.NewLine + "\t\t\t.Execute();";
		}

		private static string CreateTableMethodCode(string tableName)
		{
			return String.Format("\t\tCreateTable(\"{0}\")", tableName);
		}

		private static string GetColumnCode(ColumnProvider columnProvider)
		{
			var methodCode = GetColumnMethodCode(columnProvider);
			var values = Environment.NewLine + "\t\t\t." + methodCode;
			return columnProvider.Properties.Aggregate(values, (current, property) => current + ("." + property.MethodName() + "()"));
		}

		private string GetAddForeignKeysCode()
		{
			string values = String.Empty;
			var keys = schemaHelper.GetForeignKeys();
			foreach (var key in keys)
			{
				values += String.Format("\t\tTable(\"{0}\").Column(\"{1}\").AddForeignKeyTo(\"{2}\", \"{3}\")",
										key.ForeignTable,
										key.ForeignColumn,
										key.PrimaryTable,
										key.PrimaryColumn);
				values += Environment.NewLine;
			}
			return values;
		}

		private string GetDropForeignKeysCode()
		{
			string values = String.Empty;
			var keys = schemaHelper.GetForeignKeys();
			foreach (var key in keys)
			{
				values += String.Format("\t\tTable(\"{0}\").Column(\"{1}\").DropForeignKey(\"{2}\", \"{3}\")",
										key.ForeignTable,
										key.ForeignColumn,
										key.PrimaryTable,
										key.PrimaryColumn);
				values += Environment.NewLine;
			}
			return values;
		}

		private static string GetColumnMethodCode(ColumnProvider columnProvider)
		{
			var expression = columnProvider.Method();
			string methodName = GetMethodName(expression);

			List<string> methodArguments = columnProvider.MethodArguments().ToList();
			if (columnProvider.HasDefaultValue)
			{
				methodArguments.Add(columnProvider.GetDefaultValueCode());
			}
			return CodeGenerationHelper.GenerateMethodCall(methodName, methodArguments);
		}

		private static string GetMethodName(Expression<Action<NewTable>> expression)
		{
			var methodCall = (MethodCallExpression)expression.Body;
			return methodCall.Method.Name;
		}

		public string ObtainValue(Expression expression)
		{
			object value = ExpressionHelper.ValueFromExpression(expression);

			return ReflectionHelper.ToCsharpString(value);
		}		
	}
}