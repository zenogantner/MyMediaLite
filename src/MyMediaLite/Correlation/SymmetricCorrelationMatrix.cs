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
		/// <summary>Number of entities the correlation is defined over</summary>
		public int NumEntities { get; private set; }

		/// <value>returns true if the matrix is symmetric, which is generally the case for similarity matrices</value>
		public override bool IsSymmetric { get { return true; } }

		/// <summary>Creates a CorrelationMatrix object for a given number of entities</summary>
		/// <param name="num_entities">number of entities</param>
		public SymmetricCorrelationMatrix(int num_entities) : base(num_entities)
		{
			NumEntities = num_entities;
		}

		/// <summary>Write out the correlations to a StreamWriter</summary>
		/// <param name="writer">
		/// A <see cref="StreamWriter"/>
		/// </param>
		public void Write(StreamWriter writer)
		{
			writer.WriteLine(NumEntities);
			for (int i = 0; i < NumEntities; i++)
				for (int j = i + 1; j < NumEntities; j++)
				{
					float val = this[i, j];
					if (val != 0f)
						writer.WriteLine(i + " " + j + " " + val.ToString(CultureInfo.InvariantCulture));
				}
		}

		///
		public override void Resize(int size)
		{
			base.Resize(size);
			NumEntities = size;
		}

		///
		public void AddEntity(int entity_id)
		{
			this.Resize(entity_id + 1, entity_id + 1);
		}
	}
}