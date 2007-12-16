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
using DbRefactor.Columns;
using DbRefactor.Providers.ColumnPropertiesMappers;
using DbRefactor.Providers.TypeToSqlProviders;

namespace DbRefactor.Columns
{
	/// <summary>
	/// Represents a table column properties.
	/// </summary>
	[Flags]
	public enum ColumnProperties
	{
		/// <summary>
		/// Null is allowed
		/// </summary>
		Null = 1,
		/// <summary>
		/// Null is not allowed
		/// </summary>
		NotNull = 2,
		/// <summary>
		/// Identity column, autoinc
		/// </summary>
		Identity = 4,
		/// <summary>
		/// Unique Column
		/// </summary>
		Unique = 8,
		/// <summary>
		/// Indexed Column
		/// </summary>
		Indexed = 16,
		/// <summary>
		/// Primary Key
		/// </summary>
		PrimaryKey = 64 | NotNull,
		/// <summary>
		/// Primary key. Make the column a PrimaryKey and unsigned
		/// </summary>
		PrimaryKeyWithIdentity = PrimaryKey | Identity

	}

	/// <summary>
	/// Represents a table column.
	/// </summary>
	public class Column
	{

		internal Column(string name, Type type)
			: this(name, type, 0)
		{
		}

		internal Column(string name, Type type, int size)
			: this(name, type, size, 0)
		{
		}

		internal Column(string name, Type type, ColumnProperties property)
			: this(name, type, 0, property)
		{
		}

		internal Column(string name, Type type, int size, ColumnProperties property)
			: this(name, type, size, property, null)
		{
		}

		internal Column(string name, Type type, ColumnProperties property, object defaultValue)
			: this(name, type, 0, property, defaultValue)
		{
		}

		internal Column(string name, Type type, int size, ColumnProperties property, object defaultValue)
		{
			_name = name;
			_type = type;
			_size = size;
			_property = property;
			_defaultValue = defaultValue;
		}

		private string _name;

		internal string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		private Type _type;

		internal Type Type
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
			}
		}

		private int _size;

		internal int Size
		{
			get
			{
				return _size;
			}
			set
			{
				_size = value;
			}
		}

		private ColumnProperties _property;

		internal ColumnProperties ColumnProperty
		{
			get
			{
				return _property;
			}
			set
			{
				_property = value;
			}
		}

		private object _defaultValue;

		internal object DefaultValue
		{
			get
			{
				return _defaultValue;
			}
			set
			{
				_defaultValue = value;
			}
		}

		internal void MapColumnProperties(ColumnPropertiesMapper mapper, Column column)
		{
			mapper.Name = column.Name;
			ColumnProperties properties = column.ColumnProperty;
			if ((properties & ColumnProperties.NotNull) == ColumnProperties.NotNull)
			{
				mapper.NotNull();
			}
			if ((properties & ColumnProperties.PrimaryKey) == ColumnProperties.PrimaryKey)
			{
				mapper.PrimaryKey();
			}
			if ((properties & ColumnProperties.Identity) == ColumnProperties.Identity)
			{
				mapper.Identity();
			}
			if ((properties & ColumnProperties.Unique) == ColumnProperties.Unique)
			{
				mapper.Unique();
			}
			if ((properties & ColumnProperties.Indexed) == ColumnProperties.Indexed)
			{
				mapper.Indexed();
			}
			if (column.DefaultValue != null)
			{
				if (column.Type == typeof(char) || column.Type == typeof(string))
				{
					mapper.Default(String.Format("'{0}'", column.DefaultValue));
				}
				if (column.Type == typeof(bool))
				{
					mapper.Default(Convert.ToBoolean(column.DefaultValue) ? "1" : "0");
				}
				else
				{
					mapper.Default(column.DefaultValue.ToString());
				}
			}
		}

		SQLServerTypeToSqlProvider TypeToSqlProvider
		{
			get { return new SQLServerTypeToSqlProvider(); }
		}

		internal ColumnPropertiesMapper GetColumnMapper(Column column)
		{
			if (column.Type == typeof(char))
			{
				if (column.Size <= Convert.ToInt32(byte.MaxValue))
					return TypeToSqlProvider.Char(Convert.ToByte(column.Size));
				else if (column.Size <= Convert.ToInt32(ushort.MaxValue))
					return TypeToSqlProvider.Text;
				else
					return TypeToSqlProvider.LongText;
			}

			if (column.Type == typeof(string))
			{
				if (column.Size <= 255)
					return TypeToSqlProvider.String(Convert.ToUInt16(column.Size));
				else if (column.Size <= Convert.ToInt32(ushort.MaxValue))
					return TypeToSqlProvider.Text;
				else
					return TypeToSqlProvider.LongText;
			}

			if (column.Type == typeof(int))
			{
				if ((column.ColumnProperty & ColumnProperties.PrimaryKey) == ColumnProperties.PrimaryKey)
					return TypeToSqlProvider.PrimaryKey;
				else
					return TypeToSqlProvider.Integer;
			}
			if (column.Type == typeof(long))
				return TypeToSqlProvider.Long;

			if (column.Type == typeof(float))
				return TypeToSqlProvider.Float;

			if (column.Type == typeof(double))
			{
				if (column.Size == 0)
					return TypeToSqlProvider.Double;
				else
					return TypeToSqlProvider.Decimal(column.Size);
			}

			if (column.Type == typeof(decimal))
			{
				if (typeof(DecimalColumn).IsAssignableFrom(column.GetType()))
				{
					return TypeToSqlProvider.Decimal(column.Size, ((DecimalColumn) column).Remainder);
				}
				else
				{
					return TypeToSqlProvider.Decimal(column.Size);
				}
			}

			if (column.Type == typeof(bool))
				return TypeToSqlProvider.Bool;

			if (column.Type == typeof(DateTime))
				return TypeToSqlProvider.DateTime;

			if (column.Type == typeof(byte[]))
			{
				if (column.Size <= Convert.ToInt32(byte.MaxValue))
					return TypeToSqlProvider.Binary(Convert.ToByte(column.Size));
				else if (column.Size <= Convert.ToInt32(ushort.MaxValue))
					return TypeToSqlProvider.Blob;
				else
					return TypeToSqlProvider.LongBlob;
			}

			throw new ArgumentOutOfRangeException("column", "The " + column.Type + " type is not supported");
		}

		internal string ColumnSQL()
		{
			ColumnPropertiesMapper mapper =  GetColumnMapper(this);
			MapColumnProperties(mapper, this);
			return mapper.ColumnSql;
		}

		internal string IndexSQL()
		{
			ColumnPropertiesMapper mapper = GetColumnMapper(this);
			MapColumnProperties(mapper, this);
			return mapper.IndexSql;
		}
	}
}