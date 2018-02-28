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
	internal class PascalParser : LineParser
	{
		public static readonly PascalParser Default = new PascalParser();

		bool inString = false;
		bool inMultiLineComment = false;
		bool inOldMultiLineComment = false;

		protected bool InString
		{
			get { return inString; }
		}

		protected bool InMultiLineComment
		{
			get { return inMultiLineComment; }
		}

		protected bool InOldMultiLineComment
		{
			get { return inOldMultiLineComment; }
		}

		public override void Reset()
		{
			inMultiLineComment = false;
			inOldMultiLineComment = false;
			base.Reset();
		}

		public override void ParseNextLine(string line)
		{
			inString = false;
			base.ParseNextLine(line);
		}

		protected override int ParseString(string line, int index)
		{
			if (line == null || index < 0 || index > line.Length)
				return 0;

			if (char.IsWhiteSpace(line[index]))
				return 1;
			else
				IsBlankLine = false;

			if (inMultiLineComment) {
				if (line[index] == '}')
					inMultiLineComment = false;
				else
					ContainsComment = true;
				return 1;
			}
			else if (inOldMultiLineComment) {
				if (ContainsSubstring(line, index, "*)")) {
					inOldMultiLineComment = false;
					return 2;
				}
				else {
					ContainsComment = true;
					return 1;
				}
			}
			else if (inString) {
				if (line[index] == '\'')
					inString = false;
				return 1;
			}
			else if (line[index] == '{') {
				inMultiLineComment = true;
				return 1;
			}
			else if (ContainsSubstring(line, index, "(*")) {
				inOldMultiLineComment = true;
				return 2;
			}
			else if (line[index] == '\'') {
				inString = true;
				ContainsCode = true;
				return 1;
			}
			else {
				ContainsCode = true;
				return 1;
			}
		}
	}
}
