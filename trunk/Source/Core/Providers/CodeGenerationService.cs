using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace DbRefactor.Providers
{
	public class CodeGenerationService : ICodeGenerationService
	{
		public string PrimitiveValue(object value)
		{
			var domProvider = CodeDomProvider.CreateProvider("CSharp");
			var primitiveExpression = new CodePrimitiveExpression(value);
			var options = new CodeGeneratorOptions
			{
				BracingStyle = "C",
				IndentString = "  "
			};
			var writer = new StringWriter();
			domProvider.GenerateCodeFromExpression(primitiveExpression, writer, options);
			return writer.ToString();
		}

		public string DateTimeValue(DateTime dateTime)
		{
			return String.Format("new DateTime({0}, {1}, {2})", dateTime.Year, dateTime.Month, dateTime.Day);
		}

		public string BinaryValue(byte[] bytes)
		{
			return string.Empty;
		}
	}
}