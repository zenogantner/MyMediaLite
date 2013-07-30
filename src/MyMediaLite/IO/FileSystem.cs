// Copyright (C) 2013 Zeno Gantner
//
// This file is part of MyMediaLite.
//
// MyMediaLite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MyMediaLite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.
//
using System;
using System.IO;
using Mono.Unix;
using Mono.Unix.Native;


namespace MyMediaLite.IO
{
	/// <summary>File-system related helper functions</summary>
	public static class FileSystem
	{
		/// <summary>
		/// Check whether the program runs on Unix or not
		/// </summary>
		/// <returns>
		/// true if it runs on Unix (including MacOS X, false otherwise)
		/// </returns>
		public static bool RunningOnUnix()
		{
			int p = (int) Environment.OSVersion.Platform;
			return p == 4 || p == 6 || p == 128;
		}

		/// <summary>
		/// Creates a StreamWriter which will be appended to
		/// </summary>
		/// <returns>
		/// a StreamWriter to a file that was opened with the append flag
		/// </returns>
		/// <param name='filename'>
		/// the name of the file
		/// </param>
		public static StreamWriter CreateUnixAppendStreamWriter(string filename)
		{
			OpenFlags flags = OpenFlags.O_WRONLY | OpenFlags.O_LARGEFILE | OpenFlags.O_APPEND;
			int fd = Syscall.open(filename, flags);
			UnixStream fs = new UnixStream(fd);
			return new StreamWriter(fs);
		}

		/// <summary>
		/// Given a file name, create a StreamWriter
		/// </summary>
		/// <returns>
		/// a StreamWriter that will write to the file
		/// </returns>
		/// <param name='filename'>
		/// name of the file to be written to
		/// </param>
		public static StreamWriter CreateStreamWriter(string filename)
		{
			if (RunningOnUnix() && filename.StartsWith("/dev"))
				return CreateUnixAppendStreamWriter(filename);
			else
				return new StreamWriter(filename);
		}
	}
}

