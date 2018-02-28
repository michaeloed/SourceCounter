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
using System.Runtime.Serialization;

namespace NLOC.Core
{
	/// <summary>
	/// The exception that is thrown when a <see cref="SourceFile"/> object 
	/// contains invalid file informations.
	/// </summary>
	public class InvalidFileException : Exception
	{
		SourceFile file;

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidFileException"/> 
		/// class with a specified source file.
		/// </summary>
		/// <param name="file">The source file that caused the exception.</param>
		public InvalidFileException(SourceFile file)
			: this(file, (Exception) null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidFileException"/> 
		/// class with a specified source file and error message.
		/// </summary>
		/// <param name="file">The source file that caused the exception.</param>
		/// <param name="message"></param>
		public InvalidFileException(SourceFile file, string message)
			: base(message)
		{
			this.file = file;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidFileException"/> 
		/// class with a specified source file and a reference to the inner 
		/// exception that is the cause of this exception.
		/// </summary>
		/// <param name="file">The source file that caused the exception.</param>
		/// <param name="inner">The exception that is the cause of the current 
		/// exception. If the inner parameter is not null, the current exception 
		/// is raised in a catch block that handles the inner exception.</param>
		public InvalidFileException(SourceFile file, Exception inner)
			: base("Invalid file: '" + file.Path + "'", inner)
		{
			this.file = file;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidFileException"/> 
		/// class with a specified file and error message and a reference to the 
		/// inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="file">The source file that caused the exception.</param>
		/// <param name="message">The error message that explains the reason for 
		/// the exception.</param>
		/// <param name="inner">The exception that is the cause of the current 
		/// exception. If the inner parameter is not null, the current exception 
		/// is raised in a catch block that handles the inner exception.</param>
		public InvalidFileException(SourceFile file, string message, Exception inner)
			: base(message, inner)
		{
			this.file = file;
		}

		/// <summary>
		/// Gets the source file that caused the exception.
		/// </summary>
		public SourceFile SourceFile
		{
			get { return file; }
		}
	}
}
