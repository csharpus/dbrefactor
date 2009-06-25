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
			Update,
			Delete
		} ;

		private Operation actionOperation = Operation.None;
		private object operationParameters = null;

		public ActionTable(IDatabaseEnvironment environment, string tableName) : base(environment, tableName)
		{
			columnValues = new List<string>();
		}

		/// <summary>
		/// Insert new record to database table
		/// </summary>
		/// <param name="parameters">This is parameters for operation Insert.<br />
		/// To add parameters you could use next syntaxes
		/// Table(TableName).Insert(new {ColumnName1=Parameter1, ColumName2="StringParameter2", ...})
		/// </param>
		/// <returns></returns>
		public ActionTable Insert(object parameters)
		{
			Check.Ensure(actionOperation == Operation.None || actionOperation == Operation.None, "Only One type of operation allowed.");
			actionOperation = Operation.Insert;
			operationParameters = parameters;
			Execute(operationParameters, null);
			return this;
		}

		/// <summary>
		/// Update record in database table
		/// </summary>
		/// <param name="parameters">This is parameters for operation Update.<br />
		/// To add parameters you could use next syntaxes
		/// Table(TableName).Update(new {ColumnName1=Parameter1, ColumName2="StringParameter2", ...})
		/// </param>
		/// <returns></returns>
		public ActionTable Update(object parameters)
		{
			Check.Ensure(actionOperation == Operation.None, "Please specify criteria for previous update operation.");
			operationParameters = parameters;
			actionOperation = Operation.Update;
			return this;
		}

		public ActionTable Delete()
		{
			Check.Ensure(actionOperation == Operation.None, "Please specify criteria for previous operation.");
			actionOperation = Operation.Update;
			return this;

		}


		public ActionTable Where(object parameters)
		{
			Execute(operationParameters, parameters);
			return this;
		}

		private void Execute(object operationParams, object criteriaParameters)
		{
			List<string> updateParamList = ParametersHelper.GetParameters(operationParams);
			AddParameters(updateParamList);

			Check.Ensure(actionOperation != Operation.None, "The operation has not been set.");
			Check.Ensure(columnValues.Count != 0, "Values have not been set.");

			TransformationProvider provider = new TransformationProvider(databaseEnvironment);

			if (actionOperation == Operation.Insert)
			{
				provider.Insert(TableName, columnValues.ToArray());
			}
			else if (actionOperation == Operation.Delete)
			{
				List<string> crieriaParamList = ParametersHelper.GetParameters(criteriaParameters);
				provider.Update(TableName, columnValues.ToArray(), crieriaParamList.ToArray());
			}
			else
			{
				List<string> crieriaParamList = ParametersHelper.GetParameters(criteriaParameters);
				provider.Delete(TableName, crieriaParamList.ToArray());

			}
			actionOperation = Operation.None;
			columnValues = new List<string>();
		}

		private void AddParameters(IEnumerable<string> parameters)
		{
			foreach (var parameter in parameters)
				columnValues.Add(parameter);
		}
	}
}
