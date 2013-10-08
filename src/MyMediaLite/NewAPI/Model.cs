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

namespace MyMediaLite
{
	// TODO rename to Model
	public abstract class NewModel : IModel
	{
		protected virtual string Version { get { return "4.00"; } }

		public abstract void Save(TextWriter writer);
		public void Save(string filename)
		{
			using (StreamWriter writer = GetWriter(filename, Version))
				Save(writer);
		}

		/// <summary>Get a writer object to save the model parameters of a recommender</summary>
		/// <param name="filename">the filename of the model file</param>
		/// <param name="recommender_type">the recommender type</param>
		/// <param name="version">the version string (for backwards compatibility)</param>
		/// <returns>a <see cref="StreamWriter"/></returns>
		public StreamWriter GetWriter(string filename, string version)
		{
			var writer = new StreamWriter(filename);
			writer.WriteLine(this.GetType());
			writer.WriteLine(version);
			return writer;
		}
	}
}

