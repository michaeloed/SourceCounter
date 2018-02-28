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
using NLOC.Core;

namespace NLOC.Console
{
	/// <summary>
	/// Provides a read-only interfaace to the parameters of a console task.
	/// </summary>
	public interface ITaskParameters
	{
		/// <summary>
		/// An <see cref="IFileHandler"/> object that represents a source file collector.
		/// </summary>
		IFileHandler FileSet
		{
			get;
		}

		/// <summary>
		/// Gets the output path of the text file report.
		/// </summary>
		/// <value>The output path string or null if the output is not defined.</value>
		string Output
		{
			get;
		}

		/// <summary>
		/// Gets the output path of the XML report.
		/// </summary>
		/// <value>The output path string or null if the output is not defined.</value>
		string OutputXml
		{
			get;
		}

		/// <summary>
		/// Gets a <see cref="ReportElements"/> object that represents 
		/// what to show in the report.
		/// </summary>
		ReportElements VisibleElements
		{
			get;
		}

		/// <summary>
		/// Gets a value indicating whether to show percentage instead of number of lines.
		/// </summary>
		bool ShowPercentage
		{
			get;
		}

		/// <summary>
		/// Gets a value indicating whether to show logo informations.
		/// </summary>
		bool ShowLogo
		{
			get;
		}

		/// <summary>
		/// Gets a value indicating whether to show usage instructions.
		/// </summary>
		bool ShowHelp
		{
			get;
		}
	}
}
