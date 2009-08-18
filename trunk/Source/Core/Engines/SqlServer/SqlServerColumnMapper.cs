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
using DbRefactor.Infrastructure;
using DbRefactor.Providers;
using DbRefactor.Providers.Columns;
using DbRefactor.Providers.Properties;

namespace DbRefactor.Engines.SqlServer
{
	public class SqlServerColumnMapper
	{
		private readonly ICodeGenerationService codeGenerationService;
		private readonly ISqlTypes sqlTypes;
		private readonly ISqlGenerationService sqlGenerationService;
		private readonly ColumnPropertyProviderFactory columnPropertyProviderFactory;

		public SqlServerColumnMapper(ICodeGenerationService codeGenerationService, ISqlTypes sqlTypes, ISqlGenerationService sqlGenerationService, ColumnPropertyProviderFactory columnPropertyProviderFactory)
		{
			this.codeGenerationService = codeGenerationService;
			this.sqlTypes = sqlTypes;
			this.sqlGenerationService = sqlGenerationService;
			this.columnPropertyProviderFactory = columnPropertyProviderFactory;
		}

		public BinaryProvider CreateBinary(ColumnData data)
		{
			return new BinaryProvider(data.Name, data.DefaultValue, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public BooleanProvider CreateBoolean(ColumnData data)
		{
			return new BooleanProvider(data.Name, data.DefaultValue, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public DateTimeProvider CreateDateTime(ColumnData data)
		{
			return new DateTimeProvider(data.Name, data.DefaultValue, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public DecimalProvider CreateDecimal(ColumnData data)
		{
			return new DecimalProvider(data.Name, data.DefaultValue, data.Precision.Value, data.Radix.Value,
			                           codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public DoubleProvider CreateDouble(ColumnData data)
		{
			return new DoubleProvider(data.Name, data.DefaultValue, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public FloatProvider CreateFloat(ColumnData data)
		{
			return new FloatProvider(data.Name, data.DefaultValue, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public IntProvider CreateInt(ColumnData data)
		{
			return new IntProvider(data.Name, data.DefaultValue, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public LongProvider CreateLong(ColumnData data)
		{
			return new LongProvider(data.Name, data.DefaultValue, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public StringProvider CreateString(ColumnData data)
		{
			int length = data.Length == null ? 10 : data.Length.Value;
			return new StringProvider(data.Name, data.DefaultValue, length, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public TextProvider CreateText(ColumnData data)
		{
			return new TextProvider(data.Name, data.DefaultValue, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}
	}

	public class ColumnProviderFactory
	{
		private readonly ICodeGenerationService codeGenerationService;
		private readonly ISqlTypes sqlTypes;
		private readonly ISqlGenerationService sqlGenerationService;
		private readonly ColumnPropertyProviderFactory columnPropertyProviderFactory;

		public ColumnProviderFactory(ICodeGenerationService codeGenerationService, ISqlTypes sqlTypes, ISqlGenerationService sqlGenerationService, ColumnPropertyProviderFactory columnPropertyProviderFactory)
		{
			this.codeGenerationService = codeGenerationService;
			this.sqlTypes = sqlTypes;
			this.sqlGenerationService = sqlGenerationService;
			this.columnPropertyProviderFactory = columnPropertyProviderFactory;
		}

		public BinaryProvider CreateBinary(string name, byte[] defaultValue)
		{
			return new BinaryProvider(name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public BinaryProvider CreateBinary(string name)
		{
			return CreateBinary(name, null);
		}

		public BooleanProvider CreateBoolean(string name, bool? defaultValue)
		{
			return new BooleanProvider(name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public DateTimeProvider CreateDateTime(string name, DateTime? defaultValue)
		{
			return new DateTimeProvider(name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public DecimalProvider CreateDecimal(string name, decimal? defaultValue, int precision, int radix)
		{
			return new DecimalProvider(name, defaultValue, precision, radix,
			                           codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public DoubleProvider CreateDouble(string name, double? defaultValue)
		{
			return new DoubleProvider(name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public FloatProvider CreateFloat(string name, float? defaultValue)
		{
			return new FloatProvider(name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public IntProvider CreateInt(string name, int? defaultValue)
		{
			return new IntProvider(name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public LongProvider CreateLong(string name, long? defaultValue)
		{
			return new LongProvider(name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public StringProvider CreateString(string name, string defaultValue, int length)
		{
			return new StringProvider(name, defaultValue, length, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public TextProvider CreateText(string name, string defaultValue)
		{
			return new TextProvider(name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}
	}

	public class ColumnPropertyProviderFactory
	{
		private readonly IColumnProperties columnProperties;

		public ColumnPropertyProviderFactory(IColumnProperties columnProperties)
		{
			this.columnProperties = columnProperties;
		}

		public NotNullProvider CreateNotNull()
		{
			return new NotNullProvider(columnProperties);
		}

		public PrimaryKeyProvider CreatePrimaryKey(string name)
		{
			return new PrimaryKeyProvider(name, columnProperties);
		}

		public UniqueProvider CreateUnique(string name)
		{
			return new UniqueProvider(name, columnProperties);
		}

		public IdentityProvider CreateIdentity()
		{
			return new IdentityProvider(columnProperties);
		}

		public EmptyProvider CreateEmpty()
		{
			return new EmptyProvider(columnProperties);
		}
	}
}