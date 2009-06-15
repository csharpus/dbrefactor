using System;
using System.Collections.Generic;
using System.Text;
using DbRefactor.Providers;
using DbRefactor.Tools.DesignByContract;

namespace DbRefactor
{
	public class ActionTable: Table
	{
		//private List<string> columnValues;
		private List<List<string>> actionList;
		private const string StringParameterPattern = "{0}='{1}'";
		private const string NotStringParameterPattern = "{0}={1}";

		private enum Operation 
		{
			None,
			Insert,
			Update
		} ;

		private Operation operation = Operation.None;

		public ActionTable(IDatabaseEnvironment environment, string tableName) : base(environment, tableName)
		{
			actionList = new List<List<string>>();
		}

		public ActionTable Insert()
		{
			actionList.Add(new List<string>());
			return TableOperation(Operation.Insert);
		}

		public ActionTable Update(string table)
		{
			actionList.Add(new List<string>());
			return TableOperation(Operation.Update);
		}

		public ActionTable AddParameter(string column, string value)
		{
			actionList[actionList.Count -1].Add(String.Format(StringParameterPattern, column, value));
			return this;
		}

		public ActionTable AddParameter(string column, int value)
		{
			actionList[actionList.Count - 1].Add(String.Format(NotStringParameterPattern, column, value));
			return this;
		}

		public ActionTable AddParameter(string column, DateTime value)
		{
			AddParameter(column, value.ToString());
			return this;
		}

		public ActionTable AddParameter(string column, decimal value)
		{
			actionList[actionList.Count - 1].Add(String.Format(NotStringParameterPattern, column, value));
			return this;
		}

		public ActionTable AddParameter(string column, bool value)
		{
			actionList[actionList.Count - 1].Add(String.Format(NotStringParameterPattern, column, value ? 1 : 0));
			return this;
		}

		public ActionTable AddParameter(string column, long value)
		{
			actionList[actionList.Count - 1].Add(String.Format(NotStringParameterPattern, column, value));
			return this;
		}

		public void Execute()
		{
			Check.Ensure(operation != Operation.None, "The operation has not been set.");
			//Check.Ensure(columnValues.Count != 0, "Values have not been set.");

			TransformationProvider provider = new TransformationProvider(databaseEnvironment);

			foreach (var columnList in actionList)
			{
				if(operation == Operation.Insert)
					provider.Insert(TableName, columnList.ToArray());
				else
					provider.Update(TableName, columnList.ToArray());
			}
		}

		private ActionTable TableOperation(Operation tableOperation)
		{
			Check.Ensure(operation == Operation.None || operation == tableOperation, "You couls not use different type of operations.");
			operation = tableOperation;
			return this;
		}

	}
}
