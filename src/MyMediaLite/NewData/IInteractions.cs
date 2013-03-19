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

namespace MyMediaLite.Data
{
	public interface IInteractions
	{
		int Count { get; }
		int MaxUserID { get; }
		int MaxItemID { get; }

		IInteractionReader Random { get; } // TODO use function instead if property for clearer semantics? This should return a *different* reader for each call
		IInteractionReader Sequential { get; }
		// IInteractionReader Chronological { get; }
		IInteractionReader ByUser(int user_id);
		IInteractionReader ByItem(int item_id);

		IList<int> Users { get; }
		IList<int> Items { get; }
		RatingScale RatingScale { get; }
	}
}

