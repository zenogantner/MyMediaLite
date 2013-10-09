// Copyright (C) 2010, 2011, 2012, 2013 Zeno Gantner
// Copyright (C) 2012 Lucas Drumond
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
using MyMediaLite.DataType;
using MyMediaLite.Eval;
using MyMediaLite.IO;
using MyMediaLite.ItemRecommendation.BPR;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Sparse Linear Methods (SLIM) for item prediction (ranking) optimized for BPR-Opt optimization criterion </summary>
	/// <remarks>
	/// This implementation differs from the algorithm in the original SLIM paper since the model here is optimized for BPR-Opt
	/// instead of the elastic net loss. The optmization algorithm used is the Sotchastic Gradient Ascent.
	///
	/// Literature:
	/// <list type="bullet">
	///   <item><description>
	///     Steffen Rendle, Christoph Freudenthaler, Zeno Gantner, Lars Schmidt-Thieme:
	///     BPR: Bayesian Personalized Ranking from Implicit Feedback.
	///     UAI 2009.
	///     http://www.ismll.uni-hildesheim.de/pub/pdfs/Rendle_et_al2009-Bayesian_Personalized_Ranking.pdf
	///   </description></item>
	///   <item><description>
	///     X. Ning, G. Karypis: Slim: Sparse linear methods for top-n recommender systems.
	///    ICDM 2011.
	///    http://glaros.dtc.umn.edu/gkhome/fetch/papers/SLIM2011icdm.pdf
	///   </description></item>
	/// </list>
	/// This recommender supports incremental updates.
	/// </remarks>
	public class BPRSLIM : SLIM
	{
		/// <summary>Sample uniformly from users</summary>
		public bool UniformUserSampling { get; set; }

		/// <summary>Learning rate alpha</summary>
		public double LearnRate { get {	return learn_rate; } set { learn_rate = value; } }
		/// <summary>Learning rate alpha</summary>
		protected double learn_rate = 0.05;

		/// <summary>Regularization parameter for positive item weights</summary>
		public double RegI { get { return reg_i; } set { reg_i = value;	} }
		/// <summary>Regularization parameter for positive item weights</summary>
		protected double reg_i = 0.0025;

		/// <summary>Regularization parameter for negative item weights</summary>
		public double RegJ { get { return reg_j; } set { reg_j = value; } }
		/// <summary>Regularization parameter for negative item weights</summary>
		protected double reg_j = 0.00025;

		/// <summary>If set (default), update factors for negative sampled items during learning</summary>
		public bool UpdateJ { get { return update_j; } set { update_j = value; } }
		/// <summary>If set (default), update factors for negative sampled items during learning</summary>
		protected bool update_j = true;

		/// <summary>Default constructor</summary>
		public BPRSLIM()
		{
			UniformUserSampling = true;
		}

		///
		public override void Train()
		{
			InitModel();

			for (int i = 0; i < NumIter; i++)
				Iterate();
		}

		private IBPRSampler CreateBPRSampler()
		{
			if (UniformUserSampling)
				return new UniformUserSampler(Interactions);
			else
				return new UniformPairSampler(Interactions);
		}

		/// <summary>Perform one iteration of stochastic gradient ascent over the training data</summary>
		/// <remarks>
		/// One iteration is samples number of positive entries in the training matrix times
		/// </remarks>
		public override void Iterate()
		{
			var bpr_sampler = CreateBPRSampler();
			int num_pos_events = Interactions.Count;
			int user_id, pos_item_id, neg_item_id;

			for (int i = 0; i < num_pos_events; i++)
			{
				bpr_sampler.NextTriple(out user_id, out pos_item_id, out neg_item_id);
				UpdateParameters(user_id, pos_item_id, neg_item_id, true, true, update_j);
			}
		}

		/// <summary>Update latent factors according to the stochastic gradient descent update rule</summary>
		/// <param name="u">the user ID</param>
		/// <param name="i">the ID of the first item</param>
		/// <param name="j">the ID of the second item</param>
		/// <param name="update_u">if true, update the user latent factors</param>
		/// <param name="update_i">if true, update the latent factors of the first item</param>
		/// <param name="update_j">if true, update the latent factors of the second item</param>
		void UpdateParameters(int u, int i, int j, bool update_u, bool update_i, bool update_j)
		{
			double x_uij = PredictWithDifference(u, i, j);

			double one_over_one_plus_ex = 1 / (1 + Math.Exp(x_uij));

			// adjust factors
			var user_items = Interactions.ByUser(u).Items;

			foreach (int f in user_items)
			{
				double w_if = item_weights[i, f];
				double w_jf = item_weights[j, f];

				if (update_i && i != f)
				{
					double update = one_over_one_plus_ex - reg_i * w_if;
					item_weights[i, f] = (float) (w_if + learn_rate * update);
				}

				if (update_j && j != f)
				{
					double update = - one_over_one_plus_ex - reg_j * w_jf;
					item_weights[j, f] = (float) (w_jf + learn_rate * update);
				}
			}
		}

		/// <summary>Compute the fit (AUC on training data)</summary>
		/// <returns>the fit</returns>
		public override float ComputeObjective()
		{
			return 0;
		}

		///
		public double PredictWithDifference(int user_id, int pos_item_id, int neg_item_id)
		{
			if (user_id > MaxUserID || pos_item_id > MaxItemID || neg_item_id > MaxItemID)
				return double.MinValue;

			var user_items = Interactions.ByUser(user_id).Items;
			double prediction = 0;

			for (int k = 0; k < user_items.Count; k++)
			{
				int f = user_items.ElementAt(k);
				prediction += item_weights[pos_item_id, f] - item_weights[neg_item_id, f];
			}
			return prediction;
		}

		///
		public override void SaveModel(string file)
		{
			/*
				writer.WriteMatrix(item_weights);
			*/
		}

		///
		public override void LoadModel(string file)
		{
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} reg_i={1} reg_j={2} num_iter={3} learn_rate={4} uniform_user_sampling={5} update_j={6}",
				this.GetType().Name, reg_i, reg_j, NumIter, learn_rate, UniformUserSampling, UpdateJ);
		}
	}
}

