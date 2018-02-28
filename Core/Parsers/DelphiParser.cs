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
	internal class DelphiParser : PascalParser
	{
		public static readonly new DelphiParser Default = new DelphiParser();

		bool inSingleLineComment = false;

		protected bool InSingleLineComment
		{
			get { return inSingleLineComment; }
		}

		public override void ParseNextLine(string line)
		{
			inSingleLineComment = false;
			base.ParseNextLine(line);
		}

		protected override int ParseString(string line, int index)
		{
			if (InString || InMultiLineComment || InOldMultiLineComment)
				return base.ParseString(line, index);

			if (line == null || index < 0 || index > line.Length)
				return 0;

			if (inSingleLineComment) {
				if (char.IsWhiteSpace(line[index])) {
					return 1;
				}
				else {
					IsBlankLine = false;
					ContainsComment = true;
					return line.Length - index;
				}
			}
			else if (ContainsSubstring(line, index, "//")) {
				inSingleLineComment = true;
				IsBlankLine = false;
				return 2;
			}
			else {
				return base.ParseString(line, index);
			}
		}
	}
}
