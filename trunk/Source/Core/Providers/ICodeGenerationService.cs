using System;

namespace DbRefactor.Providers
{
	public interface ICodeGenerationService
	{
		string PrimitiveValue(object value);
		string DateTimeValue(DateTime dateTime);
		string BinaryValue(byte[] bytes);
	}
}