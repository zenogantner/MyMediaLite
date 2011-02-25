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
using System.Collections.Generic;
using Gtk;
using MovieDemo;
using MyMediaLite.Data;
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;

public partial class MainWindow : Gtk.Window
{
	// TODO put application logic into one object (not the MainWindow)

	MovieLensMovieInfo movies = new MovieLensMovieInfo();

	//ListStore movie_store = new Gtk.ListStore( typeof(double), typeof(double), typeof(string), typeof(int) );
	ListStore movie_store = new Gtk.ListStore( typeof(Movie) );
	Gtk.TreeModelFilter filter;

	RatingData training_data;

	//RatingPredictor rating_predictor = new BiasedMatrixFactorization();
	RatingPredictor rating_predictor = new UserItemBaseline();

	EntityMapping user_mapping = new EntityMapping();
	EntityMapping item_mapping = new EntityMapping();

	// ID of the currently selected movie
	int current_movie_id;

	int current_user_id;

	Dictionary<int, double> predictions = new Dictionary<int, double>();

	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		// TODO integrate internal IDs
		Console.Error.Write("Reading in movie data ... ");
		//movies.Read("/home/mrg/data/ml10m/movies.dat"); // TODO param
		movies.Read("/home/mrg/data/ml1m/original/movies.dat"); // TODO param
		Console.Error.WriteLine("done.");

		Console.Error.Write("Reading in ratings ... "); // TODO param
		//training_data = MovieLensRatingData.Read("/home/mrg/data/ml10m/ratings.dat", 0, 5, user_mapping, item_mapping);
		training_data = RatingPredictionData.Read("/home/mrg/data/ml1m/ml1m.txt", 1, 5, user_mapping, item_mapping);
		rating_predictor.Ratings = training_data;
		Console.Error.WriteLine("done.");

		Console.Error.Write("Training ... ");
		rating_predictor.Train();
		Console.Error.WriteLine("done.");
		// TODO have option of loading from file

		current_user_id = rating_predictor.MaxUserID;
		rating_predictor.AddUser(current_user_id);
		
		PredictAllRatings();

		// build main window
		Build();

		CreateTreeView();
	}

	// inspired by http://www.mono-project.com/GtkSharp_TreeView_Tutorial
	private void CreateTreeView()
	{
		// Fire off an event when the text in the Entry changes
		filter_entry.Changed += OnFilterEntryTextChanged;

		// create a column for the prediction
		Gtk.TreeViewColumn prediction_column = new Gtk.TreeViewColumn();
		prediction_column.Title = "Prediction";
		Gtk.CellRendererText prediction_cell = new Gtk.CellRendererText();
		prediction_column.PackStart(prediction_cell, true);

		// create a column for the rating
		Gtk.TreeViewColumn rating_column = new Gtk.TreeViewColumn();
		rating_column.Title = "Rating";
		Gtk.CellRendererText rating_cell = new Gtk.CellRendererText();
		rating_column.PackStart(rating_cell, true);

		// create a column for the movie title
		Gtk.TreeViewColumn movie_column = new Gtk.TreeViewColumn();
		movie_column.Title = "Movie";
		Gtk.CellRendererText movie_cell = new Gtk.CellRendererText();
		movie_column.PackStart(movie_cell, true);

		// add the columns to the TreeView
		treeview1.AppendColumn(prediction_column);
		treeview1.AppendColumn(rating_column);
		treeview1.AppendColumn(movie_column);

		prediction_column.SetCellDataFunc(prediction_cell, new Gtk.TreeCellDataFunc(RenderPrediction));
		rating_column.SetCellDataFunc(rating_cell, new Gtk.TreeCellDataFunc(RenderRating));
		movie_column.SetCellDataFunc(movie_cell, new Gtk.TreeCellDataFunc(RenderMovieTitle)); // TODO use this for i18n
		//prediction_column.AddAttribute(prediction_cell, "text", 0);
		//rating_column.AddAttribute(rating_cell, "text", 1);
		//movie_column.AddAttribute(movie_cell, "text", 2);

		// Add some data to the store
		foreach (Movie movie in movies.movie_list)
			movie_store.AppendValues( movie );

		filter = new Gtk.TreeModelFilter(movie_store, null);

		// specify the function that determines which rows to filter out and which ones to display
		filter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc(FilterTree);

		treeview1.Model = filter;
		treeview1.ShowAll();
	}

	private void RenderPrediction(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		Movie movie = (Movie) model.GetValue(iter, 0);
		
		double prediction = -1;
		predictions.TryGetValue(movie.ID, out prediction);
		
		(cell as Gtk.CellRendererText).Text = string.Format("{0,0:0.#}", prediction);
	}

	private void RenderRating(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		Movie movie = (Movie) model.GetValue(iter, 0);
		(cell as Gtk.CellRendererText).Text = string.Format("{0,0:0.#}", 0.0);
	}

	private void RenderMovieTitle (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		Movie movie = (Movie) model.GetValue(iter, 0);
		(cell as Gtk.CellRendererText).Text = movie.Title;
	}

	private void OnFilterEntryTextChanged (object o, System.EventArgs args)
	{
		// since the filter text changed, tell the filter to re-determine which rows to display
		filter.Refilter();
	}

	private bool FilterTree(Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		Movie movie = (Movie) model.GetValue(iter, 0);
		string movie_title = movie.Title;

		if (filter_entry.Text.Equals(string.Empty))
			return true;

		if (movie_title.Contains(filter_entry.Text))
			return true;
		else
			return false;
	}

	void PredictAllRatings()
	{
		Console.Write("Predicting ... ");

		// compute ratings
		for (int i = 0; i <= rating_predictor.MaxItemID; i++)
			predictions[item_mapping.ToOriginalID(i)] = rating_predictor.Predict(current_user_id, i);

		Console.Error.WriteLine("done.");
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();
		a.RetVal = true;
	}

	void OnTreeview1SelectionChanged(object o, System.EventArgs args)
	{
		Console.WriteLine("Selection changed.");
	}
}