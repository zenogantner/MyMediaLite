// Copyright (C) 2010, 2011, 2012 Zeno Gantner
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
using MyMediaLite;
using MyMediaLite.ItemRecommendation;
using SVM;

namespace MyMediaLite.AttrToFactor
{
	/// <summary>BPR-MF with item mapping learned by support-vector regression (SVR)</summary>
	/// <remarks>
	/// Literature:
	/// <list type="bullet">
    ///   <item><description>
	///     Zeno Gantner, Lucas Drumond, Christoph Freudenthaler, Steffen Rendle, Lars Schmidt-Thieme:
	///     Learning Attribute-to-Feature Mappings for Cold-Start Recommendations.
	///     ICDM 2011.
	///     http://www.ismll.uni-hildesheim.de/pub/pdfs/Gantner_et_al2010Mapping.pdf
	///   </description></item>
	/// </list>
	///
	/// This recommender does NOT support incremental updates.
	/// </remarks>
	public class BPRMF_ItemMappingSVR : BPRMF_ItemMapping
	{
		/// <summary>C hyperparameter for the SVM</summary>
		public double C { get { return c; } set { c = value; } }
		double c = 1;

		/// <summary>Gamma parameter for RBF kernel</summary>
		public double Gamma { get { return gamma; } set { gamma = value; } }
		double gamma = (double) 1 / 500;

		SVM.Model[] models;

		///
		public override void LearnAttributeToFactorMapping()
		{
			var svm_features = new List<Node[]>();
			var relevant_items  = new List<int>();
			for (int i = 0; i < MaxItemID + 1; i++)
			{
				// ignore items w/o collaborative data
				if (Feedback.ItemMatrix[i].Count == 0)
					continue;
				// ignore items w/o attribute data
				if (item_attributes[i].Count == 0)
					continue;

				svm_features.Add( CreateNodes(i) );
				relevant_items.Add(i);
			}

			// TODO proper random seed initialization

			Node[][] svm_features_array = svm_features.ToArray();
			var svm_parameters = new Parameter();
			svm_parameters.SvmType = SvmType.EPSILON_SVR;
			//svm_parameters.SvmType = SvmType.NU_SVR;
			svm_parameters.C     = this.c;
			svm_parameters.Gamma = this.gamma;

			models = new Model[num_factors];
			for (int f = 0; f < num_factors; f++)
			{
				double[] targets = new double[svm_features.Count];
				for (int i = 0; i < svm_features.Count; i++)
				{
					int item_id = relevant_items[i];
					targets[i] = item_factors[item_id, f];
				}

				Problem svm_problem = new Problem(svm_features.Count, targets, svm_features_array, NumItemAttributes - 1);
				models[f] = SVM.Training.Train(svm_problem, svm_parameters);
			}

			_MapToLatentFactorSpace = Utils.Memoize<int, float[]>(__MapToLatentFactorSpace);
		}

		///
		protected override float[] __MapToLatentFactorSpace(int item_id)
		{
			var item_factors = new float[num_factors];

			for (int f = 0; f < num_factors; f++)
				item_factors[f] = (float) SVM.Prediction.Predict(models[f], CreateNodes(item_id));

			return item_factors;
		}

		private Node[] CreateNodes(int item_id)
		{
			// create attribute representation digestible by LIBSVM
			var attributes = this.item_attributes[item_id];
			var item_svm_data = new Node[attributes.Count];
			int counter = 0;
			foreach (int attr in attributes)
				item_svm_data[counter++] = new Node(attr, 1.0);
			return item_svm_data;
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} reg_u={2} reg_i={3} reg_j={4} num_iter={5} learn_rate={6} c={7} gamma={8}",
				this.GetType().Name, num_factors, reg_u, reg_i, reg_j, NumIter, learn_rate, c, gamma
			);
		}

	}
}
