// Copyright (C) 2011, 2012 Zeno Gantner
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

using System;
using System.Collections.Generic;

/*! \namespace MyMediaLite.HyperParameter
 *  \brief This namespace contains classes for automated hyper-parameter search.
 */

namespace MyMediaLite.HyperParameter
{
	/// <summary>Interface for classes that perform hyper-parameter search</summary>
	public interface IHyperParameterSearch
	{
		// configuration properties

		/// <summary>the delegate used to compute</summary>
		Func<IRecommender, Dictionary<string, double>> EvalJob { get; }

		/// <summary>the recommender to find the hyperparameters for</summary>
		IRecommender Recommender { get; }

		/// <summary>list of (hyper-)parameters to optimize</summary>
		IList<string> Parameters { get; }

		/// <summary>the evaluation measure to optimize</summary>
		string Measure { get; }

		/// <summary>true if evaluation measure is to be maximized, false if it is to be minimized</summary>
		bool Maximize { get; }

		// status properties

		/// <summary>size of the current epoch of the hyper-parameter search</summary>
		uint EpochSize { get; }

		/// <summary>the number of steps computed so far in this hyper-parameter search</summary>
		uint NumberOfStepsComputed { get; }

		/// <summary>the best result so far</summary>
		double BestResult { get; }

		/// <summary>the (hyper-)parameter values of the best result so far</summary>
		IList<Object> BestParameterValues { get; }

		// methods

		/// <summary>compute the next step in the current epoch</summary>
		void ComputeNextStep();

		/// <summary>complete the current epoch</summary>
		void ComputeNextEpoch();
	}
}

