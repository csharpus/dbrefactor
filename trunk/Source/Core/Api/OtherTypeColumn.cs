using DbRefactor.Factories;
using DbRefactor.Providers;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Api
{
	public class OtherTypeColumn
	{
		private readonly string tableName;
		private readonly string columnName;
		private readonly ColumnProviderFactory factory;
		private readonly TransformationProvider provider;

		internal OtherTypeColumn(string tableName, string columnName, ColumnProviderFactory factory,
		                       TransformationProvider provider)
		{
			this.tableName = tableName;
			this.columnName = columnName;
			this.factory = factory;
			this.provider = provider;
		}

		#region Column types

		public void String(int size)
		{
			AlterColumn(factory.CreateString(columnName, null, size));
		}

		public void Text()
		{
			AlterColumn(factory.CreateText(columnName, null));
		}

		public void Int()
		{
			AlterColumn(factory.CreateInt(columnName, null));
		}

		public void Long()
		{
			AlterColumn(factory.CreateLong(columnName, null));
		}

		public void DateTime()
		{
			AlterColumn(factory.CreateDateTime(columnName, null));
		}

		public void Decimal()
		{
			AlterColumn(factory.CreateDecimal(columnName, null, 18, 9)); // change this value
		}

		public void Decimal(int whole, int remainder)
		{
			AlterColumn(factory.CreateDecimal(columnName, null, whole, remainder));
		}

		public void Boolean()
		{
			AlterColumn(factory.CreateBoolean(columnName, null));
		}

		private void AlterColumn(ColumnProvider columnProvider)
		{
			provider.AlterColumn(tableName, columnProvider);
		}

		#endregion Column types
	}
}