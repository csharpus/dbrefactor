using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DbRefactor.Engines.SqlServer;
using DbRefactor.Exceptions;
using DbRefactor.Providers.Columns;

namespace DbRefactor.Providers
{
	internal abstract class SchemaProvider
	{
		private readonly IDatabaseEnvironment databaseEnvironment;
		private readonly ObjectNameService objectNameService;
		private readonly SqlServerColumnMapper sqlServerColumnMapper;

		protected SchemaProvider(IDatabaseEnvironment databaseEnvironment, ObjectNameService objectNameService,
		                         SqlServerColumnMapper sqlServerColumnMapper)
		{
			this.databaseEnvironment = databaseEnvironment;
			this.objectNameService = objectNameService;
			this.sqlServerColumnMapper = sqlServerColumnMapper;
		}

		protected IDatabaseEnvironment DatabaseEnvironment
		{
			get { return databaseEnvironment; }
		}

		public abstract List<ForeignKey> GetForeignKeys(ForeignKeyFilter filter);

		public abstract List<DatabaseConstraint> GetConstraints(ConstraintFilter filter);

		public abstract List<Index> GetIndexes(IndexFilter filter);

		public abstract bool IsNullable(string table, string column);

		public abstract bool IsIdentity(string table, string column);

		public abstract bool TableExists(string table);

		public abstract bool ColumnExists(string table, string column);

		public ColumnProvider GetColumnProvider(string tableName, string columnName)
		{
			ColumnProvider provider;
			using (
				IDataReader reader =
					DatabaseEnvironment.ExecuteQuery(String.Format(
@"
select DATA_TYPE, COLUMN_NAME, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE,
	COLUMN_DEFAULT 
from INFORMATION_SCHEMA.COLUMNS 
where TABLE_NAME = '{0}' 
	and COLUMN_NAME = '{1}'
",
					                                 	tableName, columnName)))
			{
				if (!reader.Read())
				{
					throw new DbRefactorException(String.Format("Couldn't find column '{0}' in table '{1}'", columnName,
					                                            tableName));
				}
				provider = GetProvider(reader);
			}
			AddProviderProperties(tableName, provider);
			return provider;
		}

		public List<ColumnProvider> GetColumnProviders(string table)
		{
			var providers = new List<ColumnProvider>();

			using (
				IDataReader reader =
					DatabaseEnvironment.ExecuteQuery(String.Format(
@"
select DATA_TYPE, COLUMN_NAME, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE, 
	COLUMN_DEFAULT 
from INFORMATION_SCHEMA.COLUMNS 
where TABLE_NAME = '{0}'
",
					                                 	table)))
			{
				while (reader.Read())
				{
					ColumnProvider provider = GetProvider(reader);
					providers.Add(provider);
				}
			}
			foreach (var provider in providers)
			{
				AddProviderProperties(table, provider);
			}
			return providers;
		}

		protected abstract ColumnProvider GetProvider(IDataRecord reader);

		protected static object GetDefaultValue(object databaseValue)
		{
			return databaseValue == DBNull.Value ? null : databaseValue;
		}

		protected Dictionary<string, Func<ColumnData, ColumnProvider>> GetTypesMap()
		{
			return new Dictionary<string, Func<ColumnData, ColumnProvider>>
			       	{
			       		{"bigint", sqlServerColumnMapper.CreateLong},
			       		{"binary", sqlServerColumnMapper.CreateBinary},
			       		{"bit", sqlServerColumnMapper.CreateBoolean},
			       		{"char", sqlServerColumnMapper.CreateString},
			       		{"datetime", sqlServerColumnMapper.CreateDateTime},
			       		{"decimal", sqlServerColumnMapper.CreateDecimal},
			       		{"float", sqlServerColumnMapper.CreateFloat},
			       		{"image", sqlServerColumnMapper.CreateBinary},
			       		{"int", sqlServerColumnMapper.CreateInt},
			       		{"money", sqlServerColumnMapper.CreateDecimal},
			       		{"nchar", sqlServerColumnMapper.CreateString},
			       		{"ntext", sqlServerColumnMapper.CreateText},
			       		{"numeric", sqlServerColumnMapper.CreateDecimal},
			       		{"nvarchar", sqlServerColumnMapper.CreateString},
			       		{"real", sqlServerColumnMapper.CreateFloat},
			       		{"smalldatetime", sqlServerColumnMapper.CreateDateTime},
			       		{"smallint", sqlServerColumnMapper.CreateInt},
			       		{"smallmoney", sqlServerColumnMapper.CreateDecimal},
			       		{"sql_variant", sqlServerColumnMapper.CreateBinary},
			       		{"text", sqlServerColumnMapper.CreateText},
			       		{"timestamp", sqlServerColumnMapper.CreateDateTime},
			       		{"tinyint", sqlServerColumnMapper.CreateInt},
			       		{"uniqueidentifier", sqlServerColumnMapper.CreateString},
			       		{"varbinary", sqlServerColumnMapper.CreateBinary},
			       		{"varchar", sqlServerColumnMapper.CreateString},
			       		{"xml", sqlServerColumnMapper.CreateString}
			       	};
		}

		private void AddProviderProperties(string table, ColumnProvider provider)
		{
			if (IsPrimaryKey(table, provider.Name))
			{
				provider.AddPrimaryKey(objectNameService.PrimaryKeyName(table, provider.Name));
			}
			else if (!IsNullable(table, provider.Name))
			{
				provider.AddNotNull();
			}

			if (IsIdentity(table, provider.Name))
			{
				provider.AddIdentity();
			}

			if (IsUnique(table, provider.Name))
			{
				provider.AddUnique(objectNameService.UniqueName(table, provider.Name));
			}
		}

		protected static T? NullSafeGet<T>(IDataRecord reader, string name)
			where T : struct
		{
			object value = reader[name];
			if (value == DBNull.Value)
			{
				return null;
			}
			return (T) Convert.ChangeType(value, typeof (T));
		}

		private bool IsPrimaryKey(string table, string column)
		{
			var filter = new ConstraintFilter
			             	{
			             		TableName = table,
			             		ColumnNames = new[] {column},
			             		ConstraintType = ConstraintType.PrimaryKey
			             	};
			return GetConstraints(filter).Any();
		}

		private bool IsUnique(string table, string column)
		{
			var filter = new ConstraintFilter
			             	{
			             		TableName = table,
			             		ColumnNames = new[] {column},
			             		ConstraintType = ConstraintType.Unique
			             	};
			return GetConstraints(filter).Any();
		}

		public abstract void RenameColumn(string table, string oldColumnName, string newColumnName);

		public abstract void RenameTable(string oldName, string newName);

		public abstract bool IsDefault(string table, string column);

		public abstract string[] GetTables();
	}
}