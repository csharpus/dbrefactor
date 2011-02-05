using DbRefactor.Providers.Filters;

namespace DbRefactor.Providers.Model
{
	public class DatabaseConstraint
	{
		public string Name { get; set; }
		public string TableSchema { get; set; }
		public string TableName { get; set; }
		public string ColumnName { get; set; }
		public ConstraintType ConstraintType { get; set; }
	}
}