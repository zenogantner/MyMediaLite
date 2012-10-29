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
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MyMediaLite;

namespace MyMediaLite.ItemRecommendation
{
	// TODO
	// - fix numerical issues
	//   - bold driver?
	//   - annealing?
	// - run on epinions dataset
	// - support incremental updates
	// - add bias modeling
	// - more fine-grained regularization
	// - parallelize
	// - implement MAP optimization
	
	/// <summary>Collaborative Less-is-More Filtering Matrix Factorization</summary>
	/// <remarks>
	///   WARNING: Implementation of this recommender is not finished yet.
	/// </remarks>
	/// <exception cref='NotImplementedException'>
	/// Is thrown when a requested operation is not implemented for a given type.
	/// </exception>
	public class CLiMF : MF
	{
		/// <summary>Regularization parameter</summary>
		public float Regularization { get; set; }
		/// <summary>Learning rate; step size for gradient descent updates</summary>
		public float LearnRate { get; set; }

		/// <summary>Default constructor</summary>
		public CLiMF()
		{
			Regularization = 0.0f;
			LearnRate = 0.01f;
		}

		double Sig(float x)
		{
			return 1.0 / (1.0 + Math.Exp(-x));
		}

		///
		public override void Iterate()
		{
			var users = Enumerable.Range(0, Feedback.MaxUserID + 1).ToList();
			users.Shuffle();

			foreach (int user_id in users)
				UpdateForUser(user_id);
		}

		void UpdateForUser(int user_id)
		{
			// compute user gradient ...
			var user_gradient = new double[num_factors];
			foreach (int index in Feedback.ByUser[user_id])
			{
				int item_id = Feedback.Items[index];
				double sig_neg_score = Sig(-Predict(user_id, item_id));
				for (int f = 0; f < num_factors; f++)
					user_gradient[f] += sig_neg_score * item_factors[item_id, f];
				foreach (int other_index in Feedback.ByUser[user_id])
				{
					int other_item_id = Feedback.Items[other_index];
					float score_diff = Predict(user_id, other_item_id) - Predict(user_id, item_id);
					double sig_score_diff = Sig(score_diff);
					double deriv_sig_score_diff = Sig(-score_diff) * sig_score_diff;
					for (int f = 0; f < num_factors; f++)
						user_gradient[f] += (deriv_sig_score_diff / (1 - sig_score_diff)) * (item_factors[item_id, f] - item_factors[other_item_id, f]);
				}
			}
			// ... update user factors
			for (int f = 0; f < num_factors; f++)
				user_factors[user_id, f] -= (float) (LearnRate * (user_gradient[f] - Regularization * user_factors[user_id, f]));

			foreach (int index in Feedback.ByUser[user_id])
			{
				int item_id = Feedback.Items[index];
				// compute item gradient ...
				double sig_neg_score = Sig(-Predict(user_id, item_id)); // TODO speed up: score every item just ince
				var item_gradient = new double[num_factors];
				for (int f = 0; f < num_factors; f++)
					item_gradient[f] = sig_neg_score;
				foreach (int other_index in Feedback.ByUser[user_id])
				{
					int other_item_id = Feedback.Items[other_index];
					float score_diff = Predict(user_id, item_id) - Predict(user_id, other_item_id);
					double sig_score_diff = Sig(score_diff);
					double sig_score_neg_diff = Sig(-score_diff);
					double deriv_sig_score_diff = Sig(-score_diff) * sig_score_diff;
					double a = 1.0 / (1.0 - sig_score_neg_diff);
					double b = 1.0 / (1.0 - sig_score_diff);
					double x = deriv_sig_score_diff * (a - b);
					for (int f = 0; f < num_factors; f++)
						item_gradient[f] += x;
				}
				for (int f = 0; f < num_factors; f++)
					item_gradient[f] *= user_factors[user_id, f];

				// ... update item gradient
				for (int f = 0; f < num_factors; f++)
					item_factors[item_id, f] -= (float) (LearnRate * (item_gradient[f] - Regularization * item_factors[item_id, f]));
			}
		}

		///
		public override float ComputeObjective()
		{
			throw new NotImplementedException();
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} regularization={2} num_iter={3} learn_rate={4}",
				this.GetType().Name, num_factors, Regularization, NumIter, LearnRate);
		}
	}
}

