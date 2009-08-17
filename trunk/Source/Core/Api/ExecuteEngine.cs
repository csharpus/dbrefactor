using System.Data;
using DbRefactor.Providers;

namespace DbRefactor.Api
{
	public class ExecuteEngine
	{
		private readonly TransformationProvider transformationProvider;

		public ExecuteEngine(TransformationProvider transformationProvider)
		{
			this.transformationProvider = transformationProvider;
		}

		/// <param name="sql">Supports format items to <see cref="string.Format(string,object)"/></param>
		/// <param name="values">An object to format</param>
		public void NonQuery(string sql, params string[] values)
		{
			transformationProvider.ExecuteNonQuery(sql, values);
		}

		/// <param name="sql">Supports format items to <see cref="string.Format(string,object)"/></param>
		/// <param name="values">An object to format</param>
		public IDataReader Query(string sql, params string[] values)
		{
			return transformationProvider.ExecuteQuery(sql, values);
		}

		/// <param name="sql">Supports format items to <see cref="string.Format(string,object)"/></param>
		/// <param name="values">An object to format</param>
		public object Scalar(string sql, params string[] values)
		{
			return transformationProvider.ExecuteScalar(sql, values);
		}
	}
}