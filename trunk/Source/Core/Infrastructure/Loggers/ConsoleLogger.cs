namespace DbRefactor.Infrastructure.Loggers
{
	public class ConsoleLogger : Logger
	{
		public ConsoleLogger() : base(new ConsoleWriter())
		{
			
		}
	}
}