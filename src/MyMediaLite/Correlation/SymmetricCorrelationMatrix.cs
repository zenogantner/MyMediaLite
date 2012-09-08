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
using System.Globalization;
using System.IO;
using MyMediaLite.DataType;
using MyMediaLite.Data;
using MyMediaLite.IO;
using MyMediaLite.Taxonomy;

/*! \namespace MyMediaLite.Correlation
 *  \brief This namespace contains several correlation/distance measures.
 */
namespace MyMediaLite.Correlation
{
	/// <summary>Class for computing and storing correlations and similarities</summary>
	public class SymmetricCorrelationMatrix : SymmetricSparseMatrix<float>, ICorrelationMatrix
	{
		///
		public int NumEntities { get { return num_entities; } }

		/// <summary>Number of entities, e.g. users or items</summary>
		protected int num_entities;

		/// <value>returns true if the matrix is symmetric, which is generally the case for similarity matrices</value>
		public override bool IsSymmetric { get { return true; } }

		/// <summary>Creates a CorrelationMatrix object for a given number of entities</summary>
		/// <param name="num_entities">number of entities</param>
		public SymmetricCorrelationMatrix(int num_entities) : base(num_entities)
		{
			this.num_entities = num_entities;
		}

		/// <summary>Creates a correlation matrix</summary>
		/// <remarks>Gives out a useful warning if there is not enough memory</remarks>
		/// <param name="num_entities">the number of entities</param>
		/// <returns>the correlation matrix</returns>
		static public SymmetricCorrelationMatrix Create(int num_entities)
		{
			SymmetricCorrelationMatrix cm;
			try
			{
				cm = new SymmetricCorrelationMatrix(num_entities);
			}
			catch (OverflowException)
			{
				Console.Error.WriteLine("Too many entities: " + num_entities);
				throw;
			}
			return cm;
		}

		/// <summary>Write out the correlations to a StreamWriter</summary>
		/// <param name="writer">
		/// A <see cref="StreamWriter"/>
		/// </param>
		public void Write(StreamWriter writer)
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

		public void Resize(int size)
		{
			Resize(size, size);
		}

		///
		public void AddEntity(int entity_id)
		{
			this.Resize(entity_id + 1, entity_id + 1);
		}

		///
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
		public IList<int> GetNearestNeighbors(int entity_id, uint k)
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