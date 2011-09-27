compile:
	gmcs -debug -r:MyMediaLite.dll item_recommendation.cs
	gmcs -debug -r:MyMediaLite.dll rating_prediction.cs

run:
	mono --debug rating_prediction.exe ../../data/ml100k/u1.base ../..//data/ml100k/u1.test
	mono --debug item_recommendation.exe   ../../data/ml100k/u1.base ../../data/ml100k/u1.test

clean:
	rm -f *.exe *.mdb
