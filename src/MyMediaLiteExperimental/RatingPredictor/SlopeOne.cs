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
using MyMediaLite.Data;


namespace MyMediaLite.RatingPredictor
{
	/// <summary>
	/// Weighted Slope One
	/// </summary>
	/// <remarks>
	/// http://www.daniel-lemire.com/fr/documents/publications/SlopeOne.java
	/// </remarks>
	public class SlopeOne : Memory
	{
		// TODO use an array here (we assume our item IDs to be dense ...
  		private Dictionary<int, Dictionary<int, double>> diff_matrix;
  		private Dictionary<int, Dictionary<int, int>> freq_matrix;
		
		public override double Predict(int user_id, int item_id)
		{
			return 0;
		}
		
		public override void Train()
		{
			// create data structure
    		diff_matrix = new Dictionary<int ,Dictionary<int, double>>();
    		freq_matrix = new Dictionary<int ,Dictionary<int, int>>();
    
			foreach (var user_ratings in Ratings.ByUser)
			{
				foreach (RatingEvent r in user_ratings)
				{
					if (! diff_matrix.ContainsKey(r.item_id))					    
					{
	          			diff_matrix[r.item_id] = new Dictionary<int, double>();
	          			freq_matrix[r.item_id] = new Dictionary<int, int>();
	        		}
	        		foreach (RatingEvent r2 in user_ratings)
					{
	          			if (!freq_matrix[r.item_id].ContainsKey(r2.item_id))
							freq_matrix[r.item_id][r2.item_id] = 0;
	          			if (!diff_matrix[r.item_id].ContainsKey(r2.item_id))
	            			diff_matrix[r.item_id][r2.item_id] = 0.0;

	          			freq_matrix[r.item_id][r2.item_id] += 1;
	          			diff_matrix[r.item_id][r2.item_id] += r.rating - r2.rating;
	        		}
      			}
    		}
			foreach (int j in diff_matrix.Keys)
				foreach (int i in diff_matrix[j].Keys)
					diff_matrix[j][i] /= freq_matrix[j][i];
		}

		public override void LoadModel(string file)
		{
			throw new NotImplementedException();
		}		
		
		public override void SaveModel(string file)
		{
			throw new NotImplementedException();
		}
		                               
	}
}