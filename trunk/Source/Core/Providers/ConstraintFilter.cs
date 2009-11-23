namespace DbRefactor.Providers
{
	internal class ConstraintFilter
	{
		public string Name { get; set; }
		public string TableName { get; set; }
		public string[] ColumnNames { get; set; }
		public ConstraintType ConstraintType { get; set; }
	}

	internal enum ConstraintType
	{
		None = 0,
		PrimaryKey = 1,
		ForeignKey = 2,
		Unique = 3,
		Default = 4
	}
}