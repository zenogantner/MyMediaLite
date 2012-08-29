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

namespace MyMediaLite.Correlation
{
	public interface ICorrelationMatrix
	{
		/// <summary>Add an entity to the ICorrelationMatrix by growing it to the requested size.</summary>
		/// <remarks>
		/// Note that you still have to correctly compute and set the entity's correlation values
		/// </remarks>
		/// <param name="entity_id">the numerical ID of the entity</param>
		void AddEntity(int entity_id);
			
		/// <summary>Sum up the correlations between a given entity and the entities in a collection</summary>
		/// <param name="entity_id">the numerical ID of the entity</param>
		/// <param name="entities">a collection containing the numerical IDs of the entities to compare to</param>
		/// <returns>the correlation sum</returns>
		double SumUp(int entity_id, ICollection<int> entities);
		
		/// <summary>Get all entities that are positively correlated to an entity, sorted by correlation</summary>
		/// <param name="entity_id">the entity ID</param>
		/// <returns>a sorted list of all entities that are positively correlated to entitiy_id</returns>
		IList<int> GetPositivelyCorrelatedEntities(int entity_id);

		/// <summary>Get the k nearest neighbors of a given entity</summary>
		/// <param name="entity_id">the numerical ID of the entity</param>
		/// <param name="k">the neighborhood size</param>
		/// <returns>an array containing the numerical IDs of the k nearest neighbors</returns>
		IList<int> GetNearestNeighbors(int entity_id, uint k);

	}
}

