// Copyright (C) 2012 Zeno Gantner
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
// 
namespace MyMediaLite.Correlation
{
	/// <summary>Correlations based on rating data</summary>
	public enum RatingCorrelationType
	{
		/// <summary>binary cosine similarity</summary>
		BinaryCosine,
		/// <summary>Jaccard index (Tanimoto coefficient)</summary>
		Jaccard,
		/// <summary>conditional probability</summary>
		ConditionalProbability,
		/// <summary>bidirectional conditional probability</summary>
		BidirectionalConditionalProbability,
		/// <summary>cooccurrence counts</summary>
		Cooccurrence,
		/// <summary>use a similarity provider to get the correlation</summary>
		SimilarityProvider,
		/// <summary>use stored/precomputed correlation</summary>
		Stored,
		/// <summary>Pearson correlation</summary>
		Pearson
	}
}

