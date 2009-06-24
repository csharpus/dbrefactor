using DbRefactor.Tools;
using DbRefactor.Tools;
using DbRefactor.Tools;

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
			DataDumper d = new DataDumper(@"Data Source=.\SQLExpress;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");
			string result = d.Dump(true);
		}
	}
}
