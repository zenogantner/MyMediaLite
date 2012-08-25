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
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MyMediaLite.DataType;
using MyMediaLite.Eval;
using MyMediaLite.IO;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Co-clustering for rating prediction</summary>
	/// <remarks>
	/// Literature:
	/// <list type="bullet">
	///   <item><description>
	///     Thomas George, Srujana Merugu
	///     A Scalable Collaborative Filtering Framework based on Co-clustering.
	///     ICDM 2005.
	///     http://hercules.ece.utexas.edu/~srujana/papers/icdm05.pdf
	///   </description></item>
	/// </list>
	///
	/// This recommender does NOT support incremental updates.
	/// </remarks>
	public class CoClustering : RatingPredictor, IIterativeModel
	{
		/// <summary>Random number generator</summary>
		System.Random random;

		IList<int> user_clustering;
		IList<int> item_clustering;

		IList<float> user_averages;
		IList<float> item_averages;
		IList<int> user_counts;
		IList<int> item_counts;

		IList<float> user_cluster_averages;
		IList<float> item_cluster_averages;
		IMatrix<float> cocluster_averages;

		float global_average;

		/// <summary>The number of user clusters</summary>
		public int NumUserClusters { get; set; }

		/// <summary>The number of item clusters</summary>
		public int NumItemClusters { get; set; }

		/// <summary>The maximum number of iterations</summary>
		/// <remarks>If the algorithm converges to a stable solution, it will terminate earlier.</remarks>
		public uint NumIter { get; set; }

		/// <summary>Default constructor</summary>
		public CoClustering()
		{
			NumUserClusters = 3;
			NumItemClusters = 3;
			NumIter = 30;
		}

		void InitModel()
		{
			this.user_clustering = new int[MaxUserID + 1];
			this.item_clustering = new int[MaxItemID + 1];

			this.user_cluster_averages = new float[NumUserClusters];
			this.item_cluster_averages = new float[NumItemClusters];
			this.cocluster_averages    = new Matrix<float>(NumUserClusters, NumItemClusters);
		}

		bool IterateCheckModified()
		{
			bool clustering_modified = false;
			ComputeClusterAverages();

			// dimension users
			for (int u = 0; u < MaxUserID; u++)
				if (FindOptimalUserClustering(u))
					clustering_modified = true;

			// dimension items
			for (int i = 0; i < MaxItemID; i++)
				if (FindOptimalItemClustering(i))
					clustering_modified = true;

			return clustering_modified;
		}

		///
		public void Iterate()
		{
			IterateCheckModified();
		}

		///
		public override void Train()
		{
			random = MyMediaLite.Random.GetInstance();

			InitModel();
			for (int i = 0; i < user_clustering.Count; i++)
				user_clustering[i] = random.Next(NumUserClusters);
			for (int i = 0; i < item_clustering.Count; i++)
				item_clustering[i] = random.Next(NumItemClusters);

			ComputeAverages();

			for (uint i = 0; i < NumIter; i++)
				if (! IterateCheckModified())
					break;
		}

		///
		public override float Predict(int u, int i)
		{
			if (u > MaxUserID && i > MaxItemID)
				return global_average;
			if (u > MaxUserID)
				return item_cluster_averages[item_clustering[i]];
			if (i > MaxItemID)
				return user_cluster_averages[user_clustering[u]];

			float prediction = Predict(u, i, user_clustering[u], item_clustering[i]);
			if (prediction < MinRating)
				return MinRating;
			if (prediction > MaxRating)
				return MaxRating;
			return prediction;
		}

		float Predict(int u, int i, int uc, int ic)
		{
			return cocluster_averages[uc, ic]
				+ user_averages[u]
				- user_cluster_averages[uc]
				+ item_averages[i]
				- item_cluster_averages[ic];
		}

		bool FindOptimalUserClustering(int user_id)
		{
			bool modified = false;

			double[] errors = new double[NumUserClusters];
			for (int uc = 0; uc < NumUserClusters; uc++)
				foreach (int index in ratings.ByUser[user_id])
				{
					int item_id   = ratings.Items[index];
					double rating = ratings[index];

					errors[uc] += Math.Pow(rating - Predict(user_id, item_id, uc, item_clustering[item_id]), 2);
				}

			int minimum_index = GetMinimumIndex(errors, user_clustering[user_id]);
			if (minimum_index != user_clustering[user_id])
			{
				user_clustering[user_id] = minimum_index;
				modified = true;
			}

			return modified;
		}

		bool FindOptimalItemClustering(int item_id)
		{
			bool modified = false;

			double[] errors = new double[NumItemClusters];
			for (int ic = 0; ic < NumItemClusters; ic++)
				foreach (int index in ratings.ByItem[item_id])
				{
					int user_id = ratings.Users[index];
					double rating = ratings[index];

					errors[ic] += Math.Pow(rating - Predict(user_id, item_id, user_clustering[user_id], ic), 2);
				}

			int minimum_index = GetMinimumIndex(errors, item_clustering[item_id]);
			if (minimum_index != item_clustering[item_id])
			{
				item_clustering[item_id] = minimum_index;
				modified = true;
			}

			return modified;
		}

		void ComputeAverages()
		{
			var user_sums = new double[MaxUserID + 1];
			var item_sums = new double[MaxItemID + 1];
			double sum = 0;

			this.user_counts = new int[MaxUserID + 1];
			this.item_counts = new int[MaxItemID + 1];


			for (int i = 0; i < ratings.Count; i++)
			{
				int user_id   = ratings.Users[i];
				int item_id   = ratings.Items[i];
				double rating = ratings[i];

				user_sums[user_id] += rating;
				item_sums[item_id] += rating;
				sum                += rating;

				user_counts[user_id]++;
				item_counts[item_id]++;
			}

			this.global_average = (float) sum / ratings.Count;

			this.user_averages = new float[MaxUserID + 1];
			for (int u = 0; u <= MaxUserID; u++)
				if (user_counts[u] > 0)
					user_averages[u] = (float) (user_sums[u] / user_counts[u]);
				else
					user_averages[u] = global_average;

			this.item_averages = new float[MaxItemID + 1];
			for (int i = 0; i <= MaxItemID; i++)
				if (item_counts[i] > 0)
					item_averages[i] = (float) (item_sums[i] / item_counts[i]);
				else
					item_averages[i] = global_average;
		}

		void ComputeClusterAverages()
		{
			var user_cluster_counts = new int[NumUserClusters];
			var item_cluster_counts = new int[NumItemClusters];
			var cocluster_counts    = new int[NumUserClusters, NumItemClusters];

			for (int i = 0; i < ratings.Count; i++)
			{
				int user_id = ratings.Users[i];
				int item_id = ratings.Items[i];
				float rating = ratings[i];

				user_cluster_averages[user_clustering[user_id]] += rating;
				item_cluster_averages[item_clustering[item_id]] += rating;
				cocluster_averages[user_clustering[user_id], item_clustering[item_id]] += rating;

				user_cluster_counts[user_clustering[user_id]]++;
				item_cluster_counts[item_clustering[item_id]]++;
				cocluster_counts[user_clustering[user_id], item_clustering[item_id]]++;
			}

			for (int i = 0; i < NumUserClusters; i++)
				if (user_cluster_counts[i] > 0)
					user_cluster_averages[i] /= user_cluster_counts[i];
				else
					user_cluster_averages[i] = global_average;
			for (int i = 0; i < NumItemClusters; i++)
				if (item_cluster_counts[i] > 0)
					item_cluster_averages[i] /= item_cluster_counts[i];
				else
					item_cluster_averages[i] = global_average;

			for (int i = 0; i < NumUserClusters; i++)
				for (int j = 0; j < NumItemClusters; j++)
					if (cocluster_counts[i, j] > 0)
						cocluster_averages[i, j] /= cocluster_counts[i, j];
					else
						cocluster_averages[i, j] = global_average;
		}

		int GetMinimumIndex(double[] array, int default_index)
		{
			int minimumIndex = default_index;

			for (int i = 0; i < array.Length; i++)
				if (array[i] < array[minimumIndex])
					minimumIndex = i;

			return minimumIndex;
		}

		///
		public override void SaveModel(string filename)
		{
			using ( StreamWriter writer = Model.GetWriter(filename, this.GetType(), "2.99") )
			{
				writer.WriteVector(user_clustering);
				writer.WriteVector(item_clustering);
				writer.WriteLine(global_average.ToString(CultureInfo.InvariantCulture));
				writer.WriteVector(user_averages);
				writer.WriteVector(item_averages);
				writer.WriteVector(user_cluster_averages);
				writer.WriteVector(item_cluster_averages);
				writer.WriteMatrix(cocluster_averages);
			}
		}

		///
		public override void LoadModel(string filename)
		{
			using ( StreamReader reader = Model.GetReader(filename, this.GetType()) )
			{
				var user_clustering = reader.ReadIntVector();
				var item_clustering = reader.ReadIntVector();
				float global_average = float.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
				var user_averages = reader.ReadVector();
				var item_averages = reader.ReadVector();
				var user_cluster_averages = reader.ReadVector();
				var item_cluster_averages = reader.ReadVector();
				var cocluster_averages = reader.ReadMatrix(new Matrix<float>(0, 0));

				int num_user_clusters = user_cluster_averages.Count;
				int num_item_clusters = item_cluster_averages.Count;

				// adjust maximum IDs
				this.MaxUserID = user_clustering.Count - 1;
				this.MaxItemID = item_clustering.Count - 1;

				// adjust hyperparameters
				if (this.NumUserClusters != num_user_clusters)
				{
					Console.Error.WriteLine("Set num_user_clusters to {0}", num_user_clusters);
					this.NumUserClusters = num_user_clusters;
				}
				if (this.NumItemClusters != num_item_clusters)
				{
					Console.Error.WriteLine("Set num_item_clusters to {0}", num_item_clusters);
					this.NumItemClusters = num_item_clusters;
				}
				// assign model
				this.global_average = global_average;
				this.user_cluster_averages = user_cluster_averages;
				this.item_cluster_averages = item_cluster_averages;
				this.cocluster_averages = cocluster_averages;
				this.user_averages = user_averages;
				this.item_averages = item_averages;
				this.user_clustering = user_clustering;
				this.item_clustering = item_clustering;
			}
		}

		///
		public float ComputeObjective()
		{
			return (float) Eval.Measures.RMSE.ComputeSquaredErrorSum(this, ratings);
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_user_clusters={1} num_item_clusters={2} num_iter={3}",
				this.GetType().Name, NumUserClusters, NumItemClusters, NumIter);
		}
	}
}
