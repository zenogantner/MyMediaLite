// Copyright (C) 2013 Zeno Gantner
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
	public interface IFactory
	{
		IModel LoadModel(string filename);
		IRecommender CreateRecommender(IModel model);
		IRecommender CreateRecommender(IModel model, IDataSet dataset);
		bool SupportsUpdate { get; }
		IModel Train(IDataSet dataset, Dictionary<string, object> parameters);
		IModel Update(IModel model, IDataSet dataset, IList<int> modifiedUsers, IList<int> modifiedItems, Dictionary<string, object> parameters);
	}
}

