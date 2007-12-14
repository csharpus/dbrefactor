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
				case ForeignKeyConstraint.Restrict:
					return Restrict;
				case ForeignKeyConstraint.SetDefault:
					return SetDefault;
				case ForeignKeyConstraint.SetNull:
					return SetNull;
				default:
					return NoAction;
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

		public string Restrict
		{
			get { return "RESTRICT"; }
		}

		public string SetDefault
		{
			get { return "SET DEFAULT"; }
		}
	}
}