// Copyright (C) 2011 Zeno Gantner
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

		IList<double> user_averages;
		IList<double> item_averages;
		IList<int> user_counts;
		IList<int> item_counts;

		IList<double> user_cluster_averages;
		IList<double> item_cluster_averages;
		double[,] cocluster_averages;

		double global_average;

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

		int[] InitIntArray(int length, int max_id) // TODO find better name
		{
			var array = new int[length];
			for (int i = 0; i < length; i++)
				array[i] = random.Next(max_id);
			return array;
		}

		void InitModel()
		{
			this.user_cluster_averages = new double[NumUserClusters];
			this.item_cluster_averages = new double[NumItemClusters];
			this.cocluster_averages    = new double[NumUserClusters, NumItemClusters];
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
			InitModel();
			ComputeAverages();
			random = Util.Random.GetInstance();
			user_clustering = InitIntArray(MaxUserID + 1, NumUserClusters);
			item_clustering = InitIntArray(MaxItemID + 1, NumItemClusters);

			for (uint i = 0; i < NumIter; i++)
				if (! IterateCheckModified())
					break;
		}

		///
		public override double Predict(int u, int i)
		{
			double prediction = Predict(u, i, user_clustering[u], item_clustering[i]);
			if (prediction < Ratings.MinRating)
				return Ratings.MinRating;
			if (prediction > Ratings.MaxRating)
				return Ratings.MaxRating;
			return prediction;
		}

		double Predict(int u, int i, int uc, int ic)
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
				foreach (int index in Ratings.ByUser[user_id])
				{
					int item_id   = Ratings.Items[index];
					double rating = Ratings[index];

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
				foreach (int index in Ratings.ByItem[item_id])
				{
					int user_id = Ratings.Users[index];
					double rating = Ratings[index];

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


			for (int i = 0; i < Ratings.Count; i++)
			{
				int user_id   = Ratings.Users[i];
				int item_id   = Ratings.Items[i];
				double rating = Ratings[i];

				user_sums[user_id] += rating;
				item_sums[item_id] += rating;
				sum                += rating;

				user_counts[user_id]++;
				item_counts[item_id]++;
			}

			this.global_average = sum / Ratings.Count;

			this.user_averages = new double[MaxUserID + 1];
			for (int u = 0; u <= MaxUserID; u++)
				if (user_counts[u] > 0)
					user_averages[u] = user_sums[u] / user_counts[u];
				else
					user_averages[u] = global_average;

			this.item_averages = new double[MaxItemID + 1];
			for (int i = 0; i <= MaxItemID; i++)
				if (item_counts[i] > 0)
					item_averages[i] = item_sums[i] / item_counts[i];
				else
					item_averages[i] = global_average;
		}

		void ComputeClusterAverages()
		{
			var user_cluster_counts = new int[NumUserClusters];
			var item_cluster_counts = new int[NumItemClusters];
			var cocluster_counts    = new int[NumUserClusters, NumItemClusters];

			for (int i = 0; i < Ratings.Count; i++)
			{
				int user_id = Ratings.Users[i];
				int item_id = Ratings.Items[i];
				double rating = Ratings[i];

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
			using ( StreamWriter writer = Model.GetWriter(filename, this.GetType()) )
			{
				writer.WriteLine(NumUserClusters);
				writer.WriteLine(NumItemClusters);
				writer.WriteVector(user_clustering);
				writer.WriteVector(item_clustering);
			}
		}

		///
		public override void LoadModel(string filename)
		{
			using ( StreamReader reader = Model.GetReader(filename, this.GetType()) )
			{
				var num_user_clusters = int.Parse(reader.ReadLine());
				var num_item_clusters = int.Parse(reader.ReadLine());

				var user_clustering = reader.ReadIntVector();
				var item_clustering = reader.ReadIntVector();

				this.MaxUserID = user_clustering.Count - 1;
				this.MaxItemID = item_clustering.Count - 1;

				// assign new model
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
				this.user_clustering = user_clustering;
				this.item_clustering = item_clustering;
				
				// create averages data structures
				this.user_cluster_averages = new double[NumUserClusters];
				this.item_cluster_averages = new double[NumItemClusters];
				this.cocluster_averages    = new double[NumUserClusters, NumItemClusters];
				
				// compute averages
				ComputeAverages();
				ComputeClusterAverages();
			}
		}

		///
		public double ComputeFit()
		{
			return this.Evaluate(ratings)["RMSE"];
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

