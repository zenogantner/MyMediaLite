// Copyright (C) 2011, 2012 Zeno Gantner
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
// You should have received a copy of the GNU General Public License
// along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using MyMediaLite.DataType;

namespace MyMediaLite.Correlation
{
	/// <summary>CorrelationMatrix that computes correlations over binary data</summary>
	public interface IBinaryDataCorrelationMatrix : ICorrelationMatrix
	{
		/// <summary>If set to true, give a lower weight to evidence coming from very frequent entities</summary>
		bool Weighted { get; set; }

		/// <summary>Compute the correlations from an implicit feedback, positive-only dataset</summary>
		/// <param name="entity_data">the implicit feedback set, rows contain the entities to correlate</param>
		void ComputeCorrelations(IBooleanMatrix entity_data);

		/// <summary>Computes the correlation of two binary vectors</summary>
		/// <param name="vector_i">the first vector</param>
		/// <param name="vector_j">the second vector</param>
		/// <returns>the correlation of the two vectors</returns>
		float ComputeCorrelation(ICollection<int> vector_i, ICollection<int> vector_j);
	}
}

