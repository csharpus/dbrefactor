﻿#region License

//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace DbRefactor.Infrastructure
{
	public static class CodeGenerationHelper
	{
		public static string GenerateMethodCall(string name, IEnumerable<string> arguments)
		{
			return String.Format("{0}({1})", name, String.Join(", ", arguments.ToArray()));
		}
	}
}