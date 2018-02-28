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

namespace NLOC.Core
{
	/// <summary>
	/// Contains counting result informations.
	/// </summary>
	public class CountingResult
	{
		int totalLines;
		int codeLines;
		int commentLines;
		int blankLines;

		/// <summary>
		/// Initializes a new instance of the <see cref="CountingResult"/> class
		/// with the specified line counts.
		/// </summary>
		/// <param name="totalLines">The total number of lines. (LOC) </param>
		/// <param name="codeLines">The total number of code lines. (SLOC)</param>
		/// <param name="commentLines">The total number of comment lines. (CLOC)</param>
		/// <param name="blankLines">The total number of blank lines. (BLOC)</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///   <paramref name="totalLines"/> is less than 0.-or-
		///   <paramref name="codeLines"/> is less than 0 or greater than 
		///   <paramref name="totalLines"/>.-or-
		///   <paramref name="commentLines"/> is less than 0 or greater than 
		///   <paramref name="totalLines"/>.-or-
		///   <paramref name="blankLines"/> is less than 0 or greater than 
		///   <paramref name="totalLines"/>.
		/// </exception>
		public CountingResult(int totalLines, int codeLines, int commentLines, int blankLines)
		{
			if (totalLines < 0)
				throw new ArgumentOutOfRangeException("totalLines");
			if (codeLines < 0 || codeLines > totalLines)
				throw new ArgumentOutOfRangeException("codeLines");
			if (commentLines < 0 || commentLines > totalLines)
				throw new ArgumentOutOfRangeException("commentLines");
			if (blankLines < 0 || blankLines > totalLines)
				throw new ArgumentOutOfRangeException("blankLines");

			this.totalLines = totalLines;
			this.codeLines = codeLines;
			this.commentLines = commentLines;
			this.blankLines = blankLines;
		}

		/// <summary>Gets the total number of lines.</summary>
		public int TotalLines
		{
			get { return totalLines; }
		}

		/// <summary>Gets the total number of code lines.</summary>
		public int CodeLines
		{
			get { return codeLines; }
		}

		/// <summary>Gets the total number of comment lines.</summary>
		public int CommentLines
		{
			get { return commentLines; }
		}

		/// <summary>Gets the total number of blank lines.</summary>
		public int BlankLines
		{
			get { return blankLines; }
		}

		/// <summary>Gets the percentage of code lines.</summary>
		public float PercentageOfCodeLines
		{
			get
			{
				if (totalLines != 0)
					return ((float) codeLines / totalLines * 100);
				else
					return 0;
			}
		}

		/// <summary>Gets the percentage of comment lines.</summary>
		public float PercentageOfCommentLines
		{
			get
			{
				if (totalLines != 0)
					return ((float) commentLines / totalLines * 100);
				else
					return 0;
			}
		}

		/// <summary>Gets the percentage of blank lines.</summary>
		public float PercentageOfBlankLines
		{
			get
			{
				if (totalLines != 0)
					return ((float) blankLines / totalLines * 100);
				else
					return 0;
			}
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
			XmlElement totalElement   = document.CreateElement("Total");
			XmlElement codeElement    = document.CreateElement("Code");
			XmlElement commentElement = document.CreateElement("Comment");
			XmlElement blankElement   = document.CreateElement("Blank");

			totalElement.InnerText   = totalLines.ToString();
			codeElement.InnerText    = codeLines.ToString();
			commentElement.InnerText = commentLines.ToString();
			blankElement.InnerText   = blankLines.ToString();

			element.AppendChild(totalElement);
			element.AppendChild(codeElement);
			element.AppendChild(commentElement);
			element.AppendChild(blankElement);
		}

		/// <summary>
		/// Returns a string that represents the current <see cref="CountingResult"/>.
		/// </summary>
		/// <returns>A string that represents the current <see cref="CountingResult"/>.
		/// </returns>
		public override string ToString()
		{
			return string.Format(
				"Total: {0}, Code: {1}, Comment: {2}, Blank: {3}",
				totalLines, codeLines, commentLines, blankLines);
		}
	}
}
