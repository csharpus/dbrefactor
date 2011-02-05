using System.Data.SqlClient;
using NUnit.Framework;

namespace DbRefactor.Tests.Integration.Verification
{
	[TestFixture]
	public class Ado : ProviderTestBase
	{
		// TODO: refactor tests to inherit from another base class with closed connection
		public override void Setup()
		{
			CreateProvider();
			DatabaseEnvironment.OpenConnection();
			DatabaseEnvironment.BeginTransaction();
			DropAllTables();
			DatabaseEnvironment.CommitTransaction();
			DatabaseEnvironment.CloseConnection();
		}

		public override void TearDown()
		{

		}

		[Test]
		public void should_perform_operation_in_transaction()
		{
			CreateTestTable();

			using (var connection = new SqlConnection(ConnectionString))
			{
				connection.Open();
				try
				{
					using (SqlTransaction transaction = connection.BeginTransaction())
					{
						try
						{
							using (var command = new SqlCommand("insert into A (B) values (100)", connection, transaction))
							{
								command.ExecuteNonQuery();
							}
							transaction.Commit();
						}
						catch
						{
							transaction.Rollback();
							throw;
						}
					}
				}
				finally
				{
					connection.Close();
				}
			}

			DatabaseEnvironment.OpenConnection();
			using (var reader = Database.Table("A").Select("B"))
			{
				Assert.That(reader.Read());
				Assert.That(reader[0], Is.EqualTo(100));
			}
			DatabaseEnvironment.CloseConnection();
		}

		private void CreateTestTable()
		{
			DatabaseEnvironment.OpenConnection();
			DatabaseEnvironment.BeginTransaction();
			Database.CreateTable("A").Int("B").Execute();
			DatabaseEnvironment.CommitTransaction();
			DatabaseEnvironment.CloseConnection();
		}

		[Test]
		public void should_be_able_to_use_two_transactions_in_one_connection()
		{
			CreateTestTable();

			using (var connection = new SqlConnection(ConnectionString))
			{
				connection.Open();
				try
				{
					using (var transaction = connection.BeginTransaction())
					{
						try
						{
							using (var command = new SqlCommand("insert into A (B) values (100)", connection, transaction))
							{
								command.ExecuteNonQuery();
							}
							transaction.Commit();
						}
						catch
						{
							transaction.Rollback();
							throw;
						}
					}

					using (var transaction = connection.BeginTransaction())
					{
						try
						{
							using (var command = new SqlCommand("insert into A (B) values (100)", connection, transaction))
							{
								command.ExecuteNonQuery();
							}
							transaction.Commit();
						}
						catch
						{
							transaction.Rollback();
							throw;
						}
					}
				}
				finally
				{
					connection.Close();
				}
			}

			DatabaseEnvironment.OpenConnection();
			using (var reader = Database.Table("A").Select("B"))
			{
				Assert.That(reader.Read());
				Assert.That(reader[0], Is.EqualTo(100));
				Assert.That(reader.Read());
				Assert.That(reader[0], Is.EqualTo(100));
			}
			DatabaseEnvironment.CloseConnection();
		}

		[Test]
		public void should_be_able_to_rollback_changes()
		{
			CreateTestTable();

			using (var connection = new SqlConnection(ConnectionString))
			{
				connection.Open();
				try
				{
					using (var transaction = connection.BeginTransaction())
					{
						try
						{
							using (var command = new SqlCommand("insert into A (B) values (100)", connection, transaction))
							{
								command.ExecuteNonQuery();
							}
							using (var command = new SqlCommand("insert into NotExists (B) values (100)", connection, transaction))
							{
								command.ExecuteNonQuery();
							}
							transaction.Commit();
						}
						catch
						{
							transaction.Rollback();
							// no rethrow
						}
					}
				}
				finally
				{
					connection.Close();
				}
			}

			DatabaseEnvironment.OpenConnection();
			using (var reader = Database.Table("A").Select("B"))
			{
				Assert.That(!reader.Read());
			}
			DatabaseEnvironment.CloseConnection();
		}

		[Test]
		public void should_be_able_to_rollback_changes_in_one_transaction()
		{
			CreateTestTable();

			using (var connection = new SqlConnection(ConnectionString))
			{
				connection.Open();
				try
				{
					using (var transaction = connection.BeginTransaction())
					{
						try
						{
							using (var command = new SqlCommand("insert into A (B) values (100)", connection, transaction))
							{
								command.ExecuteNonQuery();
							}
							transaction.Commit();
						}
						catch
						{
							transaction.Rollback();
							throw;
						}
					}

					using (var transaction = connection.BeginTransaction())
					{
						try
						{
							using (var command = new SqlCommand("insert into NotExists (B) values (100)", connection, transaction))
							{
								command.ExecuteNonQuery();
							}
							transaction.Commit();
						}
						catch
						{
							transaction.Rollback();
							// no rethrow
						}
					}
				}
				finally
				{
					connection.Close();
				}
			}

			DatabaseEnvironment.OpenConnection();
			using (var reader = Database.Table("A").Select("B"))
			{
				Assert.That(reader.Read());
				Assert.That(reader[0], Is.EqualTo(100));
			}
			DatabaseEnvironment.CloseConnection();
		}
	}
}