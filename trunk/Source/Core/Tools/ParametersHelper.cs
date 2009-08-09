#region License

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

namespace DbRefactor.Tools
{
	internal class ParametersHelper
	{
// TODO: refactor this helper: retrieve table structure first and then generate values using ColumnMappers
		public static List<string> GetParameters(object obj)
		{
			const string stringPropertyType = "{0}='{1}'";
			const string customPropertyType = "{0}={1}";
			const string nullPropertyType = "{0} is {1}";

			var parameters = new List<string>();

			foreach (var property in obj.GetType().GetProperties())
			{
				object value;
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
