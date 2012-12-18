// Copyright (C) 2010, 2011, 2012 Zeno Gantner
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
using System.Globalization;
using System.IO;
using System.Linq;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Eval;
using MyMediaLite.IO;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Baseline method for rating prediction</summary>
	/// <remarks>
	///   <para>
	///     Uses the average rating value, plus a regularized user and item bias
	///     for prediction.
	///   </para>
	///   <para>
	///     The method was described in section 2.1 of the paper below.
	///     One difference is that we support several iterations of alternating optimization,
	///     instead of just one.
	///   </para>
	///   <para>
	///     The optimization problem solved by the Train() method is the following:
	///     \f[
	///        \min_{\mathbf{a}, \mathbf{b}}
	///          \sum_{(u, i, r) \in R} (r - \mu_R - a_u - b_i)^2 + \lambda_1 \|\mathbf{a}\|^2 + \lambda_2 \|\mathbf{b}\|^2,
	///     \f]
	///    where \f$R\f$ are the known ratings, and
	///    \f$\lambda_1\f$ and \f$\lambda_2\f$ are the regularization constants <see cref="RegU">RegU</see> and <see cref="RegI">RegI</see>.
	///    The sum represents the least squares error, while the two terms starting with \f$\lambda_1\f$ and \f$\lambda_2\f$, respectively,
	///    are regularization terms that control the parameter sizes to avoid overfitting.
	///    The optimization problem is solved an alternating least squares method.
	///   </para>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         Yehuda Koren: Factor in the Neighbors: Scalable and Accurate Collaborative Filtering,
	///         Transactions on Knowledge Discovery from Data (TKDD), 2009.
	///         http://public.research.att.com/~volinsky/netflix/factorizedNeighborhood.pdf
	///       </description></item>
	///     </list>
	///   </para>
	///   <para>
	///     This recommender supports incremental updates.
	///   </para>
	/// </remarks>
	public class UserItemBaseline : IncrementalRatingPredictor, IIterativeModel
	{
		/// <summary>Regularization parameter for the user biases</summary>
		public float RegU { get; set; }

		/// <summary>Regularization parameter for the item biases</summary>
		public float RegI { get; set; }

		///
		public uint NumIter { get; set; }

		/// <summary>the global rating average</summary>
		protected float global_average;

		/// <summary>the user biases</summary>
		protected float[] user_biases;

		/// <summary>the item biases</summary>
		protected float[] item_biases;

		/// <summary>Default constructor</summary>
		public UserItemBaseline() : base()
		{
			RegU = 15;
			RegI = 10;
			NumIter = 10;
		}

		///
		public override void Train()
		{
			user_biases = new float[MaxUserID + 1];
			item_biases = new float[MaxItemID + 1];

			global_average = ratings.Average;

			for (uint i = 0; i < NumIter; i++)
				Iterate();
		}

		///
		public void Iterate()
		{
			OptimizeItemBiases();
			OptimizeUserBiases();
		}

		void OptimizeUserBiases()
		{
			int[] user_ratings_count = new int[MaxUserID + 1];
			for (int u = 0; u <= MaxUserID; u++)
				user_biases[u] = 0;

			for (int index = 0; index < ratings.Count; index++)
			{
				user_biases[ratings.Users[index]] += ratings[index] - global_average - item_biases[ratings.Items[index]];
				user_ratings_count[ratings.Users[index]]++;
			}
			for (int u = 0; u < user_biases.Length; u++)
				if (user_ratings_count[u] != 0)
					user_biases[u] = user_biases[u] / (RegU + user_ratings_count[u]);
		}

		void OptimizeItemBiases()
		{
			int[] item_ratings_count = new int[MaxItemID + 1];
			for (int i = 0; i <= MaxItemID; i++)
				item_biases[i] = 0;

			for (int index = 0; index < ratings.Count; index++)
			{
				item_biases[ratings.Items[index]] += ratings[index] - global_average - user_biases[ratings.Users[index]];
				item_ratings_count[ratings.Items[index]]++;
			}
			for (int i = 0; i < item_biases.Length; i++)
				if (item_ratings_count[i] != 0)
					item_biases[i] = item_biases[i] / (RegI + item_ratings_count[i]);
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			float user_bias = (user_id < user_biases.Length && user_id >= 0) ? user_biases[user_id] : 0;
			float item_bias = (item_id < item_biases.Length && item_id >= 0) ? item_biases[item_id] : 0;
			float result = global_average + user_bias + item_bias;

			if (result > MaxRating)
				return MaxRating;
			if (result < MinRating)
				return MinRating;
			return result;
		}

		///
		public virtual void RetrainUser(int user_id)
		{
			if (UpdateUsers)
			{
				foreach (int index in ratings.ByUser[user_id])
					user_biases[user_id] += ratings[index] - global_average - item_biases[ratings.Items[index]];
				if (ratings.ByUser[user_id].Count != 0)
					user_biases[user_id] = user_biases[user_id] / (RegU + ratings.ByUser[user_id].Count);
			}
		}

		///
		public virtual void RetrainItem(int item_id)
		{
			if (UpdateItems)
			{
				foreach (int index in ratings.ByItem[item_id])
					item_biases[item_id] += ratings[index] - global_average;
				if (ratings.ByItem[item_id].Count != 0)
					item_biases[item_id] = item_biases[item_id] / (RegI + ratings.ByItem[item_id].Count);
			}
		}

		///
		public override void AddRatings(IRatings ratings)
		{
			base.AddRatings(ratings);
			foreach (int user_id in ratings.AllUsers)
				RetrainUser(user_id);
			foreach (int item_id in ratings.AllItems)
				RetrainItem(item_id);
		}

		///
		public override void UpdateRatings(IRatings ratings)
		{
			base.UpdateRatings(ratings);
			foreach (int user_id in ratings.AllUsers)
				RetrainUser(user_id);
			foreach (int item_id in ratings.AllItems)
				RetrainItem(item_id);
		}

		///
		public override void RemoveRatings(IDataSet ratings)
		{
			base.RemoveRatings(ratings);
			foreach (int user_id in ratings.AllUsers)
				RetrainUser(user_id);
			foreach (int item_id in ratings.AllItems)
				RetrainItem(item_id);
		}

		///
		protected override void AddUser(int user_id)
		{
			base.AddUser(user_id);
			Array.Resize(ref user_biases, MaxUserID + 1);
		}

		///
		protected override void AddItem(int item_id)
		{
			base.AddItem(item_id);
			Array.Resize(ref item_biases, MaxItemID + 1);
		}

		///
		public override void SaveModel(string filename)
		{
			using ( StreamWriter writer = Model.GetWriter(filename, this.GetType(), "2.99") )
			{
				writer.WriteLine(global_average.ToString(CultureInfo.InvariantCulture));
				writer.WriteVector(user_biases);
				writer.WriteVector(item_biases);
			}
		}

		///
		public override void LoadModel(string filename)
		{
			using ( StreamReader reader = Model.GetReader(filename, this.GetType()) )
			{
				var global_average = float.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
				var user_biases = reader.ReadVector();
				var item_biases = reader.ReadVector();

				this.global_average = global_average;
				this.user_biases = user_biases.ToArray();
				this.item_biases = item_biases.ToArray();
			}
		}

		///
		public float ComputeObjective()
		{
			return (float) (
				Eval.Measures.RMSE.ComputeSquaredErrorSum(this, ratings)
				+ RegU * Math.Pow(user_biases.EuclideanNorm(), 2)
				+ RegI * Math.Pow(item_biases.EuclideanNorm(), 2));
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} reg_u={1} reg_i={2} num_iter={3}",
				this.GetType().Name, RegU, RegI, NumIter);
		}
	}
}
