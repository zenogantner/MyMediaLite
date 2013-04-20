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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Options;
using MyMediaLite;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Eval;
using MyMediaLite.IO;

public abstract class CommandLineProgram<T> where T:IRecommender
{
	protected T recommender;

	protected OptionSet options;

	// command line parameters
	protected string data_dir = string.Empty;
	protected string training_file;
	protected string test_file;
	protected string save_model_file;
	protected string load_model_file;
	protected string save_user_mapping_file;
	protected string save_item_mapping_file;
	protected string load_user_mapping_file;
	protected string load_item_mapping_file;
	protected string user_attributes_file;
	protected string item_attributes_file;
	protected string user_relations_file;
	protected string item_relations_file;
	protected string prediction_file;
	protected bool compute_fit = false;
	protected bool no_id_mapping = false;
	protected bool online_eval;
	protected int random_seed = -1;
	protected uint cross_validation;
	protected double test_ratio = 0;

	// recommender arguments
	protected string method              = null;
	protected string recommender_options = string.Empty;

	// help/version
	protected bool show_help     = false;
	protected bool show_measures = false;
	protected bool show_version  = false;

	// arguments for iteration search
	protected uint find_iter = 0;
	protected uint num_iter = 0;
	protected uint max_iter = 50;
	protected string measures;
	protected IList<string> eval_measures;
	protected double epsilon = 0;
	protected double cutoff;

	// ID mapping objects
	protected IMapping user_mapping = new Mapping();
	protected IMapping item_mapping = new Mapping();

	// user and item attributes
	protected IBooleanMatrix user_attributes;
	protected IBooleanMatrix item_attributes;

	// time statistics
	protected List<double> training_time_stats = new List<double>();
	protected List<double> fit_time_stats      = new List<double>();
	protected List<double> eval_time_stats     = new List<double>();

	protected abstract ICollection<string> Measures { get; }

	protected virtual void Usage(string message)
	{
		Console.WriteLine(message);
		Console.WriteLine();
		Usage(-1);
	}

	protected abstract void Usage(int exit_code);

	protected abstract void SetupOptions();

	protected abstract void ShowVersion();

	protected void ShowMeasures()
	{
		Console.WriteLine("--measures=\"LIST\"");
		Console.WriteLine();
		Console.WriteLine("LIST is comma- or space-separated list that may contain the following elements:");
		foreach (var measure in Measures)
			Console.WriteLine("  " + measure);
		Environment.Exit(0);
	}

	protected void ShowVersion(string program_name, string copyright)
	{
		var version = Assembly.GetEntryAssembly().GetName().Version;
		Console.WriteLine("MyMediaLite {0} {1}.{2:00}", program_name, version.Major, version.Minor);
		Console.WriteLine(copyright);
		Console.WriteLine("This is free software; see the source for copying conditions.  There is NO");
		Console.WriteLine("warranty; not even for MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.");
		Environment.Exit(0);
	}

	protected virtual void SetupRecommender()
	{
		// in case something went wrong ...
		if (recommender == null && method != null)
			Usage(string.Format("Unknown recommender: '{0}'", method));
		if (recommender == null && load_model_file != null)
			Abort(string.Format("Could not load model from file {0}.", load_model_file));

		recommender.Configure(recommender_options, (string msg) => { Console.Error.WriteLine(msg); Environment.Exit(-1); });

		if (recommender is INeedsMappings)
		{
			((INeedsMappings) recommender).UserMapping = user_mapping;
			((INeedsMappings) recommender).ItemMapping = item_mapping;
		}
	}

	protected virtual void CheckParameters(IList<string> extra_args)
	{
		if (cross_validation == 1)
			Abort("--cross-validation=K requires K to be at least 2.");

		if (cross_validation > 1 && test_ratio != 0)
			Abort("--cross-validation=K and --test-ratio=NUM are mutually exclusive.");

		if (cross_validation > 1 && prediction_file != null)
			Abort("--cross-validation=K and --prediction-file=FILE are mutually exclusive.");

		if (cross_validation > 1 && save_model_file != null)
			Abort("--cross-validation=K and --save-model=FILE are mutually exclusive.");

		if (cross_validation > 1 && load_model_file != null)
			Abort("--cross-validation=K and --load-model=FILE are mutually exclusive.");

		if (recommender is IUserAttributeAwareRecommender && user_attributes_file == null)
			Abort("Recommender expects --user-attributes=FILE.");

		if (recommender is IItemAttributeAwareRecommender && item_attributes_file == null)
			Abort("Recommender expects --item-attributes=FILE.");

		if (recommender is IUserRelationAwareRecommender && user_relations_file == null)
			Abort("Recommender expects --user-relations=FILE.");

		if (recommender is IItemRelationAwareRecommender && user_relations_file == null)
			Abort("Recommender expects --item-relations=FILE.");

		if (online_eval && test_file == null && test_ratio == 0 && cross_validation == 0)
			Abort("--online-evaluation needs either --test-file=FILE, --test-ratio=NUM, or cross-validation=K");

		if (online_eval && find_iter != 0)
			Abort("--online-evaluation cannot be combined with --find-iter=NUM");

		if (no_id_mapping)
		{
			if (save_user_mapping_file != null)
				Abort("--save-user-mapping=FILE and --no-id-mapping are mutually exclusive.");
			if (save_item_mapping_file != null)
				Abort("--save-item-mapping=FILE and --no-id-mapping are mutually exclusive.");
			if (load_user_mapping_file != null)
				Abort("--load-user-mapping=FILE and --no-id-mapping are mutually exclusive.");
			if (load_item_mapping_file != null)
				Abort("--load-item-mapping=FILE and --no-id-mapping are mutually exclusive.");
		}

		if (extra_args.Count > 0)
			Usage("Did not understand " + extra_args[0]);
	}

	protected virtual void Run(string[] args)
	{
		AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Handlers.UnhandledExceptionHandler);
		Console.CancelKeyPress += new ConsoleCancelEventHandler(AbortHandler);

		options = new OptionSet() {
			// string-valued options
			{ "training-file=",       v              => training_file        = v },
			{ "test-file=",           v              => test_file            = v },
			{ "recommender=",         v              => method               = v },
			{ "recommender-options=", v              => recommender_options += " " + v },
			{ "data-dir=",            v              => data_dir             = v },
			{ "user-attributes=",     v              => user_attributes_file = v },
			{ "item-attributes=",     v              => item_attributes_file = v },
			{ "user-relations=",      v              => user_relations_file  = v },
			{ "item-relations=",      v              => item_relations_file  = v },
			{ "save-model=",          v              => save_model_file      = v },
			{ "load-model=",          v              => load_model_file      = v },
			{ "save-user-mapping=",   v              => save_user_mapping_file = v },
			{ "save-item-mapping=",   v              => save_item_mapping_file = v },
			{ "load-user-mapping=",   v              => load_user_mapping_file = v },
			{ "load-item-mapping=",   v              => load_item_mapping_file = v },
			{ "prediction-file=",     v              => prediction_file      = v },
			{ "measures=",            v              => measures             = v },
			// integer-valued options
			{ "find-iter=",           (uint v)       => find_iter            = v },
			{ "max-iter=",            (uint v)       => max_iter             = v },
			{ "num-iter=",            (uint v)       => num_iter             = v },
			{ "random-seed=",         (int v)        => random_seed          = v },
			{ "cross-validation=",    (uint v)       => cross_validation     = v },
			// double-valued options
			{ "epsilon=",             (double v)     => epsilon              = v },
			{ "cutoff=",              (double v)     => cutoff               = v },
			{ "test-ratio=",          (double v)     => test_ratio           = v },
			// boolean options
			{ "compute-fit",          v => compute_fit       = v != null },
			{ "online-evaluation",    v => online_eval       = v != null },
			{ "no-id-mapping",        v => no_id_mapping     = v != null },
			{ "help",                 v => show_help         = v != null },
			{ "help-measures",        v => show_measures     = v != null },
			{ "version",              v => show_version      = v != null },
		};
		SetupOptions();

		IList<string> extra_args = options.Parse(args);
		if (show_version)
			ShowVersion();
		if (show_measures)
			ShowMeasures();
		if (show_help)
			Usage(0);

		if (random_seed != -1)
			MyMediaLite.Random.Seed = random_seed;

		if (no_id_mapping)
		{
			user_mapping = new IdentityMapping();
			item_mapping = new IdentityMapping();
		}
		if (load_user_mapping_file != null)
			user_mapping = load_user_mapping_file.LoadMapping();
		if (load_item_mapping_file != null)
			item_mapping = load_item_mapping_file.LoadMapping();

		if (measures != null)
			eval_measures = measures.Split(' ', ',');

		SetupRecommender();
		CheckParameters(extra_args);

		// load all the data
		LoadData();
		SaveIDMappings();
	}

	protected void SaveIDMappings()
	{
		// if requested, save ID mappings
		if (save_user_mapping_file != null)
			user_mapping.SaveMapping(save_user_mapping_file);
		if (save_item_mapping_file != null)
			item_mapping.SaveMapping(save_item_mapping_file);
	}

	protected virtual void LoadData()
	{
		training_file = Path.Combine(data_dir, training_file);
		if (test_file != null)
			test_file = Path.Combine(data_dir, test_file);

		// user attributes
		if (user_attributes_file != null)
			user_attributes = AttributeData.Read(Path.Combine(data_dir, user_attributes_file), user_mapping);
		if (recommender is IUserAttributeAwareRecommender)
			((IUserAttributeAwareRecommender)recommender).UserAttributes = user_attributes;

		// item attributes
		if (item_attributes_file != null)
			item_attributes = AttributeData.Read(Path.Combine(data_dir, item_attributes_file), item_mapping);
		if (recommender is IItemAttributeAwareRecommender)
			((IItemAttributeAwareRecommender)recommender).ItemAttributes = item_attributes;

		// user relation
		if (recommender is IUserRelationAwareRecommender)
		{
			((IUserRelationAwareRecommender)recommender).UserRelation = RelationData.Read(Path.Combine(data_dir, user_relations_file), user_mapping);
			Console.WriteLine("relation over {0} users", ((IUserRelationAwareRecommender)recommender).NumUsers);
		}

		// item relation
		if (recommender is IItemRelationAwareRecommender)
		{
			((IItemRelationAwareRecommender)recommender).ItemRelation = RelationData.Read(Path.Combine(data_dir, item_relations_file), item_mapping);
			Console.WriteLine("relation over {0} items", ((IItemRelationAwareRecommender)recommender).NumItems);
		}
	}

	protected string Render(EvaluationResults results)
	{
		results.MeasuresToShow = eval_measures;
		return results.ToString();
	}

	protected void Abort(string message)
	{
		Console.Error.WriteLine(message);
		Environment.Exit(-1);
	}

	protected void AbortHandler(object sender, ConsoleCancelEventArgs args)
	{
		DisplayStats();
	}

	protected void DisplayStats()
	{
		if (training_time_stats.Count > 0)
			Console.Error.WriteLine(
				string.Format(
					CultureInfo.InvariantCulture,
					"iteration_time: min={0:0.##}, max={1:0.##}, avg={2:0.##}",
					training_time_stats.Min(), training_time_stats.Max(), training_time_stats.Average()));
		if (eval_time_stats.Count > 0)
			Console.Error.WriteLine(
				string.Format(
					CultureInfo.InvariantCulture,
					"eval_time: min={0:0.##}, max={1:0.##}, avg={2:0.##}",
					eval_time_stats.Min(), eval_time_stats.Max(), eval_time_stats.Average()));
		if (compute_fit && fit_time_stats.Count > 0)
			Console.Error.WriteLine(
				string.Format(
					CultureInfo.InvariantCulture,
					"fit_time: min={0:0.##}, max={1:0.##}, avg={2:0.##}",
					fit_time_stats.Min(), fit_time_stats.Max(), fit_time_stats.Average()));
		Console.Error.WriteLine("memory {0}", Memory.Usage);
	}
}

