using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DbRefactor.Providers;
using NUnit.Framework;
using Rhino.Mocks;

namespace DbRefactor.Tests
{
	public class TestMigration: Migration
	{
		public override void Up()
		{
			throw new System.NotImplementedException();
		}

		public override void Down()
		{
			throw new System.NotImplementedException();
		}
	}

	[TestFixture]
	public class Demonstration
	{
		private TransformationProvider _provider;
		private MockRepository mockery;
		private IDataReader Rreader;
		private IDatabaseEnvironment Renvironment;
		private TestMigration testMigrator; 

		[SetUp]
		public void SetUp()
		{
			mockery = new MockRepository();
			Rreader = mockery.CreateMock<IDataReader>();
			Renvironment = mockery.CreateMock<IDatabaseEnvironment>();
			_provider = new TransformationProvider(Renvironment);
			
			testMigrator = new TestMigration();
			testMigrator.TransformationProvider = _provider;

			Expect.Call(Rreader.Dispose).Repeat.Any();
		}

		[Test]
		public void CreateTable()
		{
			testMigrator
				.CreateTable("User")
				.Int("Id").PrimaryKey()
				.String("Name", 50)
				.Execute();
		}

		[Test]
		public void TableInsert()
		{
			testMigrator
				.Table("User")
				.Insert().AddParameter("Name", "Mx")
				.Insert().AddParameter("Name", "Dima")
				.Insert().AddParameter("Name", "Egor");
		}

	}
}
