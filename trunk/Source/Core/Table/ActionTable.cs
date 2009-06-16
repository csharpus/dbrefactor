using System;
using System.Collections.Generic;
using System.Text;
using DbRefactor.Providers;
using DbRefactor.Tools.DesignByContract;

namespace DbRefactor
{
	public class ActionTable: Table
	{
		private List<string> columnValues;
		//private List<List<string>> actionList;
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
			//actionList = new List<List<string>>();
			columnValues = new List<string>();
		}

		public ActionTable Insert()
		{
			if(Operation.None != operation)
				Execute();
			//actionList.Add(new List<string>());
			return TableOperation(Operation.Insert);
		}

		public ActionTable Update(string table)
		{
			if (Operation.None != operation)
				Execute();
			//actionList.Add(new List<string>());
			return TableOperation(Operation.Update);
		}

		public ActionTable AddParameter(string column, string value)
		{
			columnValues.Add(String.Format(StringParameterPattern, column, value));
			return this;
		}

		public ActionTable AddParameter(string column, int value)
		{
			columnValues.Add(String.Format(NotStringParameterPattern, column, value));
			return this;
		}

		public ActionTable AddParameter(string column, DateTime value)
		{
			AddParameter(column, value.ToString());
			return this;
		}

		public ActionTable AddParameter(string column, decimal value)
		{
			columnValues.Add(String.Format(NotStringParameterPattern, column, value));
			return this;
		}

		public ActionTable AddParameter(string column, bool value)
		{
			columnValues.Add(String.Format(NotStringParameterPattern, column, value ? 1 : 0));
			return this;
		}

		public ActionTable AddParameter(string column, long value)
		{
			columnValues.Add(String.Format(NotStringParameterPattern, column, value));
			return this;
		}

		private void Execute()
		{
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

		private ActionTable TableOperation(Operation tableOperation)
		{
			Check.Ensure(operation == Operation.None || operation == tableOperation, "You couls not use different type of operations.");
			operation = tableOperation;
			return this;
		}

	}
}
