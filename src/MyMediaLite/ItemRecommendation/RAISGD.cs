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
using C5;
using System.Linq;
using System.Globalization;
using MyMediaLite.DataType;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>
	///   Recency-Adjusted Incremental Stochastic Gradient Descent (RAISGD) algorithm for item prediction.
	/// </summary>
	/// <remarks>
	///   <para>
	///     Literature:
	/// 	<list type="bullet">
	///       <item><description>
	///         João Vinagre, Alípio Mário Jorge, João Gama:
	///         Collaborative filtering with recency-based negative feedback.
	///         ACM SAC 2015.
	///         http://dl.acm.org/citation.cfm?id=2695998
	///       </description></item>
	///     </list>
	///   </para>
	///   <para>
	///     This algorithm extends ISGD to accept recency-based negative feedback in incremental updates.
	///   </para>
	/// </remarks>
	public class RAISGD : ISGD
	{
		/// <summary>Apply user-based negative feedback in user factors</summary>
		public bool NegUsersInUsers { get; set; }
		/// <summary>Apply item-based negative feedback in user factors</summary>
		public bool NegItemsInUsers { get; set; }
		/// <summary>Apply user-based negative feedback in item factors</summary>
		public bool NegUsersInItems { get; set; }
		/// <summary>Apply item-based negative feedback in item factors</summary>
		public bool NegItemsInItems { get; set; }

		/// <summary>Number of negative examples for each positive example.</summary>
		public int NegFeedbackAmount { get { return neg_feedback_amount; } set { neg_feedback_amount = value; } }
		int neg_feedback_amount = 1;

		bool negate_users;
		bool negate_items;

		/// <summary>Item queue for item-based negative feedback (contains items ordered by time of latest occurrence)</summary>
		protected HashedLinkedList<int> item_queue;
		/// <summary>Item queue for user-based negative feedback (contains users ordered by time of latest occurrence)</summary>
		protected HashedLinkedList<int> user_queue;

		/// <summary>Default constructor.</summary>
		public RAISGD()
		{
			NegUsersInUsers = false;
			NegUsersInItems = false;
			NegItemsInUsers = true;
			NegItemsInItems = false;
		}

		/// <summary>
		/// Initiates the base model and the user and/or item queue(s)
		/// </summary>
		protected override void InitModel()
		{
			base.InitModel();

			negate_users = NegUsersInUsers || NegUsersInItems;
			negate_items = NegItemsInUsers || NegItemsInItems;

			if (negate_users)
			{
				user_queue = new HashedLinkedList<int>();
				user_queue.AddAll(Feedback.Users);
			}
			if (negate_items)
			{
				item_queue = new HashedLinkedList<int>();
				item_queue.AddAll(Feedback.Items);
			}
		}

		///
		protected override void Retrain(System.Collections.Generic.ICollection<Tuple<int, int>> feedback)
		{
			foreach (var entry in feedback)
			{
				RetrainEntry(entry);
			}
		}

		/// <summary>
		/// Retrains a single user-item pair.
		/// </summary>
		/// <param name="entry">The user-item pair</param>
		protected virtual void RetrainEntry(Tuple<int,int> entry)
		{
			InsertNegFeedback(entry);
			for (uint i = 0; i < IncrIter; i++)
				UpdateFactors(entry.Item1, entry.Item2, UpdateUsers, UpdateItems, 1);
		}

		/// <summary>
		/// Imputes a single negative feedback entry (a "negative" user-item pair).
		/// </summary>
		/// <param name="entry">The user-item pair</param>
		protected virtual void InsertNegFeedback(Tuple<int,int> entry)
		{
			int[] qu = Enumerable.Repeat(-1, neg_feedback_amount).ToArray();
			int[] qi = Enumerable.Repeat(-1, neg_feedback_amount).ToArray();
			if (negate_users)
			{
				for (uint i = 0; i < qu.Length && i < user_queue.Count - 1; )
				{
					qu[i] = user_queue.RemoveFirst();
					if (qu[i] != entry.Item1) i++;
				}
			}
			if (negate_items)
			{
				for (uint i = 0; i < qi.Length && i < item_queue.Count - 1; )
				{
					qi[i] = item_queue.RemoveFirst();
					if (qi[i] != entry.Item2) i++;
				}
			}
			//Console.WriteLine("Forgetting item "+qi);
			if (negate_users)
				foreach (var usr in qu.Reverse())
					if (usr >= 0) 
						for (uint i = 0; i < IncrIter; i++)
							UpdateFactors(usr, entry.Item2, NegUsersInUsers, NegUsersInItems, 0);
			if (negate_items)
				foreach (var itm in qi.Reverse())
					if (itm >= 0)
						for (uint i = 0; i < IncrIter; i++)
							UpdateFactors(entry.Item1, itm, NegItemsInUsers, NegItemsInItems, 0);

			if (negate_items)
			{
				item_queue.Remove(entry.Item2);
				item_queue.InsertLast(entry.Item2);

				foreach (var itm in qi.Reverse())
					if (itm >= 0 && itm != entry.Item2)
						item_queue.InsertLast(itm);
			}
			if (negate_users)
			{
				user_queue.Remove(entry.Item1);
				user_queue.InsertLast(entry.Item1);

				foreach (var usr in qu.Reverse())
					if (usr >= 0 && usr != entry.Item1)
						user_queue.InsertLast(usr);
			}
		}

		///
		public override void RemoveItem(int item_id)
		{
			base.RemoveItem(item_id);

			if (negate_items)
				item_queue.Remove(item_id);
		}

		///
		public override void RemoveUser(int user_id)
		{
			base.RemoveUser(user_id);

			if (negate_users)
				user_queue.Remove(user_id);
		}

		/// <summary>
		/// Performs factor updates for a user and item pair.
		/// </summary>
		/// <param name="user_id">The user ID</param>
		/// <param name="item_id">The item ID</param>
		/// <param name="update_user">Update user factors</param>
		/// <param name="update_item">Update item factors</param>
		/// <param name="rating_val">The rating (1 for positive feedback, 0 for negative feedback)</param>
		protected virtual void UpdateFactors(int user_id, int item_id, bool update_user, bool update_item, float rating_val = 1)
		{
			//Console.WriteLine(float.MinValue);
			float err = rating_val - Predict(user_id, item_id, false);

			// adjust factors
			for (int f = 0; f < NumFactors; f++)
			{
				float u_f = user_factors[user_id, f];
				float i_f = item_factors[item_id, f];

				// if necessary, compute and apply updates
				if (update_user)
				{
					double delta_u = err * i_f - Regularization * u_f;
					user_factors.Inc(user_id, f, current_learnrate * delta_u);
				}
				if (update_item)
				{
					double delta_i = err * u_f - Regularization * i_f;
					item_factors.Inc(item_id, f, current_learnrate * delta_i);
				}
			}

		}


		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"RAISGD num_factors={0} regularization={1} learn_rate={2} num_iter={3} incr_iter={4} decay={5}, neg_feedback_amount={6}",
				NumFactors, Regularization, LearnRate, NumIter, IncrIter, Decay, NegFeedbackAmount);
		}


	}
}

