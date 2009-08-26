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
using DbRefactor.Providers.Columns;

namespace DbRefactor.Tools
{
	public class DataDumper
	{
		private readonly TransformationProvider provider;

		internal DataDumper(TransformationProvider provider)
		{
			this.provider = provider;
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

		public string GenerateDropStatement()
		{
			shouldDelete = true;
			writer = new StringWriter();
			List<string> tables = provider.GetTablesSortedByDependency();
			foreach (string table in tables)
			{
				DisableConstraints(table);
			}
			foreach (string table in tables)
			{
				DropStatement(table);
			}
			return writer.ToString();
		}

		private void DropStatement(string table)
		{
			writer.WriteLine("DROP TABLE [{0}]", table);
		}

		public string GenerateDeleteStatement()
		{
			shouldDelete = true;
			writer = new StringWriter();
			List<string> tables = provider.GetTablesSortedByDependency();
			foreach (string table in tables)
			{
				DisableConstraints(table);
			}
			foreach (string table in tables)
			{
				DeleteStatement(table);
			}
			foreach (string table in tables)
			{
				EnableConstraints(table);
			}
			return writer.ToString();
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
				using (IDataReader reader = provider.ExecuteQuery("select * from [{0}]", table))
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
							writer.Write(columnProvider.GetValueSql(reader[columnProvider.Name]));

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
	}
}