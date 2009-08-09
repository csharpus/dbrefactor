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
using DbRefactor.Providers;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Tools
{
	internal class SchemaDumper
	{
		private readonly TransformationProvider provider;

		public SchemaDumper(TransformationProvider transformationProvider)
		{
			provider = transformationProvider;
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

			foreach (string table in provider.GetTables())
			{
				writer.WriteLine(GetTableCode(table));
			}

			writer.WriteLine(GetAddForeignKeysCode());

			writer.WriteLine("\t}");
			writer.WriteLine();
			writer.WriteLine("\tpublic override void Down()");
			writer.WriteLine("\t{");

			writer.WriteLine(GetDropForeignKeysCode());

			foreach (string table in provider.GetTables())
			{
				writer.WriteLine("\t\tDropTable(\"{0}\");", table);
			}

			writer.WriteLine("\t}");
			writer.WriteLine("}");

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
			var providers = provider.GetColumnProviders(tableName);
			string values = CreateTableMethodCode(tableName);
			foreach (var columnProvider in providers)
			{
				values += GetColumnCode(columnProvider);
			}
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

		private string GetColumnCode(ColumnProvider columnProvider)
		{
			var methodCode = GetColumnMethodCode(columnProvider);
			var values = Environment.NewLine + "\t\t\t." + methodCode;
			foreach (var property in columnProvider.Properties)
			{
				values += "." + property.MethodName() + "()";
			}
			return values;
		}

		private string GetAddForeignKeysCode()
		{
			string values = String.Empty;
			var keys = provider.GetForeignKeys();
			foreach (var key in keys)
			{
				values += String.Format("Table(\"{0}\").AddForeignKey(\"{1}\", \"{2}\").References(\"{3}\", \"{4}\")",
										key.ForeignTable,
										key.Name,
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
			var keys = provider.GetForeignKeys();
			foreach (var key in keys)
			{
				values += String.Format("Table(\"{0}\").DropForeignKey(\"{1}\")",
										key.ForeignTable,
										key.Name);
				values += Environment.NewLine;
			}
			return values;
		}

		private string GetColumnMethodCode(ColumnProvider columnProvider)
		{
			var expression = columnProvider.Method();
			string methodName = GetMethodName(expression);

			List<string> methodArguments = GetMethodArguments(expression);
			if (columnProvider.HasDefaultValue)
			{
				methodArguments.Add(columnProvider.GetDefaultValueCode());
			}
			return CodeGenerationHelper.GenerateMethodCall(methodName, methodArguments);
		}

		private List<string> GetMethodArguments(Expression<Action<NewTable>> expression)
		{
			var methodCall = (MethodCallExpression)expression.Body;
			IList<Expression> argumentsList = methodCall.Method.IsStatic
			                                  	? methodCall.Arguments.Skip(1).ToList()
			                                  	: methodCall.Arguments.ToList();
			return argumentsList.Select(a => ObtainValue(a)).ToList();
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