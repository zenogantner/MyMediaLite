// Copyright (C) 2011, 2012 Zeno Gantner
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
using MyMediaLite.IO;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Most-popular item recommender</summary>
	/// <remarks>
	///   <para>
	///     Items are weighted by how often they have been seen in the past.
	///   </para>
	///   <para>
	///     This method is not personalized.
	///   </para>
	///   <para>
	///     This recommender supports incremental updates.
	///   </para>
	/// </remarks>
	public class MostPopular : IncrementalItemRecommender
	{
		/// <summary>
		/// If true, the popularity of an item is measured by the number of unique users that have accessed it.
		/// If false, the popularity is measured by the number of accesses to the item.
		/// </summary>
		public bool ByUser { get; set; }

		/// <summary>View count</summary>
		IList<int> view_count;

		/// <summary>Default constructor</summary>
		public MostPopular()
		{
			UpdateItems = true;
		}

		///
		public override void Train()
		{
			view_count = new List<int>(MaxItemID + 1);
			for (int i = 0; i <= MaxItemID; i++)
				view_count.Add(0);

			if (ByUser)
				for (int item_id = 0; item_id <= MaxItemID; item_id++)
					view_count[item_id] = Feedback.ItemMatrix.NumEntriesByRow(item_id);
			else
				foreach (int i in Feedback.Items)
					view_count[i]++;
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			if (item_id <= MaxItemID)
				return (float) view_count[item_id] / (ByUser ? MaxUserID + 1 : Feedback.Count);
			else
				return float.MinValue;
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
		public override void AddFeedback(ICollection<Tuple<int, int>> feedback)
		{
			base.AddFeedback(feedback);
			if (UpdateItems)
			{
				var items = from t in feedback select t.Item2;
				foreach (int item_id in items)
					view_count[item_id]++;
			}
		}

		///
		public override void RemoveFeedback(ICollection<Tuple<int, int>> feedback)
		{
			base.RemoveFeedback(feedback);
			if (UpdateItems)
			{
				var items = from t in feedback select t.Item2;
				foreach (int item_id in items)
					view_count[item_id]--;
			}
		}

		///
		public override void SaveModel(string filename)
		{
			using ( StreamWriter writer = Model.GetWriter(filename, this.GetType(), "2.03") )
			{
				writer.WriteLine(MaxItemID + 1);
				for (int i = 0; i <= MaxItemID; i++)
					writer.WriteLine(i + " " + view_count[i]);
			}
		}

		///
		public override void LoadModel(string filename)
		{
			using ( StreamReader reader = Model.GetReader(filename, this.GetType()) )
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
				this.MaxItemID = view_count.Length - 1;
			}
		}

		///
		public override string ToString()
		{
			return string.Format(
				"{0} by_user={1}",
				this.GetType().Name, ByUser);
		}
	}
}