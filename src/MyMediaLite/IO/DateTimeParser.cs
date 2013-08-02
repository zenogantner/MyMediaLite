// Copyright (C) 2010, 2011, 2012, 2013 Zeno Gantner
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
using System.Globalization;

namespace MyMediaLite.IO
{
	public static class DateTimeParser
	{
		public static DateTime Parse(string date_string)
		{
			var time_split_chars = new char[] { ' ', '-', ':' };
			uint seconds;
			if (date_string.Length == 19) // format "yyyy-mm-dd hh:mm:ss"
			{
				var date_time_tokens = date_string.Split(time_split_chars);
				return new DateTime(
					int.Parse(date_time_tokens[0]),
					int.Parse(date_time_tokens[1]),
					int.Parse(date_time_tokens[2]),
					int.Parse(date_time_tokens[3]),
					int.Parse(date_time_tokens[4]),
					int.Parse(date_time_tokens[5]));
			}
			else if (date_string.Length == 10 && date_string[4] == '-') // format "yyyy-mm-dd"
			{
				var date_time_tokens = date_string.Split(time_split_chars);
				return new DateTime(
					int.Parse(date_time_tokens[0]),
					int.Parse(date_time_tokens[1]),
					int.Parse(date_time_tokens[2]));
			}
			else if (uint.TryParse(date_string, out seconds)) // unsigned integer value, interpreted as seconds since Unix epoch
			{
				var time = new DateTime(seconds * 10000000L).AddYears(1969);
				var offset = TimeZone.CurrentTimeZone.GetUtcOffset(time);
				return time - offset;
			}
			else
			{
				return DateTime.Parse(date_string, CultureInfo.InvariantCulture);
			}
		}
	}
}

