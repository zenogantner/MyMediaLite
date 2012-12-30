// Copyright (C) 2011, 2012 Zeno Gantner
// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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
using System.Threading.Tasks;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.IO;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Matrix factorization with explicit user and item bias, learning is performed by stochastic gradient descent</summary>
	/// <remarks>
	///   <para>
	///     Per default optimizes for RMSE.
	///     Alternatively, you can set the Loss property to MAE or LogisticLoss.
	///     If set to log likelihood and with binary ratings, the recommender
	///     implements a simple version Menon and Elkan's LFL model,
	///     which predicts binary labels, has no advanced regularization, and uses no side information.
	///   </para>
	///   <para>
	///     This recommender makes use of multi-core machines if requested.
	///     Just set MaxThreads to a large enough number (usually multiples of the number of available cores).
	///     The parallelization is based on ideas presented in the paper by Gemulla et al.
	///   </para>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         Ruslan Salakhutdinov, Andriy Mnih:
	///         Probabilistic Matrix Factorization.
	///         NIPS 2007.
	///         http://www.mit.edu/~rsalakhu/papers/nips07_pmf.pdf
	///       </description></item>
	///       <item><description>
	///         Steffen Rendle, Lars Schmidt-Thieme:
	///         Online-Updating Regularized Kernel Matrix Factorization Models for Large-Scale Recommender Systems.
	///         RecSys 2008.
	///         http://www.ismll.uni-hildesheim.de/pub/pdfs/Rendle2008-Online_Updating_Regularized_Kernel_Matrix_Factorization_Models.pdf
	///       </description></item>
	///       <item><description>
	///         Aditya Krishna Menon, Charles Elkan:
	///         A log-linear model with latent features for dyadic prediction.
	///         ICDM 2010.
	///         http://cseweb.ucsd.edu/~akmenon/LFL-ICDM10.pdf
	///       </description></item>
	///       <item><description>
	///         Rainer Gemulla, Peter J. Haas, Erik Nijkamp, Yannis Sismanis:
	///         Large-Scale Matrix Factorization with Distributed Stochastic Gradient Descent.
	///         KDD 2011.
	///         http://www.mpi-inf.mpg.de/~rgemulla/publications/gemulla11dsgd.pdf
	///       </description></item>
	///     </list>
	///   </para>
	///   <para>
	///       This recommender supports incremental updates. See the paper by Rendle and Schmidt-Thieme.
	///   </para>
	/// </remarks>
	public class BiasedMatrixFactorization : MatrixFactorization
	{
		/// <summary>Index of the bias term in the user vector representation for fold-in</summary>
		protected const int FOLD_IN_BIAS_INDEX = 0;
		/// <summary>Start index of the user factors in the user vector representation for fold-in</summary>
		protected const int FOLD_IN_FACTORS_START = 1;

		/// <summary>Learn rate factor for the bias terms</summary>
		public float BiasLearnRate { get; set; }

		/// <summary>regularization factor for the bias terms</summary>
		public float BiasReg { get; set; }

		/// <summary>regularization constant for the user factors</summary>
		public float RegU { get; set; }

		/// <summary>regularization constant for the item factors</summary>
		public float RegI { get; set; }

		///
		public override float Regularization
		{
			set {
				base.Regularization = value;
				RegU = value;
				RegI = value;
			}
		}

		/// <summary>Regularization based on rating frequency</summary>
		/// <description>
		/// Regularization proportional to the inverse of the square root of the number of ratings associated with the user or item.
		/// As described in the paper by Menon and Elkan.
		/// </description>
		public bool FrequencyRegularization { get; set; }

		/// <summary>The optimization target</summary>
		public OptimizationTarget Loss { get; set; }

		/// <summary>the maximum number of threads to use</summary>
		/// <remarks>
		///   For parallel learning, set this number to a multiple of the number of available cores/CPUs
		/// </remarks>
		public int MaxThreads { get; set; }

		/// <summary>Use bold driver heuristics for learning rate adaption</summary>
		/// <remarks>
		/// Literature:
		/// <list type="bullet">
		///   <item><description>
		///     Rainer Gemulla, Peter J. Haas, Erik Nijkamp, Yannis Sismanis:
		///     Large-Scale Matrix Factorization with Distributed Stochastic Gradient Descent.
		///     KDD 2011.
		///     http://www.mpi-inf.mpg.de/~rgemulla/publications/gemulla11dsgd.pdf
		///   </description></item>
		/// </list>
		/// </remarks>
		public bool BoldDriver { get; set; }

		/// <summary>Use 'naive' parallelization strategy instead of conflict-free 'distributed' SGD</summary>
		/// <remarks>
		/// The exact sequence of updates depends on the thread scheduling.
		/// If you want reproducible results, e.g. when setting --random-seed=N, do NOT set this property.
		/// </remarks>
		public bool NaiveParallelization { get; set; }

		/// <summary>Loss for the last iteration, used by bold driver heuristics</summary>
		protected double last_loss = double.NegativeInfinity;

		/// <summary>the user biases</summary>
		protected internal float[] user_bias;
		/// <summary>the item biases</summary>
		protected internal float[] item_bias;

		/// <summary>size of the interval of valid ratings</summary>
		protected float rating_range_size;

		/// <summary>delegate to compute the common term of the error gradient</summary>
		protected Func<double, double, float> compute_gradient_common;

		IList<int>[,] thread_blocks;
		IList<IList<int>> thread_lists;

		/// <summary>Default constructor</summary>
		public BiasedMatrixFactorization() : base()
		{
			BiasReg = 0.01f;
			BiasLearnRate = 1.0f;
			MaxThreads = 1;
		}

		///
		protected internal override void InitModel()
		{
			base.InitModel();

			user_bias = new float[MaxUserID + 1];
			item_bias = new float[MaxItemID + 1];

			if (BoldDriver)
				last_loss = ComputeObjective();
		}

		///
		public override void Train()
		{
			InitModel();

			// if necessary, prepare stuff for parallel processing
			if (MaxThreads > 1)
			{
				if (NaiveParallelization)
					thread_lists = ratings.PartitionIndices(MaxThreads);
				else
					thread_blocks = ratings.PartitionUsersAndItems(MaxThreads);
			}

			rating_range_size = max_rating - min_rating;

			// compute global bias
			double avg = (ratings.Average - min_rating) / rating_range_size;
			global_bias = (float) Math.Log(avg / (1 - avg));

			for (int current_iter = 0; current_iter < NumIter; current_iter++)
				Iterate();
		}

		///
		public override void Iterate()
		{
			if (MaxThreads > 1)
			{
				if (NaiveParallelization)
				{
					Parallel.For(0, thread_lists.Count, i => Iterate(thread_lists[i], true, true));
				}
				else
				{
					int num_threads = thread_blocks.GetLength(0);

					// generate random sub-epoch sequence
					var subepoch_sequence = new List<int>(Enumerable.Range(0, num_threads));
					subepoch_sequence.Shuffle();

					foreach (int i in subepoch_sequence) // sub-epoch
						Parallel.For(0, num_threads, j => Iterate(thread_blocks[j, (i + j) % num_threads], true, true));
				}
				UpdateLearnRate(); // otherwise done in base.Iterate(), which is not called here
			}
			else
				base.Iterate();

			UpdateLearnRate();
		}

		///
		protected override void UpdateLearnRate()
		{
			if (BoldDriver)
			{
				double loss = ComputeObjective();

				if (loss > last_loss)
					current_learnrate *= 0.5f;
				else if (loss < last_loss)
					current_learnrate *= 1.05f;

				last_loss = loss;

				Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "objective {0} learn_rate {1} ", loss, current_learnrate));
			}
			else
			{
				current_learnrate *= Decay;
			}
		}

		/// <summary>Set up the common part of the error gradient of the loss function to optimize</summary>
		protected void SetupLoss()
		{
			switch (Loss)
			{
				case OptimizationTarget.MAE:
					compute_gradient_common = (sig_score, err) => (float) (Math.Sign(err) * sig_score * (1 - sig_score) * rating_range_size);
					break;
				case OptimizationTarget.RMSE:
					compute_gradient_common = (sig_score, err) => (float) (err * sig_score * (1 - sig_score) * rating_range_size);
					break;
				case OptimizationTarget.LogisticLoss:
					compute_gradient_common = (sig_score, err) => (float) err;
					break;
			}
		}

		///
		protected override void Iterate(IList<int> rating_indices, bool update_user, bool update_item)
		{
			SetupLoss();

			foreach (int index in rating_indices)
			{
				int u = ratings.Users[index];
				int i = ratings.Items[index];

				double score = global_bias + user_bias[u] + item_bias[i] + DataType.MatrixExtensions.RowScalarProduct(user_factors, u, item_factors, i);
				double sig_score = 1 / (1 + Math.Exp(-score));

				double prediction = min_rating + sig_score * rating_range_size;
				double err = ratings[index] - prediction;

				float gradient_common = compute_gradient_common(sig_score, err);

				float user_reg_weight = FrequencyRegularization ? (float) (RegU / Math.Sqrt(ratings.CountByUser[u])) : RegU;
				float item_reg_weight = FrequencyRegularization ? (float) (RegI / Math.Sqrt(ratings.CountByItem[i])) : RegI;

				// adjust biases
				if (update_user)
					user_bias[u] += BiasLearnRate * current_learnrate * (gradient_common - BiasReg * user_reg_weight * user_bias[u]);
				if (update_item)
					item_bias[i] += BiasLearnRate * current_learnrate * (gradient_common - BiasReg * item_reg_weight * item_bias[i]);

				// adjust latent factors
				for (int f = 0; f < NumFactors; f++)
				{
					double u_f = user_factors[u, f];
					double i_f = item_factors[i, f];

					if (update_user)
					{
						double delta_u = gradient_common * i_f - user_reg_weight * u_f;
						user_factors.Inc(u, f, current_learnrate * delta_u);
						// this is faster (190 vs. 260 seconds per iteration on Netflix w/ k=30) than
						//    user_factors[u, f] += learn_rate * delta_u;
					}
					if (update_item)
					{
						double delta_i = gradient_common * u_f - item_reg_weight * i_f;
						item_factors.Inc(i, f, current_learnrate * delta_i);
					}
				}
			}
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			double score = global_bias;

			if (user_id < user_bias.Length)
				score += user_bias[user_id];
			if (item_id < item_bias.Length)
				score += item_bias[item_id];
			if (user_id < user_factors.dim1 && item_id < item_factors.dim1)
				score += DataType.MatrixExtensions.RowScalarProduct(user_factors, user_id, item_factors, item_id);

			return (float) (min_rating + ( 1 / (1 + Math.Exp(-score)) ) * rating_range_size);
		}

		///
		protected override float Predict(float[] user_vector, int item_id)
		{
			var user_factors = new float[NumFactors];
			Array.Copy(user_vector, FOLD_IN_FACTORS_START, user_factors, 0, NumFactors);
			double score = global_bias + user_vector[FOLD_IN_BIAS_INDEX];
			if (item_id < item_factors.dim1)
				score += item_bias[item_id] + DataType.MatrixExtensions.RowScalarProduct(item_factors, item_id, user_factors);
			return (float) (min_rating + 1 / (1 + Math.Exp(-score)) * rating_range_size);
		}

		///
		public override void SaveModel(string filename)
		{
			using ( StreamWriter writer = Model.GetWriter(filename, this.GetType(), "2.99") )
			{
				writer.WriteLine(global_bias.ToString(CultureInfo.InvariantCulture));
				writer.WriteLine(min_rating.ToString(CultureInfo.InvariantCulture));
				writer.WriteLine(max_rating.ToString(CultureInfo.InvariantCulture));
				writer.WriteVector(user_bias);
				writer.WriteMatrix(user_factors);
				writer.WriteVector(item_bias);
				writer.WriteMatrix(item_factors);
			}
		}

		///
		public override void LoadModel(string filename)
		{
			using ( StreamReader reader = Model.GetReader(filename, this.GetType()) )
			{
				var bias       = float.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
				var min_rating = float.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
				var max_rating = float.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);

				var user_bias = reader.ReadVector();
				var user_factors = (Matrix<float>) reader.ReadMatrix(new Matrix<float>(0, 0));
				var item_bias = reader.ReadVector();
				var item_factors = (Matrix<float>) reader.ReadMatrix(new Matrix<float>(0, 0));

				if (user_factors.dim2 != item_factors.dim2)
					throw new IOException(
						string.Format(
							"Number of user and item factors must match: {0} != {1}",
							user_factors.dim2, item_factors.dim2));
				if (user_bias.Count != user_factors.dim1)
					throw new IOException(
						string.Format(
							"Number of users must be the same for biases and factors: {0} != {1}",
							user_bias.Count, user_factors.dim1));
				if (item_bias.Count != item_factors.dim1)
					throw new IOException(
						string.Format(
							"Number of items must be the same for biases and factors: {0} != {1}",
							item_bias.Count, item_factors.dim1));

				this.MaxUserID = user_factors.dim1 - 1;
				this.MaxItemID = item_factors.dim1 - 1;

				// assign new model
				this.global_bias = bias;
				if (this.NumFactors != user_factors.dim2)
				{
					Console.Error.WriteLine("Set NumFactors to {0}", user_factors.dim2);
					this.NumFactors = (uint) user_factors.dim2;
				}
				this.user_factors = user_factors;
				this.item_factors = item_factors;
				this.user_bias = user_bias.ToArray();
				this.item_bias = item_bias.ToArray();
				this.min_rating = min_rating;
				this.max_rating = max_rating;

				rating_range_size = max_rating - min_rating;
			}
		}

		///
		protected override void AddUser(int user_id)
		{
			base.AddUser(user_id);
			Array.Resize(ref user_bias, MaxUserID + 1);
		}

		///
		protected override void AddItem(int item_id)
		{
			base.AddItem(item_id);
			Array.Resize(ref item_bias, MaxItemID + 1);
		}

		///
		public override void RetrainUser(int user_id)
		{
			user_bias[user_id] = 0;
			base.RetrainUser(user_id);
		}

		///
		public override void RetrainItem(int item_id)
		{
			item_bias[item_id] = 0;
			base.RetrainItem(item_id);
		}

		///
		public override void RemoveUser(int user_id)
		{
			user_bias[user_id] = 0;
			base.RemoveUser(user_id);
		}

		///
		public override void RemoveItem(int item_id)
		{
			item_bias[item_id] = 0;
			base.RemoveItem(item_id);
		}

		///
		protected override float[] FoldIn(IList<Tuple<int, float>> rated_items)
		{
			SetupLoss();

			// initialize user parameters
			float user_bias = 0;
			var factors = new float[NumFactors];
			factors.InitNormal(InitMean, InitStdDev);

			float reg_weight = FrequencyRegularization ? (float) (RegU / Math.Sqrt(rated_items.Count)) : RegU;

			// perform training
			rated_items.Shuffle();
			for (uint it = 0; it < NumIter; it++)
				for (int index = 0; index < rated_items.Count; index++)
				{
					int item_id = rated_items[index].Item1;

					// compute rating and error
					double score = global_bias + user_bias + item_bias[item_id] + DataType.MatrixExtensions.RowScalarProduct(item_factors, item_id, factors);
					double sig_score = 1 / (1 + Math.Exp(-score));
					double prediction = min_rating + sig_score * rating_range_size;
					double err = rated_items[index].Item2 - prediction;

					float gradient_common = compute_gradient_common(sig_score, err);

					// adjust bias
					user_bias += BiasLearnRate * LearnRate * (gradient_common - BiasReg * reg_weight * user_bias);

					// adjust factors
					for (int f = 0; f < NumFactors; f++)
					{
						float u_f = factors[f];
						float i_f = item_factors[item_id, f];

						double delta_u = gradient_common * i_f - reg_weight * u_f;
						factors[f] += (float) (LearnRate * delta_u);
					}
				}

			var user_vector = new float[NumFactors + 1];
			user_vector[FOLD_IN_BIAS_INDEX] = user_bias;
			Array.Copy(factors, 0, user_vector, FOLD_IN_FACTORS_START, NumFactors);

			return user_vector;
		}

		/// <summary>Computes the value of the loss function that is currently being optimized</summary>
		/// <returns>the loss</returns>
		protected double ComputeLoss()
		{
			double loss = 0;
			switch (Loss)
			{
				case OptimizationTarget.MAE:
					loss += Eval.Measures.MAE.ComputeAbsoluteErrorSum(this, ratings);
					break;
				case OptimizationTarget.RMSE:
					loss += Eval.Measures.RMSE.ComputeSquaredErrorSum(this, ratings);
					break;
				case OptimizationTarget.LogisticLoss:
					loss += Eval.Measures.LogisticLoss.ComputeSum(this, ratings, min_rating, rating_range_size);
					break;
			}
			return loss;
		}

		///
		public override float ComputeObjective()
		{
			double complexity = 0;
			if (FrequencyRegularization)
			{
				for (int u = 0; u <= MaxUserID; u++)
				{
					if (ratings.CountByUser[u] > 0)
					{
						complexity += (RegU / Math.Sqrt(ratings.CountByUser[u]))           * Math.Pow(user_factors.GetRow(u).EuclideanNorm(), 2);
						complexity += (RegU / Math.Sqrt(ratings.CountByUser[u])) * BiasReg * Math.Pow(user_bias[u], 2);
					}
				}
				for (int i = 0; i <= MaxItemID; i++)
				{
					if (ratings.CountByItem[i] > 0)
					{
						complexity += (RegI / Math.Sqrt(ratings.CountByItem[i]))           * Math.Pow(item_factors.GetRow(i).EuclideanNorm(), 2);
						complexity += (RegI / Math.Sqrt(ratings.CountByItem[i])) * BiasReg * Math.Pow(item_bias[i], 2);
					}
				}
			}
			else
			{
				for (int u = 0; u <= MaxUserID; u++)
				{
					complexity += ratings.CountByUser[u] * RegU * Math.Pow(user_factors.GetRow(u).EuclideanNorm(), 2);
					complexity += ratings.CountByUser[u] * RegU * BiasReg * Math.Pow(user_bias[u], 2);
				}
				for (int i = 0; i <= MaxItemID; i++)
				{
					complexity += ratings.CountByItem[i] * RegI * Math.Pow(item_factors.GetRow(i).EuclideanNorm(), 2);
					complexity += ratings.CountByItem[i] * RegI * BiasReg * Math.Pow(item_bias[i], 2);
				}
			}

			return (float) (ComputeLoss() + complexity);
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} bias_reg={2} reg_u={3} reg_i={4} frequency_regularization={5} learn_rate={6} bias_learn_rate={7} learn_rate_decay={8} num_iter={9} bold_driver={10} loss={11} max_threads={12} naive_parallelization={13}",
				this.GetType().Name, NumFactors, BiasReg, RegU, RegI, FrequencyRegularization, LearnRate, BiasLearnRate, Decay, NumIter, BoldDriver, Loss, MaxThreads, NaiveParallelization);
		}
	}
}
