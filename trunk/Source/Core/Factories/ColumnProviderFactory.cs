using System;
using DbRefactor.Engines;
using DbRefactor.Infrastructure;
using DbRefactor.Providers;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Factories
{
	internal class ColumnProviderFactory
	{
		private readonly ICodeGenerationService codeGenerationService;
		private readonly ISqlTypes sqlTypes;
		private readonly ISqlGenerationService sqlGenerationService;
		private readonly ColumnPropertyProviderFactory columnPropertyProviderFactory;

		public ColumnProviderFactory(ICodeGenerationService codeGenerationService, ISqlTypes sqlTypes,
		                             ISqlGenerationService sqlGenerationService,
		                             ColumnPropertyProviderFactory columnPropertyProviderFactory)
		{
			this.codeGenerationService = codeGenerationService;
			this.sqlTypes = sqlTypes;
			this.sqlGenerationService = sqlGenerationService;
			this.columnPropertyProviderFactory = columnPropertyProviderFactory;
		}

		public BinaryProvider CreateBinary(string name, byte[] defaultValue)
		{
			return new BinaryProvider(name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService,
			                          columnPropertyProviderFactory);
		}

		public BinaryProvider CreateBinary(string name)
		{
			return CreateBinary(name, null);
		}

		public BooleanProvider CreateBoolean(string name, bool? defaultValue)
		{
			return new BooleanProvider(name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService,
			                           columnPropertyProviderFactory);
		}

		public DateTimeProvider CreateDateTime(string name, DateTime? defaultValue)
		{
			return new DateTimeProvider(name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService,
			                            columnPropertyProviderFactory);
		}

		public DecimalProvider CreateDecimal(string name, decimal? defaultValue, int precision, int radix)
		{
			return new DecimalProvider(name, defaultValue, precision, radix,
			                           codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public DoubleProvider CreateDouble(string name, double? defaultValue)
		{
			return new DoubleProvider(name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService,
			                          columnPropertyProviderFactory);
		}

		public FloatProvider CreateFloat(string name, float? defaultValue)
		{
			return new FloatProvider(name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService,
			                         columnPropertyProviderFactory);
		}

		public IntProvider CreateInt(string name, int? defaultValue)
		{
			return new IntProvider(name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService,
			                       columnPropertyProviderFactory);
		}

		public LongProvider CreateLong(string name, long? defaultValue)
		{
			return new LongProvider(name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService,
			                        columnPropertyProviderFactory);
		}

		public StringProvider CreateString(string name, string defaultValue, int length)
		{
			return new StringProvider(name, defaultValue, length, codeGenerationService, sqlTypes, sqlGenerationService,
			                          columnPropertyProviderFactory);
		}

		public TextProvider CreateText(string name, string defaultValue)
		{
			return new TextProvider(name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService,
			                        columnPropertyProviderFactory);
		}
	}
}