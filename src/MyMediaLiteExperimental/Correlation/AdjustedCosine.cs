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
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Taxonomy;

namespace MyMediaLite.Correlation
{
	/// <summary>Class for (shrunk) adjusted cosine similarity</summary>
	/// <remarks>
	/// Badrul Sarwar, George Karypis, Joseph Konstan, John Riedl:
	/// Item-based collaborative filtering recommendation algorithms.
	/// WWW 2001
	/// </remarks>
	public sealed class AdjustedCosine : RatingCorrelationMatrix
	{
		/// <summary>shrinkage parameter</summary>
		public float shrinkage = 10;

		/// <summary>Constructor. Create a AdjustedCosine matrix</summary>
		/// <param name="num_entities">the number of entities</param>
		public AdjustedCosine(int num_entities) : base(num_entities) { }

		/// <summary>Create a AdjustedCosine matrix from given data</summary>
		/// <param name="ratings">the ratings data</param>
		/// <param name="entity_type">the entity type, either USER or ITEM</param>
		/// <param name="shrinkage">a shrinkage parameter</param>
		/// <returns>the complete AdjustedCosine matrix</returns>
		static public CorrelationMatrix Create(IRatings ratings, EntityType entity_type, float shrinkage)
		{
			AdjustedCosine cm;
			int num_entities = 0;
			if (entity_type.Equals(EntityType.USER))
				num_entities = ratings.MaxUserID + 1;
			else if (entity_type.Equals(EntityType.ITEM))
				num_entities = ratings.MaxItemID + 1;
			else
				throw new ArgumentException("Unknown entity type: " + entity_type);

			try
			{
				cm = new AdjustedCosine(num_entities);
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
		public static float ComputeCorrelation(IRatings ratings, EntityType entity_type, int i, int j, float shrinkage)
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

			List<Ratings> ratings_by_other_entity = (entity_type == EntityType.USER) ? ratings.ByItem : ratings.ByUser;

			double sum_ij = 0;
			double sum_ii = 0;
			double sum_jj = 0;

			foreach (int other_entity_id in e1)
			{
				double average_rating = ratings_by_other_entity[other_entity_id].Average;

				// get ratings
				double r1 = 0;
				double r2 = 0;
				if (entity_type == EntityType.USER)
				{
					r1 = ratings.Get(i, other_entity_id, ratings1);
					r2 = ratings.Get(j, other_entity_id, ratings2);
				}
				else
				{
					r1 = ratings.Get(other_entity_id, i, ratings1);
					r2 = ratings.Get(other_entity_id, j, ratings2);
				}

				double dev_i = r1 - average_rating;
				double dev_j = r2 - average_rating;

				// update sums
				sum_ij += dev_i * dev_j;
				sum_ii += dev_i * dev_i;
				sum_jj += dev_j * dev_j;
			}

			double denominator = Math.Sqrt( sum_ii * sum_jj );

			if (denominator == 0)
				return 0;
			double adjusted_cosine = sum_ij / denominator;

			return (float) adjusted_cosine * (n / (n + shrinkage));
		}

		/// <summary>Compute correlations for given ratings</summary>
		/// <param name="ratings">the rating data</param>
		/// <param name="entity_type">the entity type, either USER or ITEM</param>
		public override void ComputeCorrelations(IRatings ratings, EntityType entity_type)
		{
			if (entity_type != EntityType.USER && entity_type != EntityType.ITEM)
				throw new ArgumentException("entity type must be either USER or ITEM, not " + entity_type);

			List<Ratings> ratings_by_other_entity = (entity_type == EntityType.USER) ? ratings.ByItem : ratings.ByUser;

			var freqs  = new SparseMatrix<int>(num_entities, num_entities);
			var sum_ij = new SparseMatrix<double>(num_entities, num_entities);
			var sum_ii = new SparseMatrix<double>(num_entities, num_entities);
			var sum_jj = new SparseMatrix<double>(num_entities, num_entities);

			foreach (var other_entity_ratings in ratings_by_other_entity)
			{
				double average_rating = other_entity_ratings.Average;

				for (int i = 0; i < other_entity_ratings.Count; i++)
				{
					var r1 = other_entity_ratings[i];
					int x = (entity_type == EntityType.USER) ? r1.user_id : r1.item_id;

					// update pairwise scalar product and frequency
	        		for (int j = i + 1; j < other_entity_ratings.Count; j++)
					{
						var r2 = other_entity_ratings[j];
						int y = (entity_type == EntityType.USER) ? r2.user_id : r2.item_id;

						// compute deviations from mean
						double dev_i = r1.rating - average_rating;
						double dev_j = r2.rating - average_rating;

						// update sums
						if (x < y)
						{
							freqs[x, y]  += 1;
							sum_ij[x, y] += dev_i * dev_j;
							sum_ii[x, y] += dev_i * dev_i;
							sum_jj[x, y] += dev_j * dev_j;
						}
						else
						{
							freqs[y, x]  += 1;
							sum_ij[y, x] += dev_i * dev_j;
							sum_ii[y, x] += dev_i * dev_i;
							sum_jj[y, x] += dev_j * dev_j;
						}
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

				double denominator = Math.Sqrt( sum_ii[i, j] * sum_jj[i, j] );
				if (denominator == 0)
				{
					this[i, j] = 0;
					continue;
				}

				double adjusted_cosine = sum_ij[i, j] / denominator;
				this[i, j] = (float) (adjusted_cosine * (n / (n + shrinkage)));
			}
		}
	}
}