using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbRefactor.Tools
{
	internal class ParametersHelper
	{
		public static List<string> GetParameters(object obj)
		{
			const string stringProperty = "{0}='{1}'";
			const string customTypeProperty = "{0}={1}";
			List<string> parameters = new List<string>();

			foreach (var property in obj.GetType().GetProperties())
			{
				parameters.Add(String.Format(property.PropertyType.Name == "String" || property.PropertyType.Name == "DateTime" ? 
					stringProperty : customTypeProperty,
					property.Name,
					property.GetValue(obj, null)));
			}
			return parameters;
		}
	}
}
