using System;

namespace DbRefactor.Providers.ForeignKeys
{
	sealed class ForeignKeyConstraintMapper
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