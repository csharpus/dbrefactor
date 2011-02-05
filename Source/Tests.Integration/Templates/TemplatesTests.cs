using System.Collections.Generic;
using DbRefactor.Infrastructure;
using DbRefactor.Providers;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Templates
{
	[TestFixture]
	public class TemplatesTests
	{
		[Test]
		public void should_format_data()
		{
			const string name = "test";
			var result = new TemplateParser().Add("select {0}", name).Apply();
			Assert.That(result, Is.EqualTo("select test"));
		}

		[Test]
		public void should_skip_data_when_any_argument_is_null()
		{
			string name = null;
			var result = new TemplateParser().Add("select {0}", name).Apply();
			Assert.That(result, Is.EqualTo(""));
		}

		[Test]
		public void should_skip_data_when_any_argument_is_false()
		{
			const bool name = false;
			var result = new TemplateParser().Add("select {0}", name).Apply();
			Assert.That(result, Is.EqualTo(""));
		}

		[Test]
		public void should_skip_data_when_any_argument_is_collection_without_any_elements()
		{
			var name = new List<string>();
			var result = new TemplateParser().Add("select {0}", name).Apply();
			Assert.That(result, Is.EqualTo(""));
		}

		//[Test]
		//public void should_not_include_lines_where_value_is_null()
		//{
		//    var values = Parser.Add("select", name).Add("from", name).Add("where values")


		//        // how to compile query

		//    var templateParser = new TemplateParser();
		//    var result = templateParser.Apply(template, new SelectModel {Column = "name", Values = null});
		//    Assert.That(result, Is.EqualTo("select name"));
		//}

		public class SelectModel
		{
			public string Column { get; set; }

			public string Values { get; set; }
		}
	}
}