// Copyright (C) 2010 Zeno Gantner
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

using System.IO;
using MyMediaLite.correlation;
using MyMediaLite.util;


namespace MyMediaLite.item_recommender
{
	/// <summary>Base class for item recommenders that use some kind of kNN model</summary>
	public abstract class KNN : Memory
	{
		/// <summary>The number of neighbors to take into account for prediction</summary>
		public uint K {	get { return k;	} set {	k = value; } }
		/// <summary>The number of neighbors to take into account for prediction</summary>
		protected uint k = 80;
		
        /// <summary>Correlation matrix over some kind of entity</summary>
        protected CorrelationMatrix correlation;

		/// <inheritdoc />
		public override void SaveModel(string filePath)
		{
			using ( StreamWriter writer = Engine.GetWriter(filePath, this.GetType()) )
			{
				correlation.Write(writer);
			}
		}

		/// <inheritdoc />
		public override void LoadModel(string filePath)
		{
			using ( StreamReader reader = Engine.GetReader(filePath, this.GetType()) )
			{
				this.correlation = CorrelationMatrix.ReadCorrelationMatrix(reader);
			}
		}
	}
}