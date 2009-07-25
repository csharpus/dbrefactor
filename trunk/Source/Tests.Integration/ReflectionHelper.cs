using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace DbRefactor.Tests.Integration
{
	public class ReflectionHelper
	{
		public static string ToCsharpString(object value)
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
	}
}