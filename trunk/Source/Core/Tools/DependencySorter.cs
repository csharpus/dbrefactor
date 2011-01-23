using System.Collections.Generic;
using System.Linq;

namespace DbRefactor.Providers
{
	internal class DependencySorter
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

		private static List<string> WrongSort(List<string> tables, IEnumerable<ForeignKey> relations)
		{
			// It sorts tables by "hard" relations - relations with not null foreign key
			// Then moves all tables with "weak" relations - relations with null fk
			// just before the first table with this relation
			// It works because it is not possible to insert data to circular dependency graph
			// with all not null values
			var sortedTables = new List<string>(tables);
			HardSort(sortedTables, relations);
			WeakSort(sortedTables, relations);
			return sortedTables;
		}

		private static void WeakSort(IList<string> tables, IEnumerable<ForeignKey> relations)
		{
			for (int index = tables.Count - 1; index >= 0; index--)
			{
				for (int j = tables.Count - 1; j > index; j--)
				{
					if (WeakForeignPrimary(tables[index], tables[j], relations))
					{
						var value = tables[index];
						tables.Remove(value);
						tables.Insert(j, value);
					}
				}
			}
		}

		private static void HardSort(List<string> tables, IEnumerable<ForeignKey> relations)
		{
			tables.Sort((t1, t2) =>
			            	{
			            		if (t1 == t2) return 0;
			            		return HardForeignPrimary(t1, t2, relations) ? -1 : 1;
			            	});
		}

		private static bool WeakForeignPrimary(string t1, string t2, IEnumerable<ForeignKey> relations)
		{
			return relations.Any(r => r.ForeignTable == t1 && r.PrimaryTable == t2 && r.ForeignNullable);
		}

		private static bool HardForeignPrimary(string t1, string t2, IEnumerable<ForeignKey> relations)
		{
			return relations.Any(r => r.ForeignTable == t1 && r.PrimaryTable == t2 && !r.ForeignNullable);
		}
	}
}