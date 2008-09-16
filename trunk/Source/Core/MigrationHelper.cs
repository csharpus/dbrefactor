namespace DbRefactor
{
	using System;
	using OldMigrator = global::Migrator;
	using DbRefactor.Compatibility;

	public class MigrationHelper
	{
		private static BaseMigrationAttribute GetMigrationAttribute(Type t)
		{
			MigrationAttribute attrib = (MigrationAttribute)Attribute
				.GetCustomAttribute(t, typeof(MigrationAttribute));
			if (attrib == null)
			{
				OldMigrator.MigrationAttribute oldAttribute = (OldMigrator.MigrationAttribute)Attribute
				.GetCustomAttribute(t, typeof(OldMigrator.MigrationAttribute));
				return oldAttribute;
			}
			return attrib;
		}

		public static int GetMigrationVersion(Type t)
		{
			BaseMigrationAttribute attribute = GetMigrationAttribute(t);
			if(attribute == null)
			{
				throw new ArgumentException("Specified type doesn't marked as a Migration", "t");
			}
			return attribute.Version;
		}

		public static bool IsMigration(Type t)
		{
			BaseMigrationAttribute attribute = GetMigrationAttribute(t);
			return attribute != null 
				&& 
					(typeof (DbRefactor.Migration).IsAssignableFrom(t) 
					|| typeof(OldMigrator.Migration).IsAssignableFrom(t))
				&& !attribute.Ignore;
		}
	}
}
