namespace DbRefactor.Engines
{
	internal interface IColumnProperties
	{
		string NotNull();
		string PrimaryKey(string name);
		string Unique(string name);
		string Identity();
		string Default(string value);
		string Null();
	}
}