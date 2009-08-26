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

namespace DbRefactor.Engines.SqlServer
{
	internal sealed class SqlServerForeignKeyConstraintMapper
	{
		public string Resolve(OnDelete constraint)
		{
			switch(constraint)
			{
				case OnDelete.Cascade: 
					return "CASCADE";
				case OnDelete.SetDefault:
					return "SET DEFAULT";
				case OnDelete.SetNull:
					return "SET NULL";
				case OnDelete.NoAction:
					return "NO ACTION";
				default:
					throw new ArgumentOutOfRangeException("constraint");
			}
		}
	}
}