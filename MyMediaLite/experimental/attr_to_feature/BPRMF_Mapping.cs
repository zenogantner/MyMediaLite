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
using MyMediaLite.item_recommender;


namespace MyMediaLite.experimental.attr_to_feature
{
	public abstract class BPRMF_Mapping : BPRMF
	{
		public double LearnRateMapping { get { return learn_rate_mapping; } set { learn_rate_mapping = value; } }
		protected double learn_rate_mapping = 0.01;
		
		public int NumInitMapping {	get { return num_init_mapping; } set { num_init_mapping = value; } }
		protected int num_init_mapping = 5;
		
		public int NumIterMapping { get { return this.num_iter_mapping; } set { num_iter_mapping = value; } }
		protected int num_iter_mapping = 10;
		
		public double RegMapping { get { return this.reg_mapping; } set { reg_mapping = value; } }
		protected double reg_mapping = 0.1;
		
		// includes bias
		protected Matrix<double> attribute_to_feature;

		/// <summary>Learn the mapping</summary>
		public abstract void LearnAttributeToFactorMapping();
		/// <summary>Perform one iteration of the mapping training</summary>
		public abstract void IterateMapping();
	}
}

