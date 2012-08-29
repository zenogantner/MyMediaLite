// Copyright (C) 2011 Zeno Gantner
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
using MyMediaLite.Data;
using MyMediaLite.Taxonomy;

namespace MyMediaLite.Correlation
{
	// TODO create interface
	
	/// <summary>CorrelationMatrix that computes correlations over rating data</summary>
	public abstract class RatingCorrelationMatrix : SymmetricCorrelationMatrix
	{
		/// <summary>Constructor</summary>
		/// <param name="num_entities">the number of entities</param>
		public RatingCorrelationMatrix(int num_entities) : base(num_entities) { }

		/// <summary>Compute the correlations for a given entity type from a rating dataset</summary>
		/// <param name="ratings">the rating data</param>
		/// <param name="entity_type">the EntityType - either USER or ITEM</param>
		abstract public void ComputeCorrelations(IRatings ratings, EntityType entity_type);
	}
}

