using System;
using System.Collections.Generic;
using System.Linq;

namespace DbRefactor.Extensions
{
	internal static class StringListExtensions
	{
		public static IEnumerable<string> WithTabsOnStart(this IEnumerable<string> list, int tabCount)
		{
			const int tabSize = 4;
			var tabs = new String(' ', tabSize * tabCount);
			return list.Select(s => tabs + s);
		}

		public static IEnumerable<string> WithNewLinesOnStart(this IEnumerable<string> list)
		{
			return list.Select(s => Environment.NewLine + s);
		}

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