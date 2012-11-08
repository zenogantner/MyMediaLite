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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MyMediaLite.Data
{
	/// <summary>
	/// Class containing information about the rating scale of a data set:
	/// valid rating values, minimum/maximum rating.
	/// </summary>
	[Serializable()]
	public class RatingScale
	{
		/// <summary>the maximum rating in the dataset</summary>
		public float Max { get; private set; }
		/// <summary>the minimum rating in the dataset</summary>
		public float Min { get; private set; }

		/// <summary>list of rating levels (actual values)</summary>
		public List<float> Levels { get; private set; }

		/// <summary>mapping from level values to IDs</summary>
		public Dictionary<float, int> LevelID { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MyMediaLite.Data.RatingScale"/> class.
		/// </summary>
		/// <param name='levels'>a list of observed levels</param>
		public RatingScale(List<float> levels)
		{
			Init(levels);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MyMediaLite.Data.RatingScale"/> class,
		/// given a list of float values
		/// </summary>
		/// <param name='rating_values'>the ratings dataset</param>
		public RatingScale (IList<float> rating_values)
		{
			var levels = new HashSet<float>();
			foreach (float val in rating_values)
				levels.Add(val);

			Init(levels.ToList());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MyMediaLite.Data.RatingScale"/> class,
		/// given a list of byte values
		/// </summary>
		/// <param name='rating_values'>the ratings dataset</param>
		public RatingScale (IList<byte> rating_values)
		{
			var levels = new HashSet<float>();
			foreach (float val in rating_values)
				levels.Add((float) val);

			Init(levels.ToList());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MyMediaLite.Data.RatingScale"/> class,
		/// given two existing instances.
		/// </summary>
		/// <param name='scale1'>the first scale object</param>
		/// <param name='scale2'>the second scale object</param>
		public RatingScale(RatingScale scale1, RatingScale scale2)
		{
			Init(scale1.LevelID.Keys.Union(scale2.LevelID.Keys).ToList());
		}

		///
		public RatingScale(SerializationInfo info, StreamingContext context)
		{
			var levels = (List<float>) info.GetValue("Levels", typeof(List<float>));
			Init(levels);
		}

		///
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Levels", this.Levels);
		}

		private void Init(List<float> levels)
		{
			if (levels.Count == 0)
				throw new ArgumentException("There must be at least one level.");

			Levels = levels;
			Levels.Sort();

			Max = Levels[Levels.Count - 1];
			Min = Levels[0];
			LevelID = new Dictionary<float, int>();
			for (int i = 0; i < Levels.Count; i++)
				LevelID[Levels[i]] = i;
		}
	}
}

