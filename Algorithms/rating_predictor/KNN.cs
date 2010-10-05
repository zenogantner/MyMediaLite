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
using System.Collections.Generic;
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
	/// <seealso cref="item_recommender.kNN"/>
	/// </remarks>
	/// <author>Zeno Gantner, University of Hildesheim</author>
	public abstract class KNN : UserItemBaseline
	{
		public uint k           = UInt32.MaxValue;
		public double shrinkage = 10;

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
				double shrinkage = Double.Parse(numbers[1], ni);

				CorrelationMatrix correlation = new CorrelationMatrix(reader);

				base.Train(); // ReadData(), train baseline model
				this.correlation = new Cosine(correlation);
				this.shrinkage = shrinkage;
			}
		}
	}
}