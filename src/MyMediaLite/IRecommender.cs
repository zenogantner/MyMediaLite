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

namespace MyMediaLite
{
	// Doxygen main page
	/// \mainpage MyMediaLite API Documentation
	/// You can browse the documentation by class name, class hierarchy, and member name.
	/// Just click on the "Classes" tab.
	///
	/// Please report problems and missing information to the MyMediaLite authors: http://mymedialite.net/contact.html
	///
	/// If you want to contribute to MyMediaLite have a look at http://mymedialite.net/contribute.html

	/// <summary>Generic interface for simple recommenders</summary>
	/// <remarks></remarks>
	public interface IRecommender : ICloneable
	{
		/// <summary>Predict rating or score for a given user-item combination</summary>
		/// <remarks></remarks>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <returns>the predicted score/rating for the given user-item combination</returns>
		float Predict(int user_id, int item_id);

		/// <summary>Recommend items for a given user</summary>
		/// <param name='user_id'>the user ID</param>
		/// <param name='n'>the number of items to recommend, -1 for as many as possible</param>
		/// <param name='ignore_items'>collection if items that should not be returned; if null, use empty collection</param>
		/// <param name='candidate_items'>the candidate items to choose from; if null, use all items</param>
		/// <returns>a sorted list of (item_id, score) tuples</returns>
		IList<Tuple<int, float>> Recommend(
			int user_id, int n = -1,
			ICollection<int> ignore_items = null,
			ICollection<int> candidate_items = null);

		/// <summary>Check whether a useful prediction (i.e. not using a fallback/default answer) can be made for a given user-item combination</summary>
		/// <remarks>
		/// It is up to the recommender implementor to decide when a prediction is useful,
		/// and to document it accordingly.
		/// </remarks>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <returns>true if a useful prediction can be made, false otherwise</returns>
		bool CanPredict(int user_id, int item_id);

		/// <summary>Learn the model parameters of the recommender from the training data</summary>
		/// <remarks></remarks>
		void Train();

		/// <summary>Save the model parameters to a file</summary>
		/// <remarks></remarks>
		/// <param name="filename">the name of the file to write to</param>
		void SaveModel(string filename);

		/// <summary>Get the model parameters from a file</summary>
		/// <remarks></remarks>
		/// <param name="filename">the name of the file to read from</param>
		void LoadModel(string filename);

		/// <summary>Return a string representation of the recommender</summary>
		/// <remarks>
		/// The ToString() method of recommenders should list the class name and all hyperparameters, separated by space characters.
		/// </remarks>
		string ToString();
	}
}