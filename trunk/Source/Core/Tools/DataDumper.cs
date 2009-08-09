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

using System.Collections.Generic;
using System.Data;
using System.IO;
using DbRefactor.Providers;
using System;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Tools
{
	public class DataDumper
	{
		readonly TransformationProvider provider;

		public DataDumper(string connectionString)
		{
			provider = new ProviderFactory().Create(connectionString);
		}

		private bool hasIdentity;
		private bool shouldDelete;
		private StringWriter writer;

		private void DeleteStatement(string table)
		{
			if (shouldDelete)
			{
				writer.WriteLine("delete from [{0}]", table);
			}
		}

		private void DisableIdentity(string table)
		{
			if (hasIdentity)
			{
				writer.WriteLine("set identity_insert [{0}] on", table);
			}
		}

		private void EnableIdentity(string table)
		{
			if (hasIdentity)
			{
				writer.WriteLine("set identity_insert [{0}] off", table);
			}
		}

		private void DisableConstraints(string table)
		{
			writer.WriteLine("alter table [{0}] nocheck constraint all", table);
		}

		private void EnableConstraints(string table)
		{
			writer.WriteLine("alter table [{0}] check constraint all", table);
		}

		private void CheckContraints(string table)
		{
			writer.WriteLine("dbcc checkconstraints ('{0}')", table);
		}

		private void EmptyLine()
		{
			writer.WriteLine();
		}

		/// <param name="delete">Generate delete statement for all tables</param>
		public string Dump(bool delete)
		{
			shouldDelete = delete;
			writer = new StringWriter();
			List<string> tables = provider.GetTablesSortedByDependency();
			foreach (string table in tables)
			{
				DisableConstraints(table);
				DeleteStatement(table);
			}
			tables.Reverse();
			foreach (string table in tables)
			{
				//Column[] columns = provider.GetColumns(table);
				var columnProviders = provider.GetColumnProviders(table);
				hasIdentity = provider.TableHasIdentity(table);
				using (IDataReader reader = provider.Select("*", table))
				{
					DisableIdentity(table);
					while (reader.Read())
					{
						writer.Write("\tinsert into [{0}] (", table);
						foreach (ColumnProvider columnProvider in columnProviders)
						{
							writer.Write("[{0}]", columnProvider.Name);
							if (columnProvider != columnProviders[columnProviders.Count - 1])
							{
								writer.Write(", ");
							}
						}
						writer.Write(") values (");
						foreach (ColumnProvider columnProvider in columnProviders)
						{
							if (reader[columnProvider.Name] == DBNull.Value)
							{
								writer.Write("NULL");
							}
							else
							{
								columnProvider.GetValueSql(reader[columnProvider.Name]);
							}
							if (columnProvider != columnProviders[columnProviders.Count - 1])
							{
								writer.Write(", ");
							}
						}
						writer.WriteLine(");");
					}
					EnableIdentity(table);
					EmptyLine();
				}
			}
			foreach (string table in tables)
			{
				EnableConstraints(table);
				CheckContraints(table);
			}
			EmptyLine();
			return writer.ToString();
		}

		public string Dump()
		{
			return Dump(false);
		}

		private static string FormatDateTime(DateTime date)
		{
			return date.Year.ToString("0000")
			       + date.Month.ToString("00")
			       + date.Day.ToString("00")
			       + " "
			       + date.Hour.ToString("00")
			       + ":"
			       + date.Minute.ToString("00")
				   + ":"
			       + date.Second.ToString("00");
		}
	}
}
