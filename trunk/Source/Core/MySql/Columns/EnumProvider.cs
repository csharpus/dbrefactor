using System;
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Engines;
using DbRefactor.Factories;
using DbRefactor.Infrastructure;
using DbRefactor.Providers;
using DbRefactor.Providers.Columns;

namespace DbRefactor.MySql.Columns
{
	internal class EnumProvider : ColumnProvider
	{
		private readonly string[] values;

		public EnumProvider(string name, object defaultValue, string[] values, ICodeGenerationService codeGenerationService,
		                    ISqlTypes sqlTypes, ISqlGenerationService sqlGenerationService,
		                    ColumnPropertyProviderFactory columnPropertyProviderFactory)
			: base(
				name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService,
				columnPropertyProviderFactory)
		{
			this.values = values;
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.Enum(new string[] {});
		}

		public override string SqlType()
		{
			return String.Format("ENUM('{0}')", String.Join("', '", values));
		}
	}
}