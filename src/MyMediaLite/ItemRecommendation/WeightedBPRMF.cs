// Copyright (C) 2011, 2012, 2013 Zeno Gantner
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
using System.Globalization;
using System.Linq;
using MyMediaLite.Data;
using MyMediaLite.DataType;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Weigthed BPR-MF with frequency-adjusted sampling</summary>
	/// <remarks>
	/// Zeno Gantner, Lucas Drumond, Christoph Freudenthaler, Lars Schmidt-Thieme:
	/// Bayesian Personalized Ranking for Non-Uniformly Sampled Items.
	/// KDD Cup Workshop 2011
	/// </remarks>
	public class WeightedBPRMF : BPRMF
	{
		IInteractionReader _sampling_reader;
		
		/// <summary>Default constructor</summary>
		public WeightedBPRMF()
		{
			// de-activate until false is supported
			UniformUserSampling = true;
		}

		///
		public override void Train()
		{
			// de-activate until false is supported
			UniformUserSampling = true;
			
			_sampling_reader = Interactions.Random;
			base.Train();
		}

		///
		protected override void SampleTriple(out int u, out int i, out int j)
		{
			if (!_sampling_reader.Read())
			{
				_sampling_reader.Reset();
				_sampling_reader.Read();
			}
			u = _sampling_reader.GetUser();
			i = _sampling_reader.GetItem();

			// sample negative item
			do
				j = Feedback.Items[random.Next(Feedback.Count)]; // TODO TODO create 2nd reader
			while (Interactions.ByUser(u).Items.Contains(j));
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} bias_reg={2} reg_u={3} reg_i={4} reg_j={5} num_iter={6} learn_rate={7}",
				this.GetType().Name, num_factors, BiasReg, reg_u, reg_i, reg_j, NumIter, learn_rate);
		}
	}
}

