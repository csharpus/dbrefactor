using DbRefactor.Providers;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Api
{
	public class OtherTypeColumn
	{
		private readonly string tableName;
		private readonly string columnName;
		private readonly TransformationProvider provider;

		internal OtherTypeColumn(string tableName, string columnName, 
		                       TransformationProvider provider)
		{
			this.tableName = tableName;
			this.columnName = columnName;
			this.provider = provider;
		}

		#region Column types

		public void String(int size)
		{
			AlterColumn(new StringProvider(columnName, null, size));
		}

		public void Text()
		{
			AlterColumn(new TextProvider(columnName, null));
		}

		public void Int()
		{
			AlterColumn(new IntProvider(columnName, null));
		}

		public void Long()
		{
			AlterColumn(new LongProvider(columnName, null));
		}

		public void DateTime()
		{
			AlterColumn(new DateTimeProvider(columnName, null));
		}

		public void Decimal()
		{
			AlterColumn(new DecimalProvider(columnName, null, 18, 9)); // change this value
		}

		public void Decimal(int whole, int remainder)
		{
			AlterColumn(new DecimalProvider(columnName, null, whole, remainder));
		}

		public void Boolean()
		{
			AlterColumn(new BooleanProvider(columnName, null));
		}

		private void AlterColumn(ColumnProvider columnProvider)
		{
			provider.AlterColumn(tableName, columnProvider);
		}

		#endregion Column types
	}
}