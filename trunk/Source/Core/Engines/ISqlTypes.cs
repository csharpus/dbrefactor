#region License

//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.

#endregion

using System;

namespace DbRefactor.Engines
{
	internal interface ISqlTypes
	{
		string Binary();
		string BinaryValue(byte[] value);
		string Boolean();
		string BooleanValue(bool value);
		string DateTime();
		string DateTimeValue(DateTime dateTime);
		string Decimal(int precision, int scale);
		string DecimalValue(decimal value);
		string Double();
		string DoubleValue(double value);
		string Float();
		string FloatValue(float value);
		string Int();
		string IntValue(int value);
		string Long();
		string LongValue(long value);
		string String(int size);
		string StringValue(string value);
		string Text();
		string TextValue(string value);
		string NullValue();
	}
}