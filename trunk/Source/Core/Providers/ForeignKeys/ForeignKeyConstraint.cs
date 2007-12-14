namespace DbRefactor.Providers.ForeignKeys
{
	public enum ForeignKeyConstraint
	{
		Cascade,
		SetNull,
		NoAction,
		Restrict,
		SetDefault
	}
}