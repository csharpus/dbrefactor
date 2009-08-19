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

using DbRefactor.Factories;
using DbRefactor.Infrastructure;
using DbRefactor.Providers;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Engines.SqlServer
{
	public class SqlServerColumnMapper
	{
		private readonly ICodeGenerationService codeGenerationService;
		private readonly ISqlTypes sqlTypes;
		private readonly ISqlGenerationService sqlGenerationService;
		private readonly ColumnPropertyProviderFactory columnPropertyProviderFactory;

		public SqlServerColumnMapper(ICodeGenerationService codeGenerationService, ISqlTypes sqlTypes,
		                             ISqlGenerationService sqlGenerationService,
		                             ColumnPropertyProviderFactory columnPropertyProviderFactory)
		{
			this.codeGenerationService = codeGenerationService;
			this.sqlTypes = sqlTypes;
			this.sqlGenerationService = sqlGenerationService;
			this.columnPropertyProviderFactory = columnPropertyProviderFactory;
		}

		public BinaryProvider CreateBinary(ColumnData data)
		{
			return new BinaryProvider(data.Name, data.DefaultValue, codeGenerationService, sqlTypes, sqlGenerationService,
			                          columnPropertyProviderFactory);
		}

		public BooleanProvider CreateBoolean(ColumnData data)
		{
			return new BooleanProvider(data.Name, data.DefaultValue, codeGenerationService, sqlTypes, sqlGenerationService,
			                           columnPropertyProviderFactory);
		}

		public DateTimeProvider CreateDateTime(ColumnData data)
		{
			return new DateTimeProvider(data.Name, data.DefaultValue, codeGenerationService, sqlTypes, sqlGenerationService,
			                            columnPropertyProviderFactory);
		}

		public DecimalProvider CreateDecimal(ColumnData data)
		{
			return new DecimalProvider(data.Name, data.DefaultValue, data.Precision.Value, data.Radix.Value,
			                           codeGenerationService, sqlTypes, sqlGenerationService, columnPropertyProviderFactory);
		}

		public DoubleProvider CreateDouble(ColumnData data)
		{
			return new DoubleProvider(data.Name, data.DefaultValue, codeGenerationService, sqlTypes, sqlGenerationService,
			                          columnPropertyProviderFactory);
		}

		public FloatProvider CreateFloat(ColumnData data)
		{
			return new FloatProvider(data.Name, data.DefaultValue, codeGenerationService, sqlTypes, sqlGenerationService,
			                         columnPropertyProviderFactory);
		}

		public IntProvider CreateInt(ColumnData data)
		{
			return new IntProvider(data.Name, data.DefaultValue, codeGenerationService, sqlTypes, sqlGenerationService,
			                       columnPropertyProviderFactory);
		}

		public LongProvider CreateLong(ColumnData data)
		{
			return new LongProvider(data.Name, data.DefaultValue, codeGenerationService, sqlTypes, sqlGenerationService,
			                        columnPropertyProviderFactory);
		}

		public StringProvider CreateString(ColumnData data)
		{
			int length = data.Length == null ? 10 : data.Length.Value;
			return new StringProvider(data.Name, data.DefaultValue, length, codeGenerationService, sqlTypes, sqlGenerationService,
			                          columnPropertyProviderFactory);
		}

		public TextProvider CreateText(ColumnData data)
		{
			return new TextProvider(data.Name, data.DefaultValue, codeGenerationService, sqlTypes, sqlGenerationService,
			                        columnPropertyProviderFactory);
		}
	}
}