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

using System.Collections.Generic;
using System.Linq;

/*! \namespace MyMediaLite.Data
 *  \brief This namespace contains MyMediaLite's principal data structures,
 *  which are used e.g. to store the interaction data that is used to train
 *  personalized recommenders.
 */
namespace MyMediaLite.Data
{
	/// <summary>Abstract dataset class that implements some common functions</summary>
	public abstract class DataSet : IDataSet
	{
		///
		public IList<int> Users { get; protected set; }
		///
		public IList<int> Items { get; protected set; }

		///
		public virtual int Count { get { return Users.Count; } }

		///
		public int MaxUserID { get; protected set; }
		///
		public int MaxItemID { get; protected set; }

		///
		public IList<IList<int>> ByUser
		{
			get {
				if (by_user == null)
					BuildUserIndices();
				return by_user;
			}
		}
		/// <summary>Indices organized by user</summary>
		protected IList<IList<int>> by_user;

		/// <summary>Default constructor</summary>
		public DataSet()
		{
			Users = new List<int>();
			Items = new List<int>();
		}

		///
		public IList<IList<int>> ByItem
		{
			get {
				if (by_item == null)
					BuildItemIndices();
				return by_item;
			}
		}
		/// <summary>Indices organized by item</summary>
		protected IList<IList<int>> by_item;

		///
		public IList<int> RandomIndex
		{
			get {
				if (random_index == null || random_index.Length != Count)
					BuildRandomIndex();

				return random_index;
			}
		}
		private int[] random_index;

		///
		public IList<int> AllUsers
		{
			get {
				var result_set = new HashSet<int>();
				for (int index = 0; index < Users.Count; index++)
					result_set.Add(Users[index]);
				return result_set.ToArray();
			}
		}

		///
		public IList<int> AllItems
		{
			get {
				var result_set = new HashSet<int>();
				for (int index = 0; index < Items.Count; index++)
					result_set.Add(Items[index]);
				return result_set.ToArray();
			}
		}

		///
		public void BuildUserIndices()
		{
			by_user = new List<IList<int>>();
			for (int u = 0; u <= MaxUserID; u++)
				by_user.Add(new List<int>());

			// one pass over the data
			for (int index = 0; index < Count; index++)
				by_user[Users[index]].Add(index);
		}

		///
		public void BuildItemIndices()
		{
			by_item = new List<IList<int>>();
			for (int i = 0; i <= MaxItemID; i++)
				by_item.Add(new List<int>());

			// one pass over the data
			for (int index = 0; index < Count; index++)
				by_item[Items[index]].Add(index);
		}

		///
		public void BuildRandomIndex()
		{
			if (random_index == null || random_index.Length != Count)
			{
				random_index = new int[Count];
				for (int index = 0; index < Count; index++)
					random_index[index] = index;
			}
			Util.Utils.Shuffle<int>(random_index);
		}

		///
		public abstract void RemoveUser(int user_id);

		///
		public abstract void RemoveItem(int item_id);
	}
}
