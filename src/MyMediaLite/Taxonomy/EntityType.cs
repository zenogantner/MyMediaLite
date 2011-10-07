// Copyright (C) 2010, 2011 Zeno Gantner
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

/*! \namespace MyMediaLite.Taxonomy
 *  \brief This namespace contains taxonomical data structures, i.e. data structures that help us to describe what kind of a thing something is.
 */
namespace MyMediaLite.Taxonomy
{
	/// <summary>Type to refer to different kinds of entities like users and items</summary>
	public enum EntityType
	{
		/// <summary>users</summary>
		USER,
		/// <summary>items like movies, DVDs, books, products, etc.</summary>
		ITEM,
		/// <summary>folksonomy tags</summary>
		TAG,
		/// <summary>timestamps</summary>
		TIMESTAMP
	}
}

