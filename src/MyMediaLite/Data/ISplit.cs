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
// You should have received a copy of the GNU General Public License
// along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace MyMediaLite.Data
{
	/// <summary>generic dataset splitter interface</summary>
	/// <typeparam name="T">the kind of dataset that is split into pieces</typeparam>
	public interface ISplit<T>
	{
		/// <summary>The number of folds in this split</summary>
		/// <value>The number of folds in this split</value>
		uint NumberOfFolds { get; }
		
		/// <summary>Training data for the different folds</summary>
		/// <value>A list of T</value>
		IList<T> Train { get; }

		/// <summary>Test data for the different folds</summary>
		/// <value>A list of T</value>
		IList<T> Test { get; }
	}
}

