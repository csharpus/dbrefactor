using NUnit.Framework;
using DbRefactor.Tools;
namespace DbRefactor.Tests.Tools
{
	[TestFixture]
	public class DataDumperTest
	{
		[Test]
		public void DumpTest()
		{
			DataDumper d = new DataDumper(@"Data Source=.\SQLExpress;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");
			string result = d.Dump(true);
		}
	}
}
