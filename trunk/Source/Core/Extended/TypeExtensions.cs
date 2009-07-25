namespace DbRefactor.Extended
{
	public static class TypeExtensions
	{
		public static NewTable Binary(this NewTable newTable, string name)
		{
			newTable.Columns.Add(new Column(name, typeof(byte[])));
			return newTable;
		}

		public static NewTable Float(this NewTable newTable, string name)
		{
			newTable.Columns.Add(new Column(name, typeof(float)));
			return newTable;
		}

		public static NewTable Double(this NewTable newTable, string name)
		{
			newTable.Columns.Add(new Column(name, typeof(double)));
			return newTable;
		}
	}
}
