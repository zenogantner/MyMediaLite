// This file is part of MyMediaLite.
// Its content is in the public domain.

using System;
using System.IO;

namespace MyMediaLite.Util
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
					var dir_not_found_exception = (DirectoryNotFoundException) e;
					Console.Error.WriteLine(dir_not_found_exception.Message);
					Environment.Exit(-1);
				}

				Console.Error.WriteLine("An uncaught exception occured. Please send a bug report to mymedialite@ismll.de,");
				Console.Error.WriteLine("or report the problem in our issue tracker: https://github.com/zenogantner/MyMediaLite/issues");
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