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
using System.IO;
using Gtk;
using MovieDemo;
using MyMediaLite.Data;
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;

public partial class MainWindow : Window
{
	// TODO put application logic into one object (not the MainWindow)

	MovieLensMovieInfo movies = new MovieLensMovieInfo();
	Dictionary<int, string> german_names;

	TreeModelFilter filter;
	TreeModelSort sorter;
	TreeViewColumn prediction_column = new TreeViewColumn();
	TreeViewColumn rating_column = new TreeViewColumn();
	TreeViewColumn movie_column = new TreeViewColumn();

	Gdk.Color white = new Gdk.Color(0xff, 0xff, 0xff);

	RatingData training_data;
	RatingPredictor rating_predictor;

	// depends on dataset
	double min_rating = 1;
	double max_rating = 5;
	string ratings_file = "../../../../data/ml1m/ratings.dat";
	string movie_file   = "../../../../data/ml1m/movies-utf8.dat";
	string model_file   = "../../bmf.model";
	// MovieLens 10M
	/*
	double min_rating = 0;
	double max_rating = 5;
	string ratings_file = "../../../../data/ml10m/ratings.dat";
	string movie_file   = "../../../../data/ml10m/movies.dat";
	string model_file   = "../../ml10m-bmf.model";
	*/
	
	EntityMapping user_mapping = new EntityMapping();
	EntityMapping item_mapping = new EntityMapping();

	// application state
	int current_user_external_id = 100000;
	int current_user_id;
	Locale locale = Locale.English;

	Dictionary<int, double> ratings     = new Dictionary<int, double>();
	Dictionary<int, double> predictions = new Dictionary<int, double>();

	NumberFormatInfo ni        = new NumberFormatInfo();

	public MainWindow() : base( WindowType.Toplevel)
	{
		ni.NumberDecimalDigits = '.'; // ensure correct comma separator (for English)

		Console.Error.Write("Reading in movie data ... ");

		movies.Read(movie_file, item_mapping); // TODO param
		Console.Error.WriteLine("done.");

		german_names = IMDBAkaTitles.Read("../../german-aka-titles-utf8.list", "GERMAN", movies.IMDB_KEY_To_ID);
		SwitchInterfaceToEnglish();
		
		CreateRecommender(); // TODO do asynchronously

		// build main window
		Build();

		CreateTreeView();
	}

	private void CreateRecommender()
	{
		Console.Error.Write("Reading in ratings ... ");
		training_data = MovieLensRatingData.Read(ratings_file, min_rating, max_rating, user_mapping, item_mapping);
		BiasedMatrixFactorization recommender = new BiasedMatrixFactorization();
		recommender.Ratings = training_data;
		Console.Error.WriteLine("done.");

		Console.Error.Write("Reading in additional ratings ... ");
		string[] rating_files = Directory.GetFiles("../../saved_data/", "user-ratings-*");
		Console.Error.WriteLine("done.");
		
		Console.Error.Write("Loading prediction model ... ");
		recommender.MinRating = min_rating;
		recommender.MaxRating = max_rating; // TODO this API must be nicer ...
		recommender.UpdateUsers = true;
		recommender.UpdateItems = false;
		recommender.BiasRegularization = 0.001;
		recommender.Regularization = 0.045;
		recommender.NumIter = 60;
		recommender.LoadModel(model_file);
		Console.Error.WriteLine("done.");
		// TODO have option of loading from file

		rating_predictor = recommender;

		current_user_id = user_mapping.ToInternalID(current_user_external_id);
		rating_predictor.AddUser(current_user_id);

		PredictAllRatings();
	}

	// heavily inspired by http://www.mono-project.com/GtkSharp_TreeView_Tutorial
	private void CreateTreeView()
	{
		// fire off an event when the text in the Entry changes
		filter_entry.Changed += OnFilterEntryTextChanged;

		// create a column for the prediction
		CellRendererText prediction_cell = new CellRendererText();
		prediction_cell.BackgroundGdk = white;
		prediction_column.PackStart(prediction_cell, true);
		prediction_column.SortIndicator = true;
		prediction_column.Clickable = true;
		prediction_column.Clicked += new EventHandler( PredictionColumnClicked );

		// create a column for the rating
		CellRendererText rating_cell = new CellRendererText();
		rating_cell.Editable = true;
		rating_cell.Edited += RatingCellEdited;
		rating_cell.BackgroundGdk = white;
		//rating_cell.Alignment = Pango.Alignment.Center; // TODO this does not seem to work - what's the problem?
		rating_column.PackStart(rating_cell, true);
		rating_column.SortIndicator = true;
		rating_column.Clickable = true;
		rating_column.Clicked += new EventHandler( RatingColumnClicked );

		// set up a column for the movie title
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

		// set up the render objects for the columns
		prediction_column.SetCellDataFunc(prediction_cell, new TreeCellDataFunc(RenderPrediction));
		rating_column.SetCellDataFunc(rating_cell, new TreeCellDataFunc(RenderRating));
		movie_column.SetCellDataFunc(movie_cell, new TreeCellDataFunc(RenderMovieTitle));

		var movie_store = new ListStore( typeof(Movie) );

		// add all movies to the store
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
		string input = args.NewText.Trim();
		
		if (input == string.Empty)
		{
			Console.Error.WriteLine("Remove rating.");
			if (ratings.Remove(movie.ID))
				rating_predictor.RemoveRating(current_user_id, movie.ID);
			
			PredictAllRatings();
			return;
		}
		
		input = input.Replace(',', '.'); // also allow "German" floating point numbers
		
		try
		{
			double rating = double.Parse(input, ni);

			if (rating > max_rating)
				rating = max_rating;
			if (rating < min_rating)
				rating = min_rating;

			ratings[movie.ID] = rating;

			// if necessary, add the the new item to the recommender/dataset
			if (movie.ID > rating_predictor.MaxItemID)
				rating_predictor.AddItem(movie.ID);

			// add the new rating
			rating_predictor.AddRating(current_user_id, movie.ID, rating);

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

		if (ratings.ContainsKey(movie.ID))
			prediction = ratings[movie.ID];
		
		string text;
		if (prediction < 1)
			text = "";
		else if (prediction < 1.5)
			text = string.Format(ni, "{0,0:0.00} ★", prediction);
		else if (prediction < 2.5)
			text = string.Format(ni, "{0,0:0.00} ★★", prediction);
		else if (prediction < 3.5)
			text = string.Format(ni, "{0,0:0.00} ★★★", prediction);
		else if (prediction < 4.5)
			text = string.Format(ni, "{0,0:0.00} ★★★★", prediction);
		else
			text = string.Format(ni, "{0,0:0.00} ★★★★★", prediction);

		(cell as CellRendererText).Text = text;
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
		//(cell as CellRendererText).Text = movie.Title; // TODO use this for i18n
		
		string title;
		if (locale == Locale.German)
		{
			if (!german_names.TryGetValue(movie.ID, out title))
				title = movie.Title;
		}
		else
			title = movie.Title;
			
		(cell as CellRendererText).Text = title;
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
			predictions[i] = rating_predictor.Predict(current_user_id, i);

		Console.Error.WriteLine("done.");
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();
		a.RetVal = true;
	}

	protected virtual void OnDiscardRatingsActionActivated(object sender, System.EventArgs e)
	{
		if (GtkSharpUtils.YesNo(this, "Are you sure you want to delete all ratings?") == ResponseType.Yes)
		{
			ratings.Clear();
			rating_predictor.RemoveUser(current_user_id);
			Console.Error.WriteLine("Removed user ratings.");
			PredictAllRatings();
		}
	}
	
	protected virtual void OnDeutschActionActivated(object sender, System.EventArgs e)
	{
		SwitchInterfaceToGerman();
	}
		
	protected virtual void OnEnglishActionActivated(object sender, System.EventArgs e)
	{
		SwitchInterfaceToEnglish();
	}
		
	protected virtual void SwitchInterfaceToGerman()
	{
		locale = Locale.German;
		
		prediction_column.Title = "Vorhersage";
		rating_column.Title = "Bewertung";
		movie_column.Title = "Film";
	}
	
	protected virtual void SwitchInterfaceToEnglish()
	{
		locale = Locale.English;
		
		prediction_column.Title = "Prediction";
		rating_column.Title = "Rating";
		movie_column.Title = "Movie";		
	}
	
	protected virtual void OnSaveRatingsAnonymouslyActionActivated(object sender, System.EventArgs e)
	{
		if (GtkSharpUtils.YesNo(this, "Are you sure you want to save all ratings and end this session?") == ResponseType.Yes)
		{
			SaveRatings();
			
			ratings.Clear();
			rating_predictor.RemoveUser(current_user_id);
			PredictAllRatings();
		}
	}
	
	void SaveRatings()
	{
		using ( StreamWriter writer = new StreamWriter("../../user-ratings-" + current_user_id) )
		{
			foreach (KeyValuePair<int, double> r in ratings)
				writer.WriteLine("{0}\t{1}\t{2}",
								 user_mapping.ToOriginalID(current_user_id),
								 item_mapping.ToOriginalID(r.Key),
								 r.Value.ToString(ni));
		}
	}
	
	void SaveRatings(string name)
	{
		using ( StreamWriter writer = new StreamWriter("../../saved_data/user-ratings-" + name) )
		{
			foreach (KeyValuePair<int, double> r in ratings)
				writer.WriteLine("{0}\t{1}\t{2}",
								 user_mapping.ToOriginalID(current_user_id),
								 item_mapping.ToOriginalID(r.Key),
								 r.Value.ToString(ni));
		}
	}	

	void LoadRatings(string name)
	{
		// assumption: ratings, training data and model were reloaded
		
		ratings.Clear();
		
		using ( var reader = new StreamReader("../../saved_data/user-ratings-" + name) )
		{
			RatingData user_ratings = RatingPredictionData.Read(reader, min_rating, max_rating, user_mapping, item_mapping);
			
			foreach (RatingEvent r in user_ratings.All)
			{
				ratings[r.item_id] = r.rating;
				current_user_id = r.user_id;
			}
		}
	}	
	
	protected virtual void OnSaveRatingsAsActionActivated (object sender, System.EventArgs e)
	{
		//var user_name = GtkSharpUtils.StringInput(this, "Enter Name");

		string user_name;
		
		{
			UserNameInput dialog = new UserNameInput();
			dialog.Run();
		
			Console.Error.WriteLine("finished running dialog");

			user_name = dialog.UserName;
			
			dialog.Destroy();
		}
			
		if (user_name != string.Empty)
		{
			Console.Error.WriteLine("save as " + user_name);
			SaveRatings(user_name);
			
			ratings.Clear();
			rating_predictor.RemoveUser(current_user_id);
			user_mapping.ToInternalID(++current_user_external_id);
			PredictAllRatings();
		}
		else
		{
			Console.Error.WriteLine("Aborting saving of user ...");
		}
	}
}