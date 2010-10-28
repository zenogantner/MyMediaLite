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
using System.Text;
using MyMediaLite.rating_predictor;
using MyMediaLite.util;


namespace MyMediaLite.ensemble
{
    /// <summary>
    /// Combining several predictors with a weighted ensemble.
    /// This engine does not support online updates.
    /// </summary>
    /// <author>Steffen Rendle, Zeno Gantner, Christoph Freundenthaler,
    ///         University of Hildesheim</author>
    public class WeightedEnsemble : Ensemble
    {
		// TODO add an AddEngine routine

        /// <summary>
        /// List of weights.
        /// </summary>
        public List<double> weights = new List<double>();
		protected double weight_sum;

        /// <inheritdoc />
        public override void Train()
        {
            foreach (var engine in this.engines)
                engine.Train();

			this.weight_sum = weights.Sum();
        }

		/// <inheritdoc />
        public override double Predict(int user_id, int item_id)
        {
			double result = 0;
			{
            	for (int i = 0; i < engines.Count; i++)
                	result += weights[i] * engines[i].Predict(user_id, item_id);
			}
            return (double) result / weight_sum;
        }

		/// <inheritdoc />
		public override void SaveModel(string filePath)
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			using ( StreamWriter writer = EngineStorage.GetWriter(filePath, this.GetType()) )
			{
				writer.WriteLine(engines.Count);
				for (int i = 0; i < engines.Count; i++)
				{
					engines[i].SaveModel("model-" + i + ".txt");
					writer.WriteLine(engines[i].GetType() + " " + weights[i].ToString(ni));
				}
			}
		}

		/// <inheritdoc />
		public override void LoadModel(string filePath)
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			using ( StreamReader reader = EngineStorage.GetReader(filePath, this.GetType()) )
			{

				int numberOfComponents = System.Int32.Parse(reader.ReadLine());

				List<double>                weights = new List<double>();
				List<IRecommenderEngine> engines = new List<IRecommenderEngine>();

				for (int i = 0; i < numberOfComponents; i++)
				{
					string[] data = reader.ReadLine().Split(' ');

					Type t = System.Type.GetType(data[0]);
					engines.Add( (IRecommenderEngine) Activator.CreateInstance(t) );
					engines[i].LoadModel("model-" + i + ".txt");

					// TODO make sure the engines get their data FIXME

					weights.Add(System.Double.Parse(data[1], ni));
				}

				this.weights = weights;
				this.engines = engines;
			}
		}
    }
}
