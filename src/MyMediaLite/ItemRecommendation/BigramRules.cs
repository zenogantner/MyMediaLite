// Copyright (C) 2015 Dimitris Paraschakis
// Copyright (C) 2016 Zeno Gantner
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
	/// <summary> Recommender based on bigram association rules (item1 -&gt; item2)</summary>
	public class BigramRules : ItemRecommender
	{
		List<Dictionary<int, int>> rules_list = new List<Dictionary<int, int>>();

		///<summary>Default constructor</summary>
		public BigramRules() {}

		///
		public override void Train()
		{
			for (int item1 = 0; item1 <= MaxItemID; item1++)
			{
				var item1_vector = Feedback.ItemMatrix[item1];
				var correlated_items = new HashSet<int>();

				foreach (int user in item1_vector)
					correlated_items.UnionWith(Feedback.UserMatrix[user]);
				correlated_items.Remove(item1);

				var bigram_score = new Dictionary<int, int>();
				foreach (int item2 in correlated_items)
				{
					int intersection = 0;
					var item2_vector = Feedback.ItemMatrix[item2];
					foreach (int user in item1_vector)
					{
						if (item2_vector.Contains(user))
							intersection++;
					}
					bigram_score.Add(item2, intersection);
				}
				rules_list.Add(bigram_score);
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
				if (rules_list[item].ContainsKey(item_id))
				{
					itemTransactions = Feedback.ItemMatrix[item].Count;
					confidence = rules_list[item][item_id] / (float)itemTransactions;
					support = rules_list[item][item_id] / (float)Feedback.Count;
					score += support * confidence;
				}
			}
			return score;
		}

		///
		public override void SaveModel(string file)
		{
			using (StreamWriter writer = Model.GetWriter(file, this.GetType(), "3.12"))
			{
				writer.WriteLine(MaxUserID + " " + MaxItemID);
				foreach (var dict in rules_list)
				{
					string line = string.Join(" ", dict.OrderByDescending(x => x.Value).Select(x => x.Key + ":" + x.Value).ToArray());
					writer.WriteLine(line);
				}
			}
		}

		///
		public override void LoadModel(string file)
		{
			List<Dictionary<int, int>> rules_list = new List<Dictionary<int, int>>();
			using (StreamReader reader = Model.GetReader(file, this.GetType()))
			{
				string[] fields = reader.ReadLine().Split(' ');
				int max_user_id = int.Parse(fields[0]);
				int max_item_id = int.Parse(fields[1]);

				string line;
				while ((line = reader.ReadLine()) != null)
				{
					var dict = new Dictionary<int, int>();
					foreach (var pair in line.Split(' '))
					{
						string[] key_value = pair.Split(':');
						dict[int.Parse(key_value[0])] = int.Parse(key_value[1]);
					}
					rules_list.Add(dict);
				}

				MaxUserID = max_user_id;
				MaxItemID = max_item_id;
			}
			this.rules_list = rules_list;
		}
	}
}
