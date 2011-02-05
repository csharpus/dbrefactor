using System.Collections.Generic;
using DbRefactor.Providers.Model;
using DbRefactor.Tools;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines.Tools
{
	[TestFixture]
	public class DependencySorterTests
	{
		[Test]
		public void should_sort_tables_with_more_than_one_dependency_level()
		{
			var tables = new List<string> { "E", "D", "C", "B", "A" };

			var relations = new List<ForeignKey>
			                	{
			                		new DataDumperTest.R {Foreign = "A", Primary = "B", ForeignNullable = false},
			                		new DataDumperTest.R {Foreign = "B", Primary = "C", ForeignNullable = false},
			                		new DataDumperTest.R {Foreign = "C", Primary = "D", ForeignNullable = false},
			                		new DataDumperTest.R {Foreign = "D", Primary = "E", ForeignNullable = false},
			                	};

			var result = DependencySorter.Sort(tables, relations);

			CollectionAssert.AreEqual(new[] { "A", "B", "C", "D", "E" }, result);
		}

		[Test]
		public void should_sort_tables_with_cyclic_dependency()
		{
			// data from foreign table should be deleted first
			var tables = new List<string> { "C", "A", "B" };//, "G", "F"};
			var relations = new List<ForeignKey>
			                	{
			                		new DataDumperTest.R {Foreign = "A", Primary = "B", ForeignNullable = false},
			                		new DataDumperTest.R {Foreign = "B", Primary = "C", ForeignNullable = false},
			                		new DataDumperTest.R {Foreign = "C", Primary = "A", ForeignNullable = true},
			                		//new R {Foreign = "G", Primary = "F", ForeignNullable = false}
			                	};
			var result = DependencySorter.Sort(tables, relations);
			CollectionAssert.AreEqual(new[] { "A", "B", "C" }, result);
		}

		[Test]
		public void should_sort_tables_with_all_weak_cyclic_dependency()
		{
			var tables = new List<string> { "A", "B", "C", "G", "F" };
			var relations = new List<ForeignKey>
			                	{
			                		new DataDumperTest.R {Foreign = "A", Primary = "B", ForeignNullable = true},
			                		new DataDumperTest.R {Foreign = "B", Primary = "C", ForeignNullable = true},
			                		new DataDumperTest.R {Foreign = "C", Primary = "A", ForeignNullable = true},
			                		new DataDumperTest.R {Foreign = "G", Primary = "F", ForeignNullable = false}
			                	};
			DependencySorter.Sort(tables, relations);

			// should not be in infinite cycle
		}

		[Test]
		public void should_sort_tables_by_inner_weak_dependency()
		{
			var tables = new List<string> {"E", "D", "C", "B", "A" };
			var relations = new List<ForeignKey>
			                	{
			                		new DataDumperTest.R {Foreign = "A", Primary = "B", ForeignNullable = false},
			                		new DataDumperTest.R {Foreign = "B", Primary = "C", ForeignNullable = false},
			                		new DataDumperTest.R {Foreign = "C", Primary = "D", ForeignNullable = false},
			                		new DataDumperTest.R {Foreign = "D", Primary = "B", ForeignNullable = true},
			                		new DataDumperTest.R {Foreign = "D", Primary = "E", ForeignNullable = false},
			                	};
			var result = DependencySorter.Sort(tables, relations);

			CollectionAssert.AreEqual(new[] { "A", "B", "C", "D", "E" }, result);
		}
	}
}