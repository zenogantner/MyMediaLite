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
using MovieDemo;

public partial class MainWindow : Gtk.Window
{
	MovieLensMovieInfo movies = new MovieLensMovieInfo();
	
	NodeStore all_movies = new Gtk.NodeStore(typeof(MovieTreeNode));
	
	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		Console.Error.Write("Reading in movie data ... ");
		movies.Read("/home/mrg/data/ml10m/movies.dat");		
		Console.Error.WriteLine("done.");
		
		Build();
		
		nodeview1.AppendColumn("Movie",  new Gtk.CellRendererText(), "text", 0);
        nodeview1.AppendColumn("Rating", new Gtk.CellRendererText(), "text", 1);
		
		foreach (Movie movie in movies.movie_list)
			all_movies.AddNode(new MovieTreeNode(movie.Title, 0));
		nodeview1.NodeStore = all_movies;
		nodeview1.ShowAll();
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit();
		a.RetVal = true;
	}
	
	protected virtual void OnEntry3Changed (object sender, System.EventArgs e)
	{
		string filter = entry3.Text;
		
		if (filter.Equals(string.Empty))
		{
			nodeview1.NodeStore = all_movies;
			nodeview1.ShowAll();
			return;
		}
		
		var store = new Gtk.NodeStore(typeof(MovieTreeNode));
		foreach (Movie movie in movies.movie_list)
		{
			string title = movie.Title;
			if (title.Contains(filter))
				store.AddNode(new MovieTreeNode(title, 0));
		}
		nodeview1.NodeStore = store;
		nodeview1.ShowAll();
	}

	protected virtual void OnNodeview1ScreenChanged (object o, Gtk.ScreenChangedArgs args)
	{
    	Gtk.NodeSelection selection = (Gtk.NodeSelection) o;
        MovieTreeNode node = (MovieTreeNode) selection.SelectedNode;
        label1.Text = node.Movie;
	}
}