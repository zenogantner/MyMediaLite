// Copyright (C) 2011 Zeno Gantner
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
using Gtk;

namespace MovieDemo
{
	public class GtkSharpUtils
	{
		public static ResponseType YesNo(Window parent_window, string question)
		{
			var md = new MessageDialog (parent_window, 
												  DialogFlags.DestroyWithParent,
												  MessageType.Question, 
												  ButtonsType.YesNo, question);
			var result = (ResponseType)md.Run();
			md.Destroy();
			return result;
		}
		
		// TODO does not work correctly yet
		public static string StringInput(Window parent_window, string title)
		{
			var dialog = new Dialog(title, parent_window, Gtk.DialogFlags.DestroyWithParent);
			dialog.Modal = true;
			
			Entry text_entry = new Entry("Name");
			text_entry.Visible = true;
			
			dialog.Add( text_entry );
			dialog.AddButton("OK", ResponseType.Ok);
			dialog.AddButton("Cancel", ResponseType.Cancel);
			
			ResponseType response = (ResponseType)dialog.Run();
			
			dialog.Destroy();
			
			if (response == ResponseType.Ok)
				return text_entry.Text;
			else
				return "";
		}
	}
}