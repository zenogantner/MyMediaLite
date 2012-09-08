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

namespace MyMediaLite.Correlation
{
	/// <summary>Class for computing and storing correlations and similarities</summary>
	public class AsymmetricCorrelationMatrix : SparseMatrix<float>, ICorrelationMatrix
	{
		///
		public int NumEntities { get { return num_entities; } }

		/// <summary>Number of entities, e.g. users or items</summary>
		protected int num_entities;

		/// <value>returns false</value>
		public override bool IsSymmetric { get { return false; } }

		/// <summary>Creates a CorrelationMatrix object for a given number of entities</summary>
		/// <param name="num_entities">number of entities</param>
		public AsymmetricCorrelationMatrix(int num_entities) : base(num_entities, num_entities)
		{
			this.num_entities = num_entities;
		}

		/// <summary>Write out the correlations to a StreamWriter</summary>
		/// <param name="writer">
		/// A <see cref="StreamWriter"/>
		/// </param>
		public void Write(StreamWriter writer)
		{
			writer.WriteLine(num_entities);
			for (int i = 0; i < num_entities; i++)
				for (int j = 0; j < num_entities; j++)
				{
					float val = this[i, j];
					if (val != 0f)
						writer.WriteLine(i + " " + j + " " + val.ToString(CultureInfo.InvariantCulture));
				}
		}

		///
		public void AddEntity(int entity_id)
		{
			this.Resize(entity_id + 1);
		}

		///
		public void Resize(int num_rows)
		{
			Resize (num_rows, num_rows);
		}
	}
}