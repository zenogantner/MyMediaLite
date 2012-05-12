compile:
	dmcs -debug -r:MyMediaLite.dll item_recommendation.cs
	dmcs -debug -r:MyMediaLite.dll rating_prediction.cs
	dmcs -debug -r:MyMediaLite.dll foldin_incremental.cs

run:
	mono --debug rating_prediction.exe ../../data/ml-100k/u1.base ../../data/ml-100k/u1.test
	mono --debug item_recommendation.exe   ../../data/ml-100k/u1.base ../../data/ml-100k/u1.test
	#mono --debug foldin_incremental.exe

clean:
	rm -f *.exe *.mdb
