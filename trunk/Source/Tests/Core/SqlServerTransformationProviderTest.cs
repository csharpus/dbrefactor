using System;
using DbRefactor.Columns;
using DbRefactor.Providers;
using NUnit.Framework;
using System.Data;
using NMock2;
using NIs = NMock2.Is;
using Is = NUnit.Framework.SyntaxHelpers.Is;


namespace DbRefactor.Tests.Core
{
	[TestFixture]
	public class SqlServerTransformationProviderTest
	{
		private TransformationProvider _provider;
		private IDatabaseEnvironment environment;
		private IDataReader reader;
		private Mockery mocks;

		[SetUp]
		public void SetUp()
		{
			mocks = new Mockery();
			environment = mocks.NewMock<IDatabaseEnvironment>();
			_provider = new TransformationProvider(environment);
			reader = mocks.NewMock<IDataReader>();
			Expect.AtLeast(0).On(reader).Method("Dispose").WithNoArguments();
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void	DesignByContractException()
		{
			_provider.AddTable(null);
		}

		[Test]
		public void CheckTableExists()
		{
			ReaderReturnTrueOnRead();
			ExpectTableExistsQuery("Table1");
			_provider.TableExists("Table1");
			mocks.VerifyAllExpectationsHaveBeenMet();
		}

		[Test]
		public void RemoveTableIfTableExists()
		{
			ReaderReturnTrueOnRead();
			ExpectTableExistsQuery("Table1");
			ExpectExecuteNonQueryOn("DROP TABLE [Table1]");
			_provider.DropTable("Table1");
			mocks.VerifyAllExpectationsHaveBeenMet();
		}

		[Test]
		public void DoNotRemoveTableIfTableDoesNotExists()
		{
			ReaderReturnFalseOnRead();
			ExpectTableExistsQuery("Table1");
			_provider.DropTable("Table1");
			mocks.VerifyAllExpectationsHaveBeenMet();
		}

		[Test]
		public void RenameColumn()
		{
			ExpectExecuteNonQueryOn("EXEC sp_rename '[Table].[OldName]', '[NewName]', 'COLUMN'");
			_provider.RenameColumn("Table", "OldName", "NewName");
			mocks.VerifyAllExpectationsHaveBeenMet();
		}

		[Test]
		public void RenameTable()
		{
			ExpectExecuteNonQueryOn("EXEC sp_rename '[OldName]', '[NewName]', 'OBJECT'");
			_provider.RenameTable("OldName", "NewName");
			mocks.VerifyAllExpectationsHaveBeenMet();
		}

		[Test]
		public void ColumnExists()
		{
			ReaderReturnTrueOnRead();
			ExpectTableExistsQuery("Table1");
			ExpectColumnExistsQuery("Table1", "Column1");
			_provider.ColumnExists("Table1", "Column1");
			mocks.VerifyAllExpectationsHaveBeenMet();
		}

		[Test]
		public void TableExists()
		{
			ReaderReturnTrueOnRead();
			ExpectTableExistsQuery("Table1");
			_provider.TableExists("Table1");
			mocks.VerifyAllExpectationsHaveBeenMet();
		}

		[Test]
		public void ConstraintExists()
		{
			ReaderReturnTrueOnRead();
			ExpectExecuteQueryOn("SELECT TOP 1 * FROM sysobjects WHERE id = object_id('FK_NAME')");
			_provider.ConstraintExists("FK_NAME", "TableName");
			mocks.VerifyAllExpectationsHaveBeenMet();
		}

		[Test]
		public void StringColumn()
		{
			Column column = new Column("Column1", typeof (string), 10);
			Assert.That(column.ColumnSQL(), Is.EqualTo("Column1 nvarchar(10) NULL"));
		}

		[Test]
		public void IntNotNullColumn()
		{
			Column column = new Column("Column1", typeof(int), ColumnProperties.NotNull);
			Assert.That(column.ColumnSQL(), Is.EqualTo("Column1 int NOT NULL"));
		}

		[Test]
		public void BooleanColumnWithDefaultValue()
		{
			Column column = new Column("Column1", typeof(bool), ColumnProperties.NotNull, true);
			Assert.That(column.ColumnSQL(), Is.EqualTo("Column1 bit NOT NULL DEFAULT 1"));
		}

		[Test]
		public void PrimaryKeyColumn()
		{
			Column column = new Column("ID", typeof(int), ColumnProperties.PrimaryKeyWithIdentity);
			Assert.That(column.ColumnSQL(), Is.EqualTo("ID int NOT NULL IDENTITY PRIMARY KEY"));
		}

		[Test]
		public void CreateTableWithOneColumn()
		{
			ExpectExecuteNonQueryOn("CREATE TABLE [Table1] (ID int NULL)");
			_provider.AddTable("Table1",
				new Column("ID", typeof(int)));
			mocks.VerifyAllExpectationsHaveBeenMet();
		}

		[Test]
		public void CreateTableWithTwoColumns()
		{
			ExpectExecuteNonQueryOn("CREATE TABLE [Table1] (ID int NULL, Name nvarchar(10) NULL)");
			_provider.AddTable("Table1",
				new Column("ID", typeof(int)),
				new Column("Name", typeof(string), 10));
			mocks.VerifyAllExpectationsHaveBeenMet();
		}

		private void ReaderReturnTrueOnRead()
		{
			Expect.AtLeast(1).On(reader)
				.Method("Read")
				.WithNoArguments()
				.Will(Return.Value(true));
		}

		private void ReaderReturnFalseOnRead()
		{
			Expect.Once.On(reader)
				.Method("Read")
				.WithNoArguments()
				.Will(Return.Value(false));
		}

		private void ExpectExecuteQueryOn(string query)
		{
			Expect.Once.On(environment)
				.Method("ExecuteQuery")
				.With(query)
				.Will(Return.Value(reader));
		}

		private void ExpectExecuteNonQueryOn(string query)
		{
			Expect.Once.On(environment)
				.Method("ExecuteNonQuery")
				.With(query)
				.Will(Return.Value(1));
		}

		private void ExpectTableExistsQuery(string table)
		{
			ExpectExecuteQueryOn(
				String.Format("SELECT TOP 1 * FROM syscolumns WHERE id=object_id('{0}')", table));
		}

		private void ExpectColumnExistsQuery(string table, string column)
		{
			ExpectExecuteQueryOn(
				String.Format("SELECT TOP 1 * FROM syscolumns WHERE id=object_id('{0}') AND name='{1}'",
				table, column));
		}
	}
}