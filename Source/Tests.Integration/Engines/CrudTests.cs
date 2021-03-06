using System;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines
{
	[TestFixture]
	public class CrudTests : ProviderTestBase
	{
		[Test]
		public void can_insert_record()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Insert(new { B = 1, C = 1 });
		}

		[Test]
		public void can_update_record()
		{
			Database.CreateTable("A").Int("B").NotNull().Execute();
			Database.Table("A").Where(new { B = 2 }).Update(new { B = 1 });
		}

		[Test]
		public void can_use_db_null_in_where_clause()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Where(new { B = DBNull.Value }).Update(new { B = 1 });
		}

		[Test]
		public void can_use_db_null_in_update_clause()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Where(new { B = 1 }).Update(new { B = DBNull.Value });
		}

		[Test]
		public void can_use_where_clause_with_several_items()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Where(new { B = 1, C = 2 }).Update(new { B = DBNull.Value });
		}

		[Test]
		public void can_use_select_scalar()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Insert(new { B = 1, C = 1 });
			Database.Table("A").Insert(new { B = 2, C = 2 });
			Database.Table("A").Where(new { C = 2 }).SelectScalar<int>("B");
		}

		[Test]
		public void can_select_from_table()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Insert(new { B = 1, C = 1 });
			Database.Table("A").Insert(new { B = 2, C = 2 });
			using(var reader = Database.Table("A").Where(new {C = 2}).Select("B"))
			{
				reader.Read();
				Assert.That(reader["B"], Is.EqualTo(2));
			}
		}

		[Test]
		public void can_select_scalar_from_table()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Table("A").Insert(new {B = 1, C = 1});
			var value = Database.Table("A").Where(new { C = 1 }).SelectScalar<int>("B");
			Assert.That(value, Is.EqualTo(1));
		}

		[Test]
		public void can_delete_record()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Insert(new { B = 1 });
			Database.Table("A").Where(new { B = 1 }).Delete();
		}

		[Test]
		public void can_execute_non_query()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Execute().NonQuery("drop table A");
			Assert.False(SchemaHelper.TableExists("A"));
		}

		[Test]
		public void can_execute_query()
		{
			Database.CreateTable("A").Int("B").Execute();
			Database.Table("A").Insert(new { B = 1 });
			using(var reader = Database.Execute().Query("select B from A"))
			{
				Assert.True(reader.Read());
			}
		}

		[Test]
		public void can_execute_scalar()
		{
			Database.CreateTable("A").Int("B").Int("C").Execute();
			Database.Execute().NonQuery("alter table A drop column C");
			Assert.False(SchemaHelper.ColumnExists("A", "C"));
		}
	}
}