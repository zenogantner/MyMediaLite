
// This file has been generated by the GUI designer. Do not modify.

public partial class MainWindow
{
	private global::Gtk.UIManager UIManager;
	private global::Gtk.Action FilterAction;
	private global::Gtk.Action LanguageAction;
	private global::Gtk.Action UserAction;
	private global::Gtk.Action SaveRatingsAsAction;
	private global::Gtk.Action SaveRatingsAnonymouslyAction;
	private global::Gtk.Action DiscardRatingsAction;
	private global::Gtk.ToggleAction OnlyShow200MostPopularMoviesAction;
	private global::Gtk.Action ByGenreAction;
	private global::Gtk.ToggleAction ActionAction;
	private global::Gtk.Action TODORestByProgramAction;
	private global::Gtk.RadioAction EnglishAction;
	private global::Gtk.RadioAction DeutschAction;
	private global::Gtk.Action LoadUserAction;
	private global::Gtk.Action AndrAction;
	private global::Gtk.Action ArtusAction;
	private global::Gtk.Action ChristophAction;
	private global::Gtk.Action LucasAction;
	private global::Gtk.Action KrisztianAction;
	private global::Gtk.Action NgheAction;
	private global::Gtk.Action OsmanAction;
	private global::Gtk.Action RasoulAction;
	private global::Gtk.Action SabrinaAction;
	private global::Gtk.Action TomasAction;
	private global::Gtk.Action ZenoAction;
	private global::Gtk.VBox vbox1;
	private global::Gtk.MenuBar menubar1;
	private global::Gtk.Entry filter_entry;
	private global::Gtk.HBox hbox1;
	private global::Gtk.ScrolledWindow scrolledwindow2;
	private global::Gtk.TreeView treeview1;
	
	protected virtual void Build ()
	{
		global::Stetic.Gui.Initialize (this);
		// Widget MainWindow
		this.UIManager = new global::Gtk.UIManager ();
		global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup ("Default");
		this.FilterAction = new global::Gtk.Action (
			"FilterAction",
			global::Mono.Unix.Catalog.GetString("Filter"),
			null,
			null
		);
		this.FilterAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Filter");
		w1.Add (this.FilterAction, null);
		this.LanguageAction = new global::Gtk.Action (
			"LanguageAction",
			global::Mono.Unix.Catalog.GetString("Language"),
			null,
			null
		);
		this.LanguageAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Language");
		w1.Add (this.LanguageAction, null);
		this.UserAction = new global::Gtk.Action (
			"UserAction",
			global::Mono.Unix.Catalog.GetString("User"),
			null,
			null
		);
		this.UserAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("User");
		w1.Add (this.UserAction, null);
		this.SaveRatingsAsAction = new global::Gtk.Action (
			"SaveRatingsAsAction",
			global::Mono.Unix.Catalog.GetString("Save Ratings As ..."),
			null,
			null
		);
		this.SaveRatingsAsAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Save Ratings As ...");
		w1.Add (this.SaveRatingsAsAction, null);
		this.SaveRatingsAnonymouslyAction = new global::Gtk.Action (
			"SaveRatingsAnonymouslyAction",
			global::Mono.Unix.Catalog.GetString("Save Ratings Anonymously"),
			null,
			null
		);
		this.SaveRatingsAnonymouslyAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Save Ratings Anonymously");
		w1.Add (this.SaveRatingsAnonymouslyAction, null);
		this.DiscardRatingsAction = new global::Gtk.Action (
			"DiscardRatingsAction",
			global::Mono.Unix.Catalog.GetString("Discard Ratings"),
			null,
			null
		);
		this.DiscardRatingsAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Discard Ratings");
		w1.Add (this.DiscardRatingsAction, null);
		this.OnlyShow200MostPopularMoviesAction = new global::Gtk.ToggleAction (
			"OnlyShow200MostPopularMoviesAction",
			global::Mono.Unix.Catalog.GetString("Only Show 200 Most Popular Movies "),
			null,
			null
		);
		this.OnlyShow200MostPopularMoviesAction.Active = true;
		this.OnlyShow200MostPopularMoviesAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Only Show 200 Most Popular Movies ");
		w1.Add (this.OnlyShow200MostPopularMoviesAction, null);
		this.ByGenreAction = new global::Gtk.Action (
			"ByGenreAction",
			global::Mono.Unix.Catalog.GetString("By Genre ..."),
			null,
			null
		);
		this.ByGenreAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("By Genre ...");
		w1.Add (this.ByGenreAction, null);
		this.ActionAction = new global::Gtk.ToggleAction (
			"ActionAction",
			global::Mono.Unix.Catalog.GetString("Action"),
			null,
			null
		);
		this.ActionAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Action");
		w1.Add (this.ActionAction, null);
		this.TODORestByProgramAction = new global::Gtk.Action (
			"TODORestByProgramAction",
			global::Mono.Unix.Catalog.GetString("TODO: rest by program ..."),
			null,
			null
		);
		this.TODORestByProgramAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("TODO: rest by program ...");
		w1.Add (this.TODORestByProgramAction, null);
		this.EnglishAction = new global::Gtk.RadioAction (
			"EnglishAction",
			global::Mono.Unix.Catalog.GetString("English"),
			null,
			null,
			0
		);
		this.EnglishAction.Group = new global::GLib.SList (global::System.IntPtr.Zero);
		this.EnglishAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("English");
		w1.Add (this.EnglishAction, null);
		this.DeutschAction = new global::Gtk.RadioAction (
			"DeutschAction",
			global::Mono.Unix.Catalog.GetString("Deutsch"),
			null,
			null,
			0
		);
		this.DeutschAction.Group = this.EnglishAction.Group;
		this.DeutschAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Deutsch");
		w1.Add (this.DeutschAction, null);
		this.LoadUserAction = new global::Gtk.Action (
			"LoadUserAction",
			global::Mono.Unix.Catalog.GetString("Load User"),
			null,
			null
		);
		this.LoadUserAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Load User");
		w1.Add (this.LoadUserAction, null);
		this.AndrAction = new global::Gtk.Action (
			"AndrAction",
			global::Mono.Unix.Catalog.GetString("André"),
			null,
			null
		);
		this.AndrAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("André");
		w1.Add (this.AndrAction, null);
		this.ArtusAction = new global::Gtk.Action (
			"ArtusAction",
			global::Mono.Unix.Catalog.GetString("Artus"),
			null,
			null
		);
		this.ArtusAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Artus");
		w1.Add (this.ArtusAction, null);
		this.ChristophAction = new global::Gtk.Action (
			"ChristophAction",
			global::Mono.Unix.Catalog.GetString("Christoph"),
			null,
			null
		);
		this.ChristophAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Christoph");
		w1.Add (this.ChristophAction, null);
		this.LucasAction = new global::Gtk.Action (
			"LucasAction",
			global::Mono.Unix.Catalog.GetString("Lucas"),
			null,
			null
		);
		this.LucasAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Lucas");
		w1.Add (this.LucasAction, null);
		this.KrisztianAction = new global::Gtk.Action (
			"KrisztianAction",
			global::Mono.Unix.Catalog.GetString("Krisztian"),
			null,
			null
		);
		this.KrisztianAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Krisztian");
		w1.Add (this.KrisztianAction, null);
		this.NgheAction = new global::Gtk.Action (
			"NgheAction",
			global::Mono.Unix.Catalog.GetString("Nghe"),
			null,
			null
		);
		this.NgheAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Nghe");
		w1.Add (this.NgheAction, null);
		this.OsmanAction = new global::Gtk.Action (
			"OsmanAction",
			global::Mono.Unix.Catalog.GetString("Osman"),
			null,
			null
		);
		this.OsmanAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Osman");
		w1.Add (this.OsmanAction, null);
		this.RasoulAction = new global::Gtk.Action (
			"RasoulAction",
			global::Mono.Unix.Catalog.GetString("Rasoul"),
			null,
			null
		);
		this.RasoulAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Rasoul");
		w1.Add (this.RasoulAction, null);
		this.SabrinaAction = new global::Gtk.Action (
			"SabrinaAction",
			global::Mono.Unix.Catalog.GetString("Sabrina"),
			null,
			null
		);
		this.SabrinaAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Sabrina");
		w1.Add (this.SabrinaAction, null);
		this.TomasAction = new global::Gtk.Action (
			"TomasAction",
			global::Mono.Unix.Catalog.GetString("Tomas"),
			null,
			null
		);
		this.TomasAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Tomas");
		w1.Add (this.TomasAction, null);
		this.ZenoAction = new global::Gtk.Action (
			"ZenoAction",
			global::Mono.Unix.Catalog.GetString("Zeno"),
			null,
			null
		);
		this.ZenoAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Zeno");
		w1.Add (this.ZenoAction, null);
		this.UIManager.InsertActionGroup (w1, 0);
		this.AddAccelGroup (this.UIManager.AccelGroup);
		this.Name = "MainWindow";
		this.Title = global::Mono.Unix.Catalog.GetString ("MyMediaLite Movie Demo");
		this.Icon = global::Stetic.IconLoader.LoadIcon (
			this,
			"stock_animation",
			global::Gtk.IconSize.Menu
		);
		this.WindowPosition = ((global::Gtk.WindowPosition)(4));
		// Container child MainWindow.Gtk.Container+ContainerChild
		this.vbox1 = new global::Gtk.VBox ();
		this.vbox1.Name = "vbox1";
		this.vbox1.Spacing = 6;
		// Container child vbox1.Gtk.Box+BoxChild
		this.UIManager.AddUiFromString ("<ui><menubar name='menubar1'><menu name='FilterAction' action='FilterAction'><menuitem name='OnlyShow200MostPopularMoviesAction' action='OnlyShow200MostPopularMoviesAction'/></menu><menu name='LanguageAction' action='LanguageAction'><menuitem name='EnglishAction' action='EnglishAction'/><menuitem name='DeutschAction' action='DeutschAction'/></menu><menu name='UserAction' action='UserAction'><menuitem name='SaveRatingsAnonymouslyAction' action='SaveRatingsAnonymouslyAction'/><menuitem name='DiscardRatingsAction' action='DiscardRatingsAction'/></menu></menubar></ui>");
		this.menubar1 = ((global::Gtk.MenuBar)(this.UIManager.GetWidget ("/menubar1")));
		this.menubar1.Name = "menubar1";
		this.vbox1.Add (this.menubar1);
		global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.menubar1]));
		w2.Position = 0;
		w2.Expand = false;
		w2.Fill = false;
		// Container child vbox1.Gtk.Box+BoxChild
		this.filter_entry = new global::Gtk.Entry ();
		this.filter_entry.TooltipMarkup = "Enter string to filter the movie list";
		this.filter_entry.CanFocus = true;
		this.filter_entry.Name = "filter_entry";
		this.filter_entry.IsEditable = true;
		this.filter_entry.InvisibleChar = '●';
		this.vbox1.Add (this.filter_entry);
		global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.filter_entry]));
		w3.Position = 1;
		w3.Expand = false;
		w3.Fill = false;
		// Container child vbox1.Gtk.Box+BoxChild
		this.hbox1 = new global::Gtk.HBox ();
		this.hbox1.Name = "hbox1";
		this.hbox1.Spacing = 6;
		// Container child hbox1.Gtk.Box+BoxChild
		this.scrolledwindow2 = new global::Gtk.ScrolledWindow ();
		this.scrolledwindow2.CanFocus = true;
		this.scrolledwindow2.Name = "scrolledwindow2";
		this.scrolledwindow2.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child scrolledwindow2.Gtk.Container+ContainerChild
		this.treeview1 = new global::Gtk.TreeView ();
		this.treeview1.TooltipMarkup = "Click in column 'Rating' to enter ratings";
		this.treeview1.CanFocus = true;
		this.treeview1.Name = "treeview1";
		this.scrolledwindow2.Add (this.treeview1);
		this.hbox1.Add (this.scrolledwindow2);
		global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.scrolledwindow2]));
		w5.Position = 0;
		this.vbox1.Add (this.hbox1);
		global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.hbox1]));
		w6.Position = 2;
		this.Add (this.vbox1);
		if ((this.Child != null)) {
			this.Child.ShowAll ();
		}
		this.DefaultWidth = 635;
		this.DefaultHeight = 580;
		this.Show ();
		this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
		this.SaveRatingsAsAction.Activated += new global::System.EventHandler (this.OnSaveRatingsAsActionActivated);
		this.SaveRatingsAnonymouslyAction.Activated += new global::System.EventHandler (this.OnSaveRatingsAnonymouslyActionActivated);
		this.DiscardRatingsAction.Activated += new global::System.EventHandler (this.OnDiscardRatingsActionActivated);
		this.OnlyShow200MostPopularMoviesAction.Toggled += new global::System.EventHandler (this.OnOnlyShow200MostPopularMoviesActionToggled);
		this.EnglishAction.Activated += new global::System.EventHandler (this.OnEnglishActionActivated);
		this.DeutschAction.Activated += new global::System.EventHandler (this.OnDeutschActionActivated);
	}
}
