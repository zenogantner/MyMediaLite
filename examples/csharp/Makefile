compile:
	gmcs -debug -r:MyMediaLite.dll item_prediction.cs
	gmcs -debug -r:MyMediaLite.dll rating_prediction.cs

run:
	mono --debug rating_prediction.exe ${HOME}/data/ml100k/u1.base ${HOME}/data/ml100k/u1.test
	mono --debug item_prediction.exe   ${HOME}/data/ml100k/u1.base ${HOME}/data/ml100k/u1.test