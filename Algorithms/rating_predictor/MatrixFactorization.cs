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
using MyMediaLite.data;
using MyMediaLite.data_type;
using MyMediaLite.util;


namespace MyMediaLite.rating_predictor
{
	/// <summary>
	/// Matrix factorization engine with explicit user and item bias.
	///
	/// In the future, this engine will replace the default MF engine in the MyMedia framework.
	/// </summary>
	/// <author>Zeno Gantner, University of Hildesheim</author>
	public class BiasedMatrixFactorization : MatrixFactorization
	{
		// TODO make one MF class for both ItemRecommender and RatingPredictor (which need to be turned into interfaces for that ...)
		// TODO think about de-activating/separating regularization for the user and item bias

		/// <inheritdoc />
        protected override void _Init()
		{
			base._Init();
			if (num_features < 2)
				throw new ArgumentException("num_features must be >= 2");
        	this.user_feature.SetColumnToOneValue(0, 1);
			this.item_feature.SetColumnToOneValue(1, 1);
		}

		/// <inheritdoc />
		public override void iterate(Ratings ratings, bool update_user, bool update_item)
		{
			double rating_range_size = MaxRatingValue - MinRatingValue;

			foreach (RatingEvent rating in ratings)
            {
            	int u = rating.user_id;
                int i = rating.item_id;

				double dot_product = 0;
	            for (int f = 0; f < num_features; f++) {
    	            dot_product += user_feature.Get(u, f) * item_feature.Get(i, f);
        	    }
				double sig_dot = 1 / (1 + Math.Exp(-dot_product));

				double r = rating.rating;
                double p = MinRatingValue + sig_dot * rating_range_size;
				double err = r - p;

				double gradient_common = err * sig_dot * (1 - sig_dot) * rating_range_size;

				// Adjust features
                for (int f = 0; f < num_features; f++)
                {
                 	double u_f = user_feature.Get(u, f);
                    double i_f = item_feature.Get(i, f);

                    if (update_user && f != 0)
					{
						double delta_u = gradient_common * i_f;
						if (f != 1)
							delta_u -= regularization * u_f;
						MatrixUtils.Inc(user_feature, u, f, learn_rate * delta_u);
					}
                    if (update_item && f != 1)
					{
						double delta_i = gradient_common * u_f;
						if (f != 0)
							delta_i -= regularization * i_f;
						MatrixUtils.Inc(item_feature, i, f, learn_rate * delta_i);
					}
                }
            }
		}

        /// <inheritdoc />
        public override double Predict(int user_id, int item_id)
        {
            if (user_id >= user_feature.dim1) {
				return bias;
			}
            if (item_id >= item_feature.dim1) {
				return bias;
			}

            double dot_product = 0;

            // U*V
            for (int f = 0; f < num_features; f++) {
                dot_product += user_feature.Get(user_id, f) * item_feature.Get(item_id, f);
            }

			double result = MinRatingValue + ( 1 / (1 + Math.Exp(-dot_product)) ) * (MaxRatingValue - MinRatingValue);

            return result;
        }

		/// <inheritdoc />
		public override string ToString()
		{
			return String.Format("biased-matrix-factorization num_features={0}, regularization={1}, learn_rate={2}, num_iter={3}, init_f_mean={4}, init_f_stdev={5}",
				                 num_features, regularization, learn_rate, num_iter, init_f_mean, init_f_stdev);
		}
	}

    /// <remarks>
    /// Factorizing the observed rating values using a feature matrix for users and one for items.
    /// This class can update the factorization online.
    ///
    /// After training, an ArithmeticException is thrown if there are NaN values in the model.
    /// NaN values occur if values become too large or too small to be represented by the type double.
    /// If you encounter such problems, there are two ways to fix them:
    /// (1) Change the range of rating values (1 to 5 works generally well with the default settings)
    /// (2) Change the learn_rate (decrease it if your range is larger than 1 to 5)
    /// </remarks>
    /// <author>Steffen Rendle, Christoph Freudenthaler, Zeno Gantner, University of Hildesheim</author>
    public class MatrixFactorization : Memory
    {
        protected Matrix<double> user_feature;
        protected Matrix<double> item_feature;
        protected double bias;

        /// <summary>Number of latent features</summary>
        public int num_features = 10;
        /// <summary>Learn rate</summary>
        public double learn_rate = 0.01;
        /// <summary>Regularization parameter</summary>
        public double regularization = 0.015;
	    /// <summary>Mean of the normal distribution used to initialize the features</summary>
        public double init_f_mean = 0;
        /// <summary>Standard deviation of the normal distribution used to initialize the features</summary>
        public double init_f_stdev = 0.1;
        /// <summary>Number of iterations over the training data</summary>
		public int num_iter = 30;

        public bool update_item_features = true;
        public bool update_user_features = true;

        /// <inheritdoc />
        public override void Train()
        {
            _Init();

            // learn model parameters
            bias = ratings.all.Average;
            LearnFeatures(ratings.all, true, true);

			// check for NaN in the model
			if (MatrixUtils.ContainsNaN(user_feature))
				throw new ArithmeticException("user_feature contains NaN");
			if (MatrixUtils.ContainsNaN(item_feature))
				throw new ArithmeticException("item_feature contains NaN");
        }

		/// <summary>init feature matrices</summary>
        protected virtual void _Init()
		{
        	user_feature = new Matrix<double>(ratings.max_user_id + 1, num_features);
        	item_feature = new Matrix<double>(ratings.max_item_id + 1, num_features);
        	MatrixUtils.InitNormal(user_feature, init_f_mean, init_f_stdev);
        	MatrixUtils.InitNormal(item_feature, init_f_mean, init_f_stdev);
		}

        /// <summary>
        /// Updates the latent features on a user
        /// </summary>
        /// <param name="user_id">the user ID</param>
        public void RetrainUser(int user_id)
        {
            //if (!update_user_features) { return; }
            MatrixUtils.InitNormal(user_feature, init_f_mean, init_f_stdev, user_id);
            LearnFeatures(ratings.byUser[(int)user_id], true, false);
        }

        /// <summary>Updates the latent features of an item</summary>
        /// <param name="item_id">the item ID</param>
        public void RetrainItem(int item_id)
        {
            //if (!update_item_features) { return; }
            MatrixUtils.InitNormal(item_feature, init_f_mean, init_f_stdev, item_id);
            LearnFeatures(ratings.byItem[(int)item_id], false, true);
        }

		public virtual void iterate(Ratings ratings, bool update_user, bool update_item)
		{
			foreach (RatingEvent rating in ratings)
            {
            	int u = rating.user_id;
                int i = rating.item_id;

				double r = rating.rating;
                double p = Predict(u, i, false);

				double err = r - p;

                 // Adjust features
                 for (int f = 0; f < num_features; f++)
                 {
                 	double u_f = user_feature.Get(u, f);
                    double i_f = item_feature.Get(i, f);

                    double delta_u = (err * i_f - regularization * u_f);
                    double delta_i = (err * u_f - regularization * i_f);

                    if (update_user) MatrixUtils.Inc(user_feature, u, f, learn_rate * delta_u);
                    if (update_item) MatrixUtils.Inc(item_feature, i, f, learn_rate * delta_i);
                 }
            }
		}

        private void LearnFeatures(Ratings rating_set, bool update_user, bool update_item)
        {
            for (int current_iter = 0; current_iter < num_iter; current_iter++)
            {
				iterate(rating_set, update_user, update_item);
            }
        }

        /// <inheritdoc />
        protected double Predict(int user_id, int item_id, bool bound)
        {
            double result = bias;

            // U*V
            for (int f = 0; f < num_features; f++) {
                result += user_feature.Get(user_id, f) * item_feature.Get(item_id, f);
            }

            if (bound) {
                if (result > MaxRatingValue)
					result = MaxRatingValue;
                if (result < MinRatingValue)
					result = MinRatingValue;
            }
            return result;
        }

		/// <summary>
		/// Predict the rating of a given user for a given item.
		///
		/// If the user or the item are not known to the engine, the global average is returned.
		/// To avoid this behavior for unknown entities, use CanPredictRating() to check before.
		/// </summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <returns>the predicted rating</returns>
        public override double Predict(int user_id, int item_id)
        {
            if (user_id >= user_feature.dim1) {
				return bias;
			}
            if (item_id >= item_feature.dim1) {
				return bias;
			}

            return Predict(user_id, item_id, true);
        }

        /// <inheritdoc/>
        public override void AddRating(int user_id, int item_id, double rating)
        {
			base.AddRating(user_id, item_id, rating);
            RetrainUser(user_id);
            RetrainItem(item_id);
        }

        /// <inheritdoc/>
        public override void UpdateRating(int user_id, int item_id, double rating)
        {
			base.UpdateRating(user_id, item_id, rating);
            RetrainUser(user_id);
            RetrainItem(item_id);
        }

        /// <inheritdoc/>
        public override void RemoveRating(int user_id, int item_id)
        {
			base.RemoveRating(user_id, item_id);
            RetrainUser(user_id);
            RetrainItem(item_id);
        }

        /// <inheritdoc/>
        public override void AddUser(int user_id)
        {
			if (user_id > MaxUserID)
			{
            	base.AddUser(user_id);
				user_feature.AddRows(user_id + 1);
            	MatrixUtils.InitNormal(user_feature, init_f_mean, init_f_stdev, user_id);
			}
        }

        /// <inheritdoc/>
        public override void AddItem(int item_id)
        {
			if (item_id > MaxItemID)
			{
            	base.AddItem(item_id);
				item_feature.AddRows(item_id + 1);
            	MatrixUtils.InitNormal(item_feature, init_f_mean, init_f_stdev, item_id);
			}
        }

        /// <inheritdoc/>
        public override void RemoveUser(int user_id)
        {
            base.RemoveUser(user_id);
            // set user features to zero
            user_feature.SetRowToOneValue(user_id, 0);
        }

        /// <inheritdoc/>
        public override void RemoveItem(int item_id)
        {
            base.RemoveItem(item_id);
            // set item features to zero
            item_feature.SetRowToOneValue(item_id, 0);
        }

        /// <inheritdoc />
		public override void SaveModel(string filePath)
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			using ( StreamWriter writer = EngineStorage.GetWriter(filePath, this.GetType()) )
			{
            	writer.WriteLine(bias.ToString(ni));

            	writer.WriteLine(user_feature.dim1 + " " + user_feature.dim2);
            	for (int i = 0; i < user_feature.dim1; i++)
                	for (int j = 0; j < user_feature.dim2; j++)
                    	writer.WriteLine(i + " " + j + " " + user_feature.Get(i, j).ToString(ni));

            	writer.WriteLine(item_feature.dim1 + " " + item_feature.dim2);
            	for (int i = 0; i < item_feature.dim1; i++)
                	for (int j = 0; j < item_feature.dim2; j++)
                    	writer.WriteLine(i + " " + j + " " + item_feature.Get(i, j).ToString(ni));
			}
		}

		/// <inheritdoc />
		public override void LoadModel(string filePath)
        {
            NumberFormatInfo ni = new NumberFormatInfo();
            ni.NumberDecimalDigits = '.';

            using ( StreamReader reader = EngineStorage.GetReader(filePath, this.GetType()) )
			{
            	double bias = System.Double.Parse(reader.ReadLine(), ni);

            	string[] numbers = reader.ReadLine().Split(' ');
            	int num_users         = System.Int32.Parse(numbers[0]);
            	int num_user_features = System.Int32.Parse(numbers[1]);
            	Matrix<double> user_feature = new Matrix<double>(num_users, num_user_features);

            	while ((numbers = reader.ReadLine().Split(' ')).Length == 3)
            	{
                	int i = System.Int32.Parse(numbers[0]);
                	int j = System.Int32.Parse(numbers[1]);
                	double v = System.Double.Parse(numbers[2], ni);

                	if (i >= num_users)
	                    throw new Exception("i = " + i);
	                if (j >= num_user_features)
	                    throw new Exception("j = " + j);

	                user_feature.Set(i, j, v);
    	        }

        	    int num_items         = System.Int32.Parse(numbers[0]);
            	int num_item_features = System.Int32.Parse(numbers[1]);
            	if (num_user_features != num_item_features)
                	throw new Exception(String.Format("Number of user and item features must match.", num_user_features, num_item_features));

            	Matrix<double> item_feature = new Matrix<double>(num_items, num_item_features);

            	while (!reader.EndOfStream)
            	{
                	numbers = reader.ReadLine().Split(' ');
                	int i = System.Int32.Parse(numbers[0]);
                	int j = System.Int32.Parse(numbers[1]);
                	double v = System.Double.Parse(numbers[2], ni);

                	if (i >= num_items)
                    	throw new Exception("i = " + i);
	                if (j >= num_item_features)
	                    throw new Exception("j = " + j);

	                item_feature.Set(i, j, v);
    	        }

				// TODO maybe we could release some data to save memory
				this.MaxUserID = num_users - 1;
				this.MaxItemID = num_items - 1;

            	// assign new model
            	this.bias = bias;
				if (this.num_features != num_user_features)
				{
					Console.Error.WriteLine("Set num_features to {0}", num_user_features);
            		this.num_features = num_user_features;
				}
            	this.user_feature = user_feature;
            	this.item_feature = item_feature;
			}
        }

		/// <summary>Compute approximated fit (RMSE) on the training data</summary>
		/// <returns>the root mean square error (RMSE) on the training data</returns>
		public double ComputeFit()
		{
			double rmse_sum = 0;
			foreach (RatingEvent rating in ratings.all)
            {
				rmse_sum += Math.Pow(Predict(rating.user_id, rating.item_id) - rating.rating, 2);
			}
			return Math.Sqrt((double) rmse_sum / ratings.all.Count);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return String.Format("matrix-factorization num_features={0} regularization={1} learn_rate={2} num_iter={3} init_f_mean={4} init_f_stdev={5}",
				                 num_features, regularization, learn_rate, num_iter, init_f_mean, init_f_stdev);
		}
    }
}
