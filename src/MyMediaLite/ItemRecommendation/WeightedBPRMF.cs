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
using MyMediaLite.ItemRecommendation.BPR;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Weigthed BPR-MF with frequency-adjusted sampling</summary>
	/// <remarks>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         Zeno Gantner, Lucas Drumond, Christoph Freudenthaler, Lars Schmidt-Thieme:
	///         Bayesian Personalized Ranking for Non-Uniformly Sampled Items.
	///         KDD Cup Workshop 2011
	///         http://jmlr.csail.mit.edu/proceedings/papers/v18/gantner12a/gantner12a.pdf
	///       </description></item>
	///     </list>
	///   </para>
	/// </remarks>
	public class WeightedBPRMF : BPRMF
	{
		protected override IBPRSampler CreateBPRSampler()
		{
			if (UniformUserSampling)
				return new UniformUserFrequencyItemSampler(Interactions);
			else
				return new UniformPairFrequencyItemSampler(Interactions);
		}
	}
}

