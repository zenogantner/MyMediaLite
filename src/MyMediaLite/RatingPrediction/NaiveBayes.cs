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
using System.Globalization;
using System.IO;
using System.Linq;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.IO;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Attribute-aware rating predictor using Naive Bayes</summary>
	/// <remarks>
	/// This recommender supports incremental updates.
	/// </remarks>
	[Obsolete]
	public class NaiveBayes : IncrementalRatingPredictor, IItemAttributeAwareRecommender
	{
		/// <summary>Smoothing parameter for the class probabilities (rating priors)</summary>
		public float ClassSmoothing { get; set; }
		/// <summary>Smoothing parameter for the attribute (given class/rating) probabilities</summary>
		public float AttributeSmoothing { get; set; }

		///
		public IBooleanMatrix ItemAttributes
		{
			get { return this.item_attributes; }
			set {
				this.item_attributes = value;
				this.NumItemAttributes = item_attributes.NumberOfColumns;
				this.MaxItemID = Math.Max(MaxItemID, item_attributes.NumberOfRows - 1);
			}
		}
		private IBooleanMatrix item_attributes;

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
			ComputeProbabilities(Enumerable.Range(0, MaxUserID + 1).ToArray());
		}

		void ComputeProbabilities(IList<int> users)
		{
			foreach (int user_id in users)
			{
				// initialize counter variables
				var user_class_counts = new int[ratings.Scale.Levels.Count];
				var user_attribute_given_class_counts = new SparseMatrix<int>(ratings.Scale.Levels.Count, ItemAttributes.NumberOfColumns);

				// count
				foreach (int index in ratings.ByUser[user_id])
				{
					int item_id = ratings.Items[index];
					int level_id = ratings.Scale.LevelID[ratings[index]];

					user_class_counts[level_id]++;
					foreach (int attribute_id in item_attributes.GetEntriesByRow(item_id))
						user_attribute_given_class_counts[attribute_id, level_id]++;
				}

				// compute probabilities
				float denominator = user_class_counts.Sum() + ClassSmoothing;

				foreach (int level_id in ratings.Scale.LevelID.Values)
				{
					user_class_probabilities[user_id, level_id] = (user_class_counts[level_id] + ClassSmoothing) / denominator;

					// TODO sparsify?
					for (int attribute_id = 0; attribute_id < NumItemAttributes; attribute_id++)
						user_attribute_given_class_probabilities[user_id][attribute_id, level_id]
							= (user_attribute_given_class_counts[attribute_id, level_id] + AttributeSmoothing) / (NumItemAttributes + AttributeSmoothing);
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
		public override void LoadModel(string filename)
		{
			using ( StreamReader reader = Model.GetReader(filename, this.GetType()) )
			{
				var user_class_probabilities = (Matrix<float>) reader.ReadMatrix(new Matrix<float>(0, 0));
				var num_users = int.Parse(reader.ReadLine());
				var user_attribute_given_class_probabilities = new List<SparseMatrix<float>>();
				for (int user_id = 0; user_id < num_users; user_id++)
					user_attribute_given_class_probabilities.Add(
						(SparseMatrix<float>) reader.ReadMatrix(new SparseMatrix<float>(0, 0))
					);

				this.user_class_probabilities = user_class_probabilities;
				this.user_attribute_given_class_probabilities = user_attribute_given_class_probabilities;
			}
		}

		///
		public override void SaveModel(string filename)
		{
			using ( StreamWriter writer = Model.GetWriter(filename, this.GetType(), "3.02") )
			{
				writer.WriteMatrix(user_class_probabilities);
				writer.WriteLine(user_attribute_given_class_probabilities.Count);
				foreach (var m in user_attribute_given_class_probabilities)
					writer.WriteSparseMatrix(m);
			}
		}

		///
		public override void AddRatings(IRatings ratings)
		{
			base.AddRatings(ratings);
			ComputeProbabilities(ratings.AllUsers);
		}

		///
		public override void UpdateRatings(IRatings ratings)
		{
			base.UpdateRatings(ratings);
			ComputeProbabilities(ratings.AllUsers);
		}

		///
		public override void RemoveRatings(IDataSet ratings)
		{
			base.RemoveRatings(ratings);
			ComputeProbabilities(ratings.AllUsers);
		}

		///
		protected override void AddUser(int user_id)
		{
			base.AddUser(user_id);
			user_class_probabilities.AddRows(user_id + 1);
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
