namespace DbRefactor.Engines.SqlServer
{
	static class NameEncoderHelper
	{
		public static string Encode(string name)
		{
			return "[" + name + "]";
		}
	}
}
