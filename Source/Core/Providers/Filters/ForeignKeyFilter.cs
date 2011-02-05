namespace DbRefactor.Providers.Filters
{
	public class ForeignKeyFilter
	{
		public string Name { get; set; }
		public string ForeignKeyTable { get; set; }
		public string[] ForeignKeyColumns { get; set; }
		public string PrimaryKeyTable { get; set; }
		public string[] PrimaryKeyColumns { get; set; }
	}
}