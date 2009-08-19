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
			TransformationProvider provider =
				new ProviderFactory().Create(@"Data Source=.\SQLExpress;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");
			var d = new DataDumper(provider);
			string result = d.Dump(true);
		}
	}
}