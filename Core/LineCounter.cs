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
using NLOC.Core.Parsers;

namespace NLOC.Core
{
	/// <summary>
	/// Represents a counter class for counting the source lines and generating 
	/// report informations.
	/// </summary>
	public class LineCounter
	{
		IFileHandler fileSet;

		/// <summary>
		/// Initializes a new instance of the <see cref="LineCounter"/> class 
		/// with the specified file set.
		/// </summary>
		/// <param name="fileSet">A <see cref="IFileHandler"/> object that 
		/// contains essential project and file informations.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="fileSet"/> is null.
		/// </exception>
		public LineCounter(IFileHandler fileSet)
		{
			if (fileSet == null)
				throw new ArgumentNullException("fileSet");

			this.fileSet = fileSet;
		}

		/// <summary>
		/// Counts the required statistical data about the files and projects, and 
		/// generates <see cref="CountingResult"/> objects from the results.
		/// </summary>
		/// <exception cref="InvalidFileException">
		/// A source file is missing or has invalid extension.
		/// </exception>
		public void Count()
		{
			int totalLines = 0;
			int totalCodeLines = 0;
			int totalCommentLines = 0;
			int totalBlankLines = 0;

			// Count each project
			foreach (Project project in fileSet.Projects) {
				int projectTotalLines = 0;
				int projectCodeLines = 0;
				int projectCommentLines = 0;
				int projectBlankLines = 0;

				// Count each file
				foreach (SourceFile file in project.Files) {
					CountingResult result = ParseFile(file);

					// Create report object for file
					projectTotalLines += result.TotalLines;
					projectCodeLines += result.CodeLines;
					projectCommentLines += result.CommentLines;
					projectBlankLines += result.BlankLines;
					file.Report = result;
				}

				// Create report object for project
				CountingResult projectResult = new CountingResult(projectTotalLines,
					projectCodeLines, projectCommentLines, projectBlankLines);
				project.Report = projectResult;

				totalLines += projectTotalLines;
				totalCodeLines += projectCodeLines;
				totalCommentLines += projectCommentLines;
				totalBlankLines += projectBlankLines;
			}

			// Create summary report
			CountingResult summaryResult = new CountingResult(
				totalLines, totalCodeLines, totalCommentLines, totalBlankLines);
			fileSet.Report = summaryResult;
		}

		/// <exception cref="InvalidFileException">
		/// <paramref name="file"/>'s extension is unknown and cannot be parsed.-or-
		/// <paramref name="file"/> could not be loaded.
		/// </exception>
		private CountingResult ParseFile(SourceFile file)
		{
			LineParser parser;

			try {
				parser = GetParser(file.Extension);
			}
			catch (ArgumentException ex) {
				if (string.IsNullOrEmpty(file.Extension)) {
					throw new InvalidFileException(file,
						"File extension for '" + file.Path + "' is missing.", ex);
				}
				else {
					throw new InvalidFileException(file,
						"File extension '" + file.Extension + "' is unknown.", ex);
				}
			}

			parser.Reset(); // Clear previous counting results
			try {
				using (StreamReader reader = new StreamReader(file.Path))
				{
					// Parse each line from the file
					while (!reader.EndOfStream) {
						string line = reader.ReadLine();
						parser.ParseNextLine(line);
					}
				}
			}
			catch (FileNotFoundException ex) {
				throw new InvalidFileException(file, "Input file '" + file.Path +
					"' could not be found.", ex);
			}
			catch (DirectoryNotFoundException ex) {
				throw new InvalidFileException(file, "Input file '" + file.Path +
					"' could not be found.", ex);
			}
			catch (UnauthorizedAccessException ex) {
				throw new InvalidFileException(file, "Input file '" + file.Path +
					"' could not be opened.", ex);
			}
			catch (IOException ex) {
				throw new InvalidFileException(file,
					"An error occured reading the file '" + file.Path + "'.", ex); 
			}

			return parser.GenerateResult();
		}

		/// <exception cref="ArgumentException">
		/// <paramref name="extension"/> is unknown.
		/// </exception>
		private static LineParser GetParser(string extension)
		{
			switch (extension.ToLower()) {
				case ".c":
				case ".cpp":
				case ".h":
				case ".hpp":
				case ".cc":
				case ".hh":
				case ".cxx":
				case ".hxx":
					return CParser.Default;

				case ".cs":
					return CSParser.Default;

				case ".java":
					return CParser.Default;

				case ".vb":
				case ".bas":
				case ".cls":
				case ".frm":
					return VBParser.Default;

				case ".p":
				case ".pp":
					return PascalParser.Default;

				case ".pas":
				case ".dpr":
					return DelphiParser.Default;

				default:
					throw new ArgumentException("Unknown file extension.", "extension");
			}
		}
	}
}
