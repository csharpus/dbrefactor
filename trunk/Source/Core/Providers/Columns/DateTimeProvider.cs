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
	public class DateTimeProvider : ColumnProvider
	{
		public DateTimeProvider(string name, object defaultValue, ICodeGenerationService codeGenerationService, ISqlTypes sqlTypes, ISqlGenerationService sqlGenerationService, ColumnPropertyProviderFactory columnPropertyProviderFactory)
			: base(name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory)
		{
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.DateTime(Name);
		}

		protected override string DefaultValueCode()
		{
			return CodeGenerationService.DateTimeValue((DateTime) DefaultValue);
		}

		public override string SqlType()
		{
			return SQLTypes.DateTime();
		}

		protected override string ValueSql(object value)
		{
			return SQLTypes.DateTimeValue((DateTime) value);
		}
	}
}