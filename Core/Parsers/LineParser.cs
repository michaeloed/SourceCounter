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

namespace NLOC.Core.Parsers
{
	internal abstract class LineParser
	{
		int totalLineCount = 0;
		int codeLineCount = 0;
		int commentLineCount = 0;
		int blankLineCount = 0;
		bool isBlankLine = true;
		bool containsComment = false;
		bool containsCode = false;

		protected bool IsBlankLine
		{
			get { return isBlankLine; }
			set { isBlankLine = value; }
		}

		protected bool ContainsComment
		{
			get { return containsComment; }
			set { containsComment = value; }
		}

		protected bool ContainsCode
		{
			get { return containsCode; }
			set { containsCode = value; }
		}

		public virtual void Reset()
		{
			totalLineCount = 0;
			codeLineCount = 0;
			commentLineCount = 0;
			blankLineCount = 0;
		}

		public CountingResult GenerateResult()
		{
			return new CountingResult(
				totalLineCount, codeLineCount, commentLineCount, blankLineCount);
		}

		/// <exception cref="ArgumentNullException">
		/// <paramref name="line"/> is null.
		/// </exception>
		public virtual void ParseNextLine(string line)
		{
			if (line == null)
				throw new ArgumentNullException("line");

			IsBlankLine = true;
			ContainsCode = false;
			ContainsComment = false;

			for (int index = 0; index < line.Length; )
				index += ParseString(line, index);

			if (IsBlankLine)
				blankLineCount++;
			if (ContainsComment)
				commentLineCount++;
			if (ContainsCode)
				codeLineCount++;
			totalLineCount++;
		}

		protected static bool ContainsSubstring(string line, int index, string substring)
		{
			if (line == null || substring == null)
				return false;

			if (line.Length < index + substring.Length || index < 0)
				return false;

			for (int i = 0; i < substring.Length; i++) {
				if (substring[i] != line[index + i])
					return false;
			}
			return true;
		}

		protected abstract int ParseString(string line, int index);
	}
}
