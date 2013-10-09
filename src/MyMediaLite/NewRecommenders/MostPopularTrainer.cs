// Copyright (C) 2011, 2012, 2013 Zeno Gantner
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
//
using System;
using System.Collections.Generic;

namespace MyMediaLite
{
	public class MostPopularTrainer : ITrainer
	{
		// TODO later
		public bool SupportsUpdate { get { return false; } }

		public IModel Train(IDataSet dataset, Dictionary<string, object> parameters = null)
		{
			bool byUser = false; // TODO support via parameters
			int maxUserID = dataset.UserItemInteractions.MaxUserID;
			int maxItemID = dataset.UserItemInteractions.MaxItemID;

			var viewCount = new List<float>(maxItemID + 1);
			for (int item_id = 0; item_id <= maxItemID; item_id++)
				viewCount.Add(0);

			if (byUser)
			{
				for (int item_id = 0; item_id <= maxItemID; item_id++)
					viewCount[item_id] = dataset.UserItemInteractions.ByItem(item_id).Users.Count;
			}
			else
			{
				var reader = dataset.UserItemInteractions.Sequential;
				while (reader.Read())
					viewCount[reader.GetItem()] += 1;
			}
			return new StaticItemModel(viewCount);
		}

		public IModel Update(IModel model, IDataSet dataset, IList<int> modifiedUsers, IList<int> modifiedItems, Dictionary<string, object> parameters)
		{
			// TODO: lazy solution, just retrain everything
			return Train (dataset, parameters);
		}
	}
}

