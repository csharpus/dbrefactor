using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DbRefactor.Providers.Columns
{
	public abstract class ColumnProvider
	{
		private readonly object defaultValue;
		private readonly ICodeGenerationService codeGenerationService;
		private readonly List<PropertyProvider> properties = new List<PropertyProvider>();

		protected ColumnProvider(string name, object defaultValue, ICodeGenerationService codeGenerationService)
		{
			this.defaultValue = defaultValue;
			this.codeGenerationService = codeGenerationService;
			Name = name;
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
	}
}