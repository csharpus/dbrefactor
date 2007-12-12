using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Migrator.Providers;
using NUnit.Framework.SyntaxHelpers;
using System.Data;

namespace Tipait.DbRefactor.Tests.Core
{
	[TestFixture]
	public class SqlServerTransformationProviderTest
	{
		private TransformationProvider _provider;
		private DatabaseEnvironmentStub _databaseEnvironmentStub;
		private IDatabaseEnvironment environment;
		private IDataReader reader;
		private NMock2.Mockery mocks;

		public SqlServerTransformationProviderTest()
		{
		}

		[SetUp]
		public void SetUp()
		{
			mocks = new NMock2.Mockery();
			environment = mocks.NewMock<IDatabaseEnvironment>();
			_databaseEnvironmentStub = new DatabaseEnvironmentStub();
			_provider = new TransformationProvider(environment);
			reader = mocks.NewMock<IDataReader>();
			NMock2.Expect.AtLeast(0).On(reader).Method("Dispose").WithNoArguments();
		}

		[Test]
		public void Creation()
		{
			TransformationProvider provider
				= new TransformationProvider(new DatabaseEnvironmentStub());
		}

		public void CheckExecuteNonQuery()
		{
			
		}

		//[Test]
		//public void CheckAddForeignKey()
		//{
		//    _provider.AddForeignKey("FK_Name", "PrimaryTable", "PrimaryColumn", "RefTable", "RefColumn");
		//    Assert.That(_databaseEnvironmentStub.LattestSql, 
		//        Is.EqualTo(
		//            "ALTER TABLE PrimaryTable ADD CONSTRAINT FK_Name " +
		//            "FOREIGN KEY (PrimaryColumn) REFERENCES RefTable (RefColumn)"));
		//}

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
			ExpectExecuteNonQueryOn("DROP TABLE Table1");
			_provider.RemoveTable("Table1");
			mocks.VerifyAllExpectationsHaveBeenMet();
		}

		[Test]
		public void DoNotRemoveTableIfTableDoesNotExists()
		{
			ReaderReturnFalseOnRead();
			ExpectTableExistsQuery("Table1");
			_provider.RemoveTable("Table1");
			mocks.VerifyAllExpectationsHaveBeenMet();
		}

		private void ReaderReturnTrueOnRead()
		{
			NMock2.Expect.Once.On(reader)
				.Method("Read").WithNoArguments().Will(NMock2.Return.Value(true));
		}

		private void ReaderReturnFalseOnRead()
		{
			NMock2.Expect.Once.On(reader)
				.Method("Read").WithNoArguments().Will(NMock2.Return.Value(false));
		}

		private void ExpectExecuteQueryOn(string query)
		{
			NMock2.Expect.Once.On(environment)
				.Method("ExecuteQuery")
				.With(query)
				.Will(NMock2.Return.Value(reader));
		}

		private void ExpectExecuteNonQueryOn(string query)
		{
			NMock2.Expect.Once.On(environment)
				.Method("ExecuteNonQuery")
				.With(query)
				.Will(NMock2.Return.Value(1));
		}

		private void ExpectTableExistsQuery(string table)
		{
			ExpectExecuteQueryOn(
				String.Format("SELECT TOP 1 * FROM syscolumns WHERE id=object_id('{0}')", table));
		}
	}
}
