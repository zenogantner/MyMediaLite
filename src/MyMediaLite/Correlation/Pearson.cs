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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Taxonomy;

namespace MyMediaLite.Correlation
{
	/// <summary>Shrunk Pearson correlation for rating data</summary>
	/// <remarks>
	///   <para>
	///     The correlation values are shrunk towards zero, depending on the number of ratings the estimate is based on.
	///     Otherwise, we would give too much weight to similarities estimated from just a few examples.
	///   </para>
	///   <para>
	///     http://en.wikipedia.org/wiki/Pearson_correlation
	///   </para>
	///   <para>
	///     We apply shrinkage as in formula (5.16) of chapter 5 of the Recommender Systems Handbook.
	///     Note that the shrinkage formula has changed betweem the two publications.
	///     It is now based on the assumption that the true correlations are normally distributed;
	///     the shrunk estimate is the posterior mean of the empirical estimate.
	///   </para>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         Yehuda Koren: Factor in the Neighbors: Scalable and Accurate Collaborative Filtering,
	///         Transactions on Knowledge Discovery from Data (TKDD), 2009.
	///         http://public.research.att.com/~volinsky/netflix/factorizedNeighborhood.pdf
	///       </description></item>
	///       <item><description>
	///         Yehuda Koren, Robert Bell: Advances in Collaborative Filtering,
	///         Chapter 5 of the Recommender Systems Handbook, Springer, 2011.
	///         http://research.yahoo.net/files/korenBellChapterSpringer.pdf
	///       </description></item>
	///     </list>
	///   </para>
	/// </remarks>
	public sealed class Pearson : SymmetricCorrelationMatrix, IRatingCorrelationMatrix
	{
		/// <summary>shrinkage parameter, if set to 0 we have the standard Pearson correlation without shrinkage</summary>
		public float Shrinkage { get; set; }

		/// <summary>Constructor. Create a Pearson correlation matrix</summary>
		/// <param name="num_entities">the number of entities</param>
		/// <param name="shrinkage">shrinkage parameter</param>
		public Pearson(int num_entities, float shrinkage) : base(num_entities)
		{
			Shrinkage = shrinkage;
		}

		// TODO get rid of some code here

		///
		public float ComputeCorrelation(IRatings ratings, EntityType entity_type, int i, int j)
		{
			if (i == j)
				return 1;

			IList<int> indexes1 = (entity_type == EntityType.USER) ? ratings.ByUser[i] : ratings.ByItem[i];
			IList<int> indexes2 = (entity_type == EntityType.USER) ? ratings.ByUser[j] : ratings.ByItem[j];

			// get common ratings for the two entities
			var e1 = (entity_type == EntityType.USER) ? ratings.GetItems(indexes1) : ratings.GetUsers(indexes1);
			var e2 = (entity_type == EntityType.USER) ? ratings.GetItems(indexes2) : ratings.GetUsers(indexes2);

			e1.IntersectWith(e2);

			int n = e1.Count;
			if (n < 2)
				return 0;

			// single-pass variant
			double i_sum = 0;
			double j_sum = 0;
			double ij_sum = 0;
			double ii_sum = 0;
			double jj_sum = 0;
			foreach (int other_entity_id in e1)
			{
				// get ratings
				float r1 = 0;
				float r2 = 0;
				if (entity_type == EntityType.USER)
				{
					r1 = ratings.Get(i, other_entity_id, indexes1);
					r2 = ratings.Get(j, other_entity_id, indexes2);
				}
				else
				{
					r1 = ratings.Get(other_entity_id, i, indexes1);
					r2 = ratings.Get(other_entity_id, j, indexes2);
				}

				// update sums
				i_sum  += r1;
				j_sum  += r2;
				ij_sum += r1 * r2;
				ii_sum += r1 * r1;
				jj_sum += r2 * r2;
			}

			double denominator = Math.Sqrt( (n * ii_sum - i_sum * i_sum) * (n * jj_sum - j_sum * j_sum) );

			if (denominator == 0)
				return 0;
			double pmcc = (n * ij_sum - i_sum * j_sum) / denominator;

			return (float) pmcc * ((n - 1) / (n - 1 + Shrinkage));
		}

		///
		public float ComputeCorrelation(IRatings ratings, EntityType entity_type, IList<Tuple<int, float>> entity_ratings, int j)
		{
			IList<int> indexes2 = (entity_type == EntityType.USER) ? ratings.ByUser[j] : ratings.ByItem[j];

			// get common ratings for the two entities
			var e1 = new HashSet<int>(from pair in entity_ratings select pair.Item1);
			var e2 = (entity_type == EntityType.USER) ? ratings.GetItems(indexes2) : ratings.GetUsers(indexes2);

			e1.IntersectWith(e2);
			var ratings1 = new Dictionary<int, float>();
			for (int index = 0; index < entity_ratings.Count; index++)
				if (e1.Contains(entity_ratings[index].Item1))
					ratings1.Add(entity_ratings[index].Item1, entity_ratings[index].Item2);

			int n = e1.Count;
			if (n < 2)
				return 0;

			// single-pass variant
			double i_sum = 0;
			double j_sum = 0;
			double ij_sum = 0;
			double ii_sum = 0;
			double jj_sum = 0;
			foreach (int other_entity_id in e1)
			{
				// get ratings
				float r1 = ratings1[other_entity_id];
				float r2 = 0;
				if (entity_type == EntityType.USER)
					r2 = ratings.Get(j, other_entity_id, indexes2);
				else
					r2 = ratings.Get(other_entity_id, j, indexes2);

				// update sums
				i_sum  += r1;
				j_sum  += r2;
				ij_sum += r1 * r2;
				ii_sum += r1 * r1;
				jj_sum += r2 * r2;
			}

			double denominator = Math.Sqrt( (n * ii_sum - i_sum * i_sum) * (n * jj_sum - j_sum * j_sum) );

			if (denominator == 0)
				return 0;
			double pmcc = (n * ij_sum - i_sum * j_sum) / denominator;

			return (float) pmcc * ((n - 1) / (n - 1 + Shrinkage));
		}

		///
		public void ComputeCorrelations(IRatings ratings, EntityType entity_type)
		{
			int num_entities = (entity_type == EntityType.USER) ? ratings.MaxUserID + 1 : ratings.MaxItemID + 1;
			Resize(num_entities);

			if (entity_type != EntityType.USER && entity_type != EntityType.ITEM)
				throw new ArgumentException("entity type must be either USER or ITEM, not " + entity_type);

			IList<IList<int>> ratings_by_other_entity = (entity_type == EntityType.USER) ? ratings.ByItem : ratings.ByUser;

			var freqs   = new SymmetricMatrix<int>(num_entities);
			var i_sums  = new SymmetricMatrix<float>(num_entities);
			var j_sums  = new SymmetricMatrix<float>(num_entities);
			var ij_sums = new SymmetricMatrix<float>(num_entities);
			var ii_sums = new SymmetricMatrix<float>(num_entities);
			var jj_sums = new SymmetricMatrix<float>(num_entities);

			foreach (IList<int> other_entity_ratings in ratings_by_other_entity)
				for (int i = 0; i < other_entity_ratings.Count; i++)
				{
					var index1 = other_entity_ratings[i];
					int x = (entity_type == EntityType.USER) ? ratings.Users[index1] : ratings.Items[index1];

					// update pairwise scalar product and frequency
					for (int j = i + 1; j < other_entity_ratings.Count; j++)
					{
						var index2 = other_entity_ratings[j];
						int y = (entity_type == EntityType.USER) ? ratings.Users[index2] : ratings.Items[index2];

						float rating1 = (float) ratings[index1];
						float rating2 = (float) ratings[index2];

						// update sums
						freqs[x, y]   += 1;
						i_sums[x, y]  += rating1;
						j_sums[x, y]  += rating2;
						ij_sums[x, y] += rating1 * rating2;
						ii_sums[x, y] += rating1 * rating1;
						jj_sums[x, y] += rating2 * rating2;
					}
				}

			// the diagonal of the correlation matrix
			for (int i = 0; i < num_entities; i++)
				this[i, i] = 1;

			for (int i = 0; i < num_entities; i++)
				for (int j = i + 1; j < num_entities; j++)
				{
					int n = freqs[i, j];

					if (n < 2)
					{
						this[i, j] = 0;
						continue;
					}

					double numerator = ij_sums[i, j] * n - i_sums[i, j] * j_sums[i, j];

					double denominator = Math.Sqrt( (n * ii_sums[i, j] - i_sums[i, j] * i_sums[i, j]) * (n * jj_sums[i, j] - j_sums[i, j] * j_sums[i, j]) );
					if (denominator == 0)
					{
						this[i, j] = 0;
						continue;
					}

					double pmcc = numerator / denominator;
					this[i, j] = (float) (pmcc * ((n - 1) / (n - 1 + Shrinkage)));
				}
		}
	}
}