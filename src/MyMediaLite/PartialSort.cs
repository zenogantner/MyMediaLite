// Copyright (C) 2013 Nino Antulov-Fantulin, Matej Mihelcic, Zeno Gantner
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
using System.Linq;
using System.Text;
using C5;
using MyMediaLite.DataType;
using MyMediaLite.Correlation;

namespace MyMediaLite
{
	public static class PartialSort
	{
		public static System.Collections.Generic.IList<int> Sort(System.Collections.Generic.IList<int> entities, ICorrelationMatrix c, int entity_id, int k)
		{
			var comparer = new DelegateComparer<Tuple<int, float>>( (a, b) => a.Item2.CompareTo(b.Item2) );
			var pq = new IntervalHeap<Tuple<int, float>>(k, comparer);

			for (int i = 0; i < k; i++) // TODO will break if k too small
			{
				Tuple<int, float> tempItem = new Tuple<int, float>(entities.ElementAt(i), c[entities.ElementAt(i), entity_id]);
				pq.Add(tempItem);
			}

			double minScore = pq.FindMin().Item2;

			for (int i = (int) k; i < entities.Count; i++)
			{
				Tuple<int, float> tempItem = new Tuple<int, float>(entities.ElementAt(i), c[entities.ElementAt(i), entity_id]);

				if (tempItem.Item2 >= minScore)
				{
					pq.DeleteMin();
					pq.Add(tempItem);
					minScore = pq.FindMin().Item2;
				}
			}

			int [] result = new int[k];
			for (int i = 0; i < k; i++)
				result[k - i - 1] = pq.DeleteMin().Item1;

			return result;
		}

	}
}
