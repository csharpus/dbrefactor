namespace DbRefactor.Providers
{
	internal class ForeignKey
	{
		public string Name { get; set; }
		public string PrimaryTable { get; set; }
		public string PrimaryColumn { get; set; }
		public string ForeignTable { get; set; }
		public string ForeignColumn { get; set; }
	}
}