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
// 

using System;
using System.Globalization;

namespace MyMediaLite.ItemRecommendation
{
	public class BPRMF_Sample : BPRMF
	{
		public override void Iterate()
		{
			int num_pos_examples = Feedback.Count;
			
			if (WithReplacement)
				for (int i = 0; i < num_pos_examples; i++)
				{
					int index = random.Next(num_pos_examples);
					int user_id = Feedback.Users[index];
					int pos_item_id = Feedback.Items[index];
					int neg_item_id = -1;
					SampleOtherItem(user_id, pos_item_id, out neg_item_id);
					UpdateFactors(user_id, pos_item_id, neg_item_id, true, true, update_j);
				}
			else
				foreach (int index in Feedback.RandomIndex)
				{
					int user_id = Feedback.Users[index];
					int pos_item_id = Feedback.Items[index];
					int neg_item_id = -1;
					SampleOtherItem(user_id, pos_item_id, out neg_item_id);
					UpdateFactors(user_id, pos_item_id, neg_item_id, true, true, update_j);
				}

			if (BoldDriver)
			{
				double loss = ComputeLoss();

				if (loss > last_loss)
					LearnRate *= 0.5;
				else if (loss < last_loss)
					LearnRate *= 1.1;

				last_loss = loss;

				Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "loss {0} learn_rate {1} ", loss, LearnRate));
			}
		}
	}
}

