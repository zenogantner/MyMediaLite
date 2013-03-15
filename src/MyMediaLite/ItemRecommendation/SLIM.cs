// Copyright (C) 2012 Lucas Drumond
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
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MyMediaLite.DataType;
using MyMediaLite.Taxonomy;
using MyMediaLite.IO;
using MyMediaLite.Eval;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Abstract class for SLIM based item predictors proposed by Ning and Karypis</summary>
	/// <remarks>
	///   <para>
	///     This class only implements the prediction model presented in the original paper.
	///   </para>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         X. Ning, G. Karypis: Slim: Sparse linear methods for top-n recommender systems.
	///         ICDM 2011.
	///         http://glaros.dtc.umn.edu/gkhome/fetch/papers/SLIM2011icdm.pdf
	///       </description></item>
	/// 	</list>
	///   </para>
	/// </remarks>
	public abstract class SLIM : IncrementalItemRecommender, IIterativeModel
	{
		/// <summary>Item weight matrix (the W matrix in the original paper)</summary>
		protected Matrix<float> item_weights;

		/// <summary>Mean of the normal distribution used to initialize the latent factors</summary>
		public double InitMean { get; set; }

		/// <summary>Standard deviation of the normal distribution used to initialize the latent factors</summary>
		public double InitStdDev { get; set; }

		/// <summary>Number of iterations over the training data</summary>
		public uint NumIter { get; set; }

		/// <summary>The item KNN used in the feature selection step</summary>
		protected ItemKNN itemKNN;

		/// <summary>Default constructor</summary>
		public SLIM()
		{
			NumIter = 15;
			InitMean = 0;
			InitStdDev = 0.1;
		}

		///
		protected virtual void InitModel()
		{
			item_weights = new Matrix<float>(MaxItemID + 1, MaxItemID + 1);
			item_weights.InitNormal(InitMean, InitStdDev);

			// set diagonal elements to 0
			for(int i = 0; i <= MaxItemID; i++)
			{
				item_weights[i, i] = 0;
			}
		}

		///
		public override void Train()
		{
			InitModel();

			for (uint i = 0; i < NumIter; i++)
				Iterate();
		}

		/// <summary>Iterate once over the data</summary>
		public abstract void Iterate();

		///
		public abstract float ComputeObjective();

		///
		public override float Predict(int user_id, int item_id)
		{
			if (user_id > MaxUserID || item_id >= item_weights.dim1)
				return float.MinValue;

			var user_items = Feedback.UserMatrix.GetEntriesByRow(user_id);
			float prediction = 0;
			foreach (int item in user_items)
				prediction += item_weights[item_id, item];

			return prediction;
		}

		///
		public override void SaveModel(string file)
		{
			using ( StreamWriter writer = Model.GetWriter(file, this.GetType(), "3.05") )
			{
				writer.WriteMatrix(item_weights);
			}
		}

		///
		public override void LoadModel(string file)
		{
			using ( StreamReader reader = Model.GetReader(file, this.GetType()) )
			{
				var item_factors = (Matrix<float>) reader.ReadMatrix(new Matrix<float>(0, 0));

				this.MaxItemID = item_factors.NumberOfRows - 1;

				this.item_weights = item_factors;
			}
		}
	}
}

