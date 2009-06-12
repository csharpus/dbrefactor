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

		private const string StringParameterPattern = "{0}='{1}'";
		private const string NotStringParameterPattern = "{0}={1}";

		private enum Operation 
		{
			None,
			Insert,
			Update
		} ;

		private Operation operation = Operation.None;

		public ActionTable(IDatabaseEnvironment environment) : base(environment)
		{
			columnValues = new List<string>();
		}

		public ActionTable Insert()
		{
			return TableOperation(Operation.Insert);
		}

		public ActionTable Update(string table)
		{
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

		public int Execute()
		{
			Check.Ensure(operation != Operation.None, "The operation has not been set.");
			Check.Ensure(columnValues.Count != 0, "Values have not been set.");

			TransformationProvider provider = new TransformationProvider(databaseEnvironment);
			return provider.Insert(TableName, columnValues.ToArray());
		}

		private ActionTable TableOperation(Operation tableOperation)
		{
			Check.Ensure(operation == Operation.None, "The operation already exists.");
			operation = tableOperation;
			return this;
		}

	}
}
