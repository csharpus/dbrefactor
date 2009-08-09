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
using DbRefactor.Providers.TypeToSqlProviders;

namespace DbRefactor.Providers.Columns
{
	public class NotNullProvider : PropertyProvider
	{
		public NotNullProvider(IColumnProperties columnProperties) : base(columnProperties)
		{
		}

		public override Expression<Action<NewTable>> Method()
		{
			return t => t.NotNull();
		}

		public override string CreateTableSql()
		{
			return ColumnProperties.NotNull();
		}

		public override string AlterTableSql()
		{
			return ColumnProperties.NotNull();
		}

		public override string AddTableSql()
		{
			return ColumnProperties.NotNull();
		}
	}
}