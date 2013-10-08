// Copyright (C) 2013 Zeno Gantner
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
using System.IO;
using MyMediaLite.IO;

namespace MyMediaLite
{
	public class MatrixFactorization : NewModel
	{
		protected override string Version { get { return "4.00"; } }

		public int NumFactors { get; private set; }
		public float GlobalBias { get; private set; }
		public IList<float> UserBias { get; private set; }
		public IList<float> ItemBias { get; private set; }
		public IList<IList<float>> UserFactors { get; private set; }
		public IList<IList<float>> ItemFactors { get; private set; }

		public MatrixFactorization(
			int numFactors,
			float globalBias, IList<float> userBias, IList<float> itemBias,
			IList<IList<float>> userFactors, IList<IList<float>> itemFactors)
		{
			NumFactors = numFactors;
			GlobalBias = globalBias;
			UserBias = userBias;
			ItemBias = itemBias;
			UserFactors = userFactors;
			ItemFactors = itemFactors;
			// TODO assert vector sizes
		}

		public float Score(int userId, int itemId)
		{
			float score = GlobalBias + UserBias[userId] + ItemBias[itemId];
			IList<float> userFactors = UserFactors[userId];
			IList<float> itemFactors = ItemFactors[itemId];
			for (int i = 0; i < userFactors.Count; i++)
				score += userFactors[i] * itemFactors[i];
			return score;
		}

		public override void Save(TextWriter writer)
		{
			throw new NotImplementedException();
			// TODO
			writer.WriteLine(GlobalBias);
			writer.WriteVector(UserBias);
			// writer.WriteMatrix(UserFactors);
			writer.WriteVector(ItemBias);
			// writer.WriteMatrix(ItemFactors);
		}
	}
}

