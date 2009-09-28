using System;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration
{
	[TestFixture]
	public class CrudTests : ProviderTestBase
	{
		[Test]
		public void Can_insert_record()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Insert(new { B = 1, C = 1 });
		}

		[Test]
		public void Can_update_record()
		{
			Database.CreateTable("A").Int("B").NotNull().Execute();
			Database.Table("A").Update(new { B = 1 }).Where(new { B = 2 }).Execute();
		}

		[Test]
		public void Can_use_db_null_in_where_clause()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Update(new { B = 1 }).Where(new { B = DBNull.Value }).Execute();
		}

		[Test]
		public void Can_use_db_null_in_update_clause()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Update(new { B = DBNull.Value }).Where(new { B = 1 }).Execute();
		}

		[Test]
		public void Can_use_where_clause_with_several_items()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Update(new { B = DBNull.Value }).Where(new { B = 1, C = 2 }).Execute();
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
		public void Can_select_from_table()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Insert(new { B = 1, C = 1 });
			Database.Table("A").Insert(new { B = 2, C = 2 });
			using(var reader = Database.Table("A").Select("B").Where(new {C = 2}).Execute())
			{
				reader.Read();
				Assert.That(reader["B"], Is.EqualTo(2));
			}
		}

		[Test]
		public void Can_select_scalar_from_table()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Insert(new {B = 1, C = 1});
			int value = Database.Table("A").SelectScalar<int>("B").Where(new {C = 1}).Execute();
			Assert.That(value, Is.EqualTo(1));
		}

		[Test]
		public void Can_delete_record()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Insert(new { B = 1 });
			Database.Table("A").Delete().Where(new { B = 1 }).Execute();
		}

		[Test]
		public void Can_execute_non_query()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Execute().NonQuery("drop table A");
			Assert.False(Provider.TableExists("A"));
		}

		[Test]
		public void Can_execute_query()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Insert(new { B = 1 });
			using(var reader = Database.Execute().Query("select B from A"))
			{
				Assert.True(reader.Read());
			}
		}

		[Test]
		public void Can_execute_scalar()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Execute().NonQuery("alter table A drop column C");
			Assert.False(Provider.ColumnExists("A", "C"));
		}
	}
}