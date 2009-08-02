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
