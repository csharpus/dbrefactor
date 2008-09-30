using System.Collections.Generic;
using System.Data;
using System.IO;
using DbRefactor.Providers;
using System;

namespace DbRefactor.Tools
{
	public class DataDumper
	{
		readonly TransformationProvider _provider;

		public DataDumper(string connectionString)
		{
			_provider = new ProviderFactory().Create(connectionString);
		}

		private bool hasIdentity = false;
		private bool shouldDelete = false;
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
			List<string> tables = _provider.GetTablesSortedByDependency();
			foreach (string table in tables)
			{
				DisableConstraints(table);
				DeleteStatement(table);
			}
			tables.Reverse();
			foreach (string table in tables)
			{
				Column[] columns = _provider.GetColumns(table);
				hasIdentity = _provider.TableHasIdentity(table);
				using (IDataReader reader = _provider.Select("*", table))
				{
					DisableIdentity(table);
					while (reader.Read())
					{
						writer.Write("\tinsert into [{0}] (", table);
						foreach (Column column in columns)
						{
							writer.Write("[{0}]", column.Name);
							if (column != columns[columns.Length - 1])
							{
								writer.Write(", ");
							}
						}
						writer.Write(") values (");
						foreach (Column column in columns)
						{

							if (column.Type == typeof(DateTime))
							{
								writer.Write("'{0}'", FormatDateTime((DateTime)reader[column.Name]));
							}
							else
							{
								writer.Write("'{0}'", reader[column.Name].ToString().Replace("'", "''"));
							}
							if (column != columns[columns.Length - 1])
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
