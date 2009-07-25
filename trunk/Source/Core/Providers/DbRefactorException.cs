using System;
using System.Runtime.Serialization;

namespace DbRefactor.Providers
{
	public class DbRefactorException : Exception
	{
		public DbRefactorException()
		{
		}

		public DbRefactorException(string message) : base(message)
		{
		}

		public DbRefactorException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected DbRefactorException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}