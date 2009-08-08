namespace DbRefactor.Tools.Loggers
{
	public class ConsoleLogger : Logger
	{
		public ConsoleLogger() : base(true, new ConsoleWriter())
		{
			
		}
	}
}
