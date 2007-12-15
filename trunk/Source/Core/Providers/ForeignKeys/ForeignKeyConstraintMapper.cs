using System;

namespace DbRefactor.Providers.ForeignKeys
{
	sealed class ForeignKeyConstraintMapper
	{
		public string Resolve(ForeignKeyConstraint constraint)
		{
			switch(constraint)
			{
				case ForeignKeyConstraint.Cascade:
					return "CASCADE";
				case ForeignKeyConstraint.SetDefault:
					return "SET DEFAULT";
				case ForeignKeyConstraint.SetNull:
					return "SET NULL";
				case ForeignKeyConstraint.NoAction:
					return "NO ACTION";
				default:
					throw new ArgumentOutOfRangeException("constraint");
			}
		}
	}
}