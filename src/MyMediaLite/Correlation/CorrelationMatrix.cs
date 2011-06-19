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
using System.Globalization;
using System.IO;
using MyMediaLite.DataType;
using MyMediaLite.Data;
using MyMediaLite.Taxonomy;

namespace MyMediaLite.Correlation
{
	/// <summary>Class for computing and storing correlations and similarities</summary>
	public class CorrelationMatrix : Matrix<float>
	{
		/// <summary>Number of entities, e.g. users or items</summary>
		protected int num_entities;

		/// <value>returns true if the matrix is symmetric, which is generally the case for similarity matrices</value>
		public override bool IsSymmetric { get { return true; } }

		///
        public override float this [int i, int j]
        {
			get { return data[i * dim2 + j]; }
			set	{
            	data[i * dim2 + j] = value;
            	data[j * dim2 + i] = value;
        	}
		}

		/// <summary>Creates a CorrelationMatrix object for a given number of entities</summary>
		/// <param name="num_entities">number of entities</param>
		public CorrelationMatrix(int num_entities) : base(num_entities, num_entities)
		{
			this.num_entities = num_entities;
		}

		/// <summary>Creates a correlation matrix</summary>
		/// <remarks>Gives out a useful warning if there is not enough memory</remarks>
		/// <param name="num_entities">the number of entities</param>
		/// <returns>the correlation matrix</returns>
		static public CorrelationMatrix Create(int num_entities)
		{
			CorrelationMatrix cm;
			try
			{
				cm = new CorrelationMatrix(num_entities);
			}
			catch (OverflowException)
			{
				Console.Error.WriteLine("Too many entities: " + num_entities);
				throw;
			}
			return cm;
		}

		/// <summary>Creates a CorrelationMatrix from the lines of a StreamReader</summary>
		/// <remarks>
		/// In the first line, we expect to be the number of entities.
		/// All the other lines have the format
		/// <pre>
		///   EntityID1 EntityID2 Correlation
		/// </pre>
		/// where EntityID1 and EntityID2 are non-negative integers and Correlation is a floating point number.
		/// </remarks>
		/// <param name="reader">the StreamReader to read from</param>
		static public CorrelationMatrix ReadCorrelationMatrix(StreamReader reader)
		{
			int num_entities = int.Parse(reader.ReadLine());

			CorrelationMatrix cm = Create(num_entities);

			// diagonal values
			for (int i = 0; i < num_entities; i++)
				cm[i, i] = 1;

			var split_chars = new char[]{ '\t', ' ', ',' };

			while (! reader.EndOfStream)
			{
				string[] numbers = reader.ReadLine().Split(split_chars);
				int i = int.Parse(numbers[0]);
				int j = int.Parse(numbers[1]);
				float c = float.Parse(numbers[2], CultureInfo.InvariantCulture);

				if (i >= num_entities)
					throw new IOException("Entity ID is too big: i = " + i);
				if (j >= num_entities)
					throw new IOException("Entity ID is too big: j = " + j);

				cm[i, j] = c;
			}

			return cm;
		}

		/// <summary>Write out the correlations to a StreamWriter</summary>
		/// <param name="writer">
		/// A <see cref="StreamWriter"/>
		/// </param>
		public void Write(StreamWriter writer) // TODO use library routine instead
		{
			writer.WriteLine(num_entities);
			for (int i = 0; i < num_entities; i++)
				for (int j = i + 1; j < num_entities; j++)
				{
					float val = this[i, j];
					if (val != 0f)
						writer.WriteLine(i + " " + j + " " + val.ToString(CultureInfo.InvariantCulture));
				}
		}

		/// <summary>Add an entity to the CorrelationMatrix by growing it to the requested size.</summary>
		/// <remarks>
		/// Note that you still have to correctly compute and set the entity's correlation values
		/// </remarks>
		/// <param name="entity_id">the numerical ID of the entity</param>
		public void AddEntity(int entity_id)
		{
			this.Grow(entity_id + 1, entity_id + 1);
		}

		/// <summary>Sum up the correlations between a given entity and the entities in a collection</summary>
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
                	result += this[entity_id, entity_id2];
			return result;
		}

		/// <summary>Get all entities that are positively correlated to an entity, sorted by correlation</summary>
		/// <param name="entity_id">the entity ID</param>
		/// <returns>a sorted list of all entities that are positively correlated to entitiy_id</returns>
		public IList<int> GetPositivelyCorrelatedEntities(int entity_id)
		{
			var result = new List<int>();
			for (int i = 0; i < num_entities; i++)
				if (this[i, entity_id] > 0)
					result.Add(i);

			result.Remove(entity_id);
			result.Sort(delegate(int i, int j) { return this[j, entity_id].CompareTo(this[i, entity_id]); });
			return result;
		}

		/// <summary>Get the k nearest neighbors of a given entity</summary>
		/// <param name="entity_id">the numerical ID of the entity</param>
		/// <param name="k">the neighborhood size</param>
		/// <returns>an array containing the numerical IDs of the k nearest neighbors</returns>
		public int[] GetNearestNeighbors(int entity_id, uint k)
		{
			var entities = new List<int>();
			for (int i = 0; i < num_entities; i++)
				entities.Add(i);

			entities.Remove(entity_id);
			entities.Sort(delegate(int i, int j) { return this[j, entity_id].CompareTo(this[i, entity_id]); });

			if (k < entities.Count)
				return entities.GetRange(0, (int) k).ToArray();
			else
				return entities.ToArray();
		}
	}
}