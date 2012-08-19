// Copyright (C) 2011, 2012 Zeno Gantner
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

using MyMediaLite.Data;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Interface for item recommenders that take into account some test data for training</summary>
	public interface ITransductiveItemRecommender
	{
		/// <summary>user-item combinations that are known to be queried</summary>
		IPosOnlyFeedback AdditionalFeedback { get; set; }
	}
}