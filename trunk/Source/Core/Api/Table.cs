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

using DbRefactor.Providers;

namespace DbRefactor.Api
{
	public abstract class Table
	{
		private readonly TransformationProvider provider;
		public string TableName { get; set; }

		internal TransformationProvider Provider
		{
			get { return provider; }
		}

		internal Table(TransformationProvider provider, string tableName)
		{
			this.provider = provider;
			TableName = tableName;
		}
	}
}