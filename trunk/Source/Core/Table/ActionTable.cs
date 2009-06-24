using System;
using System.Collections.Generic;
using DbRefactor.Providers;
using DbRefactor.Tools;
using DbRefactor.Tools.DesignByContract;

namespace DbRefactor
{
	public class ActionTable: Table
	{
		private List<string> columnValues;
        private const string StringParameterPattern = "{0}='{1}'";
		private const string NotStringParameterPattern = "{0}={1}";

		private enum Operation 
		{
			None,
			Insert,
			Update
		} ;

		//private Operation operation = Operation.None;

		public ActionTable(IDatabaseEnvironment environment, string tableName) : base(environment, tableName)
		{
			columnValues = new List<string>();
		}

		public ActionTable Insert(object parameters)
		{
			Execute(parameters, Operation.Insert);
			return this;
		}

		public ActionTable Update(object parameters)
		{
			Execute(parameters, Operation.Update);
			return this;
		}

		private void Execute(object parameters, Operation operation)
		{
			List<string> paramList = ParametersHelper.GetParameters(parameters);
			AddParameters(paramList);

			Check.Ensure(operation != Operation.None, "The operation has not been set.");
			Check.Ensure(columnValues.Count != 0, "Values have not been set.");

			TransformationProvider provider = new TransformationProvider(databaseEnvironment);
            
			if(operation == Operation.Insert)
				provider.Insert(TableName, columnValues.ToArray());
			else
				provider.Update(TableName, columnValues.ToArray());

			operation = Operation.None;
			columnValues = new List<string>();
		}

		private void AddParameters(IEnumerable<string> parameters)
		{
			foreach (var parameter in parameters)
				columnValues.Add(parameter);
		}
	}
}
