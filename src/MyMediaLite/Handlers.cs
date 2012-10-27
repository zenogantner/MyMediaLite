// This file is part of MyMediaLite.
// Its content is in the public domain.
using System;
using System.IO;
using System.Reflection;

namespace MyMediaLite
{
	/// <summary>Class containing handler functions, e.g. exception handlers</summary>
	public static class Handlers
	{
		/// <summary>React to an unhandled exceptions</summary>
		/// <remarks>
		/// Give out the error message and the stack trace, then terminate the program.
		/// FileNotFoundExceptions get special treatment.
		/// </remarks>
		/// <param name="sender">the sender of the exception</param>
		/// <param name="unhandled_event">the arguments of the unhandled exception</param>
		public static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs unhandled_event)
		{
			try
			{
				Exception e = (Exception) unhandled_event.ExceptionObject;

				if (e is FileNotFoundException)
				{
					var file_not_found_exception = (FileNotFoundException) e;
					Console.Error.WriteLine("Could not find file " + file_not_found_exception.FileName);
					Environment.Exit(-1);
				}

				if (e is DirectoryNotFoundException)
				{
					Console.Error.WriteLine(e.Message);
					Environment.Exit(-1);
				}

				if (e is IOException)
				{
					Console.Error.WriteLine(e.Message);
					Environment.Exit(-1);
				}

				Console.Error.WriteLine();
				Console.Error.WriteLine("  *****************************************************************************************************");
				Console.Error.WriteLine("  *** An uncaught exception occured. Please send a bug report to zeno.gantner+mymedialite@gmail.com,***");
				Console.Error.WriteLine("  *** or report the problem in our issue tracker: https://github.com/zenogantner/MyMediaLite/issues ***");
				var version = Assembly.GetEntryAssembly().GetName().Version;
				Console.Error.WriteLine("  *** MyMediaLite {0}.{1:00}                                                                              ***", version.Major, version.Minor);
				Console.Error.WriteLine("  *****************************************************************************************************");
				Console.Error.WriteLine(e.Message + e.StackTrace);
				Console.Error.WriteLine ("Terminate on unhandled exception.");
			}
			finally
			{
				Environment.Exit(-1);
			}
		}
	}
}