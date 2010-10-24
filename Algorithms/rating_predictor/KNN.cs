// Copyright (C) 2010 Zeno Gantner
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
using MyMediaLite.correlation;
using MyMediaLite.util;

namespace MyMediaLite.rating_predictor
{
	/// <remarks>
	/// Base class for rating predictors that use some kind of kNN
	///
	/// The method is described in section 2.2 of
	/// Yehuda Koren: Factor in the Neighbors: Scalable and Accurate Collaborative Filtering,
	/// Transactions on Knowledge Discovery from Data (TKDD), 2009
	///
	/// This engine does NOT support online updates.
	///
	/// <seealso cref="item_recommender.KNN"/>
	/// </remarks>
	/// <author>Zeno Gantner, University of Hildesheim</author>
	public abstract class KNN : UserItemBaseline
	{
		/// <summary>
		/// Number of neighbors to take into account for predictions
		/// </summary>		
		public uint K { get { return k;	} set {	k = value; } }
		private uint k = uint.MaxValue;		

		/// <summary>
		/// Shrinkage parameter
		/// </summary>		
		public float Shrinkage { get { return shrinkage; } set { shrinkage = value; } }
		private float shrinkage = 10;

        /// <summary>
        /// Correlation matrix over some kind of entity
        /// </summary>
        protected CorrelationMatrix correlation;

		/// <inheritdoc />
		public override void SaveModel(string filePath)
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			using ( StreamWriter writer = EngineStorage.GetWriter(filePath, this.GetType()) )
			{
				writer.WriteLine("shrinkage {0}", this.shrinkage.ToString(ni));
				correlation.Write(writer);
			}
		}

		/// <inheritdoc />
		public override void LoadModel(string filePath)
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

            using ( StreamReader reader = EngineStorage.GetReader(filePath, this.GetType()) )
			{
				string[] numbers = reader.ReadLine().Split(' ');
				float shrinkage  = float.Parse(numbers[1], ni);

				CorrelationMatrix correlation = CorrelationMatrix.ReadCorrelationMatrix(reader);

				base.Train(); // train baseline model
				this.correlation = new Cosine(correlation);
				this.shrinkage = shrinkage;
			}
		}
	}
}