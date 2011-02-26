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
using System.Globalization;
using Gtk;
using MovieDemo;
using MyMediaLite.Data;
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;

public partial class MainWindow : Window
{
	// TODO put application logic into one object (not the MainWindow)

	MovieLensMovieInfo movies = new MovieLensMovieInfo();

	TreeModelFilter filter;
	TreeModelSort sorter;
	TreeViewColumn prediction_column = new TreeViewColumn();
	TreeViewColumn rating_column = new TreeViewColumn();
	TreeViewColumn movie_column = new TreeViewColumn();

	Gdk.Color white = new Gdk.Color(0xff, 0xff, 0xff);

	RatingData training_data;

	//RatingPredictor rating_predictor = new BiasedMatrixFactorization();
	RatingPredictor rating_predictor = new UserItemBaseline();

	EntityMapping user_mapping = new EntityMapping();
	EntityMapping item_mapping = new EntityMapping();

	int current_user_id;

	Dictionary<int, double> ratings     = new Dictionary<int, double>();
	Dictionary<int, double> predictions = new Dictionary<int, double>();

	NumberFormatInfo ni = new NumberFormatInfo();

	double min_rating = 1;
	double max_rating = 5;

	public MainWindow() : base( WindowType.Toplevel)
	{
		ni.NumberDecimalDigits = '.'; // ensure correct comma separator (for English)

		// TODO integrate internal IDs - load after training data ...
		Console.Error.Write("Reading in movie data ... ");
		//movies.Read("/home/mrg/data/ml10m/movies.dat"); // TODO param
		movies.Read("/home/mrg/data/ml1m/original/movies.dat"); // TODO param
		Console.Error.WriteLine("done.");

		Console.Error.Write("Reading in ratings ... "); // TODO param
		//training_data = MovieLensRatingData.Read("/home/mrg/data/ml10m/ratings.dat", min_rating, max_rating, user_mapping, item_mapping);
		training_data = RatingPredictionData.Read("/home/mrg/data/ml1m/ml1m.txt", min_rating, max_rating, user_mapping, item_mapping);
		rating_predictor.Ratings = training_data;
		Console.Error.WriteLine("done.");

		Console.Error.Write("Training ... ");
		rating_predictor.MinRating = min_rating;
		rating_predictor.MaxRating = max_rating; // TODO this API must be nicer ...
		rating_predictor.UpdateUsers = true;
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

	// heavily inspired by http://www.mono-project.com/GtkSharp_TreeView_Tutorial
	private void CreateTreeView()
	{
		// fire off an event when the text in the Entry changes
		filter_entry.Changed += OnFilterEntryTextChanged;

		// create a column for the prediction
		prediction_column.Title = "Prediction";
		CellRendererText prediction_cell = new CellRendererText();
		prediction_cell.BackgroundGdk = white;
		prediction_column.PackStart(prediction_cell, true);
		prediction_column.SortIndicator = true;
		prediction_column.Clickable = true;
		prediction_column.Clicked += new EventHandler( PredictionColumnClicked );

		// create a column for the rating
		rating_column.Title = "Rating";
		CellRendererText rating_cell = new CellRendererText();
		rating_cell.Editable = true;
		rating_cell.Edited += RatingCellEdited;
		rating_cell.BackgroundGdk = white;
		rating_column.PackStart(rating_cell, true);
		rating_column.SortIndicator = true;
		rating_column.Clickable = true;
		rating_column.Clicked += new EventHandler( RatingColumnClicked );

		// set up a column for the movie title
		movie_column.Title = "Movie";
		CellRendererText movie_cell = new CellRendererText();
		movie_cell.BackgroundGdk = white;
		movie_column.PackStart(movie_cell, true);
		movie_column.SortIndicator = true;
		movie_column.Clickable = true;
		movie_column.Clicked += new EventHandler( MovieColumnClicked );

		// add the columns to the TreeView
		treeview1.AppendColumn(prediction_column);
		treeview1.AppendColumn(rating_column);
		treeview1.AppendColumn(movie_column);

		prediction_column.SetCellDataFunc(prediction_cell, new TreeCellDataFunc(RenderPrediction));
		rating_column.SetCellDataFunc(rating_cell, new TreeCellDataFunc(RenderRating));
		movie_column.SetCellDataFunc(movie_cell, new TreeCellDataFunc(RenderMovieTitle));

		var movie_store = new ListStore( typeof(Movie) );

		// Add some data to the store
		foreach (Movie movie in movies.movie_list)
			movie_store.AppendValues( movie );

		filter = new TreeModelFilter(movie_store, null);
		// specify the function that determines which rows to filter out and which ones to display
		filter.VisibleFunc = new TreeModelFilterVisibleFunc(FilterTree);

		sorter = new TreeModelSort(filter);
		sorter.DefaultSortFunc = ComparePredictionReversed;

		treeview1.Model = sorter;
		treeview1.ShowAll();
	}

	private void MovieColumnClicked(object o, EventArgs args)
	{
		if (movie_column.SortOrder == SortType.Ascending)
		{
			movie_column.SortOrder = SortType.Descending;
			sorter.DefaultSortFunc = CompareTitleReversed;
		}
		else
		{
			movie_column.SortOrder = SortType.Ascending;
			sorter.DefaultSortFunc = CompareTitle;
		}
	}

	// TODO have only one sorting function
 	private int CompareTitleReversed(TreeModel model, TreeIter a, TreeIter b)
	{
		Movie movie1 = (Movie) model.GetValue(a, 0);
		Movie movie2 = (Movie) model.GetValue(b, 0);

		return string.Compare(movie2.Title, movie1.Title);
	}

 	private int CompareTitle(TreeModel model, TreeIter a, TreeIter b)
	{
		Movie movie1 = (Movie) model.GetValue(a, 0);
		Movie movie2 = (Movie) model.GetValue(b, 0);

		return string.Compare(movie1.Title, movie2.Title);
	}

	private void PredictionColumnClicked(object o, EventArgs args)
	{
		if (prediction_column.SortOrder == SortType.Ascending)
		{
			prediction_column.SortOrder = SortType.Descending;
			sorter.DefaultSortFunc = ComparePrediction;
		}
		else
		{
			prediction_column.SortOrder = SortType.Ascending;
			sorter.DefaultSortFunc = ComparePredictionReversed;
		}
	}

 	private int ComparePredictionReversed(TreeModel model, TreeIter a, TreeIter b)
	{
		Movie movie1 = (Movie) model.GetValue(a, 0);
		Movie movie2 = (Movie) model.GetValue(b, 0);

		double prediction1 = -1;
		predictions.TryGetValue(movie1.ID, out prediction1);
		double prediction2 = -1;
		predictions.TryGetValue(movie2.ID, out prediction2);

		double diff = prediction2 - prediction1;

		if (diff > 0)
			return 1;
		if (diff < 0)
			return -1;
		return 0;
	}

 	private int ComparePrediction(TreeModel model, TreeIter a, TreeIter b)
	{
		Movie movie1 = (Movie) model.GetValue(a, 0);
		Movie movie2 = (Movie) model.GetValue(b, 0);

		double prediction1 = -1;
		predictions.TryGetValue(movie1.ID, out prediction1);
		double prediction2 = -1;
		predictions.TryGetValue(movie2.ID, out prediction2);

		double diff = prediction1 - prediction2;

		if (diff > 0)
			return 1;
		if (diff < 0)
			return -1;
		return 0;
	}

	private void RatingColumnClicked(object o, EventArgs args)
	{
		if (rating_column.SortOrder == SortType.Ascending)
		{
			rating_column.SortOrder = SortType.Descending;
			sorter.DefaultSortFunc = CompareRating;
		}
		else
		{
			rating_column.SortOrder = SortType.Ascending;
			sorter.DefaultSortFunc = CompareRatingReversed;
		}
	}

 	private int CompareRatingReversed(TreeModel model, TreeIter a, TreeIter b)
	{
		Movie movie1 = (Movie) model.GetValue(a, 0);
		Movie movie2 = (Movie) model.GetValue(b, 0);

		double rating1;
		ratings.TryGetValue(movie1.ID, out rating1);
		double rating2;
		ratings.TryGetValue(movie2.ID, out rating2);

		double diff = rating2 - rating1;

		if (diff > 0)
			return 1;
		if (diff < 0)
			return -1;
		return 0;
	}

 	private int CompareRating(TreeModel model, TreeIter a, TreeIter b)
	{
		Movie movie1 = (Movie) model.GetValue(a, 0);
		Movie movie2 = (Movie) model.GetValue(b, 0);

		double rating1;
		ratings.TryGetValue(movie1.ID, out rating1);
		double rating2;
		ratings.TryGetValue(movie2.ID, out rating2);

		double diff = rating1 - rating2;

		if (diff > 0)
			return 1;
		if (diff < 0)
			return -1;
		return 0;
	}

	private void RatingCellEdited(object o, EditedArgs args)
	{
		TreeIter iter;
		treeview1.Model.GetIter(out iter, new TreePath(args.Path));

		Movie movie = (Movie) treeview1.Model.GetValue(iter, 0);
		string input = args.NewText;
		try
		{
			double rating = double.Parse(input, ni);

			if (rating > max_rating)
				rating = max_rating;
			if (rating < min_rating)
				rating = min_rating;

			ratings[movie.ID] = rating;

			int internal_item_id = item_mapping.ToInternalID(movie.ID);

			// if necessary, add the the new item to the recommender/dataset
			if (internal_item_id > rating_predictor.MaxItemID)
				rating_predictor.AddItem(internal_item_id);

			// add the new rating
			rating_predictor.AddRating(current_user_id, internal_item_id, rating);

			PredictAllRatings();
		}
		catch (FormatException)
		{
			Console.Error.WriteLine("Could not parse input '{0}' as a number.", input);
		}
	}

	private void RenderPrediction(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
	{
		Movie movie = (Movie) model.GetValue(iter, 0);

		double prediction = -1;
		predictions.TryGetValue(movie.ID, out prediction);

		(cell as CellRendererText).Text = string.Format(ni, "{0,0:0.#}", prediction);
	}

	private void RenderRating(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
	{
		Movie movie = (Movie) model.GetValue(iter, 0);

		double rating = -1;

		if (ratings.TryGetValue(movie.ID, out rating))
			(cell as CellRendererText).Text = string.Format(ni, "{0}", rating);
		else
			(cell as CellRendererText).Text = string.Empty;
	}

	private void RenderMovieTitle( TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
	{
		Movie movie = (Movie) model.GetValue(iter, 0);
		(cell as CellRendererText).Text = movie.Title; // TODO use this for i18n
	}

	private void OnFilterEntryTextChanged(object o, System.EventArgs args)
	{
		// since the filter text changed, tell the filter to re-determine which rows to display
		filter.Refilter();
	}

	private bool FilterTree( TreeModel model,  TreeIter iter)
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
}