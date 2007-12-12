using System;
using System.Collections.Generic;
using System.Text;
using Migrator.Providers;
using System.Data;

namespace Tipait.DbRefactor.Tests.Core
{
	public class DatabaseEnvironmentStub : IDatabaseEnvironment
	{
		private string _lattestSql;

		public string LattestSql
		{
			get
			{
				return _lattestSql;
			}
		}

		#region IDatabaseEnvironment Members

		int IDatabaseEnvironment.ExecuteNonQuery(string sql)
		{
			_lattestSql = sql;
			return 0;
		}

		System.Data.IDataReader IDatabaseEnvironment.ExecuteQuery(string sql)
		{
			return new DataReader();
		}

		object IDatabaseEnvironment.ExecuteScalar(string sql)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void IDatabaseEnvironment.BeginTransaction()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void IDatabaseEnvironment.RollbackTransaction()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void IDatabaseEnvironment.CommitTransaction()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}

	public class DataReader : IDataReader
	{
		#region IDataReader Members

		public void Close()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public int Depth
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public DataTable GetSchemaTable()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public bool IsClosed
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public bool NextResult()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public bool Read()
		{
			return false;
		}

		public int RecordsAffected
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			
		}

		#endregion

		#region IDataRecord Members

		public int FieldCount
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public bool GetBoolean(int i)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public byte GetByte(int i)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public char GetChar(int i)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public IDataReader GetData(int i)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public string GetDataTypeName(int i)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public DateTime GetDateTime(int i)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public decimal GetDecimal(int i)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public double GetDouble(int i)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public Type GetFieldType(int i)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public float GetFloat(int i)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public Guid GetGuid(int i)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public short GetInt16(int i)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public int GetInt32(int i)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public long GetInt64(int i)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public string GetName(int i)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public int GetOrdinal(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public string GetString(int i)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public object GetValue(int i)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public int GetValues(object[] values)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public bool IsDBNull(int i)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public object this[string name]
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public object this[int i]
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		#endregion
	}
}
