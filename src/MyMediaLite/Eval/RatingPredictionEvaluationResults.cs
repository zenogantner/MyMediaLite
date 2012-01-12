// Copyright (C) 2011, 2012 Zeno Gantner
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
using System.Globalization;

namespace MyMediaLite.Eval
{
	/// <summary>Rating prediction evaluation results</summary>
	/// <remarks>
	/// This class is basically a Dictionary with a custom-made ToString() method.
	/// </remarks>
	public class RatingPredictionEvaluationResults : Dictionary<string, double>
	{
		/// <summary>Format rating prediction results</summary>
		/// <remarks>
		/// See http://recsyswiki.com/wiki/Root_mean_square_error and http://recsyswiki.com/wiki/Mean_absolute_error
		/// </remarks>
		/// <returns>a string containing the results</returns>
		public override string ToString()
		{
			string s = string.Format(
				CultureInfo.InvariantCulture, "RMSE {0:0.#####} MAE {1:0.#####} NMAE {2:0.#####} CBD {3:0.#####}",
				this["RMSE"], this["MAE"], this["NMAE"], this["CBD"]
			);
			if (this.ContainsKey("fit"))
				s += string.Format(CultureInfo.InvariantCulture, " fit {0:0.#####}", this["fit"]);
			return s;
		}

	}
}

