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
using System.Collections.Generic;

namespace NLOC.Core
{
	/// <summary>
	/// Represents a source file collector interface.
	/// </summary>
	public interface IFileHandler
	{
		/// <summary>
		/// Gets the collection of the known projects.
		/// </summary>
		IEnumerable<Project> Projects
		{
			get;
		}

		/// <summary>
		/// Gets the total number of source files.
		/// </summary>
		int FileCount
		{
			get;
		}

		/// <summary>
		/// Gets or sets the results of a successful counting.
		/// </summary>
		/// <value>The <see cref="CountingResult"/> object of a 
		/// successful counting or null.</value>
		CountingResult Report
		{
			get;
			set;
		}

		/// <summary>
		/// Generates an XML report document that represents all the 
		/// informations of a succesful counting.
		/// </summary>
		/// <returns>An XML document containing report informations.</returns>
		XmlDocument GenerateXmlReport();
	}
}
