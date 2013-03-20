// Copyright (C) 2013 Zeno Gantner
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
using System.Linq;
using MyMediaLite;
using MyMediaLite.Data;

namespace MyMediaLite.ItemRecommendation.BPR
{
	/// <summary>Helper class for the BPR sampling logic</summary>
	/// <remarks>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         Steffen Rendle, Christoph Freudenthaler, Zeno Gantner, Lars Schmidt-Thieme:
	///         BPR: Bayesian Personalized Ranking from Implicit Feedback.
	///         UAI 2009.
	///         http://www.ismll.uni-hildesheim.de/pub/pdfs/Rendle_et_al2009-Bayesian_Personalized_Ranking.pdf
	///       </description></item>
	///     </list>
	///   </para>
	/// </remarks>
	public class UniformUserSampler : BPRSampler
	{
		public UniformUserSampler(IInteractions interactions) : base(interactions) { }

		/// <summary>Uniformly sample a user that has viewed at least one and not all items</summary>
		/// <returns>the user ID</returns>
		public override int NextUser()
		{
			while (true)
			{
				int user_id = random.Next(max_user_id + 1);
				var user_items = interactions.ByUser(user_id).Items;
				if (user_items.Count == 0 || user_items.Count == max_item_id + 1)
					continue;
				return user_id;
			}
		}

		/// <summary>Sample a triple for BPR learning (uniform user sampling)</summary>
		/// <param name="u">the user ID</param>
		/// <param name="i">the ID of the first item</param>
		/// <param name="j">the ID of the second item</param>
		public override void NextTriple(out int u, out int i, out int j)
		{
			u = NextUser();
			var user_items = interactions.ByUser(u).Items;
			ItemPair(user_items, out i, out j);
		}
	}
}
