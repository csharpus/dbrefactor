namespace DbRefactor.Providers.Model
{
	public class ColumnData
	{
		public string DataType { get; set; }

		public string Name { get; set; }

		public int? Length { get; set; }

		public int? Precision { get; set; }

		public int? Scale { get; set; }

		public object DefaultValue { get; set; }
	}
}