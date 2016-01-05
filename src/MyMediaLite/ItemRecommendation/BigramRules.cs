// Copyright (C) 2015 Dimitris Paraschakis
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

namespace MyMediaLite.ItemRecommendation
{
	/// <summary> Recommender based on bigram association rules (item1 -&gt; item2)</summary>
	public class BigramRules : ItemRecommender
	{
		List<Dictionary<int, int>> rulesList = new List<Dictionary<int, int>>();

		///<summary>Default constructor</summary>
		public BigramRules() {}

		///
		public override void Train()
		{
			for (int item1 = 0; item1 < MaxItemID + 1; item1++)
			{
				HashSet<int> item1Vector = (HashSet<int>)Feedback.ItemMatrix[item1];
				HashSet<int> correlatedItems = new HashSet<int>();

				foreach (int user in item1Vector)
					correlatedItems.UnionWith(Feedback.UserMatrix[user]);
				correlatedItems.Remove(item1);

				Dictionary<int, int> bigramScore = new Dictionary<int, int>();
				foreach (int item2 in correlatedItems)
				{
					int intersection = 0;
					HashSet<int> item2Vector = (HashSet<int>)Feedback.ItemMatrix[item2];
					foreach (int user in item1Vector)
					{
						if (item2Vector.Contains(user))
							intersection++;
					}
					bigramScore.Add(item2, intersection);
				}
				rulesList.Add(bigramScore);
			}
		}

		// The prediction is based on linear combination of confidence and support
		///
		public override float Predict(int user_id, int item_id)
		{
			if (item_id > MaxItemID)
				return float.MinValue;

			float score = 0;
			foreach (int item in Feedback.UserMatrix[user_id])
			{
				int itemTransactions = 0;
				float confidence = 0;
				float support = 0;
				if (rulesList[item].ContainsKey(item_id))
				{
					itemTransactions = Feedback.ItemMatrix[item].Count;
					confidence = rulesList[item][item_id] / (float)itemTransactions;
					support = rulesList[item][item_id] / (float)Feedback.Count;
					score += support * confidence;
				}
			}
			return score;
		}
	}
}
