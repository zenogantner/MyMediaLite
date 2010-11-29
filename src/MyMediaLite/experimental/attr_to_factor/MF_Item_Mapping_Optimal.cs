// Copyright (C) 2010 Zeno Gantner
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
using MyMediaLite.data_type;

namespace MyMediaLite.experimental.attr_to_factor
{
	public class MF_Item_Mapping_Optimal : MF_ItemMapping
	{
		
		/// <inheritdoc/>
		public override void LearnAttributeToFactorMapping()
		{
			for (int i = 0; i < num_iter_mapping; i++)
				IterateMapping();			
		}
		
		public override void IterateMapping()
		{
			var factor_bias_gradient         = new double[factor_bias.Length];
			var attribute_to_factor_gradient = new Matrix<double>(attribute_to_factor.dim1, attribute_to_factor.dim2);
			
			// 0. compute current error
			
			// 1. compute gradients
			//
			
			// 2. gradient descent step
		}
	}
}

