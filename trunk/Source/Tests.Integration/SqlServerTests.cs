using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using DbRefactor.Providers;
using DbRefactor.Providers.Columns;
using DbRefactor.Tools;
using NUnit.Framework;
using Rhino.Mocks;

namespace DbRefactor.Tests.Integration
{
	[TestFixture]
	public class SqlServerTests
	{
		private TransformationProvider provider;

		[Test]
		public void should_create_table()
		{
			CreateMigration<CreateTableMigration>().Up();

			Assert.That(provider.TableExists("Test"), Is.True);
			Assert.That(provider.ColumnExists("Test", "Id"));
		}

		[Migration(1)]
		public class CreateTableMigration : UpMigration
		{
			public override void Up()
			{
				CreateTable("Test").Int("Id").Execute();
			}
		}

		[Test]
		public void should_drop_table()
		{
			CreateMigration<CreateTableMigration>().Up();
			CreateMigration<DropTableMigration>().Up();
			Assert.That(provider.TableExists("Test"), Is.False);
		}

		[Migration(1)]
		public class DropTableMigration : UpMigration
		{
			public override void Up()
			{
				DropTable("Test");
			}
		}

		[Test]
		public void should_add_column()
		{
			CreateMigration<CreateTableMigration>().Up();
			CreateMigration<AddColumnMigration>().Up();

			Assert.That(provider.ColumnExists("Test", "Name"), Is.True);
		}

		[Migration(1)]
		public class AddColumnMigration : UpMigration
		{
			public override void Up()
			{
				Table("Test").AddColumn().String("Name", 5).Execute();
			}
		}

		[Test]
		public void should_create_foreign_key()
		{
			CreateMigration<CreateTableMigration>().Up();
			CreateMigration<CreateForeignKeyMigration>().Up();

			Assert.That(provider.ConstraintExists("FK_Dependent_Test"), Is.True);
		}

		[Test]
		public void should_create_schema_dump()
		{
			//var dumper = new SchemaDumper(@"Data Source=.\SQLEXPRESS;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");
			//dumper.Dump();

			#region CreateTable

			provider.ExecuteNonQuery(
				@"CREATE TABLE [dbo].[Table1](
	[BI] [bigint] NULL,
	[BN] [binary](50) NULL,
	[BT] [bit] NULL,
	[CH] [char](10) COLLATE Cyrillic_General_CI_AS NULL,
	[DT] [datetime] NULL,
	[DC] [decimal](18, 0) NULL,
	[FT] [float] NULL,
	[IM] [image] NULL,
	[IT] [int] NULL,
	[MN] [money] NULL,
	[NC] [nchar](10) COLLATE Cyrillic_General_CI_AS NULL,
	[NT] [ntext] COLLATE Cyrillic_General_CI_AS NULL,
	[NM] [numeric](18, 0) NULL,
	[NV] [nvarchar](50) COLLATE Cyrillic_General_CI_AS NULL,
	[NB] [nvarchar](max) COLLATE Cyrillic_General_CI_AS NULL,
	[RL] [real] NULL,
	[SD] [smalldatetime] NULL,
	[SI] [smallint] NULL,
	[SM] [smallmoney] NULL,
	[SV] [sql_variant] NULL,
	[TX] [text] COLLATE Cyrillic_General_CI_AS NULL,
	[TS] [timestamp] NULL,
	[TI] [tinyint] NULL,
	[UI] [uniqueidentifier] NOT NULL,
	[VB] [varbinary](50) NULL,
	[VM] [varbinary](max) NULL,
	[VC] [varchar](50) COLLATE Cyrillic_General_CI_AS NULL,
	[VR] [varchar](max) COLLATE Cyrillic_General_CI_AS NULL,
	[XL] [xml] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]");

			#endregion

			var providers = provider.GetColumnProviders("Table1");
			string values = String.Empty;
			foreach (var columnProvider in providers)
			{
				var value = GetMethodValue(columnProvider);
				values += Environment.NewLine + "." + value;
				foreach (var property in columnProvider.Properties)
				{
					values += "." + property.MethodName() + "()";
				}
			}
			values += ";";
			Console.Write(values);
		}

		[Test]
		public void should_generate_method_call_from_lambda()
		{
			var codeGenerationService = MockRepository.GenerateMock<ICodeGenerationService>();
			codeGenerationService.Expect(s => s.PrimitiveValue(1)).Return("1");
			var longProvider = new LongProvider("ColumnName", 1, codeGenerationService);
			string methodValue = GetMethodValue(longProvider);
			Assert.That(methodValue, Is.EqualTo("Long(\"ColumnName\")"));
			//Assert.That(ToCsharpString(new DateTime()), Is.EqualTo("4m"));
			//Assert.That(ToCsharpStringComplex(new[] { "1" }), Is.EqualTo("4m"));
		}

		private string GetMethodValue(ColumnProvider columnProvider)
		{
			var methodCall = columnProvider.Method().Body as MethodCallExpression;
			string methodName = methodCall.Method.Name;
			
			IList<Expression> argumentsList;
			if (methodCall.Method.IsStatic)
			{
				argumentsList = methodCall.Arguments.Skip(1).ToList();
			}
			else
			{
				argumentsList = methodCall.Arguments.ToList();
			}
			var arguments = argumentsList.Select(a => ObtainValue(a)).ToArray();
			string methodArguments = String.Join(", ", arguments);
			if (columnProvider.HasDefaultValue)
			{
				methodArguments += ", " + columnProvider.GetDefaultValueCode();
			}
			return String.Format("{0}({1})", methodName, methodArguments);
		}

		public string ObtainValue(Expression expression)
		{
			object value = ValueFromExpression(expression);
			//string stringValue = value.ToString();
			//return (value is string) ? "\"" + stringValue + "\"" : stringValue;

			return ReflectionHelper.ToCsharpString(value);
		}

		private string ToCsharpStringComplex(object value)
		{
			CodeDomProvider cs = CodeDomProvider.CreateProvider("CSharp");
			var p = new CodeObjectCreateExpression(typeof (string[]), new CodePrimitiveExpression("a"));
			var options = new CodeGeneratorOptions {BracingStyle = "C", IndentString = "  "};
			var writer = new StringWriter();
			cs.GenerateCodeFromExpression(p, writer, options);
			return writer.ToString();
		}

		private static object ValueFromExpression(Expression expression)
		{
			if (expression is ConstantExpression)
			{
				return (expression as ConstantExpression).Value;
			}
			var lambda = Expression.Lambda<Func<object>>(
				Expression.Convert(expression, typeof (object)),
				new ParameterExpression[0]);
			return lambda.Compile()();
		}

		[Migration(1)]
		public class CreateForeignKeyMigration : UpMigration
		{
			public override void Up()
			{
				throw new NotImplementedException();
			}
		}

		private TMigration CreateMigration<TMigration>()
			where TMigration : Migration, new()
		{
			return new TMigration {TransformationProvider = provider};
		}

		[SetUp]
		public void Setup()
		{
			CreateProvider();
			DropAllTables();
		}

		private void DropAllTables()
		{
			foreach (var table in provider.GetTables())
			{
				provider.DropTable(table);
			}
		}

		private void CreateProvider()
		{
			provider =
				new ProviderFactory().Create(@"Data Source=.\SQLEXPRESS;Initial Catalog=dbrefactor_tests;Integrated Security=SSPI");
		}

		// private Migration CreateMigration(Func<)
	}

	public abstract class UpMigration : Migration
	{
		public override void Down()
		{
		}
	}
}