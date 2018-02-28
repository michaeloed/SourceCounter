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
	internal class CParser : LineParser
	{
		public static readonly CParser Default = new CParser();

		bool inString = false;
		bool inChar = false;
		bool inSingleLineComment = false;
		bool inMultiLineComment = false;

		protected bool InString
		{
			get { return inString; }
		}

		protected bool InChar
		{
			get { return inChar; }
		}

		protected bool InSingleLineComment
		{
			get { return inSingleLineComment; }
		}

		protected bool InMultiLineComment
		{
			get { return inMultiLineComment; }
		}

		public override void Reset()
		{
			inMultiLineComment = false;
			base.Reset();
		}

		public override void ParseNextLine(string line)
		{
			inString = false;
			inChar = false;
			inSingleLineComment = false;
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

			if (inSingleLineComment) {
				ContainsComment = true;
				return line.Length - index;
			}
			else if (inMultiLineComment) {
				if (ContainsSubstring(line, index, "*/")) {
					inMultiLineComment = false;
					return 2;
				}
				else {
					ContainsComment = true;
					return 1;
				}
			}
			else if (inString) {
				if (ContainsSubstring(line, index, @"\\")) {
					return 2;
				}
				else if (ContainsSubstring(line, index, "\\\"")) {
					return 2;
				}
				else if (line[index] == '"') {
					inString = false;
					return 1;
				}
				else {
					return 1;
				}
			}
			else if (inChar) {
				if (ContainsSubstring(line, index, @"\\")) {
					return 2;
				}
				else if (ContainsSubstring(line, index, @"\'")) {
					return 2;
				}
				else if (line[index] == '\'') {
					inChar = false;
					return 1;
				}
				else {
					return 1;
				}
			}
			else if (ContainsSubstring(line, index, "/*")) {
				inMultiLineComment = true;
				return 2;
			}
			else if (ContainsSubstring(line, index, "//")) {
				inSingleLineComment = true;
				return 2;
			}
			else if (line[index] == '"') {
				inString = true;
				ContainsCode = true;
				return 1;
			}
			else if (line[index] == '\'') {
				inChar = true;
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
