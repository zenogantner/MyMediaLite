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
using System.Collections.Generic;
using System.IO;
using MyMediaLite.DataType;

namespace MyMediaLite.Correlation
{
	/// <summary>Interface representing correlation and similarity matrices</summary>
	public interface ICorrelationMatrix : IMatrix<float>
	{
		/// <summary>size of the matrix (number of entities)</summary>
		int NumEntities { get; }

		/// <summary>Add an entity to the ICorrelationMatrix by growing it to the requested size.</summary>
		/// <remarks>
		/// Note that you still have to correctly compute and set the entity's correlation values
		/// </remarks>
		/// <param name="entity_id">the numerical ID of the entity</param>
		void AddEntity(int entity_id);

		/// <summary>Write out the correlations to a StreamWriter</summary>
		/// <param name="writer">
		/// A <see cref="StreamWriter"/>
		/// </param>
		void Write(StreamWriter writer);

		/// <summary>Resize to the given size</summary>
		/// <param name="size">the size</param>
		void Resize(int size);
	}
}

