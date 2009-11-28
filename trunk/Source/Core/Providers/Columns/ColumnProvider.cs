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
using DbRefactor.Api;
using DbRefactor.Engines;
using DbRefactor.Exceptions;
using DbRefactor.Factories;
using DbRefactor.Infrastructure;
using DbRefactor.Providers.Properties;

namespace DbRefactor.Providers.Columns
{
	internal abstract class ColumnProvider
	{
		private readonly ICodeGenerationService codeGenerationService;
		private readonly ISqlTypes sqlTypes;
		private readonly ISqlGenerationService sqlGenerationService;
		private readonly ColumnPropertyProviderFactory columnPropertyProviderFactory;
		private readonly List<PropertyProvider> properties = new List<PropertyProvider>();

		protected ColumnProvider(string name, object defaultValue, ICodeGenerationService codeGenerationService,
		                         ISqlTypes sqlTypes, ISqlGenerationService sqlGenerationService, ColumnPropertyProviderFactory columnPropertyProviderFactory)
		{
			DefaultValue = defaultValue;
			this.codeGenerationService = codeGenerationService;
			this.sqlTypes = sqlTypes;
			this.sqlGenerationService = sqlGenerationService;
			this.columnPropertyProviderFactory = columnPropertyProviderFactory;
			Name = name;

			identityProvider = columnPropertyProviderFactory.CreateEmpty();
			notNullProvider = columnPropertyProviderFactory.CreateEmpty();
			primaryKeyProvider = columnPropertyProviderFactory.CreateEmpty();
			uniqueProvider = columnPropertyProviderFactory.CreateEmpty();
		}

		public ISqlTypes SqlTypes
		{
			get { return sqlTypes; }
		}

		public string Name { get; private set; }

		public List<PropertyProvider> Properties
		{
			get { return properties; }
		}

		public object DefaultValue { get; set; }

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

		private void AddProperty(PropertyProvider provider)
		{
			Properties.Add(provider);
		}

		private PropertyProvider identityProvider;

		public PropertyProvider Identity
		{
			get { return identityProvider; }
		}

		public void AddIdentity()
		{
			identityProvider = columnPropertyProviderFactory.CreateIdentity();
			AddProperty(identityProvider);
		}

		public void RemoveIdentity()
		{
			RemoveProperty(identityProvider);
			identityProvider = columnPropertyProviderFactory.CreateEmpty();
		}

		private void RemoveProperty(PropertyProvider provider)
		{
			properties.Remove(provider);
		}

		private PropertyProvider notNullProvider;

		public PropertyProvider NotNull
		{
			get { return notNullProvider; }
		}

		public void AddNotNull()
		{
			notNullProvider = columnPropertyProviderFactory.CreateNotNull();
			AddProperty(notNullProvider);
		}

		public void RemoveNotNull()
		{
			//TODO: should add NULL definition because default behaviour is different
			RemoveProperty(notNullProvider);
			notNullProvider = columnPropertyProviderFactory.CreateNull();
			AddProperty(notNullProvider);
		}

		private PropertyProvider primaryKeyProvider;

		public PropertyProvider PrimaryKey
		{
			get { return primaryKeyProvider; }
		}

		public void AddPrimaryKey(string name)
		{
			primaryKeyProvider = columnPropertyProviderFactory.CreatePrimaryKey(name);
			AddProperty(primaryKeyProvider);
		}

		public void RemovePrimaryKey()
		{
			RemoveProperty(primaryKeyProvider);
			primaryKeyProvider = columnPropertyProviderFactory.CreateEmpty();
		}

		private PropertyProvider uniqueProvider;

		public PropertyProvider Unique
		{
			get { return uniqueProvider; }
		}

		public void AddUnique(string name)
		{
			uniqueProvider = columnPropertyProviderFactory.CreateUnique(name);
			AddProperty(uniqueProvider);
		}

		public void RemoveUnique()
		{
			RemoveProperty(uniqueProvider);
			uniqueProvider = columnPropertyProviderFactory.CreateEmpty();
		}

		public string GetDefaultValueSql()
		{
			if (!HasDefaultValue)
			{
				throw new DbRefactorException("could not generate code because default value is null");
			}
			return DefaultValueSql();
		}

		protected string DefaultValueSql()
		{
			return ValueSql(DefaultValue);
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

		public string GetCreateColumnSql()
		{
			return sqlGenerationService.GenerateCreateColumnSql(this);
		}

		public string GetAddColumnSql()
		{
			return sqlGenerationService.GenerateAddColumnSql(this);
		}

		public string GetAlterColumnSql()
		{
			return sqlGenerationService.GenerateAlterColumnSql(this);
		}

		public string GetValueSql(object value)
		{
			if (value == null || value == DBNull.Value)
			{
				return SqlTypes.NullValue();
			}
			return ValueSql(value);
		}

		protected virtual string ValueSql(object value)
		{
			return Convert.ToString(value, CultureInfo.InvariantCulture);
		}

		public void CopyPropertiesFrom(ColumnProvider provider)
		{
			foreach (var property in provider.Properties)
			{
				AddProperty(property);
			}
		}
	}
}