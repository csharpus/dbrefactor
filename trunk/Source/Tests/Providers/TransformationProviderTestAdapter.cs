#region License
//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.
#endregion
using System;

using NUnit.Framework;

using Migrator.Loggers;
using Migrator.Providers.TypeToSqlProviders;

namespace Migrator.Providers.Tests
{
	/// <summary>
	/// Cette classe d�l�gue les appels au gestionnaire de tranformations
	/// sp�cifi� dans le constructeur et v�rifie que chaque tranformation
	/// est r�ellement appliqu�e � la base de donn�es.
	/// <para>
	/// Par exemple, lors de l'appel � la m�thode AddColumn, l'ajout sera
	/// automatiquement v�rifi�e par un appel � la m�thode ColumnExists.
	/// </para>
	/// <para>
	/// Cette classe est utilis�e pour tester automatiquement les migrations.
	/// </para>
	/// </summary>
	public class TransformationProviderTestAdapter : TransformationProvider
	{
		TransformationProvider _realProvider;
		
		//public TransformationProviderTestAdapter(TransformationProvider realProvider)
		//{
		//    _realProvider = realProvider;
		//}
		
		public override void AddColumn(string table, string column, Type type, int size, ColumnProperties property, object defaultValue)
		{
			_realProvider.AddColumn(table, column, type, size, property, defaultValue);
			Assert.IsTrue(_realProvider.ColumnExists(table, column),
			              string.Format("The column {0}.{1} failed to be created", table, column));
		}
		
		public override void RemoveColumn(string table, string column)
		{
			_realProvider.RemoveColumn(table, column);
			Assert.IsFalse(_realProvider.ColumnExists(table, column),
			               string.Format("The column {0}.{1} failed to be removed", table, column));
		}
		
		public override void RemoveTable(string name)
		{
			_realProvider.RemoveTable(name);
			
			Assert.IsFalse(_realProvider.TableExists(name));
		}

		public override void RenameColumn(string table, string oldCOlumnName, string newColumnName)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override void RenameTable(string oldTableName, string newTableName)
		{
			throw new Exception("The method or operation is not implemented.");
		}
		
		public override void AddTable(string name, params Column[] columns)
		{
			_realProvider.AddTable(name, columns);

			Assert.IsTrue(_realProvider.TableExists(name));
			foreach (Column c in columns)
			{
				Assert.IsTrue(_realProvider.ColumnExists(name, c.Name),
				             string.Format("The column {0}.{1} failed to be created", name, c.Name));
			}
		}

		#region Simple delegate method
		public override int CurrentVersion {
			get {
				return _realProvider.CurrentVersion;
			}
			set {
				_realProvider.CurrentVersion = value;
			}
		}
		
		override public ILogger Logger
		{
			get { return _realProvider.Logger; }
			set { _realProvider.Logger = value; }
		}
		
		public override bool ColumnExists(string table, string column)
		{
			return _realProvider.ColumnExists(table, column);
		}
		
		public override bool TableExists(string table)
		{
			return _realProvider.TableExists(table);
		}
		
		public override void AddPrimaryKey(string name, string table, params string[] columns)
		{
			_realProvider.AddPrimaryKey(name, table, columns);
		}
		
		public override void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable, string[] refColumns)
		{
			_realProvider.AddForeignKey(name, primaryTable, primaryColumns, refTable, refColumns);
		}
		
		public override void RemoveForeignKey(string name, string table)
		{
			_realProvider.RemoveForeignKey(name, table);
		}
						
		public override bool ConstraintExists(string name, string table)
		{
			return _realProvider.ConstraintExists(name, table);
		}
		
		public override string[] GetTables()
		{
			return _realProvider.GetTables();
		}
		
		public override Column[] GetColumns(string table)
		{
			return _realProvider.GetColumns(table);
		}
		#endregion


		public override void AddForeignKey(string name, string primaryTable, string[] primaryColumns, string refTable, string[] refColumns, global::Migrator.Providers.ForeignKeys.ForeignKeyConstraint constraint)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override global::Migrator.Providers.ForeignKeys.IForeignKeyConstraintMapper ForeignKeyMapper
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		protected override void AddTable(string name, string columns)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override global::Migrator.Providers.TypeToSqlProviders.ITypeToSqlProvider TypeToSqlProvider
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public override void AddColumn(string table, string sqlColumn)
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}
}
