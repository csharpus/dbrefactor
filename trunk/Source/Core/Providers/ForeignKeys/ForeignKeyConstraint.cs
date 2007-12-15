namespace DbRefactor.Providers.ForeignKeys
{
	/// <summary>
	/// Controls what actions are taken when you try to delete a row to which existing foreign keys point
	/// </summary>
	public enum ForeignKeyConstraint
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