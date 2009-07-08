using System;
using System.Collections.Generic;

namespace DbRefactor.Tools
{
	internal class ParametersHelper
	{
		public static List<string> GetParameters(object obj)
		{
			const string stringPropertyType = "{0}='{1}'";
			const string customPropertyType = "{0}={1}";
			const string nullPropertyType = "{0} is {1}";

			List<string> parameters = new List<string>();

			foreach (var property in obj.GetType().GetProperties())
			{
				object value = null;
				string propertyType;
				// set specific value
				if (property.PropertyType.Name == "Boolean")
					value = (bool) property.GetValue(obj, null) ? "1" : "0";
				else if (property.GetValue(obj, null).GetType().Name == "DBNull")
					value = "NULL";
				else
					value = property.GetValue(obj, null);
				
				// set specific type
				if (property.PropertyType.Name == "String" || property.PropertyType.Name == "DateTime") 
					propertyType = stringPropertyType;
				else if (property.GetValue(obj, null).GetType().Name == "DBNull")
					propertyType = nullPropertyType;
				else 
					propertyType = customPropertyType;

				parameters.Add(String.Format(propertyType, property.Name, value));
			}
			return parameters;
		}
	}
}
