// Copyright (C) 2017 Mark Graus
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
using System.Data;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Eval;
using MyMediaLite.IO;
using MyMediaLite.ItemRecommendation;

namespace UGrowRecommendations
{
    class Program
    {
        public static void Main(string[] args)
        {

            // load the data
            var user_mapping = new Mapping();
            var item_mapping = new Mapping();
            
            var training_data = ItemData.Read("traindata.txt", user_mapping, item_mapping);
            var test_data = ItemData.Read("testdata.txt", user_mapping, item_mapping);

            var user_attributes = AttributeData.Read("user_attributes.txt", user_mapping);
            //var user_attributes = AttributeData.Read(args[2], )
            // set up the recommender
            var recommender = new MostPopular();
            recommender.Feedback = training_data;
            recommender.Train();

            // measure the accuracy on the test data set
            var results = recommender.Evaluate(test_data, training_data);
            /*
            var meh = "";
            foreach (var key in results.Keys)
                meh = meh + key + "=" + results[key] + ";";
            Console.WriteLine(meh);
            */
            Console.WriteLine(results);

            var recommender2 = new UserAttributeBPRMF();
            recommender2.Feedback = training_data;
            recommender2.UserAttributes = user_attributes;
            recommender2.Train();

            results = recommender2.Evaluate(test_data, training_data);
            /*
            var meh = "";
            foreach (var key in results.Keys)
                meh = meh + key + "=" + results[key] + ";";
            Console.WriteLine(meh);
            */
            Console.WriteLine(results);

            var recommender3 = new BPRMF();
            recommender3.Feedback = training_data;
            recommender3.Train();

            results = recommender3.Evaluate(test_data, training_data);
            /*
            var meh = "";
            foreach (var key in results.Keys)
                meh = meh + key + "=" + results[key] + ";";
            Console.WriteLine(meh);
            */
            Console.WriteLine(results);
            using (var writer = FileSystem.CreateStreamWriter("personalizedpluspredictions.txt"))
            {
                foreach (var recommendation in recommender2.Recommend(1))
                    writer.WriteLine(item_mapping.ToOriginalID(recommendation.Item1) + " " + recommendation.Item2);
            }
            using (var writer = FileSystem.CreateStreamWriter("genpoppredictions.txt"))
            {
                foreach (var recommendation in recommender.Recommend(1))
                    writer.WriteLine(item_mapping.ToOriginalID(recommendation.Item1) + " " + recommendation.Item2);
            }
            using (var writer = FileSystem.CreateStreamWriter("personalizedpredictions.txt"))
            {
                foreach (var recommendation in recommender3.Recommend(1))
                    writer.WriteLine(item_mapping.ToOriginalID(recommendation.Item1) + " " + recommendation.Item2);
            }

        }
    }
}
