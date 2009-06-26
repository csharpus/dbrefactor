﻿using System;
using DbRefactor;

namespace DbRefactor.Tests
{
	/// <summary>
	/// This is a first migration that create "Role" table.
	/// Down method exists to roll back a database structure to previos version.
	/// </summary>
	[Migration(1)]
	public class CreateRoleTable: Migration
	{
		public override void Up()
		{
			CreateTable("Role")
				.Int("Id").PrimaryKeyWithIdentity().NotNull()
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
				.Int("Id").PrimaryKeyWithIdentity().NotNull()
				.String("FirstName", 70).NotNull()
				.String("LastName", 70).NotNull()
				.Boolean("Gender")
				.DateTime("Birthday").NotNull()
				.Text("Description").Null()
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
			AlterTable("User").AddColumn().Int("RoleId", 2).NotNull().Execute();
		}

		public override void Down()
		{
			AlterTable("User").DropColumn("RoleId");
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
			AlterTable("User").AddForeignKey("RoleId", "Role", "Id", OnDelete.SetDefault);
		}

		public override void Down()
		{
			AlterTable("User").DropForeignKey("FK_User_Id");
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
				RoleId = (int)SelectScalar("RoleId", "Role", "Name = Manager"), 
				FirstName = "Robert", 
				LastName = "Tompson",
				Gender = true,					// true - male; false - female
				Birthday = DateTime.Parse("20/04/1982")
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
			AlterTable("").RenameColumn("Description", "PersonalInformation");
		}

		public override void Down()
		{
			AlterTable("").RenameColumn("PersonalInformation", "Description");
		}
	}

}
