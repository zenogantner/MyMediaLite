// Copyright (C) 2010, 2011, 2012, 2013 Zeno Gantner
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
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.IO;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Linear model optimized for BPR</summary>
	/// <remarks>
	///   <para>
	///   Literature:
	///   <list type="bullet">
	///     <item><description>
	///       Zeno Gantner, Lucas Drumond, Christoph Freudenthaler, Steffen Rendle, Lars Schmidt-Thieme:
	///       Learning Attribute-to-Feature Mappings for Cold-Start Recommendations.
	///       ICDM 2011.
	///       http://www.ismll.uni-hildesheim.de/pub/pdfs/Gantner_et_al2010Mapping.pdf
	///     </description></item>
	///   </list>
	/// </para>
	///
	/// <para>
	///   This recommender does NOT support incremental updates.
	/// </para>
	/// </remarks>
	public class BPRLinear : ItemRecommender, IItemAttributeAwareRecommender, IIterativeModel
	{
		///
		public IBooleanMatrix ItemAttributes
		{
			get { return this.item_attributes; }
			set {
				this.item_attributes = value;
				this.NumItemAttributes = item_attributes.NumberOfColumns;
				this.MaxItemID = Math.Max(MaxItemID, item_attributes.NumberOfRows - 1);
			}
		}
		private IBooleanMatrix item_attributes;

		///
		public int NumItemAttributes { get; private set; }

		// Item attribute weights
		private Matrix<float> item_attribute_weight_by_user;

		private System.Random random;

		/// <summary>Number of iterations over the training data</summary>
		public uint NumIter { get { return num_iter; } set { num_iter = value; } }
		private uint num_iter = 10;

 		/// <summary>mean of the Gaussian distribution used to initialize the features</summary>
		public double InitMean { get { return init_mean; } set { init_mean = value; } }
		double init_mean = 0;

		/// <summary>standard deviation of the normal distribution used to initialize the features</summary>
		public double InitStdev { get { return init_stdev; } set { init_stdev = value; } }
		double init_stdev = 0.1;

		/// <summary>Learning rate alpha</summary>
		public float LearnRate { get { return learn_rate; } set { learn_rate = value; } }
		float learn_rate = 0.05f;

		/// <summary>Regularization parameter</summary>
		public float Regularization { get { return regularization; } set { regularization = value; } }
		float regularization = 0.015f;

		///
		public override void Train()
		{
			random = MyMediaLite.Random.GetInstance();

			item_attribute_weight_by_user = new Matrix<float>(MaxUserID + 1, NumItemAttributes);

			for (uint i = 0; i < NumIter; i++)
				Iterate();
		}

		/// <summary>Perform one iteration of stochastic gradient ascent over the training data</summary>
		public void Iterate()
		{
			int num_pos_events = Feedback.Count;

			for (int i = 0; i < num_pos_events; i++)
			{
				if (i % 1000000 == 999999)
					Console.Error.Write(".");
				if (i % 100000000 == 99999999)
					Console.Error.WriteLine();

				int user_id, item_id_1, item_id_2;
				SampleTriple(out user_id, out item_id_1, out item_id_2);

				UpdateFeatures(user_id, item_id_1, item_id_2);
			}
		}

		/// <summary>Sample a pair of items, given a user</summary>
		/// <param name="u">the user ID</param>
		/// <param name="i">the ID of the first item</param>
		/// <param name="j">the ID of the second item</param>
		protected  void SampleItemPair(int u, out int i, out int j)
		{
			var user_items = Feedback.UserMatrix[u];
			i = user_items.ElementAt(random.Next (user_items.Count));
			do
				j = random.Next (0, MaxItemID + 1);
			while (Feedback.UserMatrix[u, j] || Feedback.ItemMatrix[j].Count == 0); // don't sample the item if it never has been viewed (maybe unknown item!)
		}

		/// <summary>Sample a user that has viewed at least one and not all items</summary>
		/// <returns>the user ID</returns>
		protected int SampleUser()
		{
			while (true)
			{
				int u = random.Next(MaxUserID + 1);
				var user_items = Feedback.UserMatrix[u];
				if (user_items.Count == 0 || user_items.Count == MaxItemID + 1)
					continue;
				return u;
			}
		}

		/// <summary>Sample a triple for BPR learning</summary>
		/// <param name="u">the user ID</param>
		/// <param name="i">the ID of the first item</param>
		/// <param name="j">the ID of the second item</param>
		protected void SampleTriple(out int u, out int i, out int j)
		{
			u = SampleUser();
			SampleItemPair(u, out i, out j);
		}

		/// <summary>Modified feature update method that exploits attribute sparsity</summary>
		protected virtual void UpdateFeatures(int u, int i, int j)
		{
			double x_uij = Predict(u, i) - Predict(u, j);

			ICollection<int> attr_i = item_attributes[i];
			ICollection<int> attr_j = item_attributes[j];

			// assumption: attributes are sparse
			var attr_i_over_j = new HashSet<int>(attr_i);
			attr_i_over_j.ExceptWith(attr_j);
			var attr_j_over_i = new HashSet<int>(attr_j);
			attr_j_over_i.ExceptWith(attr_i);

			double one_over_one_plus_ex = 1 / (1 + Math.Exp(x_uij));

			foreach (int a in attr_i_over_j)
			{
				float w_uf = item_attribute_weight_by_user[u, a];
				double uf_update = one_over_one_plus_ex - regularization * w_uf;
				item_attribute_weight_by_user[u, a] = (float) (w_uf + learn_rate * uf_update);
			}
			foreach (int a in attr_j_over_i)
			{
				float w_uf = item_attribute_weight_by_user[u, a];
				double uf_update = -one_over_one_plus_ex - regularization * w_uf;
				item_attribute_weight_by_user[u, a] = (float) (w_uf + learn_rate * uf_update);
			}
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			if (user_id >= item_attribute_weight_by_user.dim1)
				return float.MinValue;
			if (item_id > MaxItemID)
				return float.MinValue;

			double result = 0;
			foreach (int a in item_attributes[item_id])
				result += item_attribute_weight_by_user[user_id, a];
			return (float) result;
		}

		///
		public override void SaveModel(string filename)
		{
			using ( StreamWriter writer = Model.GetWriter(filename, this.GetType(), "2.99") )
				writer.WriteMatrix(item_attribute_weight_by_user);
		}

		///
		public override void LoadModel(string filename)
		{
			using ( StreamReader reader = Model.GetReader(filename, this.GetType()) )
				this.item_attribute_weight_by_user = (Matrix<float>) reader.ReadMatrix(new Matrix<float>(0, 0));
		}

		///
		public float ComputeObjective()
		{
			return -1;
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} reg={1} num_iter={2} learn_rate={3}",
				this.GetType().Name, Regularization, NumIter, LearnRate);
		}
	}
}