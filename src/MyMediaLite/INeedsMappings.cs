// Copyright (C) 2012 Zeno Gantner
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
using MyMediaLite.Data;

namespace MyMediaLite
{
	/// <summary>
	/// Interface for classes that need user and item ID mappings, e.g. for recommenders that read data
	/// from external sources and thus need to know which IDs are used externally.
	/// </summary>
	public interface INeedsMappings
	{
		/// <summary>the user mapping</summary>
		IMapping UserMapping { get; set; }
		/// <summary>the item mapping</summary>
		IMapping ItemMapping { get; set; }
	}
}

