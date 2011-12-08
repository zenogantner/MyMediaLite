// Copyright (C) 2010, 2011 Zeno Gantner
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
using System.Globalization;
using System.IO;
using MyMediaLite.Correlation;
using MyMediaLite.Data;
using MyMediaLite.IO;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Base class for rating predictors that use some kind of kNN</summary>
	/// <remarks>
	///   <para>
	///     The method is described in section 2.2 of the paper below.
	///     One difference is that we support several iterations of alternating optimization,
	///     instead of just one.
	///   </para>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         Yehuda Koren: Factor in the Neighbors: Scalable and Accurate Collaborative Filtering,
	///         Transactions on Knowledge Discovery from Data (TKDD), 2009.
	///         http://public.research.att.com/~volinsky/netflix/factorizedNeighborhood.pdf
	///       </description></item>
	///     </list>
	///   </para>
	///   <para>
	///     This recommender supports incremental updates.
	///   </para>
	///   <seealso cref="MyMediaLite.ItemRecommendation.KNN"/>
	/// </remarks>
	public abstract class KNN : IncrementalRatingPredictor
	{
		/// <summary>Number of neighbors to take into account for predictions</summary>
		public uint K { get { return k;	} set {	k = value; } }
		private uint k = uint.MaxValue;

		///
		public override IRatings Ratings
		{
			set {
				base.Ratings = value;
				baseline_predictor.Ratings = value;
			}
		}

		/// <summary>regularization constant for the user bias of the underlying baseline predictor</summary>
		public double RegU { get { return baseline_predictor.RegU; } set { baseline_predictor.RegU = value; } }

		/// <summary>regularization constant for the item bias of the underlying baseline predictor</summary>
		public double RegI { get { return baseline_predictor.RegI; } set { baseline_predictor.RegI = value; } }

		/// <summary>Correlation matrix over some kind of entity</summary>
		protected CorrelationMatrix correlation;

		/// <summary>underlying baseline predictor</summary>
		protected UserItemBaseline baseline_predictor = new UserItemBaseline() { RegU = 10, RegI = 5 };

		///
		public override void SaveModel(string filename)
		{
			baseline_predictor.SaveModel(filename + "-global-effects");

			using ( StreamWriter writer = Model.GetWriter(filename, this.GetType()) )
				correlation.Write(writer);
		}

		///
		public override void LoadModel(string filename)
		{
			baseline_predictor.LoadModel(filename + "-global-effects");
			if (ratings != null)
				baseline_predictor.Ratings = ratings;

			using ( StreamReader reader = Model.GetReader(filename, this.GetType()) )
			{
				CorrelationMatrix correlation = CorrelationMatrix.ReadCorrelationMatrix(reader);
				this.correlation = correlation;
			}
		}
	}
}