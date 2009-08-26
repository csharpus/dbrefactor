namespace DbRefactor.Providers
{
	internal class ColumnData
	{
		public string DataType { get; set; }

		public string Name { get; set; }

		public int? Length { get; set; }

		public int? Precision { get; set; }

		public int? Radix { get; set; }

		public object DefaultValue { get; set; }
	}
}