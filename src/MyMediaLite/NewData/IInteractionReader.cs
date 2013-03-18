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
using System.Data;

// TODO: inherit this interface from an IRecord interface
//       consider sharing some things with IInteractions

namespace MyMediaLite.Data
{
	public interface IInteractionReader
	{
		void Reset();
		bool Read();

		int Count { get; }

		int GetUser();
		ICollection<int> Users { get; }
		int GetItem();
		ICollection<int> Items { get; }
		float GetRating();
		//ICollection<float> Ratings { get; }
		DateTime GetDateTime();
		//ICollection<DateTime> DateTimes { get; }
		long GetTimestamp();
		//ICollection<long> Timestamps { get; }

		// long GetDuration();

		// int GetContext();

		// GeoLocation GetGeoLocation();

		// string GetQuery()
	}
}

