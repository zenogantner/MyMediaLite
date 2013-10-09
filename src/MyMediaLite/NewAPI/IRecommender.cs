// Copyright (C) 2010, 2011, 2012, 2013 Zeno Gantner
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

namespace MyMediaLite
{
	// Doxygen main page
	/// \mainpage MyMediaLite API Documentation
	/// You can browse the documentation by class name, class hierarchy, and member name.
	/// Just click on the "Classes" tab.
	///
	/// Please report problems and missing information to the MyMediaLite authors: http://mymedialite.net/contact.html
	///
	/// If you want to contribute to MyMediaLite have a look at http://mymedialite.net/contribute.html

	/// <summary>Generic recommender interface</summary>
	/// <remarks></remarks>
	public interface IRecommender
	{
		bool SupportsFoldIn { get; }
		float Score(int userId, int itemId);
		IList<Tuple<int, float>> Recommend(int userId, IEnumerable<int> itemSet, int n);
		IList<Tuple<int, float>> FoldIn(IUserData userData, IEnumerable<int> itemSet, int n);
	}
}

