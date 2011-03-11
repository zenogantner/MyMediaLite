// Copyright (C) 2010, 2011 Zeno Gantner
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
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Taxonomy;

namespace MyMediaLite.Correlation
{
	/// <summary>Correlation class for Pearson correlation</summary>
	/// <remarks>
	/// http://en.wikipedia.org/wiki/Pearson_correlation
	/// </remarks>
	public sealed class Pearson : RatingCorrelationMatrix
	{
		/// <summary>shrinkage parameter</summary>
		public float shrinkage = 10;

		/// <summary>Constructor. Create a Pearson correlation matrix</summary>
		/// <param name="num_entities">the number of entities</param>
		public Pearson(int num_entities) : base(num_entities) { }

		/// <summary>Create a Pearson correlation matrix from given data</summary>
		/// <param name="ratings">the ratings data</param>
		/// <param name="entity_type">the entity type, either USER or ITEM</param>
		/// <param name="shrinkage">a shrinkage parameter</param>
		/// <returns>the complete Pearson correlation matrix</returns>
		static public CorrelationMatrix Create(Ratings ratings, EntityType entity_type, float shrinkage)
		{
			Pearson cm;
			int num_entities = 0;
			if (entity_type.Equals(EntityType.USER))
				num_entities = ratings.MaxUserID + 1;
			else if (entity_type.Equals(EntityType.ITEM))
				num_entities = ratings.MaxItemID + 1;
			else
				throw new ArgumentException("Unknown entity type: " + entity_type);

			try
			{
				cm = new Pearson(num_entities);
			}
			catch (OverflowException)
			{
				Console.Error.WriteLine("Too many entities: " + num_entities);
				throw;
			}
			cm.shrinkage = shrinkage;
			cm.ComputeCorrelations(ratings, entity_type);
			return cm;
		}

		/// <summary>Compute correlations between two entities for given ratings</summary>
		/// <param name="ratings">the rating data</param>
		/// <param name="entity_type">the entity type, either USER or ITEM</param>
		/// <param name="i">the ID of first entity</param>
		/// <param name="j">the ID of second entity</param>
		/// <param name="shrinkage">the shrinkage parameter</param>
		public static float ComputeCorrelation(Ratings ratings, EntityType entity_type, int i, int j, float shrinkage)
		{
			if (i == j)
				return 1;

			IList<int> ratings1 = (entity_type == EntityType.USER) ? ratings.ByUser[i] : ratings.ByItem[i];
			IList<int> ratings2 = (entity_type == EntityType.USER) ? ratings.ByUser[j] : ratings.ByItem[j];

			// get common ratings for the two entities
			HashSet<int> e1 = (entity_type == EntityType.USER) ? ratings.GetItems(ratings1) : ratings.GetUsers(ratings1);
			HashSet<int> e2 = (entity_type == EntityType.USER) ? ratings.GetItems(ratings2) : ratings.GetUsers(ratings2);

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
				double r1 = 0;
				double r2 = 0;
				if (entity_type == EntityType.USER)
				{
					r1 = ratings.FindRating(i, other_entity_id, ratings1);
					r2 = ratings.FindRating(j, other_entity_id, ratings2);
				}
				else
				{
					r1 = ratings.FindRating(other_entity_id, i, ratings1);
					r2 = ratings.FindRating(other_entity_id, j, ratings2);
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

			return (float) pmcc * (n / (n + shrinkage));
		}

		/// <summary>Compute correlations for given ratings</summary>
		/// <param name="ratings">the rating data</param>
		/// <param name="entity_type">the entity type, either USER or ITEM</param>
		public override void ComputeCorrelations(Ratings ratings, EntityType entity_type)
		{
			if (entity_type != EntityType.USER && entity_type != EntityType.ITEM)
				throw new ArgumentException("entity type must be either USER or ITEM, not " + entity_type);

			IList<IList<int>> ratings_by_other_entity = (entity_type == EntityType.USER) ? ratings.ByItem : ratings.ByUser;

			var freqs   = new SparseMatrix<int>(num_entities, num_entities);
			var i_sums  = new SparseMatrix<double>(num_entities, num_entities);
			var j_sums  = new SparseMatrix<double>(num_entities, num_entities);
			var ij_sums = new SparseMatrix<double>(num_entities, num_entities);
			var ii_sums = new SparseMatrix<double>(num_entities, num_entities);
			var jj_sums = new SparseMatrix<double>(num_entities, num_entities);

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

						double rating1 = ratings[index1];
						double rating2 = ratings[index2];

						// update sums
						if (x < y)
						{
							freqs[x, y]   += 1;
							i_sums[x, y]  += rating1;
							j_sums[x, y]  += rating2;
							ij_sums[x, y] += rating1 * rating2;
							ii_sums[x, y] += rating1 * rating1;
							jj_sums[x, y] += rating2 * rating2;
						}
						else
						{
							freqs[y, x]   += 1;
							i_sums[y, x]  += rating1;
							j_sums[y, x]  += rating2;
							ij_sums[y, x] += rating1 * rating2;
							ii_sums[y, x] += rating1 * rating1;
							jj_sums[y, x] += rating2 * rating2;
						}
	        		}
				}

			// the diagonal of the correlation matrix
			for (int i = 0; i < num_entities; i++)
				this[i, i] = 1;

			// fill the entries with interactions
			foreach (var index_pair in freqs.NonEmptyEntryIDs)
			{
				int i = index_pair.First;
				int j = index_pair.Second;
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
				this[i, j] = (float) (pmcc * (n / (n + shrinkage)));
			}
		}
	}
}