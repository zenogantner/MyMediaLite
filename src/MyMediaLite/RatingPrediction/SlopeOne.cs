// Copyright (C) 2011, 2012, 2013 Zeno Gantner
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
using MyMediaLite.IO;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Frequency-weighted Slope-One rating prediction</summary>
	/// <remarks>
	/// Daniel Lemire, Anna Maclachlan:
	/// Slope One Predictors for Online Rating-Based Collaborative Filtering.
	/// SIAM Data Mining (SDM 2005).
	/// http://www.daniel-lemire.com/fr/abstracts/SDM2005.html
	///
	/// This recommender does NOT support incremental updates. They would be easy to implement, though.
	/// </remarks>
	public class SlopeOne : RatingPredictor
	{
		private SkewSymmetricSparseMatrix diff_matrix;
		private SymmetricSparseMatrix<int> freq_matrix;

		private float global_average;

		///
		public override bool CanPredict(int user_id, int item_id)
		{
			if (user_id > MaxUserID || item_id > MaxItemID)
				return false;

			foreach (int index in ratings.ByUser[user_id])
				if (freq_matrix[item_id, ratings.Items[index]] != 0)
					return true;
			return false;
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			if (item_id > MaxItemID || user_id > MaxUserID)
				return global_average;

			double prediction = 0.0;
			int frequency = 0;

			foreach (int index in ratings.ByUser[user_id])
			{
				int other_item_id = ratings.Items[index];
				int f = freq_matrix[item_id, other_item_id];
				if (f != 0)
				{
					prediction += ( diff_matrix[item_id, other_item_id] + ratings[index] ) * f;
					frequency += f;
				}
			}

			if (frequency == 0)
				return global_average;

			float result = (float) (prediction / frequency);
			if (result > MaxRating)
				return MaxRating;
			if (result < MinRating)
				return MinRating;
			return result;
		}

		void InitModel()
		{
			diff_matrix = new SkewSymmetricSparseMatrix(MaxItemID + 1);
			freq_matrix = new SymmetricSparseMatrix<int>(MaxItemID + 1);
		}

		///
		public override void Train()
		{
			InitModel();

			// default value if no prediction can be made
			global_average = ratings.Average;

			// compute difference sums and frequencies
			foreach (var by_user_indices in ratings.ByUser)
			{
				for (int i = 0; i < by_user_indices.Count; i++)
				{
					int index1 = by_user_indices[i];

					for (int j = i + 1; j < by_user_indices.Count; j++)
					{
						int index2 = by_user_indices[j];

						freq_matrix[ratings.Items[index1], ratings.Items[index2]] += 1;
						diff_matrix[ratings.Items[index1], ratings.Items[index2]] += (float) (ratings[index1] - ratings[index2]);
					}
				}
			}

			// compute average differences
			foreach (var pair in freq_matrix.NonEmptyEntryIDs)
			{
				int i = pair.Item1;
				int j = pair.Item2;
				if (i < j)
					diff_matrix[i, j] /= freq_matrix[i, j];
			}
		}

		///
		public override void LoadModel(string file)
		{
			InitModel();

			using ( StreamReader reader = Model.GetReader(file, this.GetType()) )
			{
				var global_average = float.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);

				var diff_matrix = (SkewSymmetricSparseMatrix) reader.ReadMatrix(this.diff_matrix);
				var freq_matrix = (SymmetricSparseMatrix<int>) reader.ReadMatrix(this.freq_matrix);

				// assign new model
				this.global_average = global_average;
				this.diff_matrix = diff_matrix;
				this.freq_matrix = freq_matrix;
			}
		}

		///
		public override void SaveModel(string file)
		{
			using ( StreamWriter writer = Model.GetWriter(file, this.GetType(), "2.99") )
			{
				writer.WriteLine(global_average.ToString(CultureInfo.InvariantCulture));
				writer.WriteSparseMatrix(diff_matrix);
				writer.WriteSparseMatrix(freq_matrix);
			}
		}
	}
}