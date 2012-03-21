// Copyright (C) 2012 Zeno Gantner
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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MyMediaLite.IO
{
	/// <summary>Static class for serializing objects to binary files</summary>
	public static class FileSerializer
	{
		/// <summary>Serialize the specified object to a binary file</summary>
		/// <param name='object_to_serialize'>object to serialize</param>
		/// <param name='filename'>name of the file to save to</param>
		public static void Serialize(this ISerializable object_to_serialize, string filename)
		{
			Stream stream = File.Open(filename, FileMode.Create);
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, object_to_serialize);
			stream.Close();
		}

		/// <summary>Deserialize an object from a binary file</summary>
		/// <param name='filename'>name of the file to load from</param>
		public static ISerializable Deserialize(string filename)
		{
			Stream stream = File.Open(filename, FileMode.Open);
			BinaryFormatter formatter = new BinaryFormatter();
			ISerializable object_to_serialize = (ISerializable) formatter.Deserialize(stream);
			stream.Close();
			return object_to_serialize;
		}
	}
}

