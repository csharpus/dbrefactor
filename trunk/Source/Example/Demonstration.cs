using System;
using System.Collections.Generic;
using System.Data;
using System.Resources;
using DbRefactor;

namespace Example
{

	/// <summary>
	/// This is a first migration that creates "Role" table.
	/// Down method exists to roll back a database structure to the previous version.
	/// </summary>
	[Migration(1)]
	public class CreateRoleTable: Migration
	{
		public override void Up()
		{
			CreateTable("Role")
				.Int("Id").PrimaryKeyWithIdentity()
				.String("Name", 30).Unique().NotNull()
				.Execute();
		}

		public override void Down()
		{
			DropTable("Role");
		}
	}

	/// <summary>
	/// This is second migration that create User table with some columns.
	/// Columns could have different options that easy to set using fluent interface.
	/// </summary>
	[Migration(2)]
	public class CreateUserTable : Migration
	{
		public override void Up()
		{
			CreateTable("User")
				.Long("Id").PrimaryKey().Identity()
				.String("Name", 70).NotNull().Unique()
				.Boolean("IsRegistered", false)
				.DateTime("Birthday").NotNull()
				.Text("Description").Null()
				.Int("Age")
				.Decimal("Salary", 10, 2)
				.Execute();
		}

		public override void Down()
		{
			DropTable("User");
		}
	}

	/// <summary>
	/// Existing table could be extracted with new column that appear 
	/// during development process as well as you could drop column that become out of the day.
	/// </summary>
	[Migration(3)]
	public class AddColumnToUserTable : Migration
	{
		public override void Up()
		{
			Table("User").AddColumn().Int("RoleId", 2).NotNull().Execute();
		}

		public override void Down()
		{
			Table("User").DropColumn("RoleId");
		}
	}

	/// <summary>
	/// It is easy to reach column with foreign key or drop it.
	/// </summary>
	[Migration(4)]
	public class AddForeignKeyToUserTable: Migration
	{
		public override void Up()
		{
			Table("User").AddForeignKey("RoleId", "Role", "Id", OnDelete.SetDefault);
		}

		public override void Down()
		{
			Table("User").DropForeignKey("FK_User_Role");
		}
	}

	/// <summary>
	/// DBRefactor allow to insert or update rows in database table,
	/// so you could create kind of dictionaries in your migrations
	/// </summary>
	[Migration(5)]
	public class InsertToRoleTable : Migration
	{
		public override void Up()
		{
			Table("Role").Insert(new {Name = "Administrator"});
			Table("Role").Insert(new {Name = "Manager"});
		}

		public override void Down()
		{
			Table("Role").Delete().Where(new {Name = "Administrator"});
			Table("Role").Delete().Where(new { Name = "Manager" });
		}
	}

	/// <summary>
	/// To keep independens from Ids, use SelectScalar with query by constant field
	/// Rows could be deleted. Using "Where" to filter deleted rows
	/// </summary>
	[Migration(6)]
	public class InsertToUserTable : Migration
	{
		public override void Up()
		{
			Table("User").Insert(new {
				RoleId = Table("Role").SelectScalar<int>("Id", new { Name = "Manager" }), 
				Name = "Robert Tompson",
				IsRegistered = true,					
				Birthday = new DateTime(1982, 4, 20)
			});
		}

		public override void Down()
		{
			Table("User").Delete().Where(new { FirstName = "Robert", LastName = "Tompson" });
		}
	}

	/// <summary>
	/// More over, migrations is a way to rename column
	/// </summary>
	[Migration(7)]
	public class RenameDescriptionColumnn : Migration
	{
		public override void Up()
		{
			Table("User").RenameColumn("Description", "PersonalInformation");
		}

		public override void Down()
		{
			Table("User").RenameColumn("PersonalInformation", "Description");
		}
	}
	
	/// <summary>
	/// Also, DBRefactor could be usefull to change column type or any options
	/// that have been set before.
	/// </summary>
	[Migration(8)]
	public class AlterColumnn : Migration
	{
		public override void Up()
		{
			Table("User").Update(new {PersonalInformation = "none..."}).Where(new {PersonalInformation = DBNull.Value});

			Table("User").AlterColumn().String("PersonalInformation", 1000).NotNull().Execute();
		}

		public override void Down()
		{
			Table("User").AlterColumn().Text("PersonalInformation").Null().Execute();
			Table("User").Update(new { PersonalInformation = DBNull.Value }).Where(new { PersonalInformation = "none..." });
		}
	}

	/// <summary>
	/// Likewise, you could use migrations to rename table
	/// </summary>
	[Migration(9)]
	public class RenameRoleTable : Migration
	{
		public override void Up()
		{
			Table("Role").RenameTo("ActorType");
		}

		public override void Down()
		{
			Table("ActorType").RenameTo("Role");
		}
	}

	[Migration(10)]
	public class ExecuteQuery : Migration
	{
		public override void Up()
		{
			using (IDataReader reader = ExecuteQuery("select Id, [Name] from [User] where IsRegistered = 0 or PersonalInformation is null"))
			{

				var systemUsers = new List<string>();

				while (reader.Read())
				{
					var name = reader["Name"].ToString();
					if (name.Contains("$"))
					{
						systemUsers.Add(name);
					}
				}
				foreach (var user in systemUsers)
				{
					Table("User").Update(new {Name = "System/" + user}).Where(new {Name = user});
				}
			}
		
		}

		public override void Down()
		{

		}
	}

	[Migration(11)]
	public class ExecuteScalar : Migration
	{
		public override void Up()
		{
			int value = Convert.ToInt32(ExecuteScalar("select Id from [User] where IsRegistered = 1 or PersonalInformation is null"));
		}

		public override void Down()
		{

		}
	}

	[Migration(12)]
	public class ExecuteNonQuery : Migration
	{
		public override void Up()
		{
			ExecuteNonQuery("delete from [User] where IsRegistered = 0 or PersonalInformation is null");
		}

		public override void Down()
		{

		}
	}

	[Migration(13)]
	public class CreateProcedure_GetAllRoles : Migration
	{
		public override void Up()
		{
			string filePath = @"D:\Projects\CSharpUs.DBRefactor\Source\Example\ScriptFiles\013\CreateProcedure_GetAllRoles.sql";
			ExecuteFile(filePath);
		}

		public override void Down()
		{
			string filePath = @"D:\Projects\CSharpUs.DBRefactor\Source\Example\ScriptFiles\013\DropProcedure_GetAllRoles.sql";
			ExecuteFile(filePath);
		}
	}

	[Migration(14)]
	public class CreateProcedureFromEmbeddedRsource : Migration
	{
		public override void Up()
		{
			string filePath = @"Example.ScriptFiles._014.CreateProcedure_GetAllUsers.sql";
            ExecuteResource(this.GetType().Assembly.FullName, filePath);
		}

		public override void Down()
		{
			string filePath = @"Example.ScriptFiles._014.DropProcedure_GetAllUsers.sql";
			ExecuteResource(this.GetType().Assembly.FullName, filePath);
		}
	}
}
