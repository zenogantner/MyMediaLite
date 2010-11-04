// Copyright (C) 2010 Steffen Rendle, Zeno Gantner, Christoph Freudenthaler
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
using System.Globalization;
using System.IO;
using MyMediaLite.data_type;
using MyMediaLite.taxonomy;
using MyMediaLite.util;


namespace MyMediaLite.item_recommender
{
    /// <summary>Abstract class for matrix factorization based item predictors</summary>
    public abstract class MF : Memory, IIterativeModel, ILatentFactorModel
    {
        /// <summary>Latent user factor matrix</summary>
        protected Matrix<double> user_factors;
        /// <summary>Latent item factor matrix</summary>
        protected Matrix<double> item_factors;

        /// <summary>Mean of the normal distribution used to initialize the features</summary>
		public double InitMean { get { return init_mean; } set { init_mean = value;	} }
        /// <summary>Mean of the normal distribution used to initialize the features</summary>
        protected double init_mean = 0;

        /// <summary>Standard deviation of the normal distribution used to initialize the features</summary>
		public double InitStdev { get {	return init_stdev; } set { init_stdev = value; } }
        /// <summary>Standard deviation of the normal distribution used to initialize the features</summary>
        protected double init_stdev = 0.1;

        /// <summary>Number of latent factors per user/item</summary>
		public int NumFactors { get { return num_factors; } set { num_factors = value; } }
        /// <summary>Number of latent factors per user/item</summary>
        protected int num_factors = 10;

		/// <summary>Number of iterations over the training data</summary>
		public int NumIter { get { return num_iter; } set { num_iter = value; } }
        int num_iter = 30;

        /// <inheritdoc />
        public override void Train()
        {
			user_factors = new Matrix<double>(MaxUserID + 1, num_factors);
        	item_factors = new Matrix<double>(MaxItemID + 1, num_factors);

            MatrixUtils.InitNormal(user_factors, init_mean, init_stdev);
        	MatrixUtils.InitNormal(item_factors, init_mean, init_stdev);

			for (int i = 0; i < num_iter; i++)
				Iterate();
        }

		/// <summary>Iterate once over the data</summary>
		/// <returns>true if training should be aborted</returns>
		public abstract void Iterate();

		/// <summary>Computes the fit (optimization criterion) on the training data</summary>
		/// <returns>a double representing the fit, lower is better</returns>
		public abstract double ComputeFit();

		/// <summary>
		/// Predict the weight for a given user-item combination.
		///
		/// If the user or the item are not known to the engine, zero is returned.
		/// To avoid this behavior for unknown entities, use CanPredict() to check before.
		/// </summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <returns>the predicted weight</returns>
        public override double Predict(int user_id, int item_id)
        {
            if ((user_id < 0) || (user_id >= user_factors.dim1))
            {
                Console.Error.WriteLine("user is unknown: " + user_id);
				return 0;
            }
            if ((item_id < 0) || (item_id >= item_factors.dim1))
            {
                Console.Error.WriteLine("item is unknown: " + item_id);
				return 0;
            }

            double result = 0;
            for (int f = 0; f < num_factors; f++)
                result += user_factors[user_id, f] * item_factors[item_id, f];
            return result;
        }

		/// <inheritdoc />
		public double[] GetLatentFactors(EntityType entity_type, int id)
		{
			switch (entity_type)
			{
				case EntityType.USER:
					if (id < user_factors.dim1)
						return user_factors.GetRow(id);
					else
						throw new ArgumentException("Unknown user: " + id);
				case EntityType.ITEM:
					if (id < item_factors.dim1)
						return item_factors.GetRow(id);
					else
						throw new ArgumentException("Unknown item: " + id);
				default:
					throw new ArgumentException("Model does not contain entities of type " + entity_type.ToString());
			}
		}

		/// <inheritdoc />
		public Matrix<double> GetLatentFactors(EntityType entity_type)
		{
			switch (entity_type)
			{
				case EntityType.USER:
					return user_factors;
				case EntityType.ITEM:
					return item_factors;
				default:
					throw new ArgumentException("Model does not contain entities of type " + entity_type.ToString());
			}
		}

		/// <inheritdoc />
		public override void SaveModel(string fileName)
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			using ( StreamWriter writer = Engine.GetWriter(fileName, GetType()) )
			{
				// TODO move matrix reading and writing to the MatrixUtils class
            	writer.WriteLine(user_factors.dim1 + " " + user_factors.dim2);
            	for (int i = 0; i < user_factors.dim1; i++)
                	for (int j = 0; j < user_factors.dim2; j++)
                    	writer.WriteLine(i + " " + j + " " + user_factors[i, j].ToString(ni));

            	writer.WriteLine(item_factors.dim1 + " " + item_factors.dim2);
            	for (int i = 0; i < item_factors.dim1; i++)
                	for (int j = 0; j < item_factors.dim2; j++)
                    	writer.WriteLine(i + " " + j + " " + item_factors[i, j].ToString(ni));
			}
		}

		// TODO share code with MatrixFactorization
		/// <inheritdoc />
		public override void LoadModel(string filePath)
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

            using ( StreamReader reader = Engine.GetReader(filePath, GetType()) )
			{
            	string[] numbers = reader.ReadLine().Split(' ');
				int num_users = System.Int32.Parse(numbers[0]);
				int dim2 = System.Int32.Parse(numbers[1]);

				MaxUserID = num_users - 1;
				Matrix<double> user_feature = new Matrix<double>(num_users, dim2);
				int num_features = dim2;

            	while ((numbers = reader.ReadLine().Split(' ')).Length == 3)
            	{
					int i = System.Int32.Parse(numbers[0]);
					int j = System.Int32.Parse(numbers[1]);
					double v = System.Double.Parse(numbers[2], ni);

                	if (i >= num_users)
						throw new Exception(string.Format("Invalid user ID {0} is greater than {1}.", i, num_users - 1));
					if (j >= num_features)
						throw new Exception(string.Format("Invalid feature ID {0} is greater than {1}.", j, num_features - 1));

                	user_feature[i, j] = v;
				}

            	int num_items = System.Int32.Parse(numbers[0]);
				dim2 = System.Int32.Parse(numbers[1]);
				if (dim2 != num_features)
					throw new Exception("dim2 of item_feature must be feature_count");
				Matrix<double> item_feature = new Matrix<double>(num_items, dim2);

            	while (!reader.EndOfStream)
            	{
					numbers = reader.ReadLine().Split(' ');
					int i = System.Int32.Parse(numbers[0]);
					int j = System.Int32.Parse(numbers[1]);
					double v = System.Double.Parse(numbers[2], ni);

                	if (i >= num_items)
						throw new Exception(string.Format("Invalid item ID {0} is greater than {1}.", i, num_items - 1));
					if (j >= num_features)
						throw new Exception(string.Format("Invalid feature ID {0} is greater than {1}.", j, num_features - 1));

					item_feature[i, j] = v;
				}

				// fix MaxUserID and MaxItemID - our model does not know more
				MaxUserID = num_users - 1;
				MaxItemID = num_items - 1;

            	// assign new model
				if (this.num_factors != num_features)
				{
					Console.Error.WriteLine("Set num_features to {0}", num_features);
            		this.num_factors = num_features;
				}
            	this.user_factors = user_feature;
            	this.item_factors = item_feature;
			}
        }
    }
}
