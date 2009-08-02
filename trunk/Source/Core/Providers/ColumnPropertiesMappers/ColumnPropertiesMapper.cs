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
using DbRefactor.Tools.DesignByContract;

namespace DbRefactor.Providers.ColumnPropertiesMappers
{
	/// <summary>
	/// This is basically a just a helper base class
	/// per-database implementors may want to override ColumnSql
	/// </summary>
	sealed class ColumnPropertiesMapper
	{
		/// <summary>
		/// the type of the column
		/// </summary>
		private readonly string type;

		/// <summary>
		/// name of the column
		/// </summary>
		private string name;

		/// <summary>
		/// This should be set to whatever passes for NULL in implementing 
		/// classes constructors, if it is not NULL
		/// </summary>
		private string sqlNull = "NULL";

		/// <summary>
		/// Sql if This column is a primary key
		/// </summary>
		private string sqlPrimaryKey;

		/// <summary>
		/// Sql if This column is Unique
		/// </summary>
		private string sqlUnique;

		/// <summary>
		/// Sql if This column is Indexed
		/// </summary>
		private bool indexed = false;

		/// <summary>
		/// Sql if This column is an Identity Colu,m
		/// </summary>
		private string sqlIdentity;

		/// <summary>
		/// Sql if this column has a default value
		/// </summary>
		private string sqlDefault;

		public ColumnPropertiesMapper(string type)
		{
			this.type = type;
		}

		#region IColumnPropertiesMapper Members

		/// <summary>
		/// The sql for this column, override in database-specific implementation classes
		/// </summary>
		public string ColumnSql
		{
			get
			{
				return IgnoreEmptyJoin(" ", new string[] { "[" + name + "]", type, sqlNull, sqlIdentity, sqlUnique, sqlPrimaryKey, sqlDefault });
			}
		}

		private static string IgnoreEmptyJoin(string delimiter, IEnumerable<string> sqlElements)
		{
			string result = String.Empty;
			foreach (string s in sqlElements)
			{
				if (!String.IsNullOrEmpty(s))
				{
					result += s + delimiter;
				}
			}
			Check.Ensure(result.Length > 0);
			return result.Substring(0, result.Length - 1);
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public void Indexed()
		{
			indexed = true;
		}

		public string IndexSql
		{
			get
			{
				return null;
			}
		}

		public void NotNull()
		{
			sqlNull = "NOT NULL";
		}

		public void PrimaryKey()
		{
			sqlPrimaryKey = "PRIMARY KEY";
		}

		public void Unique()
		{
			sqlUnique = "UNIQUE";
		}

		public void Identity()
		{
			sqlIdentity = "IDENTITY";
		}

		public void Default(string defaultValue)
		{
			sqlDefault = String.Format("DEFAULT {0}", defaultValue);
		}

		#endregion
	}
}