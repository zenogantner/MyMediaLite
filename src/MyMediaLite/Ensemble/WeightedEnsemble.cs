// Copyright (C) 2011, 2012 Zeno Gantner
// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;

namespace MyMediaLite.Ensemble
{
	/// <summary>Combining several predictors with a weighted ensemble</summary>
	/// <remarks>
	/// This recommender does NOT support incremental updates.
	/// </remarks>
	public class WeightedEnsemble : Ensemble
	{
		/// <summary>List of component weights</summary>
		public IList<float> weights = new List<float>();

		/// <summary>Sum of the component weights</summary>
		protected double weight_sum;

		///
		public override void Train()
		{
			foreach (var recommender in this.recommenders)
				recommender.Train();

			this.weight_sum = weights.Sum();
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			double result = 0;

			for (int i = 0; i < recommenders.Count; i++)
				result += weights[i] * recommenders[i].Predict(user_id, item_id);

			return (float) (result / weight_sum);
		}

		///
		public override IList<Tuple<int, float>> Recommend(
			int user_id, int n = 20,
			ICollection<int> ignore_items = null,
			ICollection<int> candidate_items = null)
		{
			//var component_results = new IList<Tuple<int, float>>[recommenders.Count];
			//for (int i = 0; i < recommenders.Count; i++)
			//	component_results = recommenders[i].PredictItems(user_id, n, candidate_items);
			throw new NotImplementedException();
		}

		///
		public override void SaveModel(string file)
		{
			using ( StreamWriter writer = Model.GetWriter(file, this.GetType(), "2.03") )
			{
				writer.WriteLine(recommenders.Count);
				for (int i = 0; i < recommenders.Count; i++)
				{
					recommenders[i].SaveModel("model-" + i + ".txt");
					writer.WriteLine(recommenders[i].GetType() + " " + weights[i].ToString(CultureInfo.InvariantCulture));
				}
			}
		}

		///
		public override void LoadModel(string file)
		{
			using ( StreamReader reader = Model.GetReader(file, this.GetType()) )
			{
				int numberOfComponents = int.Parse(reader.ReadLine());

				var weights = new List<float>();
				var recommenders = new List<IRecommender>();

				for (int i = 0; i < numberOfComponents; i++)
				{
					string[] data = reader.ReadLine().Split(' ');

					Type t = Type.GetType(data[0]);
					recommenders.Add( (IRecommender) Activator.CreateInstance(t) );
					recommenders[i].LoadModel("model-" + i + ".txt");

					// TODO make sure the recommenders get their data?

					weights.Add(float.Parse(data[1], CultureInfo.InvariantCulture));
				}

				this.weights = weights;
				this.recommenders = recommenders;
			}
		}
	}
}
