using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using MyMediaLite;
using MyMediaLite.data;
using MyMediaLite.eval;


namespace MyMediaLite.rating_demo
{
    public partial class frmDemo : Form
    {
        private int user_id;
        public Dictionary<int, string> titles;
        public Dictionary<string, int> titles_rev;
        private IRecommenderEngine engine;
		private RatingData ratings;

        private HashSet<int> movie_pool; // only these movies can be recommended as we only want to present movies that the user is likely to have seen
        private HashSet<int> movies_rated = new HashSet<int>();

        public frmDemo(IRecommenderEngine engine, RatingData ratings, int user_id)
        {
            InitializeComponent();

            this.user_id = user_id;
            this.engine  = engine;
			this.ratings = ratings;

            // Read movie names...
            titles = new Dictionary<int, string>();
            titles_rev = new Dictionary<string, int>();
            {
                StreamReader sr = new StreamReader("..\\..\\..\\Data\\movies.dat");
                String line;

                while ((line = sr.ReadLine()) != null)
                {
                    int pos = line.IndexOf("::");
                    //String[] tokens = line.Split("::");
                    int item_id = Int32.Parse(line.Substring(0, pos));
                    line = line.Substring(pos + 2);
                    pos = line.IndexOf("::");
                    string title = line.Substring(0, pos);
                    titles.Add(item_id, title);
                    titles_rev.Add(title, item_id);
                }
                sr.Close();
            }

            // restrict the set of recommended movies to the most popular 250... 
            movie_pool = new HashSet<int>();
            {
                Dictionary<int, int> view_count = new Dictionary<int, int>();
                
				foreach (RatingEvent r in ratings)
                {
                    if (view_count.ContainsKey(r.item_id)) {
                        view_count[r.item_id]++;
                    } else {
                        view_count.Add(r.item_id, 1);
                    }
                }
                List<int> counts = new List<int>(view_count.Values);
                counts.Sort();
                foreach (int id in view_count.Keys)
                {
                    if (view_count[id] >= counts[counts.Count-250])
                    {
                        movie_pool.Add(id);
                    }
                }
            }


            update_Recommendations();
        }

        private void update_Recommendations()
        {
            lbItems.Items.Clear();
            int[] topn = ItemPrediction.PredictItems(engine, user_id, ratings.MaxItemID);
            foreach (int item_id in topn)
            {
                if (movie_pool.Contains(item_id))
                {
                    if (!movies_rated.Contains(item_id))
                    {
                        lbItems.Items.Add(titles[item_id]);
                    }
                }
                if (lbItems.Items.Count >= 50)
                {
                    break;
                }
            }

        }
        

        private void cmdRate_Click(object sender, EventArgs e)
        {
            // Update the recommender model
            {
                int item_id = titles_rev[(string)lbItems.Items[lbItems.SelectedIndex]];
                double rating = 5 - comboBox1.SelectedIndex;
				// TODO we need support for online updates here ...
				engine.AddRelation(RelationType.Rated, new int[] { user_id, item_id }, new object[] { rating });
                lbFeedback.Items.Add(rating + " stars for " + titles[item_id]);
                movies_rated.Add(item_id);
            }
            // Show the predictions
            update_Recommendations();
        }

        private void lbItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            int item_id = titles_rev[(string)lbItems.Items[lbItems.SelectedIndex]];
            double d = engine.PredictRelation(RelationType.Rated, new int[] {user_id, item_id});
            lblPrediction.Text = "Personalized prediction " + d;
        }
    }
}
