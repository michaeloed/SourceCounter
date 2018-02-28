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
using System.Reflection;
using System.Collections.Generic;
using NLOC.Core;

namespace NLOC.Console
{
	/// <summary>
	/// Provides counter task operations using the standard output stream.
	/// </summary>
	public class ConsoleTask
	{
		const int MaxNameLength = 30;
		const string Tabulation = "{0,-30}{1,10}{2,10}{3,10}{4,10}";
		const int LineLength = 30 + (4 * 10);
		static readonly string separator = new string('-', LineLength);

		ITaskParameters parameters;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConsoleTask"/> class
		/// with specified task parameters.
		/// </summary>
		/// <param name="parameters">An <see cref="ITaskParameters"/> object
		/// containing essential task informations.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="parameters"/> is null.
		/// </exception>
		public ConsoleTask(ITaskParameters parameters)
		{
			if (parameters == null)
				throw new ArgumentNullException("parameters");

			this.parameters = parameters;
		}

		/// <summary>
		/// Starts the task and writes the results to the standard output stream.
		/// </summary>
		public void Run()
		{
			if (parameters.ShowLogo)
				WriteLogo();
			
			if (parameters.ShowHelp) {
				WriteHelp();
			}			
			else if (parameters.FileSet.FileCount == 0) { // If there is no file to analyse
				System.Console.Error.WriteLine("Error: No input files were specified.\n");
				WriteHelp();
			}
			else {
				int fileCount = parameters.FileSet.FileCount;
				if (fileCount == 1)
					System.Console.WriteLine("1 file is being parsed.\n");
				else
					System.Console.WriteLine("{0} files are being parsed.\n", fileCount);

				try {
					LineCounter counter = new LineCounter(parameters.FileSet);
					counter.Count();

					CreateXmlReportFile();
					ShowResults();
				}
				catch (InvalidFileException ex) {
					System.Console.Error.WriteLine("Error: " + ex.Message);
				}
			}
		}

		private void CreateXmlReportFile()
		{
			string outputFile = parameters.OutputXml;

			if (string.IsNullOrEmpty(outputFile))
				return;

			try {
				// Create target directory if it doesn't exist
				string directory = Path.GetDirectoryName(outputFile);
				if (!Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				// Save XML report file
				XmlDocument report = parameters.FileSet.GenerateXmlReport();
				report.Save(outputFile);
				System.Console.WriteLine(
					"Output file '" + outputFile + "' has been created.\n");
			}
			catch (Exception ex) {
				System.Console.Error.WriteLine("Error: " + ex.Message);
				System.Console.Error.WriteLine();
			}
		}

		private void ShowResults()
		{
			string output = parameters.Output;

			if (string.IsNullOrEmpty(output)) {
				ShowResults(System.Console.Out);
			}
			else {
				try {
					// Create target directory if it doesn't exist
					string directory = Path.GetDirectoryName(output);
					if (!Directory.Exists(directory))
						Directory.CreateDirectory(directory);

					using (StreamWriter writer = new StreamWriter(output))
					{
						ShowResults(writer);
						System.Console.WriteLine(
							"Output file '" + output + "' has been created.");
					}
				}
				catch (Exception ex) {
					System.Console.Error.WriteLine("Error: " + ex.Message);
				}
			}
		}

		private void ShowResults(TextWriter writer)
		{
			List<SourceFile> files = new List<SourceFile>(10);
			bool showFiles = (parameters.VisibleElements & ReportElements.Files) != 0;
			bool showProjects = (parameters.VisibleElements & ReportElements.Projects) != 0;
			bool showSummary = (parameters.VisibleElements & ReportElements.Summary) != 0;
			bool percentage = parameters.ShowPercentage;

			// Write initial headers
			if (showFiles && !showProjects)
				WriteHeader(writer, "Files");
			else if (!showFiles && showProjects)
				WriteHeader(writer, "Projects");

			if (showProjects || showFiles) {
				foreach (Project project in parameters.FileSet.Projects) {
					if (project.FileCount == 0)
						continue;

					files.Clear();
					files.AddRange(project.Files);
					files.Sort(delegate(SourceFile file1, SourceFile file2) {
						return string.Compare(file1.Name, file2.Name);
					});

					if (showFiles) {
						if (showProjects)
							WriteHeader(writer, project.Name); // Write project header

						// Write counting results
						for (int i = 0; i < files.Count; i++) {
							SourceFile file = files[i];
							WriteReport(writer, file.Name, file.Report, percentage);
						}

						// Write total report and group separator
						if (showProjects) {
							if (project.FileCount > 1)
								WriteReport(writer, "-= TOTAL =-", project.Report, percentage);
							writer.WriteLine();
						}
					}
					else {
						// Show only projects without files
						WriteReport(writer, project.Name, project.Report, percentage);
					}
				}
				if (showSummary && (showProjects ^ showFiles))
					writer.WriteLine();
			}

			// Write counting summary
			if (showSummary) {
				WriteHeader(writer, "Summary");
				WriteReport(writer, "Grand Total", parameters.FileSet.Report, percentage);
			}
		}

		private static void WriteHeader(TextWriter writer, string title)
		{
			string header = string.Format(Tabulation,
				title, "Total", "Code", "Comment", "Blank");

			writer.WriteLine(separator);
			writer.WriteLine(header);
			writer.WriteLine(separator);
		}

		private static void WriteReport(TextWriter writer, string name,
			CountingResult report, bool percentage)
		{
			if (name != null && name.Length > MaxNameLength)
				name = name.Substring(0, MaxNameLength - 3) + "...";

			string line;
			if (percentage) {
				string codePercentage = ((int) report.PercentageOfCodeLines) + "%";
				string commentPercentage = ((int) report.PercentageOfCommentLines) + "%";
				string blankPercentage = ((int) report.PercentageOfBlankLines) + "%";


				line = string.Format(Tabulation, name,
					report.TotalLines, codePercentage,
					commentPercentage, blankPercentage);
			}
			else {
				line = string.Format(Tabulation, name, report.TotalLines,
					report.CodeLines, report.CommentLines, report.BlankLines);
			}

			writer.WriteLine(line);
		}

		/// <summary>
		/// Writes application logo text to the standard output stream.
		/// </summary>
		public static void WriteLogo()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			Version version = assembly.GetName().Version;
			DateTime startDate = new DateTime(2007, 2, 4); // First release date
			DateTime releaseDate = startDate.AddDays(version.Build);
			
			// Write program name and version informations
			System.Console.WriteLine("NLOC {0} (Build {1}; release; {2})",
				version.ToString(2), version, releaseDate.ToShortDateString());

			// Write copyright header
			if (startDate.Year == releaseDate.Year) {
				System.Console.WriteLine("Copyright (C) {0} " + Program.Author,
					startDate.Year);
			}
			else {
				System.Console.WriteLine("Copyright (C) {0}-{1} " + Program.Author,
					startDate.Year, releaseDate.Year);
			}

			// Write program's homepage
			System.Console.WriteLine(Program.HomePage);
			System.Console.WriteLine();
		}

		/// <summary>
		/// Writes usage instructions to the standard output stream.
		/// </summary>
		public static void WriteHelp()
		{
			System.Console.WriteLine(
@"Usage: NLOC [options] <files>*

Options:
-out:<file>         Specify the output report file
-out_xml:<file>     Specify the XML output file
-recurse:<path>     Include all files in the path and its subdirectories (-r:)
-exclude:<pattern>  Specify files to exclude (Short form: -e)
-show:(f|p|s)+      Specify which elements to report (Files|Projects|Summary)
-percentage         Show percentage instead of number of lines (Short form: -p)
-nologo             Suppress display of application logo
-help               Display usage instructions (Short format: -?)

Parameters:
<files>, <path> can be a single file or file group with wildcars.
Project can be specified explicitly with the ';' mark.
   program.c        (notice: file path can be relative or absolute)
   C:\MyProgram\MyProgram.sln
   src\*.java;JavaProject

<pattern> is a file name pattern with optional wildcards.
It will exclude every file matching the pattern.
   *.Designer.vb    (NOTICE: this is not a relative file path)
   AssemblyInfo.cs");
		}
	}
}
