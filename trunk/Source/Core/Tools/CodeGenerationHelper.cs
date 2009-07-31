using System;
using System.Collections.Generic;
using System.Linq;

namespace DbRefactor.Tools
{
	public static class CodeGenerationHelper
	{
		public static string GenerateMethodCall(string name, IEnumerable<string> arguments)
		{
			return String.Format("{0}({1})", name, String.Join(", ", arguments.ToArray()));
		}
	}
}
