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
		[Ignore(
			"This value can be inserted - 0xC9CBBBCCCEB9C8CABCCCCEB9C9CBBB, but we need to check order of numbers when converting bytes to hex. It is better to insert and select one pixel png file"
			)]
		public void can_generate_binary_sql()
		{
			Database.CreateTable("A").Binary("B", new byte[] {1}).Execute();
		}

		[Test]
		public void can_generate_datetime_sql()
		{
			Database.CreateTable("A").DateTime("B", new DateTime(2000, 1, 2, 12, 34, 56)).Execute();
		}

		[Test]
		public void can_generate_decimal_sql()
		{
			Database.CreateTable("A").Decimal("B", 1.5M).Execute();
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