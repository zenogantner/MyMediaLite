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
using MyMediaLite.DataType;
using MyMediaLite.Correlation;

namespace MyMediaLite.ItemRecommendation
{
	public class IncrementalItemKNN: ItemKNN, IncrementalItemRecommender
	{
		protected Cooccurrence freqMatrix = new Cooccurrence(0);

		public override void Train()
		{
			base.Train();
			freqMatrix.ComputeCorrelations(DataMatrix);
			correctFreqMatrixDiagonal();
		}
	
		private void correctFreqMatrixDiagonal()
		{
			for (int i = 0; i < freqMatrix.NumEntities; i++)
				freqMatrix[i, i] = DataMatrix.NumEntriesByRow(i);
		}
	
		public override void AddFeedback(ICollection<Tuple<int, int>> feedback)
		{
			Dictionary<int, List<int>> ratings = new Dictionary<int, ICollection<int>>();
			foreach (var tuple in feedback)
			{
				int user_id = tuple.Item1;
				if (! ratings.ContainsKey(user_id))
					ratings[user_id] = new HashSet();
				int item_id = tuple.Item2;
				ratings[user_id].Add(item_id);
			
				if (user_id > MaxUserID)
					AddUser(user_id);
				if (item_id > MaxItemID) {
					AddItem(item_id);
					freqMatrix.AddEntity(item_id);
				}
				freqMatrix[item_id, item_id]++;

				Feedback.Add(user_id, item_id);
			}
			
			foreach (KeyValuePair<int,HashSet<int>> rat in ratings)
			{
				List rated_items = DataMatrix.GetEntriesByColumn(rat.Key);
				List new_items = rat.Value.ToList();
				foreach (int rated_item in rated_items)
				{
					foreach(int new_item in new_items)
					{
						freqMatrix[rated_item, new_item]++;
						// TODO: calculate new similarities
					}
				}
			}
			
		}			
	}
}

