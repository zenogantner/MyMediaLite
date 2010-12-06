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
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using MyMediaLite;
using MyMediaLite.data_type;
using MyMediaLite.rating_predictor;


namespace MyMediaLite.experimental.attr_to_factor
{
	/// <summary>Base class for biased MF plus attribute-to-factor mapping</summary>
	public abstract class MF_Mapping : BiasedMatrixFactorization
	{
		/// <summary>The learn rate for training the mapping functions</summary>
		public double LearnRateMapping { get { return learn_rate_mapping; } set { learn_rate_mapping = value; } }
		/// <summary>The learn rate for training the mapping functions</summary>
		protected double learn_rate_mapping = 0.01;
		
		/// <summary>number of times the regression is computed (to avoid local minima)</summary>
		/// <remarks>may be ignored by the engine</remarks>
		public int NumInitMapping {	get { return num_init_mapping; } set { num_init_mapping = value; } }
		/// <summary>number of times the regression is computed (to avoid local minima)</summary>
		/// <remarks>may be ignored by the engine</remarks>
		protected int num_init_mapping = 5;		
		
		/// <summary>number of iterations of the mapping training procedure</summary>
		public int NumIterMapping { get { return this.num_iter_mapping; } set { num_iter_mapping = value; } }
		/// <summary>number of iterations of the mapping training procedure</summary>
		protected int num_iter_mapping = 10;
		
		/// <summary>regularization constant for the mapping</summary>
		public double RegMapping { get { return this.reg_mapping; } set { reg_mapping = value; } }
		/// <summary>regularization constant for the mapping</summary>
		protected double reg_mapping = 0.1;
		
		/// <summary>The matrix representing the attribute-to-factor mapping</summary>
		/// <remarks>includes bias</remarks>
		protected Matrix<double> attribute_to_factor;

		/// <summary>Learn the mapping</summary>
		public abstract void LearnAttributeToFactorMapping();
		/// <summary>Perform one iteration of the mapping training</summary>
		public abstract void IterateMapping();
	}
}

