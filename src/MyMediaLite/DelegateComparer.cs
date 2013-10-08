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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyMediaLite
{
    internal class DelegateComparer<T> : IComparer<T>
    {
        private Comparison<T> comparison;
        
        public DelegateComparer(Comparison<T> comparison)
        {
            this.comparison = comparison;
        }
        
        public int Compare(T x, T y)
        {
            return comparison(x, y);
        }
    }
}
