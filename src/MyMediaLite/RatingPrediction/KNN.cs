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
using MyMediaLite.Util;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Base class for rating predictors that use some kind of kNN</summary>
	/// <remarks>
	/// The method is described in section 2.2 of
	/// Yehuda Koren: Factor in the Neighbors: Scalable and Accurate Collaborative Filtering,
	/// Transactions on Knowledge Discovery from Data (TKDD), 2009.
	///
	/// This engine does NOT support online updates.
	///
	/// <seealso cref="ItemRecommendation.KNN"/>
	/// </remarks>
	public abstract class KNN : UserItemBaseline
	{
		/// <summary>Number of neighbors to take into account for predictions</summary>
		public uint K { get { return k;	} set {	k = value; } }
		private uint k = uint.MaxValue;

		/// <summary>Correlation matrix over some kind of entity</summary>
		protected CorrelationMatrix correlation;

		/// <summary>Create a new KNN recommender</summary>
		public KNN()
		{
			RegU = 10;
			RegI = 5;
		}
		
		///
		public override void SaveModel(string filename)
		{
			using ( StreamWriter writer = Recommender.GetWriter(filename, this.GetType()) )
				correlation.Write(writer);
		}

		///
		public override void LoadModel(string filename)
		{
			using ( StreamReader reader = Recommender.GetReader(filename, this.GetType()) )
			{
				CorrelationMatrix correlation = CorrelationMatrix.ReadCorrelationMatrix(reader);

				base.Train(); // train baseline model
				this.correlation = new BinaryCosine(correlation);
			}
		}
	}
}