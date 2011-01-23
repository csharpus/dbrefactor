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

namespace DbRefactor.Core
{
	/// <summary>
	/// Controls what actions are taken when you try to delete a row to which existing foreign keys point
	/// </summary>
	public enum OnDelete
	{
		/// <summary>
		/// Specifies that all the rows with foreign keys pointing to the deleted row are also deleted
		/// </summary>
		Cascade,
		/// <summary>
		/// Specifies that all rows with foreign keys pointing to the deleted row are set to NULL
		/// </summary>
		SetNull,
		/// <summary>
		/// Specifies that the deletion fails with an error
		/// </summary>
		NoAction,
		/// <summary>
		/// Specifies that all rows with foreign keys pointing to the deleted row are set to their default value
		/// </summary>
		SetDefault
	}
}