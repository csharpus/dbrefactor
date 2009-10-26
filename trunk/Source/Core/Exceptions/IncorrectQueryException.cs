using System;
using System.Data.SqlClient;

namespace DbRefactor.Exceptions
{
	public class IncorrectQueryException : DbRefactorException
	{
		private readonly string sql = String.Empty;

		public IncorrectQueryException(string sql, SqlException innerException)
			: base(GetMessage(sql, innerException), innerException)
		{
			this.sql = sql;
		}

		public string Sql
		{
			get { return sql; }
		}

		private static string GetMessage(string sql, SqlException sqlException)
		{
			return String.Format("Couldn't execute query: \r\n {0} \r\n {1}", sql, sqlException.Message);
		}
	}
}