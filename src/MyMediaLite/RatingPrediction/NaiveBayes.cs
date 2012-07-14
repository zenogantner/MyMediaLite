// Copyright (C) 2012 Zeno Gantner
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
using System.Linq;
using MyMediaLite.DataType;
using MyMediaLite.Util;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Attribute-aware rating predictor using Naive Bayes</summary>
	/// <remarks>
	/// This recommender DOES NOT support incremental updates.
	/// </remarks>
	public class NaiveBayes : RatingPredictor, IItemAttributeAwareRecommender
	{
		/// <summary>Smoothing parameter for the class probabilities (rating priors)</summary>
		public float ClassSmoothing { get; set; }
		/// <summary>Smoothing parameter for the attribute (given class/rating) probabilities</summary>
		public float AttributeSmoothing { get; set; }

		///
		public SparseBooleanMatrix ItemAttributes
		{
			get { return this.item_attributes; }
			set {
				this.item_attributes = value;
				this.NumItemAttributes = item_attributes.NumberOfColumns;
				this.MaxItemID = Math.Max(MaxItemID, item_attributes.NumberOfRows - 1);
			}
		}
		private SparseBooleanMatrix item_attributes;

		Matrix<float> user_class_probabilities;
		IList<SparseMatrix<float>> user_attribute_given_class_probabilities;

		///
		public int NumItemAttributes { get; private set; }
		
		/// <summary>Default constructor</summary>
		public NaiveBayes()
		{
			ClassSmoothing = 1;
			AttributeSmoothing = 1;
		}

		void InitModel()
		{
			user_class_probabilities = new Matrix<float>(MaxUserID + 1, ratings.Scale.Levels.Count);
			user_attribute_given_class_probabilities = new List<SparseMatrix<float>>();
			for (int u = 0; u <= MaxUserID; u++)
				user_attribute_given_class_probabilities.Add(new SparseMatrix<float>(ratings.Scale.Levels.Count, ItemAttributes.NumberOfColumns));
		}

		///
		public override void Train()
		{
			InitModel();

			// initialize counter variables
			var user_class_counts = new Matrix<int>(MaxUserID + 1, ratings.Scale.Levels.Count);
			var user_attribute_given_class_counts = new List<SparseMatrix<int>>();
			for (int user_id = 0; user_id <= MaxUserID; user_id++)
				user_attribute_given_class_counts.Add(new SparseMatrix<int>(ratings.Scale.Levels.Count, ItemAttributes.NumberOfColumns));

			// count
			for (int index = 0; index < Ratings.Count; index++)
			{
				int user_id = ratings.Users[index];
				int item_id = ratings.Items[index];
				int level_id = ratings.Scale.LevelID[ratings[index]];

				user_class_counts[user_id, level_id]++;
				foreach (int attribute_id in item_attributes.GetEntriesByRow(item_id))
					user_attribute_given_class_counts[user_id][attribute_id, level_id]++;
			}

			// compute probabilities
			for (int user_id = 0; user_id <= MaxUserID; user_id++)
			{
				float denominator = user_class_counts.GetRow(user_id).Sum() + ClassSmoothing;

				foreach (int level_id in ratings.Scale.LevelID.Values)
				{
					user_class_probabilities[user_id, level_id] = (user_class_counts[user_id, level_id] + ClassSmoothing) / denominator;

					// TODO more sparse implementation of this?
					for (int attribute_id = 0; attribute_id < NumItemAttributes; attribute_id++)
						user_attribute_given_class_probabilities[user_id][attribute_id, level_id]
							= (user_attribute_given_class_counts[user_id][attribute_id, level_id] + AttributeSmoothing) / (NumItemAttributes + AttributeSmoothing);
				}
			}
		}

		double PredictProbabilityProportions(int user_id, int item_id, int level_id)
		{
			double log_sum = Math.Log(user_class_probabilities[user_id, level_id]);
			foreach (int attribute_id in item_attributes.GetEntriesByRow(item_id))
				log_sum += Math.Log(user_attribute_given_class_probabilities[user_id][attribute_id, level_id]);
			return Math.Exp(log_sum);
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			double prob_sum = 0;
			double score_sum = 0;
			for (int level_id = 0; level_id < ratings.Scale.Levels.Count; level_id++)
			{
				double prob = PredictProbabilityProportions(user_id, item_id, level_id);
				prob_sum += prob;
				score_sum += prob * ratings.Scale.Levels[level_id];
			}
			return (float) (score_sum / prob_sum);
		}

		///
		public override string ToString()
		{
			return string.Format(
				"{0} class_smoothing={1} attribute_smoothing={2}",
				this.GetType().Name, ClassSmoothing, AttributeSmoothing);
		}
	}

}
