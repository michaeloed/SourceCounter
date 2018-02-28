﻿// NLOC - source line counter tool
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
using System.Runtime.Serialization;

namespace NLOC.Console
{
	/// <summary>
	/// The exception that is thrown when a command-line argument is invalid.
	/// </summary>
	internal class InvalidArgumentException : Exception
	{
		string argument;

		public InvalidArgumentException(string argument)
			: base("Invalid argument: '" + argument + "'")
		{
			this.argument = argument;
		}

		public InvalidArgumentException(string argument, string message)
			: base(message)
		{
			this.argument = argument;
		}

		public InvalidArgumentException(string argument, string message, Exception inner)
			: base(message, inner)
		{
			this.argument = argument;
		}

		public InvalidArgumentException(string message, Exception inner)
			: base(message, inner)
		{
		}

		public string Argument
		{
			get { return argument; }
		}
	}
}