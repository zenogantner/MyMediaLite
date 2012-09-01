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
using System.Linq;
using MyMediaLite.DataType;

namespace MyMediaLite.Correlation
{
	/// <summary>Class for storing and computing conditional probabilities</summary>
	/// <remarks>
	/// </remarks>
	public sealed class ConditionalProbability : BinaryDataAsymmetricCorrelationMatrix
	{
		/// <summary>Creates an object of type ConditionalProbability</summary>
		/// <param name="num_entities">the number of entities</param>
		public ConditionalProbability(int num_entities) : base(num_entities) { }

		///
		protected override float ComputeCorrelationFromOverlap(float overlap, float count_x, float count_y)
		{
			if (count_x != 0)
				return overlap / count_x;
			else
				return 0.0f;
		}
	}
}
