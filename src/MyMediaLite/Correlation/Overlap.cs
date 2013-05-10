// Copyright (C) 2012, 2013 Zeno Gantner
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
using System.Linq;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Taxonomy;

namespace MyMediaLite.Correlation
{
	/// <summary>Class containing routines for computing overlaps</summary>
	public static class Overlap
	{
		/// <summary>Compute the overlap between the vectors in a binary matrix</summary>
		/// <returns>a sparse matrix with the overlaps</returns>
		/// <param name='entity_data'>the binary matrix</param>
		public static Tuple<IMatrix<float>, IList<float>> ComputeWeighted(IBooleanMatrix entity_data)
		{
			var transpose = (IBooleanMatrix) entity_data.Transpose();

			var other_entity_weights = new float[transpose.NumberOfRows];
			for (int row_id = 0; row_id < transpose.NumberOfRows; row_id++)
			{
				int freq = transpose.GetEntriesByRow(row_id).Count;
				other_entity_weights[row_id] = 1f / (float) Math.Log(3 + freq, 2); // TODO make configurable
			}

			IMatrix<float> weighted_overlap = new SymmetricMatrix<float>(entity_data.NumberOfRows);
			IList<float> entity_weights = new float[entity_data.NumberOfRows];

			// go over all (other) entities
			for (int row_id = 0; row_id < transpose.NumberOfRows; row_id++)
			{
				var row = transpose.GetEntriesByRow(row_id);
				for (int i = 0; i < row.Count; i++)
				{
					int x = row[i];
					entity_weights[x] += other_entity_weights[row_id];
					for (int j = i + 1; j < row.Count; j++)
					{
						int y = row[j];
						weighted_overlap[x, y] += other_entity_weights[row_id] * other_entity_weights[row_id];
					}
				}
			}

			return Tuple.Create(weighted_overlap, entity_weights);
		}

		/// <summary>Compute the overlap between the row vectors in a binary matrix</summary>
		/// <param name='entity_data'>the binary matrix</param>
		public static Tuple<IMatrix<float>, IList<float>> Compute(IBooleanMatrix entity_data)
		{
			var transpose = entity_data.Transpose() as IBooleanMatrix;

			IMatrix<float> overlap = new SymmetricSparseMatrix<float>(entity_data.NumberOfRows);

			for (int row_id = 0; row_id < transpose.NumberOfRows; row_id++)
			{
				var row = transpose.GetEntriesByRow(row_id);
				for (int i = 0; i < row.Count; i++)
				{
					int x = row[i];
					for (int j = i + 1; j < row.Count; j++)
						overlap[x, row[j]] += 1;
				}
			}

			IList<float> counts = new float[entity_data.NumberOfRows];
			for (int row_id = 0; row_id < entity_data.NumberOfRows; row_id++)
				counts[row_id] = entity_data.NumEntriesByRow(row_id);

			return Tuple.Create(overlap, counts);
		}
		
		// TODO write unit test comparing old and new interfaces
		public static Tuple<IMatrix<float>, IList<float>> ComputeWeighted(IInteractions interactions, EntityType entity_type)
		{
			int num_entities = (entity_type == EntityType.USER) ? interactions.MaxUserID + 1 : interactions.MaxItemID + 1;
			int num_other_entities = (entity_type == EntityType.USER) ? interactions.MaxItemID + 1 : interactions.MaxUserID + 1;
			
			var other_entity_weights = new float[num_other_entities];
			for (int other_entity_id = 0; other_entity_id < num_other_entities; other_entity_id++)
			{
				int freq;
				if (entity_type == EntityType.USER)
					freq = interactions.ByItem(other_entity_id).Users.Count;
				else
					freq = interactions.ByUser(other_entity_id).Items.Count;
				other_entity_weights[other_entity_id] = 1f / (float) Math.Log(3 + freq, 2); // TODO make configurable
			}

			IMatrix<float> weighted_overlap = new SymmetricMatrix<float>(num_entities);
			IList<float> entity_weights = new float[num_entities];

			// go over all (other) entities
			for (int other_entity_id = 0; other_entity_id < num_other_entities; other_entity_id++)
			{
				var row = (entity_type == EntityType.USER) ? interactions.ByItem(other_entity_id).Users.ToList() : interactions.ByUser(other_entity_id).Items.ToList();
				
				for (int i = 0; i < row.Count; i++)
				{
					int x = row[i];
					entity_weights[x] += other_entity_weights[other_entity_id];
					for (int j = i + 1; j < row.Count; j++)
					{
						int y = row[j];
						weighted_overlap[x, y] += other_entity_weights[other_entity_id] * other_entity_weights[other_entity_id];
					}
				}
			}

			return Tuple.Create(weighted_overlap, entity_weights);
		}

		public static Tuple<IMatrix<float>, IList<float>> Compute(IInteractions interactions, EntityType entity_type)
		{
			int num_entities = (entity_type == EntityType.USER) ? interactions.MaxUserID + 1 : interactions.MaxItemID + 1;
			int num_other_entities = (entity_type == EntityType.USER) ? interactions.MaxItemID + 1 : interactions.MaxUserID + 1;
			
			IMatrix<float> overlap = new SymmetricSparseMatrix<float>(num_entities);

			for (int other_entity_id = 0; other_entity_id < num_other_entities; other_entity_id++)
			{
				var row = (entity_type == EntityType.USER) ? interactions.ByItem(other_entity_id).Users.ToList() : interactions.ByUser(other_entity_id).Items.ToList();
				for (int i = 0; i < row.Count; i++)
				{
					int x = row[i];
					for (int j = i + 1; j < row.Count; j++)
						overlap[x, row[j]] += 1;
				}
			}

			IList<float> counts = new float[num_entities];
			for (int i = 0; i < num_entities; i++)
				counts[i] = (entity_type == EntityType.USER) ? interactions.ByUser(i).Items.Count : interactions.ByUser(i).Items.Count;

			return Tuple.Create(overlap, counts);
		}

	}
}

