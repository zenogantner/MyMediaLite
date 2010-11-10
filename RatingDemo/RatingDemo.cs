using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MyMediaLite;
using MyMediaLite.data;
//using MyMediaLite.data_type;
using MyMediaLite.ensemble;
//using MyMediaLite.eval;
using MyMediaLite.io;
using MyMediaLite.rating_predictor;


namespace MyMediaLite.rating_demo
{
    class RatingDemo
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
			// ID mapping objects
			EntityMapping user_mapping = new EntityMapping();
			EntityMapping item_mapping = new EntityMapping();

			// read training data
			string training_file = args[0];
			RatingData training_data = RatingPredictionData.Read(training_file, 1, 5, user_mapping, item_mapping);
            
            // Setting up an ensemble of Matrix Factorization and Item-Average...
            WeightedEnsemble ensemble = new WeightedEnsemble();
            ItemAverage item_avg = new ItemAverage();
            item_avg.Ratings = training_data;
            MatrixFactorization mf = new MatrixFactorization();
            mf.Ratings = training_data;
            
			ensemble.engines.Add(mf);
            ensemble.engines.Add(item_avg);
            ensemble.weights.Add(0.8);
            ensemble.weights.Add(0.2);
            Console.WriteLine("Training the recommender..."); 
            ensemble.Train();

            int user_id = training_data.MaxUserID + 1;
			item_avg.AddUser(user_id);
			mf.AddUser(user_id);

            frmDemo frm = new frmDemo(ensemble, training_data, user_id);
            frm.ShowDialog();
            return;
        }
    }
}