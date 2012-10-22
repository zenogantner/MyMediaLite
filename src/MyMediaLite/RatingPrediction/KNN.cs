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
using System.Globalization;
using System.IO;
using MyMediaLite.Correlation;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.IO;
using MyMediaLite.Taxonomy;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Base class for rating predictors that use some kind of kNN</summary>
	/// <remarks>
	///   <para>
	///     The method is described in section 2.2 of the paper below.
	///     One difference is that we support several iterations of alternating optimization,
	///     instead of just one.
	///   </para>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         Yehuda Koren: Factor in the Neighbors: Scalable and Accurate Collaborative Filtering,
	///         Transactions on Knowledge Discovery from Data (TKDD), 2009.
	///         http://public.research.att.com/~volinsky/netflix/factorizedNeighborhood.pdf
	///       </description></item>
	///     </list>
	///   </para>
	///   <seealso cref="MyMediaLite.ItemRecommendation.KNN"/>
	/// </remarks>
	public abstract class KNN : IncrementalRatingPredictor
	{
		/// <summary>Number of neighbors to take into account for predictions</summary>
		public uint K { get { return k; } set { k = value; } }
		private uint k = 80;

		///
		public override IRatings Ratings
		{
			set {
				base.Ratings = value;
				baseline_predictor.Ratings = value;
			}
		}

		/// <summary>The kind of correlation to use</summary>
		public RatingCorrelationType Correlation { get; set; }

		/// <summary>The entity type of the neighbors used for rating prediction</summary>
		abstract protected EntityType Entity { get; }

		/// <summary>Return the data matrix that can be used to compute a correlation based on binary data</summary>
		/// <remarks>If a purely rating-based correlation is used, this property is ignored.</remarks>
		abstract protected IBooleanMatrix BinaryDataMatrix { get; }

		/// <summary>regularization constant for the user bias of the underlying baseline predictor</summary>
		public float RegU { get { return baseline_predictor.RegU; } set { baseline_predictor.RegU = value; } }

		/// <summary>regularization constant for the item bias of the underlying baseline predictor</summary>
		public float RegI { get { return baseline_predictor.RegI; } set { baseline_predictor.RegI = value; } }

		/// <summary>number of iterations used for training the underlying baseline predictor</summary>
		public uint NumIter { get { return baseline_predictor.NumIter; } set { baseline_predictor.NumIter = value; } }

		/// <summary>Correlation matrix over some kind of entity</summary>
		protected ICorrelationMatrix correlation;

		/// <summary>Alpha parameter for BidirectionalConditionalProbability, or shrinkage parameter for Pearson</summary>
		public float Alpha { get; set; }

		/// <summary>If set to true, give a lower weight to evidence coming from very frequent entities</summary>
		public bool WeightedBinary { get; set; }

		/// <summary>underlying baseline predictor</summary>
		protected UserItemBaseline baseline_predictor = new UserItemBaseline();

		void InitModel()
		{
			int num_entities = 0;
			switch (Correlation)
			{
				case RatingCorrelationType.BinaryCosine:
					correlation = new BinaryCosine(num_entities);
					break;
				case RatingCorrelationType.Jaccard:
					correlation = new Jaccard(num_entities);
					break;
				case RatingCorrelationType.ConditionalProbability:
					correlation = new ConditionalProbability(num_entities);
					break;
				case RatingCorrelationType.BidirectionalConditionalProbability:
					correlation = new BidirectionalConditionalProbability(num_entities, Alpha);
					break;
				case RatingCorrelationType.Cooccurrence:
					correlation = new Cooccurrence(num_entities);
					break;
				case RatingCorrelationType.Pearson:
					correlation = new Pearson(num_entities, Alpha);
					break;
				default:
					throw new NotImplementedException(string.Format("Support for {0} is not implemented", Correlation));
			}
			if (correlation is IBinaryDataCorrelationMatrix)
				((IBinaryDataCorrelationMatrix) correlation).Weighted = WeightedBinary;
		}

		///
		public override void Train()
		{
			baseline_predictor.Train();
			InitModel();
			if (correlation is IBinaryDataCorrelationMatrix)
				((IBinaryDataCorrelationMatrix) correlation).ComputeCorrelations(BinaryDataMatrix);
			else
				((IRatingCorrelationMatrix) correlation).ComputeCorrelations(Ratings, Entity);
		}

		///
		public override void SaveModel(string filename)
		{
			baseline_predictor.SaveModel(filename + "-global-effects");

			using ( StreamWriter writer = Model.GetWriter(filename, this.GetType(), "3.03") )
			{
				writer.WriteLine(Correlation);
				correlation.Write(writer);
			}
		}

		///
		public override void LoadModel(string filename)
		{
			baseline_predictor.LoadModel(filename + "-global-effects");

			using ( StreamReader reader = Model.GetReader(filename, this.GetType()) )
			{
				Correlation = (RatingCorrelationType) Enum.Parse(typeof(RatingCorrelationType), reader.ReadLine());
				InitModel();

				if (correlation is SymmetricCorrelationMatrix)
					((SymmetricCorrelationMatrix) correlation).ReadSymmetricCorrelationMatrix(reader);
				else if (correlation is AsymmetricCorrelationMatrix)
					((AsymmetricCorrelationMatrix) correlation).ReadAsymmetricCorrelationMatrix(reader);
				else
					throw new NotSupportedException("Unsupported correlation type: " + correlation.GetType());
			}
		}

		///
		public override string ToString()
		{
			return string.Format(
				"{0} k={1} correlation={2} weighted_binary={3} alpha={4} (only for BidirectionalConditionalProbability and Pearson); baseline predictor: reg_u={5} reg_i={6} num_iter={7}",
				this.GetType().Name, k == uint.MaxValue ? "inf" : k.ToString(), Correlation, WeightedBinary, Alpha, RegU, RegI, NumIter);
		}
	}
}