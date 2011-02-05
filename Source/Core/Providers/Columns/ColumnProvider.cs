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
using System.Linq.Expressions;
using DbRefactor.Api;
using DbRefactor.Exceptions;
using DbRefactor.Infrastructure;
using DbRefactor.Providers.Properties;

namespace DbRefactor.Providers.Columns
{
	public abstract class ColumnProvider
	{
		private readonly ICodeGenerationService codeGenerationService;
		private readonly List<PropertyProvider> properties = new List<PropertyProvider>();

		protected ColumnProvider(string name, object defaultValue)
		{
			DefaultValue = defaultValue;
			codeGenerationService = new CodeGenerationService();
			Name = name;

			identityProvider = new EmptyProvider();
			notNullProvider = new EmptyProvider();
			primaryKeyProvider = new EmptyProvider();
			uniqueProvider = new EmptyProvider();
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

		public virtual string MethodCode()
		{
			return Method().Name + "(" + "\"" + Name + "\"" + ")";
		}

		public virtual string[] MethodArguments()
		{
			return new [] { "\"" + Name + "\""};
		}

		public string MethodName()
		{
			return ((MethodCallExpression) Method().Body).Method.Name;
		}

		private void AddProperty(PropertyProvider provider)
		{
			Properties.Add(provider);
		}

		private PropertyProvider identityProvider;

		public void AddIdentity()
		{
			identityProvider = new IdentityProvider();
			AddProperty(identityProvider);
		}

		public void RemoveIdentity()
		{
			RemoveProperty(identityProvider);
			identityProvider = new EmptyProvider();
		}

		public bool IsIdentity
		{
			get { return !(identityProvider is EmptyProvider); }
		}

		public bool IsPrimaryKey
		{
			get { return !(primaryKeyProvider is EmptyProvider); }
		}

		public bool IsNull
		{
			get { return notNullProvider is NullProvider || notNullProvider == null || notNullProvider is EmptyProvider; }
		}

		public bool IsNotNull
		{
			get { return !IsNull; }
		}

		public bool IsUnique
		{
			get { return !(uniqueProvider is EmptyProvider); }
		}

		private void RemoveProperty(PropertyProvider provider)
		{
			properties.Remove(provider);
		}

		private PropertyProvider notNullProvider;

		public void AddNotNull()
		{
			notNullProvider = new NotNullProvider();
			AddProperty(notNullProvider);
		}

		public void RemoveNotNull()
		{
			//TODO: should add NULL definition because default behaviour is different
			RemoveProperty(notNullProvider);
			notNullProvider = new NullProvider();
			AddProperty(notNullProvider);
		}

		private PropertyProvider primaryKeyProvider;

		public void AddPrimaryKey(string name)
		{
			primaryKeyProvider = new PrimaryKeyProvider(name);
			AddProperty(primaryKeyProvider);
		}

		public void RemovePrimaryKey()
		{
			RemoveProperty(primaryKeyProvider);
			primaryKeyProvider = new EmptyProvider();
		}

		private PropertyProvider uniqueProvider;
		public string PrimaryKeyName { get
		{
			return (primaryKeyProvider is PrimaryKeyProvider)
			       	? ((PrimaryKeyProvider) primaryKeyProvider).Name
			       	: string.Empty;
		}}

		public string UniqueName
		{
			get { return (uniqueProvider is UniqueProvider)
				? ((UniqueProvider) uniqueProvider).Name
				: string.Empty; }
		}

		public void AddUnique(string name)
		{
			uniqueProvider = new UniqueProvider(name);
			AddProperty(uniqueProvider);
		}

		public void RemoveUnique()
		{
			RemoveProperty(uniqueProvider);
			uniqueProvider = new EmptyProvider();
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

		public void CopyPropertiesFrom(ColumnProvider provider)
		{
			foreach (var property in provider.Properties)
			{
				AddProperty(property);
			}
		}
	}
}