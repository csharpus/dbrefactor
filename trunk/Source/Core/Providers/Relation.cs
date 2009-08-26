namespace DbRefactor.Providers
{
	internal class Relation
	{
		public Relation(string parent, string child)
		{
			Parent = parent;
			Child = child;
		}

		public string Parent { get; set; }

		public string Child { get; set; }
	}
}