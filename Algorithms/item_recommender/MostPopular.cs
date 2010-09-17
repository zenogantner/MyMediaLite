// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MyMediaLite.util;


namespace MyMediaLite.item_recommender
{
    /// <summary>
    /// Most-popular item recommender. Items are weighted by how often they have been seen in the past.
    /// This method is not personalized.
    /// This engine does not support online updates.
    /// </summary>
    /// <author>Steffen Rendle, Zeno Gantner, University of Hildesheim</author>
    public class MostPopular : Memory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemRecommenderMostPopular"/> class.
        /// </summary>
        public MostPopular() { }
        /// <summary>
        /// View count
        /// </summary>
        protected Dictionary<int, int> view_count = new Dictionary<int, int>();

        /// <inheritdoc />
        public override void Train()
        {
            for (int i = 0; i <= max_item_id; i++)
                view_count[i] = this.data_item.GetRow(i).Count;
        }

		/// <inheritdoc />
        public override double Predict(int user_id, int item_id)
        {
            int cnt = 0;
            if (view_count.TryGetValue(item_id, out cnt))
                return cnt;
            else
                return 0;
        }

		/// <inheritdoc />
		public override void RemoveItem (int item_id)
		{
			view_count.Remove(item_id);
		}

		/// <inheritdoc/>
        public override void AddFeedback(int user_id, int item_id)
        {
			if (!view_count.ContainsKey(item_id))
            {
                view_count[item_id] = 0;
            }

			view_count[item_id]++;
        }

        /// <inheritdoc/>
        public override void RemoveFeedback(int user_id, int item_id)
        {
        	view_count[item_id]--;
		}

		/// <inheritdoc />
		public override void SaveModel(string filePath)
		{
			using ( StreamWriter writer = EngineStorage.GetWriter(filePath, this.GetType()) )
			{
				foreach (int key in view_count.Keys)
					writer.WriteLine(key + " " + view_count[key]);
			}
		}

		/// <inheritdoc />
		public override void LoadModel(string filePath)
		{
			using ( StreamReader reader = EngineStorage.GetReader(filePath, this.GetType()) )
			{
				view_count = new Dictionary<int, int>();
				while (! reader.EndOfStream)
				{
					string[] numbers = reader.ReadLine().Split(' ');
					int key   = System.Int32.Parse(numbers[0]);
					int count = System.Int32.Parse(numbers[1]);
			 		view_count[key] = count;
				}
			}
		}

		/// <summary>
		/// returns the name of the method
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public override string ToString()
		{
			return "most-popular";
		}
    }
}