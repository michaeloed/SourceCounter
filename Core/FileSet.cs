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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NLOC.Core
{
	/// <summary>
	/// Represents a file handler class.
	/// </summary>
	public class FileSet : IFileHandler
	{
		static readonly Project defaultProject = new Project("Default Project");
		static string[] validExtensions = new string[] {
			".c", ".cpp", ".h", ".hpp", ".cc", ".hh", ".cxx", ".hxx",
			".cs", ".java", ".vb", ".bas", ".cls", ".frm", ".p", ".pp", ".pas", ".dpr"
		};

		Dictionary<string, Project> projects = new Dictionary<string, Project>();
		List<string> excludePatterns = new List<string>();
		CountingResult report;
		int fileCount = 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="FileSet"/> class.
		/// </summary>
		public FileSet()
		{
			AddProject(defaultProject);
		}

		/// <summary>
		/// Gets the collection of the known projects.
		/// </summary>
		public IEnumerable<Project> Projects
		{
			get { return projects.Values; }
		}

		/// <summary>
		/// Gets the total number of source files.
		/// </summary>
		public int FileCount
		{
			get { return fileCount; }
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
		/// Clears all the file and project informations.
		/// </summary>
		public void Clear()
		{
			projects.Clear();
			AddProject(defaultProject);
			excludePatterns.Clear();
			fileCount = 0;
			report = null;
		}

		private bool IsPermittedExtension(string extension)
		{
			int index = -1;

			// Varifying the extension whether it is valid
			for (int i = 0; i < validExtensions.Length; i++) {
				if (extension == validExtensions[i])
					index = i;
			}

			if (index == -1) {
				return false; // The extension is invalid
			}
			else {
				// Brings the current extension to the first place
				if (index > 0) {
					for (int i = index; i > 0; i--)
						validExtensions[i] = validExtensions[i - 1];
					validExtensions[0] = extension;
				}
				return true; // The extension is valid
			}
		}

		/// <exception cref="ArgumentException">
		/// <paramref name="pathPattern"/> is invalid or cannot be accessed.
		/// </exception>
		private FileInfo[] GetFiles(string pathPattern, bool recurse)
		{
			try {
				string dir = Path.GetDirectoryName(pathPattern);
				string filePattern = Path.GetFileName(pathPattern);

				if (dir == null)
					dir = Path.GetPathRoot(pathPattern);
				if (dir == "")
					dir = ".";

				if (filePattern == "")
					throw new ArgumentException("Path does not contain file part.");

				DirectoryInfo directory = new DirectoryInfo(dir);
				if (recurse)
					return directory.GetFiles(filePattern, SearchOption.AllDirectories);
				else
					return directory.GetFiles(filePattern, SearchOption.TopDirectoryOnly);
			}
			catch (UnauthorizedAccessException ex) {
				throw new ArgumentException(
					"Path cannot be accessed.", ex);
			}
			catch (IOException ex) {
				throw new ArgumentException(
					"Path is invalid or could not be found.", ex);
			}
		}

		private SourceFile AddFileIfNew(SourceFile file, Project targetProject)
		{
			// Check whether file is already assigned to a named project
			foreach (Project project in projects.Values) {
				SourceFile newFile = project.Find(file);

				// If file has been added before
				if (newFile != null) {
					// Check whether file is in defaultProject
					if (project == defaultProject && targetProject != defaultProject) {
						defaultProject.Remove(file);
						targetProject.AddFile(file);
					}
					return newFile;
				}
			}

			if (defaultProject.Contains(file)) { // the file has been added before 
				return file;
			}			
			else { // the file is new
				targetProject.AddFile(file);
				fileCount++;
				return file;
			}
		}

		/// <summary>
		/// Adds the file(s) to the default project.
		/// </summary>
		/// <param name="filePath">The path of the file(s) to be added.
		/// The file name can contain wildcards optionally.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="filePath"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="filePath"/> is invalid.
		/// </exception>
		public void AddFile(string filePath)
		{
			if (filePath == null)
				throw new ArgumentNullException("filePath");

			AddFile(filePath, defaultProject);
		}

		/// <summary>
		/// Adds the file(s) to the project with the specified name.
		/// </summary>
		/// <param name="filePath">The path of the file(s) to be added.
		/// The file name can contain wildcards optionally.</param>
		/// <param name="projectName">The name of the target project.
		/// If the <paramref name="projectName"/> is null or empty, 
		/// the file will be added to the default project.
		/// If the <paramref name="projectName"/> is unknown, a new 
		/// project will be created with the specified name.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="filePath"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="filePath"/> is invalid or cannot be accessed.
		/// </exception>
		public void AddFile(string filePath, string projectName)
		{
			if (filePath == null)
				throw new ArgumentNullException("filePath");

			Project targetProject = defaultProject;

			if (!string.IsNullOrEmpty(projectName))
				targetProject = CreateProjectIfNew(projectName);

			AddFile(filePath, targetProject);
		}

		/// <exception cref="ArgumentException">
		/// <paramref name="filePath"/> is invalid or cannot be accessed.
		/// </exception>
		private void AddFile(string filePath, Project targetProject)
		{
			bool hasWildcard = (filePath.Contains("*") || filePath.Contains("?"));

			if (hasWildcard) {
				AddFiles(filePath, false, targetProject);
			}
			else {
				string extension = Path.GetExtension(filePath);

				if (!IsPermittedExtension(extension)) {
					throw new ArgumentException(
						"File extension '" + extension + "' is not permitted.");
				}
				if (!File.Exists(filePath))
					throw new ArgumentException("File '" + filePath + "' does not exist.");

				SourceFile file = new SourceFile(filePath);
				AddFileIfNew(file, targetProject);
			}
		}

		/// <summary>
		/// Adds the files to the defautl project.
		/// </summary>
		/// <param name="path">The top directory path with the file name pattern.
		/// The file name in the path can contains wildcards optionally.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="path"/> is invalid or cannot be accessed.
		/// </exception>
		public void AddFilesRecursively(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			AddFiles(path, true, defaultProject);
		}

		/// <summary>
		/// Adds the files to the project with the specified name.
		/// </summary>
		/// <param name="path">The top directory path with the file name pattern.
		/// The file name of the path can contains wildcards optionally.</param>
		/// <param name="projectName">The name of the target project.
		/// If the <paramref name="projectName"/> is null or empty, the file will 
		/// be added to the default project. If the <paramref name="projectName"/> 
		/// is unknown, a new project will be created with the specified name.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="path"/> is invalid or cannot be accessed.
		/// </exception>
		public void AddFilesRecursively(string path, string projectName)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			Project targetProject = defaultProject;

			if (!string.IsNullOrEmpty(projectName))
				targetProject = CreateProjectIfNew(projectName);

			AddFiles(path, true, targetProject);
		}

		/// <exception cref="ArgumentException">
		/// <paramref name="path"/> is invalid or cannot be accessed.
		/// </exception>
		private void AddFiles(string path, bool recurse, Project targetProject)
		{
			FileInfo[] files = GetFiles(path, recurse);

			for (int i = 0; i < files.Length; i++) {
				if (IsPermittedExtension(files[i].Extension)) {
					SourceFile file = new SourceFile(files[i].FullName);
					AddFileIfNew(file, targetProject);
				}
			}
		}

		private void AddProject(Project project)
		{
			if (project != null && !projects.ContainsKey(project.Name))
				projects.Add(project.Name, project);
		}

		/// <exception cref="ArgumentException">
		/// <paramref name="projectName"/> is null or empty string.
		/// </exception>
		private Project CreateProjectIfNew(string projectName)
		{
			Project project;

			if (!projects.TryGetValue(projectName, out project)) {
				project = new Project(projectName);
				AddProject(project);
			}
			return project;
		}

		private bool TryLoadVs2005Project(string dirPath, XmlDocument document)
		{
			XmlNamespaceManager manager = new XmlNamespaceManager(document.NameTable);
			manager.AddNamespace("p", "http://schemas.microsoft.com/developer/msbuild/2003");

			// Get the project name node
			XmlNode nameNode = document.SelectSingleNode(
				"/p:Project/p:PropertyGroup/p:AssemblyName", manager);
			if (nameNode == null)
				return false;

			// Read the project name
			string projectName = nameNode.InnerText;
			if (projectName == "")
				return false;

			Project project = CreateProjectIfNew(projectName);

			// Get the source files
			XmlNodeList nodes = document.SelectNodes(
				"/p:Project/p:ItemGroup/p:Compile[@Include]", manager);
			for (int i = 0; i < nodes.Count; i++) {
				string relPath = ((XmlElement) nodes[i]).GetAttribute("Include");
				string filePath = Path.Combine(dirPath, relPath);

				SourceFile file = new SourceFile(filePath);
				AddFileIfNew(file, project);
			}

			return true;
		}

		private bool TryLoadVs2003Project(string dirPath, XmlDocument document)
		{
			// Get the project name node
			XmlNode nameNode = document.SelectSingleNode(
				"/VisualStudioProject/*/Build/Settings/@AssemblyName");
			if (nameNode == null)
				return false;

			// Read the project name
			string projectName = nameNode.InnerText;
			if (projectName == "")
				return false;

			Project project = CreateProjectIfNew(projectName);

			// Get the source files
			XmlNodeList nodes = document.SelectNodes("/VisualStudioProject/*/" +
				"Files/Include/File[@RelPath][@BuildAction='Compile']");
			for (int i = 0; i < nodes.Count; i++) {
				string relPath = ((XmlElement) nodes[i]).GetAttribute("RelPath");
				string filePath = Path.Combine(dirPath, relPath);

				SourceFile file = new SourceFile(filePath);
				AddFileIfNew(file, project);
			}

			return true;
		}

		private bool TryLoadVisualCppProject(string dirPath, XmlDocument document)
		{
			// Get the project name node
			XmlNode nameNode = document.SelectSingleNode("/VisualStudioProject/@Name");
			if (nameNode == null)
				return false;

			// Read the project name
			string projectName = nameNode.InnerText;
			if (projectName == "")
				return false;

			Project project = CreateProjectIfNew(projectName);

			// Get the source files
			XmlNodeList nodes = document.SelectNodes(
				"/VisualStudioProject/Files//File/@RelativePath");
			for (int i = 0; i < nodes.Count; i++) {
				string relPath = nodes[i].InnerText;
				string filePath = Path.Combine(dirPath, relPath);
				string extension = Path.GetExtension(filePath);

				if (IsPermittedExtension(extension)) {
					SourceFile file = new SourceFile(filePath);
					AddFileIfNew(file, project);
				}
			}

			return true;
		}

		/// <exception cref="ArgumentException">
		/// <paramref name="projectFile"/> is invalid or cannot be accessed.
		/// </exception>
		private void AddProjectFile(string projectFile)
		{
			XmlDocument document = new XmlDocument();

			try {
				document.Load(projectFile);
			}
			catch (XmlException ex) {
				throw new ArgumentException(
					"Project '" + projectFile + "' is invalid and cannot be loaded.", ex);
			}
			catch (UnauthorizedAccessException ex) {
				throw new ArgumentException(
					"Project file '" + projectFile + "' cannot be accessed.", ex);
			}
			catch (IOException ex) {
				throw new ArgumentException("Project file '" + projectFile +
					"' is invalid or could not be found.", ex);
			}

			string dirPath = Path.GetDirectoryName(projectFile);

			try {
				if (TryLoadVs2005Project(dirPath, document))
					return;
				if (TryLoadVs2003Project(dirPath, document))
					return;
				if (TryLoadVisualCppProject(dirPath, document))
					return;
			}
			catch (ArgumentException ex) {
				throw new ArgumentException(
					"Project file '" + projectFile + "' is in unsupported format.", ex);
			}

			throw new ArgumentException(
				"Project file '" + projectFile + "' is in unsupported format.");
		}

		/// <exception cref="ArgumentException">
		/// <paramref name="solutionPath"/> is invalid or cannot be accessed.
		/// </exception>
		private void AddProjectFiles(string projectPath)
		{
			FileInfo[] files = GetFiles(projectPath, false);

			for (int i = 0; i < files.Length; i++)
				AddProjectFile(files[i].FullName);
		}

		/// <summary>
		/// Adds the Visual Studio project file(s) containing source file informations.
		/// </summary>
		/// <param name="projectPath">The file path of the project file(s).
		/// The file name of the path can contain wildcards optionally.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="projectPath"/> is invalid or cannot be accessed.
		/// </exception>
		public void AddProject(string projectPath)
		{
			bool hasWildcard = (projectPath.Contains("*") || projectPath.Contains("?"));

			if (hasWildcard)
				AddProjectFiles(projectPath);
			else
				AddProjectFile(projectPath);
		}

		private void AddSolutionFile(string solutionFile)
		{
			// Regex pattern to the project specifications
			const string RegexPattern = @"Project\(""\{\w{8}-\w{4}-\w{4}-\w{4}-\w{12}\}""\)" +
				@" = ""[^""]+"", ""(?<file>[^""]+)"", ""\{\w{8}-\w{4}-\w{4}-\w{4}-\w{12}\}""";

			Regex regex = new Regex(RegexPattern, RegexOptions.ExplicitCapture);
			string dirPath = Path.GetDirectoryName(solutionFile);

			try {
				using (StreamReader reader = new StreamReader(solutionFile))
				{
					while (!reader.EndOfStream) { // Read all lines through the file
						string line = reader.ReadLine();
						Match match = regex.Match(line);

						if (match.Success) {
							// Read the project informations
							Group fileGroup = match.Groups["file"];
							string relPath = fileGroup.Value;
							string projectPath = Path.Combine(dirPath, relPath);
							string extension = Path.GetExtension(projectPath);

							if (extension == ".csproj" || extension == ".vbproj" ||
								extension == ".vcproj" || extension == ".vjsproj")
							{
								AddProjectFile(projectPath);
							}
						}
					}
				}
			}
			catch (UnauthorizedAccessException ex) {
				throw new ArgumentException(
					"Solution file '" + solutionFile + "' cannot be accessed.", ex);
			}
			catch (IOException ex) {
				throw new ArgumentException("Solution file '" + solutionFile +
					"' is invalid or could not be found.", ex);
			}
		}

		/// <exception cref="ArgumentException">
		/// <paramref name="solutionPath"/> is invalid or cannot be accessed.
		/// </exception>
		private void AddSolutionFiles(string solutionPath)
		{
			FileInfo[] files = GetFiles(solutionPath, false);

			for (int i = 0; i < files.Length; i++)
				AddSolutionFile(files[i].FullName);
		}

		/// <summary>
		/// Adds the Visual Studio solution file(s) containing project file informations.
		/// </summary>
		/// <param name="solutionPath">The file path of the solution file(s).
		/// The file name of the path can contain wildcards optionally.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="solutionPath"/> is invalid or cannot be accessed.
		/// </exception>
		public void AddSolution(string solutionPath)
		{
			bool hasWildcard = (solutionPath.Contains("*") || solutionPath.Contains("?"));

			if (hasWildcard)
				AddSolutionFiles(solutionPath);
			else
				AddSolutionFile(solutionPath);
		}

		/// <summary>
		/// Adds a file name pattern with optionally wildcards to exclude specific files.
		/// </summary>
		/// <remarks>For removing the files the <see cref="ValidateExcludePatterns"/>
		/// must be invoked after the patterns are specified.</remarks>
		/// <param name="pattern">The file name pattern.</param>
		public void AddExludePattern(string pattern)
		{
			if (!string.IsNullOrEmpty(pattern) && !excludePatterns.Contains(pattern))
				excludePatterns.Add(pattern);
		}

		private static string GetRegexPattern(string wildcardPattern)
		{
			StringBuilder builder = new StringBuilder(wildcardPattern);

			builder.Replace(@"\", @"\\");
			builder.Replace(".", "\\.");
			builder.Replace("^", "\\^");
			builder.Replace("$", "\\$");
			builder.Replace("{", "\\{");
			builder.Replace("[", "\\[");
			builder.Replace("(", "\\(");
			builder.Replace(")", "\\)");
			builder.Replace("|", "\\|");
			builder.Replace("+", "\\+");
			builder.Replace("*", ".*");
			builder.Replace("?", ".");

			builder.Insert(0, "^");
			builder.Append("$");

			return builder.ToString();
		}

		/// <summary>
		/// Removes every file matching at least one of the exclusion patterns.
		/// </summary>
		/// <remarks>This method must be invoked after adding the patterns.</remarks>
		public void ValidateExcludePatterns()
		{
			for (int i = 0; i < excludePatterns.Count; i++) {
				string regexPattern = GetRegexPattern(excludePatterns[i]);				
				Regex pattern;
				
				// On Windows the file names are incase-sensitive
				if (SourceFile.IgnoreCase)
					pattern = new Regex(regexPattern, RegexOptions.IgnoreCase);
				else
					pattern = new Regex(regexPattern);

				// Remove files from every project that are matching the pattern
				foreach (Project project in projects.Values) {
					int removedCount = project.ExcludeFiles(pattern);
					fileCount -= removedCount;
				}
			}
		}

		/// <summary>
		/// Generates an XML report document that represents all the 
		/// informations of a succesful counting.
		/// </summary>
		/// <remarks>A DTD description will be added to the document.
		/// If the document is created before the counting succeeds, 
		/// the document will be missing relevant report elements.</remarks>
		/// <returns>An XML document containing report informations.</returns>
		/// <exception cref="InvalidOperationException">
		/// No input file has been added.
		/// </exception>
		public XmlDocument GenerateXmlReport()
		{
			if (fileCount == 0) {
				throw new InvalidOperationException(
					"Cannot generate XML report until input file has been added.");
			}

			XmlDocument document = new XmlDocument();
			XmlElement root = document.CreateElement("Report");
			XmlDocumentType docType = document.CreateDocumentType("Report", null, null, @"
  <!ELEMENT Report    (Result, Project+)>

  <!ELEMENT Project   (Name, Result, File+)>
  <!ELEMENT Name      (#PCDATA)>

  <!ELEMENT File      (Directory, FileName, Result)>
  <!ELEMENT Directory (#PCDATA)>
  <!ELEMENT FileName  (#PCDATA)>

  <!ELEMENT Result    (Total, Code, Comment, Blank)>
  <!ELEMENT Total     (#PCDATA)>
  <!ELEMENT Code      (#PCDATA)>
  <!ELEMENT Comment   (#PCDATA)>
  <!ELEMENT Blank     (#PCDATA)>
");

			document.AppendChild(docType); // Add DTD description to the document
			document.AppendChild(root);

			// Add summary element
			XmlElement resultElement = document.CreateElement("Result");
			if (report != null)
				report.Serialize(resultElement);
			root.AppendChild(resultElement);

			foreach (Project project in projects.Values) {
				// Serialize each non-empty project
				if (project.FileCount > 0) {
					XmlElement projectElement = document.CreateElement("Project");

					project.Serialize(projectElement);
					root.AppendChild(projectElement);
				}
			}

			return document;
		}

		/// <summary>
		/// Returns a string that represents the current <see cref="FileSet"/>.
		/// </summary>
		/// <returns>A string that represents the current <see cref="FileSet"/>.
		/// </returns>
		public override string ToString()
		{
			if (report != null)
				return fileCount + " file(s); " + report.ToString();
			else
				return fileCount + " file(s)";
		}
	}
}
