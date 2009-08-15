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
	public class StringProvider : ColumnProvider
	{
		private readonly int size;

		public StringProvider(string name, object defaultValue, int size, ICodeGenerationService codeGenerationService, ISqlTypes sqlTypes, ISqlGenerationService sqlGenerationService)
			: base(name, defaultValue, codeGenerationService, sqlTypes, sqlGenerationService)
		{
			this.size = size;
		}

		public int Size
		{
			get { return size; }
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.String(Name, Size);
		}

		public override string SqlType()
		{
			return SQLTypes.String(Size);
		}

		protected override string ValueSql(object value)
		{
			return SQLTypes.StringValue((string)value);
		}
	}
}