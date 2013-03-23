// Copyright (C) 2012, 2013 Zeno Gantner
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
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Eval;

class RatingBasedRanking : RatingPrediction
{
	IList<int> test_users;
	IList<int> candidate_items;
	CandidateItems eval_item_mode = CandidateItems.UNION;

	string test_users_file;
	string candidate_items_file;
	bool overlap_items;
	bool in_training_items;
	bool in_test_items;
	bool all_items;

	protected override ICollection<string> Measures { get { return Items.Measures; } }

	public RatingBasedRanking()
	{
		eval_measures = new string[] { "AUC", "prec@5" };
	}

	protected override void ShowVersion()
	{
		ShowVersion(
			"Rating-based Item Ranking",
			"Copyright (C) 2011, 2012, 2013 Zeno Gantner\nCopyright (C) 2010 Zeno Gantner, Steffen Rendle"
		);
	}

	protected override void SetupOptions()
	{
		base.SetupOptions();

		options
			.Add("test-users=",          v => test_users_file      = v)
			.Add("candidate-items=",     v => candidate_items_file = v)
			.Add("overlap-items",        v => overlap_items     = v != null)
			.Add("all-items",            v => all_items         = v != null)
			.Add("in-training-items",    v => in_training_items = v != null)
			.Add("in-test-items",        v => in_test_items     = v != null);
	}

	protected override void CheckParameters(IList<string> extra_args)
	{
		base.CheckParameters(extra_args);

		if (cross_validation > 0 && find_iter != 0)
			Abort("The combination of --cross-validation=K and --find-iter is not supported for rating-based ranking.");
	}

	protected override void LoadData()
	{
		base.LoadData();

		// test users
		if (test_users_file != null)
			test_users = user_mapping.ToInternalID( File.ReadLines(Path.Combine(data_dir, test_users_file)).ToArray() );
		else
			test_users = test_data != null ? test_data.AllUsers : training_data.AllUsers;

		// candidate items
		if (candidate_items_file != null)
			candidate_items = item_mapping.ToInternalID( File.ReadLines(Path.Combine(data_dir, candidate_items_file)).ToArray() );
		else if (all_items)
			candidate_items = Enumerable.Range(0, item_mapping.InternalIDs.Max() + 1).ToArray();

		if (candidate_items != null)
			eval_item_mode = CandidateItems.EXPLICIT;
		else if (in_training_items)
			eval_item_mode = CandidateItems.TRAINING;
		else if (in_test_items)
			eval_item_mode = CandidateItems.TEST;
		else if (overlap_items)
			eval_item_mode = CandidateItems.OVERLAP;
		else
			eval_item_mode = CandidateItems.UNION;
	}

	protected override EvaluationResults Evaluate()
	{
		int predict_items_number = -1;
		var test_data_posonly = new PosOnlyFeedback<SparseBooleanMatrix>(test_data);
		var training_data_posonly = new PosOnlyFeedback<SparseBooleanMatrix>(training_data);
		return recommender.Evaluate(
			test_data_posonly, training_data_posonly,
			test_users, candidate_items,
			eval_item_mode, RepeatedEvents.No, predict_items_number
		);
	}

	protected override EvaluationResults DoCrossValidation()
	{
		var candidate_items = new List<int>(training_data.AllItems);
		return recommender.DoRatingBasedRankingCrossValidation(cross_validation, candidate_items, CandidateItems.UNION);
	}

	static void Main(string[] args)
	{
		var program = new RatingBasedRanking();
		program.Run(args);
	}
}
