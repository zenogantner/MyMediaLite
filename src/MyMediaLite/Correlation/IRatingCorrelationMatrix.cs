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
using MyMediaLite.Data;
using MyMediaLite.Taxonomy;

namespace MyMediaLite.Correlation
{
	/// <summary>CorrelationMatrix that computes correlations over rating data</summary>
	public interface IRatingCorrelationMatrix : ICorrelationMatrix
	{
		/// <summary>Compute the correlations for a given entity type from a rating dataset</summary>
		/// <param name="ratings">the rating data</param>
		/// <param name="entity_type">the EntityType - either USER or ITEM</param>
		void ComputeCorrelations(IRatings ratings, EntityType entity_type);

		/// <summary>Computes the correlation of two rating vectors</summary>
		/// <param name="ratings">the rating data</param>
		/// <param name="entity_type">the entity type, either USER or ITEM</param>
		/// <param name="i">the ID of the first entity</param>
		/// <param name="j">the ID of the second entity</param>
		/// <returns>the correlation of the two vectors</returns>
		float ComputeCorrelation(IRatings ratings, EntityType entity_type, int i, int j);

		/// <summary>Compute correlation between two entities for given ratings</summary>
		/// <param name="ratings">the rating data</param>
		/// <param name="entity_type">the entity type, either USER or ITEM</param>
		/// <param name="entity_ratings">ratings identifying the first entity</param>
		/// <param name="j">the ID of second entity</param>
		float ComputeCorrelation(IRatings ratings, EntityType entity_type, IList<Tuple<int, float>> entity_ratings, int j);
	}
}

