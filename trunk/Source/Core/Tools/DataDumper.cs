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

		/// <param name="delete">Generate delete statement for all tables</param>
		public string Dump(bool delete)
		{
			shouldDelete = delete;
			writer = new StringWriter();
			foreach (string table in _provider.GetTables())
			{
				Column[] columns = _provider.GetColumns(table);
				hasIdentity = _provider.TableHasIdentity(table);
				using (IDataReader reader = _provider.Select("*", table))
				{
					DeleteStatement(table);
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
					writer.WriteLine();
				}
			}
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
