using DbRefactor.Providers;

namespace DbRefactor.Tests.Tools
{
	using NUnit.Framework;
	using DbRefactor.Tools;

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
