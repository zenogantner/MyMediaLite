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
using System.Globalization;
using System.IO;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Util;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Frequency-weighted Slope-One rating prediction</summary>
	/// <remarks>
	/// Daniel Lemire, Anna Maclachlan:
	/// Slope One Predictors for Online Rating-Based Collaborative Filtering.
	/// SIAM Data Mining (SDM 2005)
	/// http://www.daniel-lemire.com/fr/abstracts/SDM2005.html
	///
	/// This recommender does NOT support online updates. They would be easy to implement, though.
	/// </remarks>
	public class SlopeOne : RatingPredictor
	{
  		private SkewSymmetricSparseMatrix diff_matrix;
  		private SymmetricSparseMatrix<int> freq_matrix;

		// TODO one more way to save memory: use short instead of int internally in the SparseMatrix datatypes

		private double global_average;

		void InitModel()
		{
			// default value if no prediction can be made
			global_average = Ratings.Average;

			// create data structure
			diff_matrix = new SkewSymmetricSparseMatrix(MaxItemID + 1);
			freq_matrix = new SymmetricSparseMatrix<int>(MaxItemID + 1);
		}

		/// <inheritdoc/>
		public override bool CanPredict(int user_id, int item_id)
		{
			if (user_id > MaxUserID || item_id > MaxItemID)
				return false;

			foreach (int index in Ratings.ByUser[user_id])
				if (freq_matrix[item_id, Ratings.Items[index]] != 0)
					return true;
			return false;
		}

		/// <inheritdoc/>
		public override double Predict(int user_id, int item_id)
		{
			if (item_id > MaxItemID || user_id > MaxUserID)
				return global_average;

			double prediction = 0.0;
			int frequency = 0;

			foreach (int index in Ratings.ByUser[user_id])

			{
				int other_item_id = Ratings.Items[index];
				int f = freq_matrix[item_id, other_item_id];
				if (f != 0)
				{
					prediction += ( diff_matrix[item_id, other_item_id] + Ratings[index] ) * f;
					frequency += f;
				}
			}

			if (frequency == 0)
				return global_average;

			return (double) prediction / frequency;
		}

		/// <inheritdoc/>
		public override void Train()
		{
			InitModel();

			// compute difference sums and frequencies
			foreach (var by_user_indices in Ratings.ByUser)
			{
				for (int i = 0; i < by_user_indices.Count; i++)
				{
					int index1 = by_user_indices[i];

					for (int j = i + 1; j < by_user_indices.Count; j++)
					{
						int index2 = by_user_indices[j];

			  			freq_matrix[Ratings.Items[index1], Ratings.Items[index2]] += 1;
			  			diff_matrix[Ratings.Items[index1], Ratings.Items[index2]] += (float) (Ratings[index1] - Ratings[index2]);
					}
				}
			}

			// compute average differences
			for (int i = 0; i <= MaxItemID; i++)
				foreach (int j in freq_matrix[i].Keys)
					diff_matrix[i, j] /= freq_matrix[i, j];
		}

		/// <inheritdoc/>
		public override void LoadModel(string file)
		{
			InitModel();

			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			using ( StreamReader reader = Recommender.GetReader(file, this.GetType()) )
			{
				var global_average = double.Parse(reader.ReadLine(), ni);

				var diff_matrix = (SkewSymmetricSparseMatrix) IMatrixUtils.ReadMatrix(reader, this.diff_matrix);  // TODO take symmetric matrix into account
				var freq_matrix = (SymmetricSparseMatrix<int>) IMatrixUtils.ReadMatrix(reader, this.freq_matrix); // TODO take anti-symmetric matrix into account

				// assign new model
				this.global_average = global_average;
				this.diff_matrix = diff_matrix;
				this.freq_matrix = freq_matrix;
			}
		}

		/// <inheritdoc/>
		public override void SaveModel(string file)
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			using ( StreamWriter writer = Recommender.GetWriter(file, this.GetType()) )
			{
				writer.WriteLine(global_average.ToString(ni));
				IMatrixUtils.WriteSparseMatrix(writer, diff_matrix);
				IMatrixUtils.WriteSparseMatrix(writer, freq_matrix);
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			 return string.Format("SlopeOne");
		}
	}
}