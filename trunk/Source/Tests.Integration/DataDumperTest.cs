using DbRefactor.Factories;
using DbRefactor.Providers;
using DbRefactor.Tools;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration
{
	[TestFixture]
	public class DataDumperTest
	{
		[Test]
		[Ignore]
		public void DumpTest()
		{
			var factory = new SqlServerFactory(@"Data Source=.\SQLExpress;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");
			
			TransformationProvider provider = factory.GetProvider();
			var d = new DataDumper(provider);
			string result = d.Dump(true);
		}
	}
}