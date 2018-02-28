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
using System.IO;
using System.Xml;

namespace NLOC.Core
{
	/// <summary>
	/// Represents a source file.
	/// </summary>
	public class SourceFile
	{
		FileInfo file;
		string fileKey;
		CountingResult report;

		/// <summary>
		/// Initializes a new instance of the <see cref="SourceFile"/> class 
		/// using the specified path.
		/// </summary>
		/// <param name="filePath">The relative or absolute path of the file.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="filePath"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="filePath"/> is invalid.
		/// </exception>
		public SourceFile(string filePath)
		{
			if (filePath == null)
				throw new ArgumentNullException("filePath");

			try {
				file = new FileInfo(filePath);
				fileKey = GenerateFileKey(file);
			}
			catch (Exception ex) {
				throw new ArgumentException("File path is invalid.", "filePath", ex);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SourceFile"/> class 
		/// using the specified <see cref="FileInfo"/> object.
		/// </summary>
		/// <param name="file">The <see cref="FileInfo"/> object that represents 
		/// the file.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="filePath"/> is null.
		/// </exception>
		public SourceFile(FileInfo file)
		{
			if (file == null)
				throw new ArgumentNullException("file");

			this.file = file;
			this.fileKey = GenerateFileKey(file);
		}

		/// <summary>
		/// Gets the value whether the file system is incase-sensitive.
		/// </summary>
		public static bool IgnoreCase
		{
			get { return (Environment.OSVersion.Platform != PlatformID.Unix); }
		}

		/// <summary>Gets the name of the file.</summary>
		public string Name
		{
			get { return file.Name; }
		}

		/// <summary>Gets the full path of the file's directory.</summary>
		public string Directory
		{
			get { return file.DirectoryName; }
		}

		/// <summary>Gets the full path of the file.</summary>
		public string Path
		{
			get { return file.FullName; }
		}

		/// <summary>Gets the extension part of the file.</summary>
		public string Extension
		{
			get { return file.Extension; }
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

		internal string FileKey
		{
			get { return fileKey; }
		}

		private static string GenerateFileKey(FileInfo file)
		{
			if (IgnoreCase)
				return file.FullName.ToLower();
			else
				return file.FullName;
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
			XmlElement dirElement = document.CreateElement("Directory");
			XmlElement nameElement = document.CreateElement("FileName");
			XmlElement resultElement = document.CreateElement("Result");

			dirElement.InnerText = Directory;
			nameElement.InnerText = Name;
			if (report != null)
				report.Serialize(resultElement);

			element.AppendChild(dirElement);
			element.AppendChild(nameElement);
			element.AppendChild(resultElement);
		}

		/// <summary>
		/// Determines whether the specified <see cref="object"/> is equal to 
		/// the current <see cref="SourceFile"/>.
		/// </summary>
		/// <param name="obj">The <see cref="object"/> to compare with the 
		/// current <see cref="SourceFile"/>.</param>
		/// <returns>true if the specified <see cref="object"/> is equal to 
		/// the current <see cref="SourceFile"/>; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return (obj is SourceFile && ((SourceFile) obj).fileKey == this.fileKey);
		}

		/// <summary>
		/// Serves as a hash function for a particular type. <see cref="GetHashCode"/> is 
		/// suitable for use in hashing algorithms and data structures like a hash table.
		/// </summary>
		/// <returns>A hash code for the current <see cref="SourceFile"/>.</returns>
		public override int GetHashCode()
		{
			return fileKey.GetHashCode();
		}

		/// <summary>
		/// Returns the full path of the file.
		/// </summary>
		/// <returns>The full path of the file.</returns>
		public override string ToString()
		{
			return file.FullName;
		}
	}
}
