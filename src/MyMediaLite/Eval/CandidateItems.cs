// Copyright (C) 2011 Zeno Gantner
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

namespace MyMediaLite.Eval
{
	/// <summary>Different modes for choosing candiate items in item recommender evaluation</summary>
	public enum CandidateItems
	{
		/// <summary>use all items in the training set</summary>
		TRAINING,
		/// <summary>use all items in the test set</summary>
		TEST,
		/// <summary>use all items that are both in the training and the test set</summary>
		OVERLAP,
		/// <summary>use all items that are both in the training and the test set</summary>
		UNION,
		/// <summary>use items provided in a list given by the user</summary>
		EXPLICIT
	}
}

