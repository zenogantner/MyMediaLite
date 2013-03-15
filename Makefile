PDF_VIEWER=evince
EDITOR=editor
GENDARME_OPTIONS=--quiet --severity critical+
SRC_DIR=src
PREFIX=/usr/local
VERSION=3.07
HOMEPAGE=../mymedialite.net/public_html
HOMEPAGE_SRC=../mymedialite.net/src
HOMEPAGE_INC=../mymedialite.net/lib
HTML_DOXYGEN_DIR=${HOMEPAGE}/documentation/doxygen
MYMEDIA_ASSEMBLY_DIR=$(CURDIR)/src/MyMediaLite/bin/Debug
ITEM_REC_DIR=${SRC_DIR}/Programs/ItemRecommendation
RATING_PRED_DIR=${SRC_DIR}/Programs/RatingPrediction
RATING_RANK_DIR=${SRC_DIR}/Programs/RatingBasedRanking
ACK=ack-grep
export IRONPYTHONPATH := ${MYMEDIA_ASSEMBLY_DIR}

.PHONY: all clean veryclean mymedialite install uninstall todo gendarme monodoc doxygen view-doxygen flyer edit-flyer test release download-movielens copy-packages-website example-python example-ruby unittests

all: mymedialite

mymedialite:
	cd ${SRC_DIR} && make all

clean:
	cd ${SRC_DIR} && make clean
	cd examples/csharp && make clean
	rm -rf ${SRC_DIR}/Programs/*/bin/Debug/*
	rm -rf ${SRC_DIR}/Programs/*/bin/Release/*
	rm -rf ${SRC_DIR}/*/bin/Debug/*
	rm -rf ${SRC_DIR}/*/bin/Release/*
	rm -rf ${SRC_DIR}/test-results
	rm -rf ${SRC_DIR}/*/*.tar.gz
	rm -rf ${SRC_DIR}/*/*.pidb
	rm -rf doc/monodoc/*
	rm -rf lib/mymedialite/*
	rm -rf MyMediaLite-*/

veryclean: clean
	rm -f *.tar.gz
	rm -rf doc/doxygen/*

install:
	cd ${SRC_DIR} && make install PREFIX=${PREFIX}

uninstall:
	cd ${SRC_DIR} && make uninstall PREFIX=${PREFIX}

MyMediaLite-${VERSION}.tar.gz:
	mkdir MyMediaLite-${VERSION}
	mkdir MyMediaLite-${VERSION}/doc/
	cp doc/Authors doc/Changes doc/ComponentLicenses doc/GPL-3 doc/Installation doc/TODO MyMediaLite-${VERSION}/doc
	cp -r bin examples scripts MyMediaLite-${VERSION}
	cp README MyMediaLite-${VERSION}
	mkdir MyMediaLite-${VERSION}/lib/
	mkdir MyMediaLite-${VERSION}/lib/mymedialite
	cp ${ITEM_REC_DIR}/bin/Debug/*.exe MyMediaLite-${VERSION}/lib/mymedialite
	cp ${ITEM_REC_DIR}/bin/Debug/*.dll MyMediaLite-${VERSION}/lib/mymedialite
	cp ${ITEM_REC_DIR}/bin/Debug/*.mdb MyMediaLite-${VERSION}/lib/mymedialite
	cp ${RATING_PRED_DIR}/bin/Debug/*.exe MyMediaLite-${VERSION}/lib/mymedialite
	cp ${RATING_PRED_DIR}/bin/Debug/*.exe.mdb MyMediaLite-${VERSION}/lib/mymedialite
	cp ${RATING_RANK_DIR}/bin/Debug/*.exe MyMediaLite-${VERSION}/lib/mymedialite
	cp ${RATING_RANK_DIR}/bin/Debug/*.exe.mdb MyMediaLite-${VERSION}/lib/mymedialite
	tar -cvzf MyMediaLite-${VERSION}.tar.gz MyMediaLite-${VERSION}
	rm -rf MyMediaLite-${VERSION}

MyMediaLite-${VERSION}.doc.tar.gz: doxygen
	mkdir MyMediaLite-${VERSION}.doc/
	mkdir MyMediaLite-${VERSION}.doc/doc/
	mkdir MyMediaLite-${VERSION}.doc/doc/api
	cp -r doc/doxygen/html MyMediaLite-${VERSION}.doc/doc/api
	tar -cvzf MyMediaLite-${VERSION}.doc.tar.gz MyMediaLite-${VERSION}.doc
	rm -rf MyMediaLite-${VERSION}.doc

MyMediaLite-${VERSION}.src.tar.gz:
	wget --output-document=MyMediaLite-${VERSION}.src.tar.gz https://github.com/zenogantner/MyMediaLite/tarball/master

test: data/ml-100k/u.data all unittests
	time tests/test_rating_prediction.sh
	time tests/test_item_recommendation.sh
	time tests/test_item_recommendation_predict.sh
	time tests/test_load_save.sh
	time tests/test_cv.sh
	time tests/test_random_split.sh
	time tests/test_item_recommendation_online.sh
	time tests/test_rating_prediction_online.sh
	time tests/test_rating_prediction_time.sh

unittests:
	cd src && make test

release: mymedialite MyMediaLite-${VERSION}.doc.tar.gz MyMediaLite-${VERSION}.tar.gz MyMediaLite-${VERSION}.src.tar.gz
	head doc/Changes
	git status
	cp doc/Changes ${HOMEPAGE_SRC}/download
	bin/rating_prediction --help > ${HOMEPAGE_INC}/rating_prediction_usage
	bin/item_recommendation --help > ${HOMEPAGE_INC}/item_recommendation_usage
	cat doc/ReleaseChecklist

example-csharp: data/ml-100k/u.data
	cd examples/csharp && make
	cd examples/csharp && make run

example-fsharp: data/ml-100k/u.data
	cd examples/fsharp && make
	cd data/ml-100k && mono ../../examples/fsharp/rating_prediction.exe
	cd data/ml-100k && mono ../../examples/fsharp/item_recommendation.exe


example-python: data/ml-100k/u.data
	cd data/ml-100k && ipy ../../examples/python/rating_prediction.py
	cd data/ml-100k && ipy ../../examples/python/item_recommendation.py

example-ruby: data/ml-100k/u.data
	cd data/ml-100k && ir -I${MYMEDIA_ASSEMBLY_DIR} ../../examples/ruby/rating_prediction.rb
	cd data/ml-100k && ir -I${MYMEDIA_ASSEMBLY_DIR} ../../examples/ruby/item_recommendation.rb

data:
	mkdir data/

data/ml-100k/u.data:
	scripts/download_movielens.sh

download-movielens: data
	scripts/download_movielens.sh

download-imdb: data
	scripts/download_imdb.sh

todo:
	${ACK} --type=csharp TODO                    ${SRC_DIR}; echo
	${ACK} --type=csharp FIXME                   ${SRC_DIR}; echo
	${ACK} --type=csharp HACK                    ${SRC_DIR}; echo
	${ACK} --type=csharp NotImplementedException ${SRC_DIR}; echo
	${ACK} --type=csharp TODO                    ${SRC_DIR} | wc -l
	${ACK} --type=csharp FIXME                   ${SRC_DIR} | wc -l
	${ACK} --type=csharp HACK                    ${SRC_DIR} | wc -l
	${ACK} --type=csharp NotImplementedException ${SRC_DIR} | wc -l

gendarme:
	gendarme ${GENDARME_OPTIONS} ${RATING_PRED_DIR}/bin/Debug/*.exe
	gendarme ${GENDARME_OPTIONS} ${ITEM_REC_DIR}/bin/Debug/*.exe
	gendarme ${GENDARME_OPTIONS} ${SRC_DIR}/MyMediaLite/bin/Debug/MyMediaLite.dll
	gendarme ${GENDARME_OPTIONS} ${SRC_DIR}/MyMediaLite/bin/Debug/SVM.dll

apidoc: doxygen

monodoc:
	mdoc update --delete -i ${SRC_DIR}/MyMediaLite/bin/Debug/MyMediaLite.xml -o doc/monodoc/ ${SRC_DIR}/MyMediaLite/bin/Debug/MyMediaLite.dll

doxygen:
	cd doc/ && doxygen
	mkdir -p ${HTML_DOXYGEN_DIR}
	cp -r doc/doxygen/html/* ${HTML_DOXYGEN_DIR}

view-doxygen:
	x-www-browser file://${HTML_DOXYGEN_DIR}/index.html

flyer:
	cd doc/flyer; pdflatex mymedialite-flyer.tex

edit-flyer:
	${EDITOR} doc/flyer/mymedialite-flyer.tex

view-flyer:
	${PDF_VIEWER} doc/flyer/mymedialite-flyer.pdf &

copy-packages-website:
	cp MyMediaLite-${VERSION}.tar.gz MyMediaLite-${VERSION}.src.tar.gz MyMediaLite-${VERSION}.doc.tar.gz ${HOMEPAGE}/download
