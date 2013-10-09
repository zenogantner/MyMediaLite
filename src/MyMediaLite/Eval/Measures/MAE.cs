// Copyright (C) 2012, 2013 Zeno Gantner
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
using MyMediaLite.Data;
using MyMediaLite.RatingPrediction;

namespace MyMediaLite.Eval.Measures
{
	/// <summary>Utility functions for the mean absolute error</summary>
	public static class MAE
	{
		/// <summary>Computes the absolute error sum</summary>
		/// <returns>the absolute error sum</returns>
		/// <param name='recommender'>the recommender to make predictions with</param>
		/// <param name='ratings'>the actual ratings</param>
		public static double ComputeAbsoluteErrorSum(this RatingPredictor recommender, IInteractions interactions)
		{
			double sum = 0;
			var reader = interactions.Sequential;
			while (reader.Read())
				sum += Math.Abs(recommender.Predict(reader.GetUser(), reader.GetItem()) - reader.GetRating());
			return sum;
		}
	}
}

