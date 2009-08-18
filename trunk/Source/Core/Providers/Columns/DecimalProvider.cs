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
using DbRefactor.Engines.SqlServer;
using DbRefactor.Infrastructure;

namespace DbRefactor.Providers.Columns
{
	public class DecimalProvider : ColumnProvider
	{
		private readonly int precision;
		private readonly int radix;

		public DecimalProvider(string name, object defaultValue, int precision, int radix, ICodeGenerationService codeGenerationService, ISqlTypes sqlTypes, ISqlGenerationService sqlGenerationService, ColumnPropertyProviderFactory columnPropertyProviderFactory)
			: base(name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory)
		{
			this.precision = precision;
			this.radix = radix;
		}

		public int Radix
		{
			get { return radix; }
		}

		public int Precision
		{
			get { return precision; }
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Decimal(Name, Precision, Radix);
		}

		public override string SqlType()
		{
			return SQLTypes.Decimal(precision, radix);
		}
	}
}