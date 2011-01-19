// Copyright (C) 2010, 2011 Zeno Gantner
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

namespace MyMediaLite.ItemRecommender
{
	/// <summary>Random prediction engine for use as experimental baseline</summary>
    public class Random : IItemRecommender
    {
        /// <inheritdoc/>
        public void Train() { }

        /// <inheritdoc/>
		public double Predict(int user_id, int item_id)
		{
			return Util.Random.GetInstance().NextDouble();
		}

		/// <inheritdoc/>
		public virtual void AddFeedback(int user_id, int item_id) { }

		/// <inheritdoc/>
		public virtual void RemoveFeedback(int user_id, int item_id) { }

		/// <inheritdoc/>
		public virtual void AddUser(int user_id) { }

		/// <inheritdoc/>
		public virtual void AddItem(int item_id) { }

		/// <inheritdoc/>
		public virtual void RemoveUser(int user_id) { }

		/// <inheritdoc/>
		public virtual void RemoveItem(int item_id) { }

        /// <inheritdoc/>
		public void SaveModel(string filename)
		{
			// do nothing
		}

        /// <inheritdoc/>
		public void LoadModel(string filename)
		{
			// do nothing
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return "random";
		}
	}
}