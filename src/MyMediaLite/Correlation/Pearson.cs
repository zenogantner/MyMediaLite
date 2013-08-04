// Copyright (C) 2010, 2011, 2012, 2013 Zeno Gantner
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

		/// <summary>Create a Pearson correlation matrix</summary>
		/// <param name="num_entities">the number of entities</param>
		/// <param name="shrinkage">shrinkage parameter</param>
		public Pearson(int num_entities, float shrinkage) : base(num_entities)
		{
			Shrinkage = shrinkage;
		}

		///
		public float ComputeCorrelation(IInteractions ratings, EntityType entity_type, int i, int j)
		{
			if (i == j)
				return 1;

			// TODO move into its own method to allow reuse ...
			var entity_ratings = new List<Tuple<int, float>>();
			IInteractionReader reader = (entity_type == EntityType.USER) ? ratings.ByUser(i) : ratings.ByItem(i);
			while (reader.Read())
			{
				if (entity_type == EntityType.USER)
					entity_ratings.Add(new Tuple<int, float>(reader.GetItem(), reader.GetRating()));
				else if (entity_type == EntityType.ITEM)
				{
					var u = reader.GetUser();
					var r = reader.GetRating();
					entity_ratings.Add(new Tuple<int, float>(u, r));
				}
				else
					throw new ArgumentException(string.Format("Only USER and ITEM are supported, but not {0}.", entity_type), "entity_type");
			}
			return ComputeCorrelation(ratings, entity_type, entity_ratings, j);
		}

		///
		public float ComputeCorrelation(IInteractions ratings, EntityType entity_type, IList<Tuple<int, float>> entity_ratings, int j)
		{
			IInteractionReader reader = (entity_type == EntityType.USER) ? ratings.ByUser(j) : ratings.ByItem(j);

			// get common ratings for the two entities
			var e1 = new HashSet<int>(from pair in entity_ratings select pair.Item1);
			var e2 = (entity_type == EntityType.USER) ? reader.Items : reader.Users;

			e1.IntersectWith(e2);
			var ratings1 = new Dictionary<int, float>();
			for (int index = 0; index < entity_ratings.Count; index++)
				if (e1.Contains(entity_ratings[index].Item1))
					ratings1.Add(entity_ratings[index].Item1, entity_ratings[index].Item2);

			int n = e1.Count;
			if (n < 2)
				return 0;

			double i_sum = 0;
			double j_sum = 0;
			double ij_sum = 0;
			double ii_sum = 0;
			double jj_sum = 0;
			while (reader.Read())
			{
				int entity_id = (entity_type == EntityType.USER) ? reader.GetItem() : reader.GetUser();
				if (!ratings1.ContainsKey(entity_id))
					continue;

				// get ratings
				float r1 = ratings1[entity_id];
				float r2 = reader.GetRating();

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
		public override void ComputeCorrelations(IInteractions ratings, EntityType entity_type)
		{
			int num_entities = (entity_type == EntityType.USER) ? ratings.MaxUserID + 1 : ratings.MaxItemID + 1;
			Resize(num_entities);

			if (entity_type != EntityType.USER && entity_type != EntityType.ITEM)
				throw new ArgumentException("entity type must be either USER or ITEM, not " + entity_type);

			// TODO: speed up
			for (int i = 0; i < num_entities; i++)
				for (int j = i + 1; j < num_entities; j++)
					this[i, j] = ComputeCorrelation(ratings, entity_type, i, j);
		}
	}
}