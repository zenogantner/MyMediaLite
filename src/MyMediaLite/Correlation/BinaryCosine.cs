// Copyright (C) 2011, 2012 Zeno Gantner
// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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
using MyMediaLite.DataType;

namespace MyMediaLite.Correlation
{
	/// <summary>Class for storing cosine similarities</summary>
	/// <remarks>
	/// http://en.wikipedia.org/wiki/Cosine_similarity
	/// </remarks>
	public sealed class BinaryCosine : BinaryDataSymmetricCorrelationMatrix
	{
		/// <summary>Creates an object of type Cosine</summary>
		/// <param name="num_entities">the number of entities</param>
		public BinaryCosine(int num_entities) : base(num_entities) { }

		///
		protected override float ComputeCorrelationFromOverlap(float overlap, float count_x, float count_y)
		{
			double size_product = count_x * count_y;
			if (size_product > 0.0)
				return (float) (overlap / Math.Sqrt(size_product));
			else
				return 0.0f;
		}
	}
}