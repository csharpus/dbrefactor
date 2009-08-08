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
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using DbRefactor.Providers.TypeToSqlProviders;

namespace DbRefactor.Providers.Columns
{
	public abstract class ColumnProvider
	{
		private readonly object defaultValue;
		private readonly ICodeGenerationService codeGenerationService;
		private readonly ISqlTypes sqlTypes;
		private readonly ISqlGenerationService sqlGenerationService;
		private readonly List<PropertyProvider> properties = new List<PropertyProvider>();

		protected ColumnProvider(string name, object defaultValue, ICodeGenerationService codeGenerationService, ISqlTypes sqlTypes, ISqlGenerationService sqlGenerationService)
		{
			this.defaultValue = defaultValue;
			this.codeGenerationService = codeGenerationService;
			this.sqlTypes = sqlTypes;
			this.sqlGenerationService = sqlGenerationService;
			Name = name;
		}

		public ISqlTypes SQLTypes
		{
			get { return sqlTypes; }
		}

		public string Name { get; private set; }

		public List<PropertyProvider> Properties
		{
			get { return properties; }
		}

		protected object DefaultValue
		{
			get { return defaultValue; }
		}

		public bool HasDefaultValue
		{
			get { return DefaultValue != null; }
		}

		protected ICodeGenerationService CodeGenerationService
		{
			get { return codeGenerationService; }
		}

		public abstract Expression<Action<NewTable>> Method();

		public string MethodName()
		{
			return ((MethodCallExpression) Method().Body).Method.Name;
		}

		public void AddProperty(PropertyProvider provider)
		{
			Properties.Add(provider);
		}

		public string GetDefaultValueSql()
		{
			if (!HasDefaultValue)
			{
				throw new DbRefactorException("could not generate code because default value is null");
			}
			return DefaultValueSql();
		}

		protected virtual string DefaultValueSql()
		{
			return Convert.ToString(defaultValue, CultureInfo.InvariantCulture);
		}

		public string GetDefaultValueCode()
		{
			if (!HasDefaultValue)
			{
				throw new DbRefactorException("could not generate code because default value is null");
			}
			return DefaultValueCode();
		}

		protected virtual string DefaultValueCode()
		{
			return CodeGenerationService.PrimitiveValue(DefaultValue);
		}

		public abstract string SqlType();

		public string GetColumnSql()
		{
			return sqlGenerationService.GenerateColumnSql(this);
		}
	}
}