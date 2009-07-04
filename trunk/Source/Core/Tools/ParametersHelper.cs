using System;
using System.Collections.Generic;

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
				object value = null;
				if (property.PropertyType.Name == "Boolean")
					value = (bool)property.GetValue(obj, null) ? "1" : "0";
				else
					value = property.GetValue(obj, null);

				parameters.Add(String.Format(property.PropertyType.Name == "String" || property.PropertyType.Name == "DateTime" ? 
					stringProperty : customTypeProperty,
					property.Name,
					value));
			}
			return parameters;
		}
	}
}
