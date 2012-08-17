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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.IO;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Bi-polar frequency-weighted Slope-One rating prediction</summary>
	/// <remarks>
	/// <list type="bullet">
    ///   <item><description>
	///     Daniel Lemire, Anna Maclachlan:
	///     Slope One Predictors for Online Rating-Based Collaborative Filtering.
	///     SIAM Data Mining (SDM 2005).
	///     http://www.daniel-lemire.com/fr/abstracts/SDM2005.html
	///   </description></item>
	/// </list>
	///
	/// This recommender does NOT support incremental updates. They would be easy to implement, though.
	/// </remarks>
	public class BiPolarSlopeOne : RatingPredictor
	{
		private SkewSymmetricSparseMatrix  diff_matrix_like;
		private SymmetricSparseMatrix<int> freq_matrix_like;
		private SkewSymmetricSparseMatrix  diff_matrix_dislike;
		private SymmetricSparseMatrix<int> freq_matrix_dislike;

		private float global_average;
		private IList<float> user_average;

		///
		public override bool CanPredict(int user_id, int item_id)
		{
			if (user_id > MaxUserID || item_id > MaxItemID)
				return false;

			foreach (int index in ratings.ByUser[user_id])
			{
				if (freq_matrix_like[item_id, ratings.Items[index]] != 0)
					return true;
				if (freq_matrix_dislike[item_id, ratings.Items[index]] != 0)
					return true;
			}
			return false;
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			if (item_id > MaxItemID || user_id > MaxUserID)
				return global_average;

			double prediction = 0.0;
			int frequencies = 0;

			foreach (int index in ratings.ByUser[user_id])
				{
					if (ratings[index] > user_average[user_id])
					{
						int f = freq_matrix_like[item_id, ratings.Items[index]];
						if (f != 0)
						{
							prediction  += ( diff_matrix_like[item_id, ratings.Items[index]] + ratings[index] ) * f;
							frequencies += f;
						}
					}
					else
					{
						int f = freq_matrix_dislike[item_id, ratings.Items[index]];
						if (f != 0)
						{
							prediction  += ( diff_matrix_dislike[item_id, ratings.Items[index]] + ratings[index] ) * f;
							frequencies += f;
						}
					}
				}

			if (frequencies == 0)
				return global_average;

			float result = (float) (prediction / frequencies);

			if (result > MaxRating)
				return MaxRating;
			if (result < MinRating)
				return MinRating;
			return result;
		}

		///
		public override void Train()
		{
			InitModel();

			// default value if no prediction can be made
			global_average = ratings.Average;

			// compute difference sums and frequencies
			foreach (int user_id in ratings.AllUsers)
			{
				float user_avg = 0;
				foreach (int index in ratings.ByUser[user_id])
					user_avg += ratings[index];
				user_avg /= ratings.ByUser[user_id].Count;

				// store for later use
				user_average[user_id] = user_avg;

				foreach (int index in ratings.ByUser[user_id])
					foreach (int index2 in ratings.ByUser[user_id])
						if (ratings[index] > user_avg && ratings[index2] > user_avg)
						{
							freq_matrix_like[ratings.Items[index], ratings.Items[index2]] += 1;
							diff_matrix_like[ratings.Items[index], ratings.Items[index2]] += (float) (ratings[index] - ratings[index2]);
						}
						else if (ratings[index] < user_avg && ratings[index2] < user_avg)
						{
							freq_matrix_dislike[ratings.Items[index], ratings.Items[index2]] += 1;
							diff_matrix_dislike[ratings.Items[index], ratings.Items[index2]] += (float) (ratings[index] - ratings[index2]);
						}

			}

			// compute average differences
			foreach (var pair in freq_matrix_like.NonEmptyEntryIDs)
			{
				int i = pair.Item1;
				int j = pair.Item2;
				if (i < j)
					diff_matrix_like[i, j] /= freq_matrix_like[i, j];
			}
			foreach (var pair in freq_matrix_dislike.NonEmptyEntryIDs)
			{
				int i = pair.Item1;
				int j = pair.Item2;
				if (i < j)
					diff_matrix_dislike[i, j] /= freq_matrix_dislike[i, j];
			}
		}

		///
		void InitModel()
		{
			// create data structure
			diff_matrix_like = new SkewSymmetricSparseMatrix(MaxItemID + 1);
			freq_matrix_like = new SymmetricSparseMatrix<int>(MaxItemID + 1);
			diff_matrix_dislike = new SkewSymmetricSparseMatrix(MaxItemID + 1);
			freq_matrix_dislike = new SymmetricSparseMatrix<int>(MaxItemID + 1);
			user_average = new float[MaxUserID + 1];
		}

		///
		public override void LoadModel(string file)
		{
			InitModel();

			using ( StreamReader reader = Model.GetReader(file, this.GetType()) )
			{
				var global_average = float.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);

				var diff_matrix_like = (SkewSymmetricSparseMatrix) reader.ReadMatrix(this.diff_matrix_like);
				var freq_matrix_like = (SymmetricSparseMatrix<int>) reader.ReadMatrix(this.freq_matrix_like);
				var diff_matrix_dislike = (SkewSymmetricSparseMatrix) reader.ReadMatrix(this.diff_matrix_dislike);
				var freq_matrix_dislike = (SymmetricSparseMatrix<int>) reader.ReadMatrix(this.freq_matrix_dislike);
				var user_average = reader.ReadVector();

				// assign new model
				this.global_average = global_average;
				this.diff_matrix_like = diff_matrix_like;
				this.freq_matrix_like = freq_matrix_like;
				this.diff_matrix_dislike = diff_matrix_dislike;
				this.freq_matrix_dislike = freq_matrix_dislike;
				this.user_average = user_average;
			}
		}

		///
		public override void SaveModel(string file)
		{
			using ( StreamWriter writer = Model.GetWriter(file, this.GetType(), "2.99") )
			{
				writer.WriteLine(global_average.ToString(CultureInfo.InvariantCulture));
				writer.WriteSparseMatrix(diff_matrix_like);
				writer.WriteSparseMatrix(freq_matrix_like);
				writer.WriteSparseMatrix(diff_matrix_dislike);
				writer.WriteSparseMatrix(freq_matrix_dislike);
				writer.WriteVector(user_average);
			}
		}
	}
}