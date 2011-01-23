using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DbRefactor.Core;
using DbRefactor.Exceptions;

namespace DbRefactor.Runner
{
	public interface IMigrationReader
	{
		IEnumerable<IVersionedMigration> ReadFrom(Assembly assembly);
	}

	public class MigrationReader : IMigrationReader
	{
		private readonly IMigrationTarget migrationTarget;

		public MigrationReader(IMigrationTarget migrationTarget)
		{
			this.migrationTarget = migrationTarget;
		}

		public IEnumerable<IVersionedMigration> ReadFrom(Assembly assembly)
		{
			var migrations = assembly.GetTypes()
				.Where(IsMigration)
				.Select(m => new VersionedMigration(
				             	migrationTarget.CreateMigration(m),
				             	GetMigrationVersion(m),
				             	ToHumanName(m.Name)))
				.OrderBy(t => t.Version)
				.Cast<IVersionedMigration>()
				.ToList();
			CheckForDuplicatedVersion(migrations);
			return migrations;
		}

		internal static string ToHumanName(string className)
		{
			string name = Regex.Replace(className, "([A-Z])", " $1").Substring(1);
			return name.Substring(0, 1).ToUpper() + name.Substring(1).ToLower();
		}

		public static int GetMigrationVersion(Type t)
		{
			MigrationAttribute attribute = GetMigrationAttribute(t);
			if (attribute == null)
			{
				throw new ArgumentException("Specified type doesn't marked as a Migration", "t");
			}
			return attribute.Version;
		}

		public static bool IsMigration(Type t)
		{
			MigrationAttribute attribute = GetMigrationAttribute(t);
			return attribute != null
			       && (typeof (Migration).IsAssignableFrom(t))
			       && !attribute.Ignore;
		}

		private static MigrationAttribute GetMigrationAttribute(Type t)
		{
			//return (MigrationAttribute) t.GetCustomAttributes(typeof (MigrationAttribute), true).FirstOrDefault();
			return (MigrationAttribute)Attribute.GetCustomAttribute(t, typeof(MigrationAttribute));
		}

		private void CheckForDuplicatedVersion(IEnumerable<IVersionedMigration> migrations)
		{
			var versions = new List<int>();

			foreach (var migration in migrations)
			{
				if (versions.Contains(migration.Version))
					throw new DbRefactorException(String.Format("Migration version #{0} is duplicated", migration.Version));

				versions.Add(migration.Version);
			}
		}
	}
}