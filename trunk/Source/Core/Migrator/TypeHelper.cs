using System;
using System.Collections.Generic;
using System.Text;

namespace Migrator
{
	public class TypeHelper
	{
		public class _Decimal
		{
		}

		public class _Money
		{
		}

		public class _Text
		{
		}

		public static Type Decimal
		{
			get {
				return typeof(_Decimal);
			}
		}

		public static Type Money
		{
			get
			{
				return typeof(_Money);
			}
		}

		public static Type Text
		{
			get
			{
				return typeof(_Text);
			}
		}
	}
}
