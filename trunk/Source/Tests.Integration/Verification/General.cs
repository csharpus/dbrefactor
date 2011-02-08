using System.IO;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Verification
{
	[TestFixture]
	public class General
	{
		[Test]
		public void should_format_string_with_empty_argument_list()
		{
			string.Format("test {{0}}", new string[] {});
		}

		[Test]
		public void reading_writing_binary_data()
		{
			var bytes = File.ReadAllBytes("../../marker.png");
			File.WriteAllBytes("new_marker.png", bytes);
		}
	}
}
