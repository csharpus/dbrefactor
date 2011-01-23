using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DbRefactor.Providers
{
	public class TemplateParser
	{
		private readonly IList<string> results = new List<string>();

		public string Apply()
		{
			return String.Join(" ", results.ToArray());
		}

		public TemplateParser Add(string template, params object[] args)
		{
			if (args.Any(a => a == null)) return this;
			if (args.Any(a => (a is bool) && !(bool)a)) return this;
			if (args.Any(a => a is IEnumerable && ((IEnumerable)a).OfType<object>().Count() == 0)) return this;
			results.Add(String.Format(template, args));
			return this;
		}
	}
}