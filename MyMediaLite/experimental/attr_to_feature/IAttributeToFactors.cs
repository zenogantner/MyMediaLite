// // Copyright (C) 2010 Zeno Gantner
// //
// // This file is part of MyMediaLite.
// //
// // MyMediaLite is free software: you can redistribute it and/or modify
// // it under the terms of the GNU General Public License as published by
// // the Free Software Foundation, either version 3 of the License, or
// // (at your option) any later version.
// //
// // MyMediaLite is distributed in the hope that it will be useful,
// // but WITHOUT ANY WARRANTY; without even the implied warranty of
// // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// // GNU General Public License for more details.
// //
// //  You should have received a copy of the GNU General Public License
// //  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.
//
//

using System;
using System.Collections.Generic;
using MyMediaLite.data_type;


namespace MyMediaLite.experimental.attr_to_feature
{
	/// <summary>Interface for attribute-to-factor mappings</summary>
	/// <remarks>
	///
	/// </remarks>
	public interface IAttributeToFactors
	{
		/// <summary>Learn mapping from an attribute space to a latent factor space</summary>
		void LearnAttributeToFactorMapping(SparseBooleanMatrix binary_attributes, Matrix<double> latent_features);

		/// <summary>Compute the component-wise fits on the given latent factors</summary>
		double[] ComputeComponentFit(SparseBooleanMatrix binary_attributes, Matrix<double> latent_features);		

		/// <summary>Compute the overall fit on the given latent factors</summary>
		double ComputeOverallFit(SparseBooleanMatrix binary_attributes, Matrix<double> latent_features);		
		
		/// <summary>Map from the attribute space to the factor space</summary>
		double[] Map(HashSet<int> attributes);
	}
}

