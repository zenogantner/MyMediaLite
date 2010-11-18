compile:
	gmcs -debug -r:MyMediaLite.dll item_prediction.cs
	gmcs -debug -r:MyMediaLite.dll rating_prediction.cs

run:
	mono --debug rating_prediction.exe ../../data/ml100k/u1.base ../..//data/ml100k/u1.test
	mono --debug item_prediction.exe   ../../data/ml100k/u1.base ../../data/ml100k/u1.test

clean:
	rm -f rating_prediction.exe rating_prediction.exe.mdb item_prediction.exe item_prediction.exe.mdb
