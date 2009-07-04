using System;
using System.Collections.Generic;
using System.Text;

namespace DbRefactor.Compatibility
{
	[Obsolete("This class will be removed in feature")]
	public abstract class BaseMigration
	{
		/// <summary>
		/// Defines tranformations to port the database to the current version.
		/// </summary>
		public abstract void Up();

		/// <summary>
		/// Defines transformations to revert things done in <c>Up</c>.
		/// </summary>
		public abstract void Down();

		public abstract NewTable CreateTable(string tableName);
		public abstract ActionTable Table(string tableName);
	}
}
