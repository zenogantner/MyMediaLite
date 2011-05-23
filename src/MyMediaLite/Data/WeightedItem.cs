// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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

using System;

namespace MyMediaLite.Data
{
	// TODO consider having a value type here
	
	/// <summary>Weighted items class</summary>
	public sealed class WeightedItem : IComparable
	{
		/// <summary>Item ID</summary>
		public int item_id;
		/// <summary>Weight</summary>
		public double weight;

		/// <summary>Default constructor</summary>
		public WeightedItem() {}

		/// <summary>Constructor</summary>
		/// <param name="item_id">the item ID</param>
		/// <param name="weight">the weight</param>
		public WeightedItem(int item_id, double weight)
		{
			this.item_id = item_id;
			this.weight  = weight;
		}

		///
		public int CompareTo(Object o)
		{
			var otherItem = o as WeightedItem;
			return this.weight.CompareTo(otherItem.weight);
		}

		///
		public override bool Equals(Object o)
		{
			if (o == null)
				return false;

			var otherItem = o as WeightedItem;
			return Math.Abs(this.weight - otherItem.weight) < 0.000001;
		}

		///
		public bool Equals(WeightedItem otherItem)
		{
			if (otherItem == null)
				return false;

			return Math.Abs(this.weight - otherItem.weight) < 0.000001;
		}

		///
		public override int GetHashCode()
		{
			return weight.GetHashCode();
		}
	}
}
