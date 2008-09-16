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

namespace Migrator.Loggers
{
	/// <summary>
	/// Interface � �tendre pour d�finir un loggeur d'�v�nement
	/// du m�diateur de migration.
	/// <see cref="ConsoleLogger">ConsoleLogger</see> est le loggeur
	/// par d�faut et affiche toute les sorties � la console.
	/// </summary>
	public interface ILogger
	{
		/// <summary>
		/// Informe que la migration d�bute.
		/// </summary>
		/// <param name="currentVersion">La version actuel de la base de donn�e</param>
		/// <param name="finalVersion">La version vers laquelle migrer</param>
		void Started(int currentVersion, int finalVersion);
		
		/// <summary>
		/// Informe qu'une migration vers le haut s'ex�cute.
		/// </summary>
		/// <param name="version">La version de la migration</param>
		/// <param name="migrationName">Le nom de la migration</param>
		void MigrateUp(int version, string migrationName);
		
		/// <summary>
		/// Informe qu'une migration vers le bas s'ex�cute.
		/// </summary>
		/// <param name="version">La version de la migration</param>
		/// <param name="migrationName">Le nom de la migration</param>
		void MigrateDown(int version, string migrationName);
		
		/// <summary>
		/// Informe qu'une migration correspondant au num�ro de
		/// version est introuvable et sera ignor�e.
		/// </summary>
		/// <param name="version">La version introuvable</param>
		void Skipping(int version);
		
		/// <summary>
		/// Informe que les modifications � la base de donn�es
		/// seront annul�es.
		/// </summary>
		/// <param name="originalVersion">
		/// Version initiale de la base de donn�es.
		/// Vers laquelle on retourne.
		/// </param>
		void RollingBack(int originalVersion);
		
		/// <summary>
		/// Informe qu'une exception est survenue lors d'une
		/// migration.
		/// </summary>
		/// <param name="version">La version de la migration qui a produire l'exception.</param>
		/// <param name="migrationName">Le nom de la migration qui a produire l'exception.</param>
		/// <param name="ex">L'exception lanc�e</param>
		void Exception(int version, string migrationName, Exception ex);
		
		/// <summary>
		/// Informe que le processus de migration s'est termin�e avec succ�s.
		/// </summary>
		/// <param name="originalVersion">La version intiale de la base de donn�es</param>
		/// <param name="currentVersion">La version actuel de la base de donn�e</param>
		void Finished(int originalVersion, int currentVersion);
		
		/// <summary>
		/// Affiche un message d'information.
		/// </summary>
		/// <param name="format">Le format ("{0}, blbla {1}") ou la cha�ne � afficher.</param>
		/// <param name="args">Les param�tres dans le cas d'un format au premier param�tre.</param>
		void Log(string format, params object[] args);
		
		/// <summary>
		/// Affiche un avertissement.
		/// </summary>
		/// <param name="format">Le format ("{0}, blbla {1}") ou la cha�ne � afficher.</param>
		/// <param name="args">Les param�tres dans le cas d'un format au premier param�tre.</param>
		void Warn(string format, params object[] args);
		
		/// <summary>
		/// Affiche une information de d�boguage.
		/// </summary>
		/// <param name="format">Le format ("{0}, blbla {1}") ou la cha�ne � afficher.</param>
		/// <param name="args">Les param�tres dans le cas d'un format au premier param�tre.</param>
		void Trace(string format, params object[] args);
	}
}
