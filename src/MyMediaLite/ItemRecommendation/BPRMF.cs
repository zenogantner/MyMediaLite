// Copyright (C) 2011, 2012, 2013 Zeno Gantner
// Copyright (C) 2010 Zeno Gantner, Christoph Freudenthaler
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
using MyMediaLite;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Eval;
using MyMediaLite.IO;
using MyMediaLite.ItemRecommendation.BPR;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Matrix factorization model for item prediction (ranking) optimized for BPR </summary>
	/// <remarks>
	///   <para>
	///     BPR reduces ranking to pairwise classification.
	///     The different variants (settings) of this recommender
	///     roughly optimize the area under the ROC curve (AUC).
	///   </para>
	///   <para>
	///     \f[
	///       \max_\Theta \sum_{(u,i,j) \in D_S}
	///                        \ln g(\hat{s}_{u,i,j}(\Theta)) - \lambda ||\Theta||^2 ,
	///     \f]
	///     where \f$\hat{s}_{u,i,j}(\Theta) := \hat{s}_{u,i}(\Theta) - \hat{s}_{u,j}(\Theta)\f$
	///     and \f$D_S = \{ (u, i, j) | i \in \mathcal{I}^+_u \wedge j \in \mathcal{I}^-_u \}\f$.
	///     \f$\Theta\f$ represents the parameters of the model and \f$\lambda\f$ is a regularization constant.
	///     \f$g\f$ is the  logistic function.
	///   </para>
	///   <para>
	///     In this implementation, we distinguish different regularization updates for users and positive and negative items,
	///     which means we do not have only one regularization constant. The optimization problem specified above thus is only
	///     an approximation.
	///   </para>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         Steffen Rendle, Christoph Freudenthaler, Zeno Gantner, Lars Schmidt-Thieme:
	///         BPR: Bayesian Personalized Ranking from Implicit Feedback.
	///         UAI 2009.
	///         http://www.ismll.uni-hildesheim.de/pub/pdfs/Rendle_et_al2009-Bayesian_Personalized_Ranking.pdf
	///       </description></item>
	///     </list>
	///   </para>
	///   <para>
	///     UniformUserSampling=true (the default) approximately optimizes the average AUC over all users.
	///   </para>
	/// </remarks>
	public class BPRMF : MF
	{
		/// <summary>Item bias terms</summary>
		protected float[] item_bias;

		/// <summary>Sample uniformly from users</summary>
		public bool UniformUserSampling { get; set; }

		/// <summary>Regularization parameter for the bias term</summary>
		public float BiasReg { get; set; }

		/// <summary>Learning rate alpha</summary>
		public float LearnRate { get { return learn_rate; } set { learn_rate = value; } }
		/// <summary>Learning rate alpha</summary>
		protected float learn_rate = 0.05f;

		/// <summary>Regularization parameter for user factors</summary>
		public float RegU { get { return reg_u; } set { reg_u = value; } }
		/// <summary>Regularization parameter for user factors</summary>
		protected float reg_u = 0.0025f;

		/// <summary>Regularization parameter for positive item factors</summary>
		public float RegI { get { return reg_i; } set { reg_i = value;	} }
		/// <summary>Regularization parameter for positive item factors</summary>
		protected float reg_i = 0.0025f;

		/// <summary>Regularization parameter for negative item factors</summary>
		public float RegJ { get { return reg_j; } set { reg_j = value; } }
		/// <summary>Regularization parameter for negative item factors</summary>
		protected float reg_j = 0.00025f;

		/// <summary>If set (default), update factors for negative sampled items during learning</summary>
		public bool UpdateJ { get { return update_j; } set { update_j = value; } }
		/// <summary>If set (default), update factors for negative sampled items during learning</summary>
		protected bool update_j = true;

		/// <summary>array of user components of triples to use for approximate loss computation</summary>
		int[] loss_sample_u;
		/// <summary>array of positive item components of triples to use for approximate loss computation</summary>
		int[] loss_sample_i;
		/// <summary>array of negative item components of triples to use for approximate loss computation</summary>
		int[] loss_sample_j;

		/// <summary>Default constructor</summary>
		public BPRMF()
		{
			UniformUserSampling = true;
		}

		///
		protected override void InitModel()
		{
			base.InitModel();

			item_bias = new float[MaxItemID + 1];
		}

		protected virtual IBPRSampler CreateBPRSampler()
		{
			if (UniformUserSampling)
				return new UniformUserSampler(Interactions);
			else
				return new UniformPairSampler(Interactions);
		}

		///
		public override void Train()
		{
			InitModel();

			{
				var bpr_sampler = CreateBPRSampler();
				int num_sample_triples = (int) Math.Sqrt(MaxUserID) * 100;
				Console.Error.WriteLine("loss_num_sample_triples={0}", num_sample_triples);
				// create the sample to estimate loss from
				loss_sample_u = new int[num_sample_triples];
				loss_sample_i = new int[num_sample_triples];
				loss_sample_j = new int[num_sample_triples];
				int user_id, item_id, other_item_id;
				for (int c = 0; c < num_sample_triples; c++)
				{
					bpr_sampler.NextTriple(out user_id, out item_id, out other_item_id);
					loss_sample_u[c] = user_id;
					loss_sample_i[c] = item_id;
					loss_sample_j[c] = other_item_id;
				}
			}

			for (int i = 0; i < NumIter; i++)
				Iterate();
		}

		/// <summary>Perform one iteration of stochastic gradient ascent over the training data</summary>
		/// <remarks>
		/// One iteration is samples number of positive entries in the training matrix times
		/// </remarks>
		public override void Iterate()
		{
			int num_pos_events = Interactions.Count;
			int user_id, pos_item_id, neg_item_id;

			var bpr_sampler = CreateBPRSampler();
			for (int i = 0; i < num_pos_events; i++)
			{
				bpr_sampler.NextTriple(out user_id, out pos_item_id, out neg_item_id);
				UpdateParameters(user_id, pos_item_id, neg_item_id, true, true, update_j);
			}
		}

		/// <summary>Update latent factors according to the stochastic gradient descent update rule</summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the ID of the first item</param>
		/// <param name="other_item_id">the ID of the second item</param>
		/// <param name="update_u">if true, update the user latent factors</param>
		/// <param name="update_i">if true, update the latent factors of the first item</param>
		/// <param name="update_j">if true, update the latent factors of the second item</param>
		protected virtual void UpdateParameters(int user_id, int item_id, int other_item_id, bool update_u, bool update_i, bool update_j)
		{
			double x_uij = item_bias[item_id] - item_bias[other_item_id] + DataType.MatrixExtensions.RowScalarProductWithRowDifference(user_factors, user_id, item_factors, item_id, item_factors, other_item_id);

			double one_over_one_plus_ex = 1 / (1 + Math.Exp(x_uij));

			// adjust bias terms
			if (update_i)
			{
				double update = one_over_one_plus_ex - BiasReg * item_bias[item_id];
				item_bias[item_id] += (float) (learn_rate * update);
			}

			if (update_j)
			{
				double update = -one_over_one_plus_ex - BiasReg * item_bias[other_item_id];
				item_bias[other_item_id] += (float) (learn_rate * update);
			}

			// adjust factors
			for (int f = 0; f < num_factors; f++)
			{
				float w_uf = user_factors[user_id, f];
				float h_if = item_factors[item_id, f];
				float h_jf = item_factors[other_item_id, f];

				if (update_u)
				{
					double update = (h_if - h_jf) * one_over_one_plus_ex - reg_u * w_uf;
					user_factors[user_id, f] = (float) (w_uf + learn_rate * update);
				}

				if (update_i)
				{
					double update = w_uf * one_over_one_plus_ex - reg_i * h_if;
					item_factors[item_id, f] = (float) (h_if + learn_rate * update);
				}

				if (update_j)
				{
					double update = -w_uf * one_over_one_plus_ex - reg_j * h_jf;
					item_factors[other_item_id, f] = (float) (h_jf + learn_rate * update);
				}
			}
		}

		///
		public override float ComputeObjective()
		{
			double ranking_loss = 0;
			for (int c = 0; c < loss_sample_u.Length; c++)
			{
				double x_uij = Predict(loss_sample_u[c], loss_sample_i[c]) - Predict(loss_sample_u[c], loss_sample_j[c]);
				ranking_loss += 1 / (1 + Math.Exp(x_uij));
			}

			double complexity = 0;
			for (int c = 0; c < loss_sample_u.Length; c++)
			{
				complexity += RegU * Math.Pow(user_factors.GetRow(loss_sample_u[c]).EuclideanNorm(), 2);
				complexity += RegI * Math.Pow(item_factors.GetRow(loss_sample_i[c]).EuclideanNorm(), 2);
				complexity += RegJ * Math.Pow(item_factors.GetRow(loss_sample_j[c]).EuclideanNorm(), 2);
				complexity += BiasReg * Math.Pow(item_bias[loss_sample_i[c]], 2);
				complexity += BiasReg * Math.Pow(item_bias[loss_sample_j[c]], 2);
			}

			return (float) (ranking_loss + 0.5 * complexity);
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			if (user_id > MaxUserID || item_id > MaxItemID)
				return float.MinValue;

			return item_bias[item_id] + DataType.MatrixExtensions.RowScalarProduct(user_factors, user_id, item_factors, item_id);
		}

		///
		public override void SaveModel(string file)
		{
			/*
				writer.WriteMatrix(user_factors);
				writer.WriteVector(item_bias);
				writer.WriteMatrix(item_factors);
			*/
		}

		///
		public override void LoadModel(string file)
		{
		}

		///
		public IList<Tuple<int, float>> ScoreItems(IList<int> accessed_items, IList<int> candidate_items)
		{
			var user_factors = FoldIn(accessed_items);

			var scored_items = new Tuple<int, float>[candidate_items.Count];
			for (int i = 0; i < scored_items.Length; i++)
			{
				int item_id = candidate_items[i];
				float score = item_bias[item_id] + item_factors.RowScalarProduct(item_id, user_factors);
				scored_items[i] = Tuple.Create(item_id, score);
			}
			return scored_items;
		}

		///
		float[] FoldIn(IList<int> accessed_items)
		{
			var positive_items = new HashSet<int>(accessed_items);

			// initialize user parameters
			var user_factors = new float[NumFactors];
			user_factors.InitNormal(InitMean, InitStdDev);

			// perform training
			for (uint it = 0; it < NumIter; it++)
				IterateUser(positive_items, user_factors);

			return user_factors;
		}

		void IterateUser(ISet<int> user_items, IList<float> user_factors)
		{
			var bpr_sampler = CreateBPRSampler();
			int num_pos_events = user_items.Count;

			for (int i = 0; i < num_pos_events; i++)
			{
				int pos_item_id, neg_item_id;
				bpr_sampler.ItemPair(user_items, out pos_item_id, out neg_item_id);

				// TODO generalize and call UpdateParameters -- need to represent factors as arrays, not matrices for this
				double x_uij = item_bias[pos_item_id] - item_bias[neg_item_id] + DataType.VectorExtensions.ScalarProduct(user_factors, DataType.MatrixExtensions.RowDifference(item_factors, pos_item_id, item_factors, neg_item_id));
				double one_over_one_plus_ex = 1 / (1 + Math.Exp(x_uij));

				// adjust factors
				for (int f = 0; f < num_factors; f++)
				{
					float w_uf = user_factors[f];
					float h_if = item_factors[pos_item_id, f];
					float h_jf = item_factors[neg_item_id, f];

					double update = (h_if - h_jf) * one_over_one_plus_ex - reg_u * w_uf;
					user_factors[f] = (float) (w_uf + learn_rate * update);
				}
			}
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} bias_reg={2} reg_u={3} reg_i={4} reg_j={5} num_iter={6} learn_rate={7} uniform_user_sampling={8} update_j={9}",
				this.GetType().Name, num_factors, BiasReg, reg_u, reg_i, reg_j, NumIter, learn_rate, UniformUserSampling, UpdateJ);
		}
	}
}
