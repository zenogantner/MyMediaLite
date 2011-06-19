// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
// Copyright (C) 2011 Zeno Gantner
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
using MyMediaLite.Util;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Most-popular item recommender</summary>
	/// <remarks>
	/// Items are weighted by how often they have been seen in the past.
	///
	/// This method is not personalized.
	///
	/// This recommender supports online updates.
	/// </remarks>
	public class MostPopular : ItemRecommender
	{
		/// <summary>View count</summary>
		protected IList<int> view_count;

		///
		public override void Train()
		{
			view_count = new List<int>(MaxItemID + 1);
			for (int i = 0; i <= MaxItemID; i++)
				view_count.Add(0);

			for (int u = 0; u < Feedback.UserMatrix.NumberOfRows; u++)
				foreach (int i in Feedback.UserMatrix[u])
					view_count[i]++;
		}

		///
		public override double Predict(int user_id, int item_id)
		{
			if (item_id <= MaxItemID)
				return view_count[item_id];
			else
				return 0;
		}

		///
		protected override void AddItem(int item_id)
		{
			base.AddItem(item_id);
			while (view_count.Count <= MaxItemID)
				view_count.Add(0);
		}

		///
		public override void RemoveItem(int item_id)
		{
			base.RemoveItem(item_id);
			view_count[item_id] = 0;
		}

		///
		public override void RemoveUser(int user_id)
		{
			foreach (int i in Feedback.UserMatrix[user_id])
				view_count[i]--;
			base.RemoveUser(user_id);
		}

		///
		public override void AddFeedback(int user_id, int item_id)
		{
			base.AddFeedback(user_id, item_id);
			view_count[item_id]++;
		}

		///
		public override void RemoveFeedback(int user_id, int item_id)
		{
			base.RemoveFeedback(user_id, item_id);
			view_count[item_id]--;
		}

		///
		public override void SaveModel(string filename)
		{
			using ( StreamWriter writer = Recommender.GetWriter(filename, this.GetType()) )
			{
				writer.WriteLine(MaxItemID + 1);
				for (int i = 0; i <= MaxItemID; i++)
					writer.WriteLine(i + " " + view_count[i]);
			}
		}

		///
		public override void LoadModel(string filename)
		{
			using ( StreamReader reader = Recommender.GetReader(filename, this.GetType()) )
			{
				int size = int.Parse(reader.ReadLine());

				var view_count = new int[size];

				while (! reader.EndOfStream)
				{
					string[] numbers = reader.ReadLine().Split(' ');
					int item_id = int.Parse(numbers[0]);
					int count   = int.Parse(numbers[1]);
			 		view_count[item_id] = count;
				}
				this.view_count = view_count;
			}
		}

		///
		public override string ToString()
		{
			return "MostPopular";
		}
	}
}