using System;
using System.Collections.Generic;
using DbRefactor.Exceptions;
using DbRefactor.Providers;

namespace DbRefactor.Engines.SqlServer
{
	internal class ConstraintQueryBuilder
	{
		private readonly ConstraintFilter filter;
		private readonly List<string> restrictions = new List<string>();

		public ConstraintQueryBuilder(ConstraintFilter filter)
		{
			this.filter = filter;
		}

		public string BuildQuery()
		{
			// INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE - view that contains all constraints except defaults
			// sys.default_constraints - view for default constraints
			// http://blogs.msdn.com/sqltips/archive/2005/07/05/435882.aspx
			const string baseQuery =
				@"
WITH AllConstraints
AS (
	SELECT 
		Constraints.CONSTRAINT_NAME AS ConstraintName,
		Constraints.TABLE_SCHEMA AS TableSchema, 
		Constraints.TABLE_NAME AS TableName, 
		Constraints.COLUMN_NAME AS ColumnName, 
		Objects.[type] AS ConstraintType
	FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE as Constraints
	JOIN sys.objects AS Objects 
		ON Objects.[Name] = Constraints.CONSTRAINT_NAME
UNION ALL
	SELECT
		DefaultConstraints.[name] AS ConstraintName,
		Schemas.[name] AS TableSchema,
		Objects.[name] As TableName,
		Columns.[name] AS ColumnName,
		'DF' AS ConstraintType
	FROM sys.default_constraints AS DefaultConstraints
	JOIN sys.objects AS Objects
		ON Objects.object_id = DefaultConstraints.parent_object_id
	JOIN sys.columns AS Columns
		ON Columns.object_id = Objects.object_id 
			AND Columns.column_id = DefaultConstraints.parent_column_id
	JOIN sys.schemas AS Schemas
		ON Schemas.schema_id = Objects.schema_id)
SELECT ConstraintName, TableSchema, TableName, ColumnName, ConstraintType
FROM AllConstraints
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
			restrictions.Add(String.Format("ConstraintName = '{0}'", filter.Name));
		}

		private void AddTypeRestriction()
		{
			if (filter.ConstraintType == ConstraintType.None) return;
			restrictions.Add(String.Format("ConstraintType = '{0}'", GetConstraintTypeSql(filter.ConstraintType)));
		}

		public static readonly Dictionary<ConstraintType, string> ConstraintTypeMap
			= new Dictionary<ConstraintType, string>
			  	{
			  		{ConstraintType.PrimaryKey, "PK"},
			  		{ConstraintType.ForeignKey, "F "},
			  		{ConstraintType.Unique, "UQ"},
			  		{ConstraintType.Default, "DF"}
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
			restrictions.Add(String.Format("ColumnName IN ('{0}')", String.Join("', '", filter.ColumnNames)));
		}

		private void AddTableRestriction()
		{
			if (filter.TableName == null) return;
			restrictions.Add(String.Format("TableName = '{0}'", filter.TableName));
		}
	}
}