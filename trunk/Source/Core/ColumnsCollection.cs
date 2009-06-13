namespace DbRefactor
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	public class ColumnsCollection
	{
		private readonly List<Column> _columns = new List<Column>();

		private ColumnsCollection()
		{
		}

		public Column[] ToArray()
		{
			return _columns.ToArray();
		}

		internal static ColumnsCollection Create()
		{
			return new ColumnsCollection();
		}

		public void Add(Column column)
		{
			_columns.Add(column);
		}

		public Column LastColumnItem
		{
			get
			{
				if (_columns.Count == 0)
				{
					throw new InvalidOperationException("Columns does not have any columns");
				}
				return _columns[_columns.Count - 1];
			}
		}

		/// <summary>
		/// Creates a string column for "CreateTable" method
		/// </summary>
		public ColumnsCollection String(string name, int size)
		{
			Add(new Column(name, typeof(string), size));
			return this;
		}

		/// <summary>
		/// Creates a string column for "CreateTable" method
		/// </summary>
		public ColumnsCollection String(string name, int size, ColumnProperties properties)
		{
			Add(new Column(name, typeof(string), size, properties));
			return this;
		}

		/// <summary>
		/// Creates a string column for "CreateTable" method
		/// </summary>
		public ColumnsCollection String(string name, int size, string defaultValue)
		{
			Add(new Column(name, typeof(string), size, ColumnProperties.Null, defaultValue));
			return this;
		}

		/// <summary>
		/// Creates a string column for "CreateTable" method
		/// </summary>
		public ColumnsCollection String(string name, int size, ColumnProperties properties, string defaultValue)
		{
			Add(new Column(name, typeof(string), size, properties, defaultValue));
			return this;
		}

		private const int defaultTextLength = 1024;

		/// <summary>
		/// Creates a text column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Text(string name)
		{
			Add(new Column(name, typeof(string), defaultTextLength));
			return this;
		}

		/// <summary>
		/// Creates a text column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Text(string name, ColumnProperties properties)
		{
			Add(new Column(name, typeof(string), defaultTextLength, properties));
			return this;
		}

		/// <summary>
		/// Creates a text column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Text(string name, string defaultValue)
		{
			Add(new Column(name, typeof(string), defaultTextLength,
				ColumnProperties.Null, defaultValue));
			return this;
		}

		/// <summary>
		/// Creates a text column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Text(string name, ColumnProperties properties, string defaultValue)
		{
			Add(new Column(name, typeof(string), defaultTextLength, properties, defaultValue));
			return this;
		}

		/// <summary>
		/// Creates an integer column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Int(string name)
		{
			Add(new Column(name, typeof(int)));
			return this;
		}

		/// <summary>
		/// Creates an integer column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Int(string name, ColumnProperties properties)
		{
			Add(new Column(name, typeof(int), properties));
			return this;
		}

		/// <summary>
		/// Creates an integer column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Int(string name, int defaultValue)
		{
			Add(new Column(name, typeof(int), ColumnProperties.Null, defaultValue));
			return this;
		}

		/// <summary>
		/// Creates an integer column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Int(string name, ColumnProperties properties, int defaultValue)
		{
			Add(new Column(name, typeof(int), properties, defaultValue));
			return this;
		}

		/// <summary>
		/// Creates a long column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Long(string name)
		{
			Add(new Column(name, typeof(long)));
			return this;
		}

		/// <summary>
		/// Creates a long column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Long(string name, ColumnProperties properties)
		{
			Add(new Column(name, typeof(long), properties));
			return this;
		}

		/// <summary>
		/// Creates a long column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Long(string name, long defaultValue)
		{
			Add(new Column(name, typeof(long), ColumnProperties.Null, defaultValue));
			return this;
		}

		/// <summary>
		/// Creates a long column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Long(string name, ColumnProperties properties, long defaultValue)
		{
			Add(new Column(name, typeof(long), properties, defaultValue));
			return this;
		}

		/// <summary>
		/// Creates a date/time column for "CreateTable" method
		/// </summary>
		public ColumnsCollection DateTime(string name)
		{
			Add(new Column(name, typeof(DateTime)));
			return this;
		}

		/// <summary>
		/// Creates a date/time column for "CreateTable" method
		/// </summary>
		public ColumnsCollection DateTime(string name, ColumnProperties properties)
		{
			Add(new Column(name, typeof(DateTime), properties));
			return this;
		}

		/// <summary>
		/// Creates a date/time column for "CreateTable" method
		/// </summary>
		public ColumnsCollection DateTime(string name, DateTime defaultValue)
		{
			Add(new Column(name, typeof(DateTime), ColumnProperties.Null, defaultValue));
			return this;
		}

		/// <summary>
		/// Creates a date/time column for "CreateTable" method
		/// </summary>
		public ColumnsCollection DateTime(string name, ColumnProperties properties, DateTime defaultValue)
		{
			Add(new Column(name, typeof(DateTime), properties, defaultValue));
			return this;
		}

		private const int defaultWhole = 18;
		private const int defaultRemainder = 0;

		/// <summary>
		/// Creates a decimal column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Decimal(string name)
		{
			Add(new DecimalColumn(name, defaultWhole, defaultRemainder));
			return this;
		}

		/// <summary>
		/// Creates a decimal column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Decimal(string name, ColumnProperties properties)
		{
			Add(new DecimalColumn(name, defaultWhole, defaultRemainder, properties));
			return this;
		}

		/// <summary>
		/// Creates a decimal column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Decimal(string name, decimal defaultValue)
		{
			Add(new DecimalColumn(name, defaultWhole, defaultRemainder,
				ColumnProperties.Null, defaultValue));
			return this;
		}

		/// <summary>
		/// Creates a decimal column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Decimal(string name, ColumnProperties properties, decimal defaultValue)
		{
			Add(new DecimalColumn(name, defaultWhole, defaultRemainder, properties, defaultValue));
			return this;
		}

		/// <summary>
		/// Creates a decimal column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Decimal(string name, int whole, int remainder)
		{
			Add(new DecimalColumn(name, whole, remainder));
			return this;
		}

		/// <summary>
		/// Creates a decimal column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Decimal(string name, int whole, int remainder, ColumnProperties properties)
		{
			Add(new DecimalColumn(name, whole, remainder, properties));
			return this;
		}

		/// <summary>
		/// Creates a decimal column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Decimal(string name, int whole, int remainder, decimal defaultValue)
		{
			Add(new DecimalColumn(name, whole, remainder, ColumnProperties.Null, defaultValue));
			return this;
		}

		/// <summary>
		/// Creates a decimal column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Decimal(string name, int whole, int remainder,
			ColumnProperties properties, decimal defaultValue)
		{
			Add(new DecimalColumn(name, whole, remainder, properties, defaultValue));
			return this;
		}

		/// <summary>
		/// Creates a boolean column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Boolean(string name)
		{
			Add(new Column(name, typeof(bool)));
			return this;
		}

		/// <summary>
		/// Creates a boolean column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Boolean(string name, ColumnProperties properties)
		{
			Add(new Column(name, typeof(bool), properties));
			return this;
		}

		/// <summary>
		/// Creates a boolean column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Boolean(string name, bool defaultValue)
		{
			Add(new Column(name, typeof(bool), ColumnProperties.Null, defaultValue));
			return this;
		}

		/// <summary>
		/// Creates a boolean column for "CreateTable" method
		/// </summary>
		public ColumnsCollection Boolean(string name, ColumnProperties properties, bool defaultValue)
		{
			Add(new Column(name, typeof(bool), properties, defaultValue));
			return this;
		}

		public void AddProperty(ColumnProperties property)
		{
			if (_columns.Count == 0)
			{
				throw new InvalidOperationException("Cant add property because ther are no columns");
			}
			_columns[_columns.Count - 1].ColumnProperty = property | _columns[_columns.Count - 1].ColumnProperty;
		}

		/// <summary>
		/// Null is allowed
		/// </summary>
		public ColumnsCollection Null()
		{
			AddProperty(ColumnProperties.Null);
			return this;
		}

		/// <summary>
		/// Null is not allowed
		/// </summary>
		public ColumnsCollection NotNull()
		{
			AddProperty(ColumnProperties.NotNull);
			return this;
		}
		/// <summary>
		/// Identity column, autoinc
		/// </summary>
		public ColumnsCollection Identity()
		{
			AddProperty(ColumnProperties.Identity);
			return this;
		}
		/// <summary>
		/// Unique Column
		/// </summary>
		public ColumnsCollection Unique()
		{
			AddProperty(ColumnProperties.Unique);
			return this;
		}
		/// <summary>
		/// Indexed Column
		/// </summary>
		public ColumnsCollection Indexed()
		{
			AddProperty(ColumnProperties.Indexed);
			return this;
		}
		/// <summary>
		/// Primary Key
		/// </summary>
		public ColumnsCollection PrimaryKey()
		{
			AddProperty(ColumnProperties.PrimaryKey);
			return this;
		}
		/// <summary>
		/// Primary key. Make the column a PrimaryKey and unsigned
		/// </summary>
		public ColumnsCollection PrimaryKeyWithIdentity()
		{
			AddProperty(ColumnProperties.PrimaryKeyWithIdentity);
			return this;
		}
	}
}
