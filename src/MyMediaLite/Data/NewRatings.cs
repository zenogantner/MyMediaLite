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
// You should have received a copy of the GNU General Public License
// along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace MyMediaLite
{
	public enum RatingDataOrg { UNKNOWN, RANDOM, BY_USER, BY_ITEM }
	
	public class NewRatingData
	{
		public List<int> user_ids;
		public List<int> item_ids;
		public List<double> ratings; // TODO try to make generic here

		public RatingDataOrg organization = RatingDataOrg.UNKNOWN;
		
		public NewRatingData()
		{
		}
		
		public List<List<int>> IndexByUser { get; set; }
		public List<List<int>> IndexByItem { get; set; }
		public List<List<int>> IndexRandom { get; set; }
	}
}

