using System;

namespace DbRefactor.Providers
{
	public sealed partial class TransformationProvider
	{
		private string category;

		public string Category
		{
			get { return category; }
			set { category = value ?? String.Empty; }
		}
	}
}
