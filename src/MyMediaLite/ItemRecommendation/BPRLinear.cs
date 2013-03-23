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
using MyMediaLite.ItemRecommendation.BPR;

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
		/// <summary>Sample uniformly from users</summary>
		public bool UniformUserSampling { get; set; }
		
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
		
		protected virtual IBPRSampler CreateBPRSampler()
		{
			if (UniformUserSampling)
				return new UniformUserSampler(Interactions);
			else
				return new UniformPairSampler(Interactions);
		}
		
		/// <summary>Perform one iteration of stochastic gradient ascent over the training data</summary>
		public void Iterate()
		{
			int num_pos_events = Interactions.Count;
			int user_id, pos_item_id, neg_item_id;

			var bpr_sampler = CreateBPRSampler();
			for (int i = 0; i < num_pos_events; i++)
			{
				bpr_sampler.NextTriple(out user_id, out pos_item_id, out neg_item_id);
				UpdateParameters(user_id, pos_item_id, neg_item_id);
			}
		}

		/// <summary>Modified feature update method that exploits attribute sparsity</summary>
		protected virtual void UpdateParameters(int u, int i, int j)
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
			// TODO regularize more attributes?
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
				"{0} reg={1} num_iter={2} learn_rate={3} uniform_user_sampling={4}",
				this.GetType().Name, Regularization, NumIter, LearnRate, UniformUserSampling);
		}
	}
}