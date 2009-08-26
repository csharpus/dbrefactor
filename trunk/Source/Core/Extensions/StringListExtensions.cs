using System;
using System.Collections.Generic;
using System.Linq;

namespace DbRefactor.Extensions
{
	internal static class StringListExtensions
	{
		public static string ComaSeparated(this IEnumerable<string> list)
		{
			return String.Join(", ", list.ToArray());
		}

		public static string SpaceSeparated(this IEnumerable<string> list)
		{
			return String.Join(" ", list.ToArray());
		}
	}
}