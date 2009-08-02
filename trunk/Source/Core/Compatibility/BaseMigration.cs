#region License

//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.

#endregion

using System;

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
