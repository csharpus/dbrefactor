using System;

namespace DbRefactor.Providers
{
	public sealed partial class TransformationProvider
	{
		private string category;

		/// <summary>
		/// Get or set the current version of the database.
		/// This determines if the migrator should migrate up or down
		/// in the migration numbers.
		/// </summary>
		/// <remark>
		/// This value should not be modified inside a migration.
		/// </remark>
		public int CurrentVersion
		{
			get
			{
				CreateSchemaInfoTable();
				object version = SelectScalar("Version", "SchemaInfo", String.Format("Category='{0}'", category));
				return Convert.ToInt32(version);
			}
			set
			{
				CreateSchemaInfoTable();
				int count = Update("SchemaInfo", new[] { "Version=" + value }, String.Format("Category='{0}'", category));
				if (count == 0)
				{
					Insert("SchemaInfo", "Version=" + value, "Category='" + category + "'");
				}
			}
		}

		public string Category
		{
			get { return category; }
			set { category = value ?? String.Empty; }
		}

		private void CreateSchemaInfoTable()
		{
			if (TableExists("SchemaInfo")) return;
			GetDatabase().CreateTable("SchemaInfo")
				.Int("Version").PrimaryKey()
				.String("Category", 50, String.Empty)
				.Execute();
		}
	}
}
