using System;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration
{
	[TestFixture]
	public class CrudTests : ProviderTestBase
	{
		[Test]
		public void Can_update_table()
		{
			Database.CreateTable("A").Int("B").NotNull().Execute();
			Database.Table("A").Update(new { B = 1 }).Where(new { B = 2 }).Execute();
		}

		[Test]
		public void Can_use_null_in_where_clause()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Update(new { B = 1 }).Where(new { B = DBNull.Value }).Execute();
		}

		[Test]
		public void Can_use_null_in_update_clause()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Update(new { B = DBNull.Value }).Where(new { B = 1 }).Execute();
		}

		[Test]
		public void Can_use_where_with_several_items()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Update(new { B = DBNull.Value }).Where(new { B = 1, C = 2 }).Execute();
		}

		[Test]
		public void Can_use_insert()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Insert(new { B = 1, C = 1 });
		}

		[Test]
		public void Can_use_select_scalar()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Insert(new { B = 1, C = 1 });
			Database.Table("A").Insert(new { B = 2, C = 2 });
			Database.Table("A").SelectScalar<int>("B").Where(new { C = 2 }).Execute();
		}

		[Test]
		public void Can_delete_record()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Insert(new { B = 1 });
			Database.Table("A").Delete().Where(new { B = 1 }).Execute();
		}
	}
}