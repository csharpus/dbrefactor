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

namespace DbRefactor
{
	using System;

	internal class MigrationHelper
	{
		private static MigrationAttribute GetMigrationAttribute(Type t)
		{
			var attrib = (MigrationAttribute)Attribute
				.GetCustomAttribute(t, typeof(MigrationAttribute));
			
			return attrib;
		}

		public static int GetMigrationVersion(Type t)
		{
			MigrationAttribute attribute = GetMigrationAttribute(t);
			if(attribute == null)
			{
				throw new ArgumentException("Specified type doesn't marked as a Migration", "t");
			}
			return attribute.Version;
		}

		public static bool IsMigration(Type t)
		{
			MigrationAttribute attribute = GetMigrationAttribute(t);
			return attribute != null 
				&& 
					(typeof (Migration).IsAssignableFrom(t) 
					)
				&& !attribute.Ignore;
		}
	}
}
