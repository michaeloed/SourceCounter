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

namespace NLOC.Console
{
	internal static class Program
	{
		public const string Author = "Balazs Tihanyi";
		public const string HomePage = "http://nloc.sourceforge.net";

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		private static void Main(string[] args)
		{
			try {
				ITaskParameters parameters = new ArgumentInterpreter(args);
				ConsoleTask task = new ConsoleTask(parameters);

				task.Run();
			}
			catch (InvalidArgumentException ex) {
				ConsoleTask.WriteLogo();
				System.Console.Error.WriteLine("Error: " + ex.Message);
				System.Console.Error.WriteLine("\nType 'NLOC -help' for usage instructions.");
			}
			catch (Exception ex) {
				ConsoleTask.WriteLogo();
				System.Console.Error.WriteLine("Critial Error: " + ex.Message);
				System.Console.Error.WriteLine("\nThe application must be terminated.");
			}
		}
	}
}
