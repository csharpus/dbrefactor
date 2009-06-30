using System;
using System.Collections.Generic;
using System.Text;

namespace DbRefactor.Compatibility
{
	/// <summary>
	/// Describe a migration
	/// </summary>
	[Obsolete("This class will be removed in feature")]
	public class BaseMigrationAttribute : Attribute
	{
		private int _version;
		private bool _ignore = false;

		/// <summary>
		/// Describe the migration
		/// </summary>
		/// <param name="version">The unique version of the migration.</param>	
		public BaseMigrationAttribute(int version)
		{
			_version = version;
		}

		/// <summary>
		/// The version reflected by the migration
		/// </summary>
		public int Version
		{
			get
			{
				return _version;
			}
			set
			{
				_version = value;
			}
		}

		/// <summary>
		/// Set to <c>true</c> to ignore this migration.
		/// </summary>
		public bool Ignore
		{
			get
			{
				return _ignore;
			}
			set
			{
				_ignore = value;
			}
		}

	}
}
