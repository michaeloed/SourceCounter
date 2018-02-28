// NLOC - source line counter tool
// Copyright (C) 2007 Balazs Tihanyi
// 
// This program is free software; you can redistribute it and/or modify it under 
// the terms of the GNU General Public License as published by the Free Software 
// Foundation; either version 3 of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT 
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with 
// this program; if not, write to the Free Software Foundation, Inc., 
// 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Xml;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NLOC.Core
{
	/// <summary>
	/// Represents a named group that contains related source files.
	/// </summary>
	public class Project
	{
		string name;
		Dictionary<string, SourceFile> files = new Dictionary<string, SourceFile>(10);
		CountingResult report;

		/// <summary>
		/// Initializes a new instance of the <see cref="Project"/> class with 
		/// the specified name.
		/// </summary>
		/// <param name="name">The name of the project. It cannot be null or 
		/// empty string.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> is null or empty string.
		/// </exception>
		public Project(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentException("Name cannot be null or empty string.", "name");

			this.name = name;
		}

		/// <summary>
		/// Gets the name of the project.
		/// </summary>
		public string Name
		{
			get { return name; }
		}

		/// <summary>
		/// Gets the collection of the files contained by the project. 
		/// </summary>
		public IEnumerable<SourceFile> Files
		{
			get { return files.Values; }
		}

		/// <summary>
		/// Gets the total number of source files.
		/// </summary>
		public int FileCount
		{
			get { return files.Count; }
		}

		/// <summary>
		/// Gets or sets the results of a successful counting.
		/// </summary>
		/// <value>The <see cref="CountingResult"/> object of a 
		/// successful counting or null.</value>
		public CountingResult Report
		{
			get { return report; }
			set { report = value; }
		}

		/// <summary>
		/// Adds a source file to the project if it the project does not 
		/// yet contain a similar file.
		/// </summary>
		/// <param name="file">The file to be added. If the 
		/// <paramref name="file"/> is null, nothing happens.</param>
		public void AddFile(SourceFile file)
		{
			if (file != null && !files.ContainsKey(file.FileKey))
				files.Add(file.FileKey, file);
		}

		/// <summary>
		/// Determines whether the project has a similar file.
		/// </summary>
		/// <param name="file">The file to locate in the project.</param>
		/// <returns>true if the file is found in the project; otherwise, 
		/// false.</returns>
		public bool Contains(SourceFile file)
		{
			if (file == null)
				return false;
			else
				return files.ContainsKey(file.FileKey);
		}

		/// <summary>
		/// Returns a similar file containing by the project.
		/// </summary>
		/// <param name="file">The file to locate in the project.</param>
		/// <returns>A <see cref="SourceFile"/> object of the project that is 
		/// similar to the given <paramref name="file"/> or null if the project 
		/// does not contain such file.</returns>
		public SourceFile Find(SourceFile file)
		{
			if (file == null)
				return null;
			
			SourceFile returnValue;
			if (files.TryGetValue(file.FileKey, out returnValue))
				return returnValue;
			else
				return null;
		}

		/// <summary>
		/// Removes the file from the project.
		/// </summary>
		/// <param name="file">The file to remove from the project.</param>
		/// <returns>true if the file is successfully removed; otherwise, false.
		/// </returns>
		public bool Remove(SourceFile file)
		{
			if (file == null)
				return false;
			else
				return files.Remove(file.FileKey);
		}

		/// <summary>
		/// Excludes every file matching the pattern.
		/// </summary>
		/// <param name="pattern">A <see cref="Regex"/> pattern for the file's 
		/// name.</param>
		/// <returns>The count of the removed files.</returns>
		public int ExcludeFiles(Regex pattern)
		{
			if (pattern == null)
				return 0;

			List<SourceFile> blacklist = new List<SourceFile>(files.Values);

			// Remove files from blacklist that are not matching the pattern
			blacklist.RemoveAll(delegate(SourceFile file) {
				return !pattern.IsMatch(file.Name);
			});
			
			// Remove files from the project specified by the blacklist
			for (int i = 0; i < blacklist.Count; i++)
				files.Remove(blacklist[i].FileKey);

			return blacklist.Count;
		}

		/// <summary>
		/// Serializes counting informations.
		/// </summary>
		/// <param name="element">An <see cref="XmlElement"/> node 
		/// used to write counting informations.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="element"/> is null.
		/// </exception>
		internal void Serialize(XmlElement element)
		{
			if (element == null)
				throw new ArgumentNullException("element");

			XmlDocument document = element.OwnerDocument;			
			XmlElement nameElement   = document.CreateElement("Name");
			XmlElement resultElement = document.CreateElement("Result");

			element.AppendChild(nameElement);
			element.AppendChild(resultElement);

			nameElement.InnerText = name;
			if (report != null)
				report.Serialize(resultElement);
			foreach (SourceFile file in files.Values) {
				XmlElement fileElement = document.CreateElement("File");
				file.Serialize(fileElement);
				element.AppendChild(fileElement);
			}
		}

		/// <summary>
		/// Returns the name of the project.
		/// </summary>
		/// <returns>The name of the project.</returns>
		public override string ToString()
		{
			return name;
		}
	}
}
