using System;
using System.Collections.Generic;
using DbRefactor.Exceptions;
using DbRefactor.Providers;

namespace DbRefactor.Engines.SqlServer.Compact
{
	internal class CeConstraintQueryBuilder
	{
		private readonly ConstraintFilter filter;
		private readonly List<string> restrictions = new List<string>();

		public CeConstraintQueryBuilder(ConstraintFilter filter)
		{
			this.filter = filter;
		}

		public string BuildQuery()
		{
			const string baseQuery =
				@"
select 
	Keys.CONSTRAINT_NAME as ConstraintName,
	Constraints.TABLE_SCHEMA as TableSchema,
	Keys.TABLE_NAME as TableName,
	Keys.COLUMN_NAME as ColumnName,
	Constraints.CONSTRAINT_TYPE as ConstraintType
from INFORMATION_SCHEMA.KEY_COLUMN_USAGE as Keys
inner join INFORMATION_SCHEMA.TABLE_CONSTRAINTS as Constraints
	on Constraints.CONSTRAINT_NAME = Keys.CONSTRAINT_NAME
				";
			AddTableRestriction();
			AddColumnRestriction();
			AddTypeRestriction();
			AddNameRestriction();
			var whereClause = String.Join(" AND ", restrictions.ToArray());
			return whereClause != String.Empty ? baseQuery + " WHERE " + whereClause : baseQuery;
		}

		private void AddNameRestriction()
		{
			if (filter.Name == null) return;
			restrictions.Add(String.Format("Keys.CONSTRAINT_NAME = '{0}'", filter.Name));
		}

		private void AddTypeRestriction()
		{
			if (filter.ConstraintType == ConstraintType.None) return;
			restrictions.Add(String.Format("Constraints.CONSTRAINT_TYPE = '{0}'", GetConstraintTypeSql(filter.ConstraintType)));
		}

		public static readonly Dictionary<ConstraintType, string> ConstraintTypeMap
			= new Dictionary<ConstraintType, string>
			  	{
			  		{ConstraintType.PrimaryKey, "PRIMARY KEY"},
			  		{ConstraintType.ForeignKey, "FOREIGN KEY"},
			  		{ConstraintType.Unique, "UNIQUE"}
			  	};

		private static string GetConstraintTypeSql(ConstraintType type)
		{
			if (!ConstraintTypeMap.ContainsKey(type))
			{
				throw new DbRefactorException(String.Format("Unsupported constraint type: '{0}'", type));
			}
			return ConstraintTypeMap[type];
		}

		private void AddColumnRestriction()
		{
			if (filter.ColumnNames == null) return;
			restrictions.Add(String.Format("Keys.COLUMN_NAME IN ('{0}')", String.Join("', '", filter.ColumnNames)));
		}

		private void AddTableRestriction()
		{
			if (filter.TableName == null) return;
			restrictions.Add(String.Format("Keys.TABLE_NAME = '{0}'", filter.TableName));
		}
	}
}