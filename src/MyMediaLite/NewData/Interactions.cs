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
using System.IO;
using System.Linq;
using MyMediaLite.IO;

namespace MyMediaLite.Data
{
	public class Interactions : IInteractions
	{
		///
		public int Count { get { return interaction_list.Count; } }

		///
		public int MaxUserID { get; private set; }

		///
		public int MaxItemID { get; private set; }

		///
		public DateTime EarliestDateTime { get; private set; }

		///
		public DateTime LatestDateTime { get; private set; }

		///
		public IList<int> Users { get { return new List<int>(user_set); } }

		///
		public IList<int> Items { get { return new List<int>(item_set); } }

		///
		public IInteractionReader Random
		{
			get {
				return new InteractionReader(RandomInteractionList, user_set, item_set);
			}
		}
		private IList<IInteraction> random_interaction_list;

		// TODO change protection level?
		///
		public IList<IInteraction> RandomInteractionList
		{
			get {
				// TODO synchronize
				if (random_interaction_list == null)
				{
					random_interaction_list = interaction_list.ToArray();
					random_interaction_list.Shuffle();
				}
				return random_interaction_list;
			}
		}

		///
		public IInteractionReader Sequential
		{
			get {
				return new InteractionReader(interaction_list, user_set, item_set);
			}
		}

		///
		IInteractionReader Chronological
		{
			get {
				return new InteractionReader(ChronologicalInteractionList, user_set, item_set);
			}
		}
		private List<IInteraction> chronological_interaction_list;
		// TODO change protection level?
		public IList<IInteraction> ChronologicalInteractionList
		{
			get {
				// TODO synchronize
				if (chronological_interaction_list == null)
				{
					chronological_interaction_list = interaction_list.ToList();
					chronological_interaction_list.Sort((a, b) => a.DateTime.CompareTo(b.DateTime));
				}
				return chronological_interaction_list;
			}
		}

		private ByUserReaders ByUserReaders { get; set; }
		///
		public IInteractionReader ByUser(int user_id)
		{
			return ByUserReaders[user_id];
		}

		private ByItemReaders ByItemReaders { get; set; }
		///
		public IInteractionReader ByItem(int item_id)
		{
			return ByItemReaders[item_id];
		}

		///
		public RatingScale RatingScale { get; private set; }

		///
		public bool HasRatings { get; private set; }

		///
		public bool HasDateTimes { get; private set; }

		// TODO change access
		///
		public IList<IInteraction> InteractionList { get { return interaction_list; } }
		private IList<IInteraction> interaction_list;
		private ISet<int> user_set;
		private ISet<int> item_set;

		public Interactions()
		{
			interaction_list = new IInteraction[0];
			user_set = new HashSet<int>();
			item_set = new HashSet<int>();
			MaxUserID = -1;
			MaxItemID = -1;
			ByUserReaders = new ByUserReaders(interaction_list, MaxUserID);
			ByItemReaders = new ByItemReaders(interaction_list, MaxItemID);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MyMediaLite.Data.Interactions"/> class, taking a list of interactions
		/// </summary>
		/// <param name='interaction_list'>a list of interactions</param>
		public Interactions(IList<IInteraction> interaction_list)
		{
			this.interaction_list = interaction_list; // TODO consider defensive copy

			if (interaction_list[0].HasDateTimes && interaction_list[0].DateTime != DateTime.MinValue)
				HasDateTimes = true;
			if (interaction_list[0].HasRatings)
				HasRatings = true;

			user_set = new HashSet<int>(from e in interaction_list select e.User);
			item_set = new HashSet<int>(from e in interaction_list select e.Item);
			MaxUserID = user_set.Max();
			MaxItemID = item_set.Max();

			if (HasRatings)
			{
				var ratings = (from e in interaction_list select e.Rating).Distinct();
				RatingScale = new RatingScale(ratings.ToList());
			}
			if (HasDateTimes)
			{
				EarliestDateTime = (from e in interaction_list select e.DateTime).Min();
				LatestDateTime = (from e in interaction_list select e.DateTime).Max();
			}

			ByUserReaders = new ByUserReaders(interaction_list, MaxUserID);
			ByItemReaders = new ByItemReaders(interaction_list, MaxItemID);
		}

		// TODO updates: batch interface, use immutable data structures in the background

		static public readonly char[] DEFAULT_SEPARATORS = new char[]{ '\t', ',' };

		// TODO stream from file (implement in different class, but share the parsing code)

		// TODO move to different file
		// TODO more elegant interface ...
		static public IInteractions FromFile(
			string filename, IMapping user_mapping = null, IMapping item_mapping = null,
			int min_num_fields = 2,
			int user_pos = 0, int item_pos = 1, int rating_pos = 2, int datetime_pos = 3,
			char[] separators = null, bool ignore_first_line = false)
		{
			using (var reader = new StreamReader(filename))
			{
				return FromTextReader(reader, user_mapping, item_mapping, min_num_fields, user_pos, item_pos, rating_pos, datetime_pos, separators, ignore_first_line);
			}
		}

		// TODO move to different file
		// TODO better checks, more elegant please
		static public IInteractions FromTextReader(
			TextReader reader, IMapping user_mapping = null, IMapping item_mapping = null,
			int min_num_fields = 2,
			int user_pos = 0, int item_pos = 1, int rating_pos = 2, int datetime_pos = 3,
			char[] separators = null, bool ignore_first_line = false)
		{
			if (user_mapping == null)
				user_mapping = new IdentityMapping();
			if (item_mapping == null)
				item_mapping = new IdentityMapping();
			if (ignore_first_line)
				reader.ReadLine();
			if (separators == null)
				separators = DEFAULT_SEPARATORS;

			var interaction_list = new List<IInteraction>();
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				string[] tokens = line.Split(separators);

				if (tokens.Length < min_num_fields)
					throw new FormatException(string.Format("Expected at least {0} columns: {1}", min_num_fields, line));

				try
				{
					if (tokens.Length == 2)
					{
						int user_id = user_mapping.ToInternalID(tokens[user_pos]);
						int item_id = item_mapping.ToInternalID(tokens[item_pos]);
						interaction_list.Add(new SimpleInteraction(user_id, item_id));
					}
					if (tokens.Length == 3)
					{
						int user_id = user_mapping.ToInternalID(tokens[user_pos]);
						int item_id = item_mapping.ToInternalID(tokens[item_pos]);
						float rating = float.Parse(tokens[rating_pos]);
						interaction_list.Add(new FullInteraction(user_id, item_id, rating, DateTime.MinValue)); // FIXME
					}
					if (tokens.Length >= 4)
					{
						int user_id = user_mapping.ToInternalID(tokens[user_pos]);
						int item_id = item_mapping.ToInternalID(tokens[item_pos]);
						float rating = float.Parse(tokens[rating_pos]);
						DateTime date_time = DateTimeParser.Parse(tokens[datetime_pos]);
						interaction_list.Add(new FullInteraction(user_id, item_id, rating, date_time));
					}
				}
				catch (Exception e)
				{
					throw new FormatException(string.Format("Could not read line '{0}'", line), e);
				}
			}
			return new Interactions(interaction_list);
		}

	}
}

