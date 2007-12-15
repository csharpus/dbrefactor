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
					return Cascade;
				case ForeignKeyConstraint.SetDefault:
					return SetDefault;
				case ForeignKeyConstraint.SetNull:
					return SetNull;
				case ForeignKeyConstraint.NoAction:
					return NoAction;
				default:
					throw new ArgumentOutOfRangeException("constraint");
			}
		}

		public string Cascade
		{
			get { return "CASCADE"; }
		}

		public string SetNull
		{
			get { return "SET NULL"; }
		}

		public string NoAction
		{
			get { return "NO ACTION"; }
		}

		public string SetDefault
		{
			get { return "SET DEFAULT"; }
		}
	}
}