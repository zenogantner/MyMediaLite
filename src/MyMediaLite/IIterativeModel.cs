// Copyright (C) 2010, 2011 Zeno Gantner
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

namespace MyMediaLite
{
	/// <summary>Interface representing iteratively trained models</summary>
	/// <remarks></remarks>
	public interface IIterativeModel : IRecommender
	{
		/// <summary>Number of iterations to run the training</summary>
		uint NumIter { get; set; }

		/// <summary>Run one iteration (= pass over the training data)</summary>
		void Iterate();

		/// <summary>Compute the fit (RMSE) on the training data</summary>
		/// <returns>the fit (RMSE) on the training data according to the optimization criterion; -1 if not implemented</returns>
		double ComputeFit();
	}
}

