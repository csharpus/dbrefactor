using System;
using System.Collections.Generic;
using System.Linq;
using DbRefactor.Factories;
using DbRefactor.Providers;
using DbRefactor.Tools;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines
{
	[TestFixture]
	public class DataDumperTest : ProviderTestBase
	{
		[Test]
		[Ignore]
		public void DumpTest()
		{
			var factory =
				new SqlServerFactory(
					@"Data Source=.\SQLExpress;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");

			TransformationProvider provider = factory.GetProvider();
			var d = new DataDumper(provider, false);
			string result = d.Dump(true);
		}

		[Test]
		public void Could_generate_delete_statement_for_db_with_cyclic_dependencies()
		{
			Database.CreateTable("A")
				.Int("Id").PrimaryKey()
				.Int("BId").NotNull()
				.Execute();
			Database.CreateTable("B")
				.Int("Id").PrimaryKey()
				.Int("CId").NotNull()
				.Execute();
			Database.CreateTable("C")
				.Int("Id").PrimaryKey()
				.Int("AId")
				.Execute();

			Database.Table("A").Column("BId").AddForeignKeyTo("B", "Id");
			Database.Table("B").Column("CId").AddForeignKeyTo("C", "Id");
			Database.Table("C").Column("AId").AddForeignKeyTo("A", "Id");

			var factory = CreateFactory();
			var d = new DataDumper(factory.GetProvider(), false);
			string result = d.GenerateDeleteStatement();
			Console.Write(result);
		}

		[Test]
		public void Should_sort_tables_with_cyclic_dependency()
		{
			// data from foreign table should be deleted first
			var tables = new List<string> { "C", "A", "B" };//, "G", "F"};
			var relations = new List<ForeignKey>
			                	{
			                		new R {Foreign = "A", Primary = "B", ForeignNullable = false},
			                		new R {Foreign = "B", Primary = "C", ForeignNullable = false},
			                		new R {Foreign = "C", Primary = "A", ForeignNullable = true},
			                		//new R {Foreign = "G", Primary = "F", ForeignNullable = false}
			                	};
			var result = DependencySorter.Sort(tables, relations);
			CollectionAssert.AreEqual(new[] {"A", "B", "C"}, result);
		}

		[Test]
		public void Should_sort_tables_with_all_weak_cyclic_dependency()
		{
			var tables = new List<string> {"A", "B", "C", "G", "F"};
			var relations = new List<ForeignKey>
			                	{
			                		new R {Foreign = "A", Primary = "B", ForeignNullable = true},
			                		new R {Foreign = "B", Primary = "C", ForeignNullable = true},
			                		new R {Foreign = "C", Primary = "A", ForeignNullable = true},
			                		new R {Foreign = "G", Primary = "F", ForeignNullable = false}
			                	};
			DependencySorter.Sort(tables, relations);

			// should not be in infinite cycle
		}


		internal class R : ForeignKey
		{
			public R()
			{
			}

			public string Foreign
			{
				get { return ForeignTable; }
				set { ForeignTable = value; }
			}

			public string Primary
			{
				get { return PrimaryTable; }
				set { PrimaryTable = value; }
			}
		}
	}
}