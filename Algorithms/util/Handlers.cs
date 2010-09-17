// This file is part of MyMediaLite.
// Its content is in the public domain.

using System;

namespace MyMediaLite.util
{
	public static class Handlers
	{
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

