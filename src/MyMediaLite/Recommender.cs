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

namespace MyMediaLite
{
	/// <summary>
	/// Abstract recommender class implementing default behaviors
	/// </summary>
	public abstract class Recommender : IRecommender
	{
		/// <summary>Maximum user ID</summary>
		public int MaxUserID { get; set; }

		/// <summary>Maximum item ID</summary>
		public int MaxItemID { get; set; }

		/// <summary>create a shallow copy of the object</summary>
		public Object Clone()
		{
			return this.MemberwiseClone();
		}

		///
		public abstract float Predict(int user_id, int item_id);

		///
		public virtual bool CanPredict(int user_id, int item_id)
		{
			return (user_id <= MaxUserID && user_id >= 0 && item_id <= MaxItemID && item_id >= 0);
		}

		///
		public virtual IList<Tuple<int, float>> Recommend(
			int user_id, int n = 20,
			ICollection<int> ignore_items = null,
			ICollection<int> candidate_items = null)
		{
			throw new NotImplementedException();
		}

		///
		public abstract void Train();

		///
		public virtual void LoadModel(string file) { throw new NotImplementedException(); }

		///
		public virtual void SaveModel(string file) { throw new NotImplementedException(); }

		///
		public override string ToString()
		{
			return this.GetType().Name;
		}
	}
}

