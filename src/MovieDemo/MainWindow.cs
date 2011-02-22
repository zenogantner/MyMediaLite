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
using MyMediaLite.Data;
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;

public partial class MainWindow : Gtk.Window
{
	// TODO put application logic into one object (not the MainWindow)
	
	MovieLensMovieInfo movies = new MovieLensMovieInfo();
	
	NodeStore all_movies = new Gtk.NodeStore(typeof(MovieTreeNode));
	
	RatingData training_data;
	
	//RatingPredictor rating_predictor = new BiasedMatrixFactorization();
	RatingPredictor rating_predictor = new UserItemBaseline();
	
	EntityMapping user_mapping = new EntityMapping();
	EntityMapping item_mapping = new EntityMapping();
	
	// ID of the currently selected movie
	int currentID;
	
	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		
		
		// TODO integrate internal IDs
		Console.Error.Write("Reading in movie data ... ");
		movies.Read("/home/mrg/data/ml10m/movies.dat"); // TODO param
		Console.Error.WriteLine("done.");
		
		Console.Error.Write("Reading in ratings ... "); // TODO param
		//training_data = MovieLensRatingData.Read("/home/mrg/data/ml10m/ratings.dat", 0, 5, user_mapping, item_mapping);
		training_data = MovieLensRatingData.Read("/home/mrg/data/ml1m/original/ratings.dat", 1, 5, user_mapping, item_mapping);
		rating_predictor.Ratings = training_data;		
		Console.Error.WriteLine("done.");
		
		Console.Error.Write("Training ... ");
		rating_predictor.Train();
		Console.Error.WriteLine("done.");
		// TODO have option of loading from file
		
		// build main window
		Build();
		
        nodeview1.AppendColumn("Rating", new Gtk.CellRendererText(), "text", 0);		
		nodeview1.AppendColumn("Movie",  new Gtk.CellRendererText(), "text", 1);
		
		foreach (Movie movie in movies.movie_list)
			all_movies.AddNode(new MovieTreeNode(movie.Title, "", movie.ID));
		nodeview1.NodeStore = all_movies;
		
		nodeview1.NodeSelection.Changed += new System.EventHandler(this.OnNodeview1SelectionChanged);
		
		nodeview1.ShowAll();
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();
		a.RetVal = true;
	}
	
	protected virtual void OnEntry3Changed(object sender, System.EventArgs e)
	{
		string filter = entry3.Text;
		Console.Error.WriteLine("Filter: '{0}'", filter);
		
		if (filter.Equals(string.Empty))
		{
			nodeview1.NodeStore = all_movies;
			nodeview1.ShowAll();
			return;
		}
		
		var store = new Gtk.NodeStore(typeof(MovieTreeNode));
		foreach (MovieTreeNode node in all_movies)
		{
			if (node.Movie.Contains(filter))
				store.AddNode(node);
		}
		//nodeview1.NodeStore = store;
		nodeview1.ShowAll();
	}

	void OnNodeview1SelectionChanged(object o, System.EventArgs args)
	{
    	Gtk.NodeSelection selection = (Gtk.NodeSelection) o;
        MovieTreeNode node = (MovieTreeNode) selection.SelectedNode;
        
		label1.Text = node.Movie;
		
		currentID = node.MovieID;
		
		spinbutton26.Value = 3.0; // TODO maybe get value from somewhere
		                          // TODO color-code if it is known or not ...
	}
	
	protected virtual void OnGtkButtonClicked (object sender, System.EventArgs e)
	{
		// store rating
		
		Console.WriteLine("Rating for '{0}' ({1}): {2}", label1.Text, currentID, spinbutton26.Value);
	}
}