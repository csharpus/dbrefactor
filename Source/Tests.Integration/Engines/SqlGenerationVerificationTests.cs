using System;
using DbRefactor.Extended;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Engines
{
	[TestFixture]
	public class SqlGenerationVerificationTests : ProviderTestBase
	{
		[Test]
		public void can_generate_boolean_sql()
		{
			Database.CreateTable("A").Boolean("B", true).Execute();
		}

		[Test]
		public void can_generate_binary_sql()
		{
			Database.CreateTable("A").Int("C").Binary("D", new byte[] {40, 41, 42}).Execute();

			Database.Table("A").Insert(new {C = 1});

			using (var reader = Database.Table("A").Select("D"))
			{
				reader.Read();
				var buffer = new byte[3];
				reader.GetBytes(0, 0, buffer, 0, 3);
				Assert.That(buffer[0], Is.EqualTo(40));
				Assert.That(buffer[1], Is.EqualTo(41));
				Assert.That(buffer[2], Is.EqualTo(42));
			}
		}

		[Test]
		public void can_generate_date_time_offset()
		{
			Database.CreateTable("A").Int("C")
				.DateTimeOffset("D", new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, -TimeSpan.FromMinutes(90))).Execute();

			Database.Table("A").Insert(new {C = 1});

			using(var reader = Database.Table("A").Select("D"))
			{
				reader.Read();
				var offset = (DateTimeOffset)reader.GetValue(0);
				Assert.That(offset.Year, Is.EqualTo(2000));
				Assert.That(offset.Month, Is.EqualTo(1));
				Assert.That(offset.Day, Is.EqualTo(1));
				Assert.That(offset.Hour, Is.EqualTo(0));
				Assert.That(offset.Minute, Is.EqualTo(0));
				Assert.That(offset.Second, Is.EqualTo(0));
				Assert.That(offset.Offset.Hours, Is.EqualTo(-1));
				Assert.That(offset.Offset.Minutes, Is.EqualTo(-30));
			}
		}

		[Test]
		public void can_generate_guid()
		{
			Database.CreateTable("A").Int("C")
				.Guid("D", new Guid("5F732BF3-6950-4FF4-B635-32C1B6199CD8")).Execute();

			Database.Table("A").Insert(new {C = 1});

			using(var reader = Database.Table("A").Select("D"))
			{
				reader.Read();
				var guid = reader.GetGuid(0);
				Assert.That(guid, Is.EqualTo(new Guid("5F732BF3-6950-4FF4-B635-32C1B6199CD8")));
			}
		}

		[Test]
		public void can_generate_time()
		{
			Database.CreateTable("A").Int("C")
				.Time("D", new TimeSpan(1, 30, 40)).Execute();

			Database.Table("A").Insert(new {C = 1});

			using(var reader = Database.Table("A").Select("D"))
			{
				reader.Read();

				var time = (TimeSpan)reader.GetValue(0);

				Assert.That(time.Hours, Is.EqualTo(1));
				Assert.That(time.Minutes, Is.EqualTo(30));
				Assert.That(time.Seconds, Is.EqualTo(40));
			}
		}

		[Test]
		public void can_generate_datetime_sql()
		{
			Database.CreateTable("A").DateTime("B", new DateTime(2000, 1, 2, 12, 34, 56)).Execute();
		}

		[Test]
		public void can_generate_decimal_sql()
		{
			Database.CreateTable("A").Int("C").Decimal("B", 1.5M).Execute();

			Database.Table("A").Insert(new {C = 1});

			using(var reader = Database.Table("A").Select("B"))
			{
				reader.Read();

				var number = reader.GetDecimal(0);

				Assert.That(number, Is.EqualTo(1.5M));
			}
		}

		[Test]
		public void can_generate_char_sql()
		{
			Database.CreateTable("A").Int("C").Char("B", 3, "абв").Execute();

			Database.Table("A").Insert(new { C = 1 });

			using (var reader = Database.Table("A").Select("B"))
			{
				reader.Read();

				var value = reader.GetString(0);

				Assert.That(value, Is.EqualTo("абв"));
			}
		}

		[Test]
		public void can_generate_nchar_sql()
		{
			Database.CreateTable("A").Int("C").NChar("B", 3, "абв").Execute();

			Database.Table("A").Insert(new { C = 1 });

			using (var reader = Database.Table("A").Select("B"))
			{
				reader.Read();

				var value = reader.GetString(0);

				Assert.That(value, Is.EqualTo("абв"));
			}
		}

		[Test]
		public void can_generate_date_sql()
		{
			Database.CreateTable("A").Int("C").Date("B", new DateTime(2000, 1, 1)).Execute();

			Database.Table("A").Insert(new { C = 1 });

			using (var reader = Database.Table("A").Select("B"))
			{
				reader.Read();

				var value = reader.GetDateTime(0);

				Assert.That(value.Year, Is.EqualTo(2000));
				Assert.That(value.Month, Is.EqualTo(1));
				Assert.That(value.Day, Is.EqualTo(1));
			}
		}


		[Test]
		public void can_generate_image()
		{
			Database.CreateTable("A").Int("C").Image("B", new byte[] {42, 43, 44}).Execute();

			Database.Table("A").Insert(new { C = 1 });

			using (var reader = Database.Table("A").Select("B"))
			{
				reader.Read();
				var buffer = new byte[3];
				reader.GetBytes(0, 0, buffer, 0, 3);
				Assert.That(buffer[0], Is.EqualTo(42));
				Assert.That(buffer[1], Is.EqualTo(43));
				Assert.That(buffer[2], Is.EqualTo(44));
			}
		}

		[Test]
		public void can_generate_double_sql()
		{
			Database.CreateTable("A").Double("B", 1.5).Execute();
		}

		[Test]
		public void can_generate_float_sql()
		{
			Database.CreateTable("A").Float("B", 1.5f).Execute();
		}

		[Test]
		public void can_generate_int_sql()
		{
			Database.CreateTable("A").Int("B", 1).Execute();
		}

		[Test]
		public void can_generate_long_sql()
		{
			Database.CreateTable("A").Long("B", 1).Execute();
		}

		[Test]
		public void can_generate_string_sql()
		{
			Database.CreateTable("A").String("B", 5, "hello").Execute();
		}
		
		[Test]
		public void can_generate_max_string_sql()
		{
			Database.CreateTable("A").String("B", Max.Value).Execute();
		}

		[Test]
		public void can_generate_string_sql_for_empty_string()
		{
			Database.CreateTable("A").String("B", 1, String.Empty).Execute();
		}

		[Test]
		public void can_generate_text_sql()
		{
			Database.CreateTable("A").Text("B", "hello").Execute();
		}
	}
}