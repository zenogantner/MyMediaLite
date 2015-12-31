// Copyright (C) 2010, 2011, 2012 Zeno Gantner
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
using System.Diagnostics;
using System.Linq;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Taxonomy;

namespace MyMediaLite.Correlation
{
	/// <summary>Rating cosine similarity for rating data</summary>
	/// <remarks>
	///   Similarity is computed between common ratings values.
	///   https://en.wikipedia.org/wiki/Cosine_similarity#Definition
	/// 
	///   This is very similar to Pearson correlation, except that the ratings are not centered.
	/// </remarks>
	public sealed class RatingCosine : Pearson
	{
		///
		public RatingCosine(int num_entities, float shrinkage) : base(num_entities, shrinkage) {}

		protected override double GetDenominator(double i_sum, double j_sum, double ii_sum, double jj_sum, int n)
		{
			return Math.Sqrt(ii_sum * jj_sum);
		}

		protected override double GetNumerator(double i_sum, double j_sum, double ij_sum, int n)
		{
			return ij_sum;
		}
	}
}

