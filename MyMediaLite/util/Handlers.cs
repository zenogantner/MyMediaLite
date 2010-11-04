// This file is part of MyMediaLite.
// Its content is in the public domain.

using System;


namespace MyMediaLite.util
{
	/// <summary>Class containing handler functions, e.g. exception handlers</summary>
	public static class Handlers
	{
		/// <summary>
		/// React to an unhandled exceptions by giving out the error message and the stack trace,
		/// and then terminating the program.
		/// </summary>
		/// <param name="sender">the sender of the exception</param>
		/// <param name="unhandled_event">the arguments of the unhandled exception</param>
		public static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs unhandled_event)
		{
			try
			{
				Exception e = (Exception)unhandled_event.ExceptionObject;
				Console.Error.WriteLine(e.Message + e.StackTrace);
			}
			finally
			{
				Console.Error.WriteLine ("Terminate on unhandled exception.");
				Environment.Exit(-1);
			}
		}

	}
}

