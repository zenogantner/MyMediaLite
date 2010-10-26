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
    /// Abtract class for combining several prediction methods.
    /// </summary>
    /// <author>Steffen Rendle, University of Hildesheim</author>
    public abstract class Ensemble : RecommenderEngine
	{
        /// <summary>
        /// List of engines.
        /// </summary>
        public List<RecommenderEngine> engines = new List<RecommenderEngine>();

        private double max_rating_value = 5;
        private double min_rating_value = 1;

		/// <inheritdoc />
		public abstract double Predict(int user_id, int item_id);

		/// <inheritdoc />
		public abstract void SaveModel(string filePath);
		/// <inheritdoc />
		public abstract void LoadModel(string filePath);

        /// <inheritdoc />
        public virtual void Train()
        {
            foreach (RecommenderEngine engine in engines)
                engine.Train();
        }


        /// <summary>
        /// Gets or sets the max rating value
        /// </summary>
        /// <value>The max rating value</value>
        public double MaxRatingValue
        {
            get
            {
                return this.max_rating_value;
            }
            set
            {
                this.max_rating_value = value;
				foreach (RecommenderEngine engine in engines)
					if (engine is RatingPredictor)
						((RatingPredictor)engine).MaxRating = value;
            }
        }

        /// <summary>
        /// Gets or sets the min rating value.
        /// </summary>
        /// <value>The min rating value.</value>
        public double MinRatingValue
        {
            get
            {
                return this.min_rating_value;
            }
            set
            {
                this.min_rating_value = value;
				foreach (RecommenderEngine engine in engines)
					if (engine is RatingPredictor)
						((RatingPredictor)engine).MinRating = value;

            }
        }
    }
}
