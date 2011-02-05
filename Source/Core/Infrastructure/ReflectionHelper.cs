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

using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace DbRefactor.Infrastructure
{
	internal static class ReflectionHelper
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