using System;
using DbRefactor.Infrastructure.Loggers;
using DbRefactor.Providers;
using NUnit.Framework;
using System.Data;
using Rhino.Mocks;
using System.Collections.Generic;


namespace DbRefactor.Tests.Core
{
	[TestFixture]
	public class SqlServerTransformationProviderTest
	{
		private TransformationProvider provider;
		private MockRepository mockery;
		private IDataReader reader;
		private IDatabaseEnvironment environment;

		[SetUp]
		public void SetUp()
		{
			mockery = new MockRepository();
			reader = mockery.CreateMock<IDataReader>();
			environment = mockery.CreateMock<IDatabaseEnvironment>();
			provider = new TransformationProvider(environment, new Logger(false), null, null);

			Expect.Call(reader.Dispose).Repeat.Any();
		}

		[Test]
		public void CheckTableExists()
		{
			using (mockery.Record())
			{
				ReaderReturnTrueOnRead();
				ExpectTableExistsQuery("Table1");
			}
			using(mockery.Playback())
			{
				provider.TableExists("Table1");
			}
		}

		[Test]
		public void RemoveTableIfTableExists()
		{
			using (mockery.Record())
			{
				ReaderReturnTrueOnRead();
				ExpectTableExistsQuery("Table1");
				ExpectExecuteNonQueryOn("DROP TABLE [Table1]");
			}
			using (mockery.Playback())
			{
				provider.DropTable("Table1");
			}
		}

		[Test]
		public void DoNotRemoveTableIfTableDoesNotExists()
		{
			using (mockery.Record())
			{
				ReaderReturnFalseOnRead();
				ExpectTableExistsQuery("Table1");
			}
			using (mockery.Playback())
			{
				provider.DropTable("Table1");
			}
		}

		[Test]
		public void RenameColumn()
		{
			using (mockery.Record())
			{
				ExpectExecuteNonQueryOn("EXEC sp_rename 'Table.OldName', 'NewName', 'COLUMN'");
			}
			using (mockery.Playback())
			{
				provider.RenameColumn("Table", "OldName", "NewName");
			}
		}

		[Test]
		public void RenameTable()
		{
			using (mockery.Record())
			{
				ExpectExecuteNonQueryOn("EXEC sp_rename 'OldName', 'NewName', 'OBJECT'");
			}
			using (mockery.Playback())
			{
				provider.RenameTable("OldName", "NewName");
			}
		}

		[Test]
		public void ColumnExists()
		{
			using (mockery.Record())
			{
				ReaderReturnTrueOnRead();
				ExpectTableExistsQuery("Table1");
				ExpectColumnExistsQuery("Table1", "Column1");
			}
			using (mockery.Playback())
			{
				provider.ColumnExists("Table1", "Column1");
			}
		}

		[Test]
		public void TableExists()
		{
			using (mockery.Record())
			{
				ReaderReturnTrueOnRead();
				ExpectTableExistsQuery("Table1");
			}
			using (mockery.Playback())
			{
				provider.TableExists("Table1");
			}
		}

		[Test]
		public void ConstraintExists()
		{
			using (mockery.Record())
			{
				ReaderReturnTrueOnRead();
				ExpectExecuteQueryOn("SELECT TOP 1 * FROM sysobjects WHERE id = object_id('FK_NAME')");
			}
			using (mockery.Playback())
			{
				provider.ConstraintExists("FK_NAME");
			}
		}

		//[Test]
		//public void StringColumn()
		//{
		//    var column = new Column("Column1", typeof (string), 10);
		//    Assert.That(column.ColumnSQL(), Is.EqualTo("[Column1] nvarchar(10) NULL"));
		//}

		//[Test]
		//public void IntNotNullColumn()
		//{
		//    var column = new Column("Column1", typeof(int), ColumnProperties.NotNull);
		//    Assert.That(column.ColumnSQL(), Is.EqualTo("[Column1] int NOT NULL"));
		//}

		//[Test]
		//public void BooleanColumnWithDefaultValue()
		//{
		//    var column = new Column("Column1", typeof(bool), ColumnProperties.NotNull, true);
		//    Assert.That(column.ColumnSQL(), Is.EqualTo("[Column1] bit NOT NULL DEFAULT 1"));
		//}

		//[Test]
		//public void PrimaryKeyColumn()
		//{
		//    var column = new Column("ID", typeof(int), ColumnProperties.PrimaryKeyWithIdentity);
		//    Assert.That(column.ColumnSQL(), Is.EqualTo("[ID] int NOT NULL IDENTITY PRIMARY KEY"));
		//}

		//[Test]
		//public void CreateTableWithOneColumn()
		//{
		//    using (mockery.Record())
		//    {
		//        ExpectExecuteNonQueryOn("CREATE TABLE [Table1] ([ID] int NULL)");
		//    }
		//    using (mockery.Playback())
		//    {
		//        provider.AddTable("Table1", 
		//            new Column("ID", typeof(int)));
		//    }
		//}

		//[Test]
		//public void CreateTableWithTwoColumns()
		//{
		//    using (mockery.Record())
		//    {
		//        ExpectExecuteNonQueryOn("CREATE TABLE [Table1] ([ID] int NULL, [Name] nvarchar(10) NULL)");
		//    }
		//    using (mockery.Playback())
		//    {
		//        provider.AddTable("Table1",
		//            new Column("ID", typeof(int)),
		//            new Column("Name", typeof(string), 10));
		//    }
		//}

		private void ReaderReturnTrueOnRead()
		{
			Expect.Call(reader.Read()).Return(true).Repeat.AtLeastOnce();
		}

		private void ReaderReturnFalseOnRead()
		{
			Expect.Call(reader.Read()).Return(false);
		}

		private void ExpectExecuteQueryOn(string query)
		{
			Expect.Call(environment.ExecuteQuery(query))
				.Return(reader);
		}

		private void ExpectExecuteNonQueryOn(string query)
		{
			Expect.Call(environment.ExecuteNonQuery(query))
				.Return(1);
		}

		private void ExpectTableExistsQuery(string table)
		{
			ExpectExecuteQueryOn(
				String.Format("SELECT TOP 1 * FROM sysobjects WHERE id = object_id('{0}')", table));
		}

		private void ExpectColumnExistsQuery(string table, string column)
		{
			ExpectExecuteQueryOn(
				String.Format("SELECT TOP 1 * FROM syscolumns WHERE id = object_id('{0}') AND name = '{1}'",
				
				table, column));
		}

		[Test]
		public void DependencySorter_should_sort_tables_ascending()
		{
			var tables = new List<string> {"Parent", "Child"};

			var relations 
				= new List<Relation> {new Relation("Parent", "Child")};

			List<string> sorted = TransformationProvider.DependencySorter.Run(tables, relations);
			Assert.That(sorted[0], Is.EqualTo("Child"));
			Assert.That(sorted[1], Is.EqualTo("Parent"));
		}

		[Test]
		public void InsertLocalizationLetters()
		{
			using (mockery.Record())
			{
				ExpectExecuteNonQueryOn(string.Format("INSERT INTO [{0}] ({1}, {2}) VALUES ({3}, '{4}')", "Table1", "Id", "Name", 1, "Èìÿ"));
			}
			using (mockery.Playback())
			{
				provider.Insert("Table1",
				string.Format("Id={0}", 1),
				string.Format("Name='{0}'", "Èìÿ"));
			}

		}
	}
}