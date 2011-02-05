using System.Collections.Generic;
using System.Linq;
using DbRefactor.Providers.Model;

namespace DbRefactor.Tools
{
	internal static class DependencySorter
	{
		public static List<string> Sort(IEnumerable<string> tables, IEnumerable<ForeignKey> relations)
		{
			var sortedTables = new List<string>();
			sortedTables.AddRange(tables.Where(t => !relations.Any(r => r.PrimaryTable == t)));

			sortedTables.AddRange(tables.Except(sortedTables).Where(t => relations.Where(r => r.PrimaryTable == t).All(r => r.ForeignNullable)));

			var tableCache = tables.Except(sortedTables).ToList();

			var cachedTablesCount = tableCache.Count;

			for (var i = 0; i < cachedTablesCount; i++)
			{
				var nextTable =
					tableCache.FirstOrDefault(t => relations.Where(r => r.PrimaryTable == t).All(r => sortedTables.Contains(r.ForeignTable)));
				if (nextTable == null)
				{
					nextTable =
						tableCache.FirstOrDefault(
							t =>
							relations.Where(r => r.PrimaryTable == t).All(r => sortedTables.Contains(r.ForeignTable) || r.ForeignNullable));
					if (nextTable == null)
					{
						nextTable = tableCache.First();
					}
				}
				sortedTables.Add(nextTable);
				tableCache.Remove(nextTable);
			}
			return sortedTables;
		}
	}
}