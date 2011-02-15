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
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Extended;

namespace DbRefactor.Providers.Columns
{
	// 123.4567
	// Precision = 7
	// Scale = 4
	public class NumericProvider : ColumnProvider
	{
		private readonly int precision;
		private readonly int scale;

		public NumericProvider(string name, object defaultValue, int precision, int scale)
			: base(
				name, defaultValue)
		{
			this.precision = precision;
			this.scale = scale;
		}

		public int Scale
		{
			get { return scale; }
		}

		public int Precision
		{
			get { return precision; }
		}

		public override string MethodCode()
		{
			return Method().Name + "(" + "\"" + Name + "\"" + ", " + Precision + ", " + Scale + ")";
		}

		public override string[] MethodArguments()
		{
			return new[] { "\"" + Name + "\"", Precision.ToString(), Scale.ToString() };
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Numeric(Name, Precision, Scale, null);
		}
	}
}