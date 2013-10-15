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
using System.Linq;
using MyMediaLite;
using MyMediaLite.Data;

namespace MyMediaLite.Program
{
	public class Train : Command
	{
		IMethod Method { get; set; }
		string DataFilename { get; set; }
		string ModelFilename { get; set; }
		IMapping UserMapping { get; set; }
		IMapping ItemMapping { get; set; }

		public override string Description
		{
			get {
				return "Train recommender";
			}
		}

		public override string Usage
		{
			get {
				return "train <recommender> <data-file> [<model-file>]";
			}
		}

		IInteractions LoadData()
		{
			UserMapping = new Mapping();
			ItemMapping = new Mapping();
			var interactions = Interactions.FromFile(DataFilename, UserMapping, ItemMapping);
			return interactions;
		}

		public override void Run()
		{
			var interactions = LoadData();
			var model = Method.Train(interactions, null); // TODO add parameters
			model.Save(ModelFilename); // TODO also save mappings
		}

		public override void Configure(string[] args)
		{
			Method = new MethodFactory()[args[0]];
			DataFilename = args[1];
			if (args.Length > 2)
				ModelFilename = args[2];
			else
				ModelFilename = args[0] + ".model";
		}
	}
}
