// Copyright (C) 2010 Zeno Gantner
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
using MyMediaLite.data_type;
using MyMediaLite.data;
using MyMediaLite.taxonomy;


namespace MyMediaLite.correlation
{
	/// <summary>Class for computing and storing correlations</summary>
	/// <author>Zeno Gantner, University of Hildesheim</author>
	public class CorrelationMatrix
	{
		/// <summary>
		/// Matrix object containing the correlations
		/// </summary>
		public Matrix<float> data;
		
		/// <summary>
		/// Number of entities, e.g. users or items
		/// </summary>
		protected int num_entities;

		/// <summary>
		/// Default constructor
		/// </summary>
		public CorrelationMatrix() { }
		
		/// <summary>
		/// Creates a CorrelationMatrix object for a given number of entities
		/// </summary>
		/// <param name="num_entities">number of entities</param>
		public CorrelationMatrix(int num_entities)
		{
			try
			{
		    	data = new Matrix<float>(num_entities, num_entities);
			}
			catch (OverflowException)
			{
				Console.Error.WriteLine("Too many entities: " + num_entities);
				throw;
			}

			this.num_entities = num_entities;
		}

		/// <summary>
		/// Create a CorrelationMatrix from the lines of a StreamReader
		/// </summary>
		/// <param name="reader">the StreamReader to read from</param>
		public CorrelationMatrix(StreamReader reader)
		{
			num_entities = System.Int32.Parse(reader.ReadLine());

			data = new Matrix<float>(num_entities, num_entities);

			// diagonal values
			for (int i = 0; i < num_entities; i++)
				data[i, i] = 1;

			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			while (! reader.EndOfStream)
			{
				string[] numbers = reader.ReadLine().Split(' ');
				int i = Int32.Parse(numbers[0]);
				int j = Int32.Parse(numbers[1]);
				float c = Single.Parse(numbers[2], ni);

				if (i >= num_entities)
					throw new Exception("i = " + i);
				if (j >= num_entities)
					throw new Exception("i = " + i);

				data[i, j] = c;
				data[j, i] = c;
			}
		}

		/// <summary>
		/// Write out the correlations to a StreamWriter
		/// </summary>
		/// <param name="writer">
		/// A <see cref="StreamWriter"/>
		/// </param>
		public void Write(StreamWriter writer)
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			int num_entities = data.dim1;
			writer.WriteLine(num_entities);
			for (int i = 0; i < num_entities; i++)
				for (int j = i + 1; j < num_entities; j++)
				{
					float val = data[i,j];
					if (val != 0)
						writer.WriteLine(i + " " + j + " " + val.ToString(ni));
				}
		}

		/// <summary>
		/// Get the correlation for a given entity pair
		/// </summary>
		/// <param name="i">the numerical ID of the first entity</param>
		/// <param name="j">the numerical ID of the second entity</param>
		/// <returns>the correlation between entity i and entity j</returns>
		public float Get(int i, int j)
		{
			return data[i, j];
		}

		/// <summary>
		/// Add an entity to the CorrelationMatrix by growing it to the requested size.
		/// 
		/// Note that you still have to correctly compute and set the entity's correlation values
		/// </summary>
		/// <param name="entity_id">the numerical ID of the entity</param>
		public void AddEntity(int entity_id)
		{
			this.data = this.data.Grow(entity_id + 1, entity_id + 1);
		}

		/// <summary>
		/// Sum up the correlations between a given entity and the entities in a collection
		/// </summary>
		/// <param name="entity_id">the numerical ID of the entity</param>
		/// <param name="entities">a collection containing the numerical IDs of the entities to compare to</param>
		/// <returns>the correlation sum</returns>
		public double SumUp(int entity_id, ICollection<int> entities)
		{
			if (entity_id < 0 || entity_id >= num_entities)
				throw new ArgumentException("Invalid entity ID: " + entity_id);

			double result = 0;
            foreach (int entity_id2 in entities)
				if (entity_id2 >= 0 && entity_id2 < num_entities)
                	result += data[entity_id, entity_id2];
			return result;
		}

		/// <summary>
		/// Get all entities that are positively correlated to an entity, sorted by correlation
		/// </summary>
		/// <param name="entity_id">the entity ID</param>
		/// <returns>a sorted list of all entities that are positively correlated to entitiy_id</returns>
		public IList<int> GetPositivelyCorrelatedEntities(int entity_id)
		{
			List<int> result = new List<int>();
			for (int i = 0; i < num_entities; i++)
				if (data[i, entity_id] > 0)
					result.Add(i);

			result.Remove(entity_id);
			result.Sort(delegate(int i, int j) { return data[j, entity_id].CompareTo(data[i, entity_id]); });
			return result;
		}

		/// <summary>
		/// Get the k nearest neighbors of a given entity
		/// </summary>
		/// <param name="entity_id">the numerical ID of the entity</param>
		/// <param name="k">the neighborhood size</param>
		/// <returns>an array containing the numerical IDs of the k nearest neighbors</returns>
		public int[] GetNearestNeighbors(int entity_id, uint k)
		{
			List<int> entities = new List<int>();
			for (int i = 0; i < num_entities; i++)
				entities.Add(i);

			entities.Remove(entity_id);
			entities.Sort(delegate(int i, int j) { return data[j, entity_id].CompareTo(data[i, entity_id]); });

			return entities.GetRange(0, (int) k).ToArray();
		}

		/// <summary>
		/// Compute the correlations for a given entity type from a rating dataset
		/// </summary>
		/// <param name="ratings">the rating data</param>
		/// <param name="entity_type">the EntityType - either USER or ITEM</param>
		public virtual void ComputeCorrelations(RatingData ratings, EntityType entity_type)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Compute the correlations from an implicit feedback, positive-only dataset
		/// </summary>
		/// <param name="entity_data">the implicit feedback set, rows contain the entities to correlate</param>
		public virtual void ComputeCorrelations(SparseBooleanMatrix entity_data)
		{
			throw new NotImplementedException();
		}
	}
}