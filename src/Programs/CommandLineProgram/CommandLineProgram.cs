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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MyMediaLite.Util;

public class CommandLineProgram
{
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

	protected bool compute_fit = false;

	// time statistics
	protected List<double> training_time_stats = new List<double>();
	protected List<double> fit_time_stats      = new List<double>();
	protected List<double> eval_time_stats     = new List<double>();

	static void Main(string[] args)
	{
		// do nothing
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

