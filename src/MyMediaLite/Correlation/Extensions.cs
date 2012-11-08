// Copyright (C) 2012 Zeno Gantner
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
using MyMediaLite.IO;

namespace MyMediaLite.Correlation
{
	/// <summary>Extension methods for correlation matrices</summary>
	public static class Extensions
	{
		/// <summary>Sum up the correlations between a given entity and the entities in a collection</summary>
		/// <param name="correlation">the correlation matrix</param>
		/// <param name="entity_id">the numerical ID of the entity</param>
		/// <param name="entities">a collection containing the numerical IDs of the entities to compare to</param>
		/// <param name="q">score exponent</param>
		/// <returns>the correlation sum</returns>
		public static double SumUp(this ICorrelationMatrix correlation, int entity_id, ICollection<int> entities, float q = 1.0f)
		{
			double result = 0;
			foreach (int entity_id2 in entities)
				result += Math.Pow(correlation[entity_id, entity_id2], q);
			return result;
		}

		/// <summary>Sets all values in a matrix to zero</summary>
		/// <param name='c'>a correlation matrix</param>
		static public void SetZero(this ICorrelationMatrix c)
		{
			int size = c.NumberOfRows;
			c.Resize(0);
			c.Resize(size); // TODO consider doing this without resize
		}

		/// <summary>Reads a SymmetricCorrelationMatrix from the lines of a StreamReader</summary>
		/// <remarks>
		/// In the first line, we expect to be the number of entities.
		/// All the other lines have the format
		/// <pre>
		///   EntityID1 EntityID2 Correlation
		/// </pre>
		/// where EntityID1 and EntityID2 are non-negative integers and Correlation is a floating point number.
		/// </remarks>
		/// <param name='correlation'>a correlation matrix</param>
		/// <param name="reader">the StreamReader to read from</param>
		static public void ReadSymmetricCorrelationMatrix(
			this SymmetricCorrelationMatrix correlation,
			StreamReader reader)
		{
			int num_entities = int.Parse(reader.ReadLine());
			correlation.Resize(num_entities);
			correlation.SetZero();

			// diagonal values
			for (int i = 0; i < num_entities; i++)
				correlation[i, i] = 1;

			string line;
			while ((line = reader.ReadLine()) != null)
			{
				string[] numbers = line.Split(Constants.SPLIT_CHARS);
				int i = int.Parse(numbers[0]);
				int j = int.Parse(numbers[1]);
				float c = float.Parse(numbers[2], CultureInfo.InvariantCulture);

				if (i >= num_entities)
					throw new IOException("Row index is too big: i = " + i);
				if (j >= num_entities)
					throw new IOException("Column index is too big: j = " + j);

				correlation[i, j] = c;
			}
		}

		/// <summary>Reads a AsymmetricCorrelationMatrix from the lines of a StreamReader</summary>
		/// <remarks>
		/// In the first line, we expect to be the number of entities.
		/// All the other lines have the format
		/// <pre>
		///   EntityID1 EntityID2 Correlation
		/// </pre>
		/// where EntityID1 and EntityID2 are non-negative integers and Correlation is a floating point number.
		/// </remarks>
		/// <param name='correlation'>a correlation matrix</param>
		/// <param name="reader">the StreamReader to read from</param>
		static public void ReadAsymmetricCorrelationMatrix(
			this AsymmetricCorrelationMatrix correlation, StreamReader reader)
		{
			int num_entities = int.Parse(reader.ReadLine());
			correlation.Resize(num_entities);
			correlation.SetZero();

			// diagonal values
			for (int i = 0; i < num_entities; i++)
				correlation[i, i] = 1;

			string line;
			while ((line = reader.ReadLine()) != null)
			{
				string[] numbers = line.Split(Constants.SPLIT_CHARS);
				int i = int.Parse(numbers[0]);
				int j = int.Parse(numbers[1]);
				float c = float.Parse(numbers[2], CultureInfo.InvariantCulture);

				if (i >= num_entities)
					throw new IOException("Row index is too big: i = " + i);
				if (j >= num_entities)
					throw new IOException("Column index is too big: j = " + j);

				correlation[i, j] = c;
			}
		}

		/// <summary>Get all entities that are positively correlated to an entity, sorted by correlation</summary>
		/// <param name="c">a correlation matrix</param>
		/// <param name="entity_id">the entity ID</param>
		/// <returns>a sorted list of all entities that are positively correlated to entitiy_id</returns>
		public static IList<int> GetPositivelyCorrelatedEntities(this ICorrelationMatrix c, int entity_id)
		{
			int num_entities = c.NumberOfRows;
			var result = new List<int>();
			for (int i = 0; i < num_entities; i++)
				if (c[i, entity_id] > 0)
					result.Add(i);

			result.Remove(entity_id);
			result.Sort(delegate(int i, int j) { return c[j, entity_id].CompareTo(c[i, entity_id]); });
			return result;
		}

		/// <summary>Get the k nearest neighbors of a given entity</summary>
		/// <param name="c">a correlation matrix</param>
		/// <param name="entity_id">the numerical ID of the entity</param>
		/// <param name="k">the neighborhood size</param>
		/// <returns>a sorted list containing the numerical IDs of the k nearest neighbors</returns>
		public static IList<int> GetNearestNeighbors(this ICorrelationMatrix c, int entity_id, uint k)
		{
			int num_entities = c.NumberOfRows;
			var entities = new List<int>();
			for (int i = 0; i < num_entities; i++)
				entities.Add(i);

			entities.Remove(entity_id);
			entities.Sort(delegate(int i, int j) { return c[j, entity_id].CompareTo(c[i, entity_id]); });

			if (k < entities.Count)
				return entities.GetRange(0, (int) k).ToArray();
			else
				return entities.ToArray();
		}
	}
}

