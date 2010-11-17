PDF_VIEWER=evince
EDITOR=editor
GENDARME_OPTIONS=--quiet --severity critical+
SRC_DIR=src
CONFIGURE_OPTIONS=--prefix=/usr/local --config=DEBUG
VERSION=0.06

.PHONY: add configure clean install uninstall todo gendarme monodoc htmldoc view-htmldoc flyer edit-flyer website copy-website binary-package source-package testsuite release download-movielens
all: configure
	cd ${SRC_DIR} && make all

configure:
	cd ${SRC_DIR} && ./configure ${CONFIGURE_OPTIONS}

clean:
	cd ${SRC_DIR} && make clean
	rm -rf doc/monodoc/*
	rm -rf website/public_html/*
	rm *.tar.gz

install:
	cd ${SRC_DIR} && make install

uninstall:
	cd ${SRC_DIR} && make uninstall

binary-package:

source-package:

testsuite:
	tests/test_rating_prediction.sh
	tests/test_item_prediction.sh
	tests/test_load_save.sh

release: clean all testsuite binary-package source-package
	head doc/Changes
	git status
	echo "Checklist:"
	echo "1. Check the output of the test suite"
	echo "2. Check the output of 'git status' above - is everything in the repository?"
	echo "3. Version numbers"
	echo "3a. Have you set the VERSION string in the Makefile to the new version? Current setting is ${VERSION}."
	echo "3b. Check above if the version number and date (tomorrow) are set correctly in the Changes file"
	echo "4. Check the contents of the source code package"
	echo "5. Check the contents of the binary package"
	echo "6. Create the release announcement"
	echo "7. Commit the website changes"
	echo "8. Copy announcement to the website"

download-movielens:
	#wget --output-document=data/ml-data.tar.gz         http://www.grouplens.org/system/files/ml-data.tar__0.gz
	#wget --output-document=data/million-ml-data.tar.gz http://www.grouplens.org/system/files/million-ml-data.tar__0.gz
	cd data && tar -zxf ml-data.tar.gz
	mv data/ml-data data/ml100k
	cd data && tar -zxf million-ml-data.tar.gz
	mkdir data/ml1m
	cd data && mv README movies.dat ratings.dat users.dat ml1m

todo:
	ack --type=csharp TODO                    ${SRC_DIR}; echo
	ack --type=csharp FIXME                   ${SRC_DIR}; echo
	ack --type=csharp HACK                    ${SRC_DIR}; echo
	ack --type=csharp NotImplementedException ${SRC_DIR}; echo
	ack --type=csharp TODO                    ${SRC_DIR} | wc -l
	ack --type=csharp FIXME                   ${SRC_DIR} | wc -l
	ack --type=csharp HACK                    ${SRC_DIR} | wc -l
	ack --type=csharp NotImplementedException ${SRC_DIR} | wc -l

gendarme:
	gendarme ${GENDARME_OPTIONS} ${SRC_DIR}/RatingPrediction/bin/Debug/*.exe
	gendarme ${GENDARME_OPTIONS} ${SRC_DIR}/ItemPrediction/bin/Debug/*.exe
	gendarme ${GENDARME_OPTIONS} ${SRC_DIR}/Mapping/bin/Debug/*.exe
	gendarme ${GENDARME_OPTIONS} ${SRC_DIR}/MyMediaLite/bin/Debug/MyMediaLite.dll
	gendarme ${GENDARME_OPTIONS} ${SRC_DIR}/MyMediaLite/bin/Debug/SVM.dll

monodoc:
	mdoc update -i ${SRC_DIR}/MyMediaLite/bin/Debug/MyMediaLite.xml -o doc/monodoc/ ${SRC_DIR}/MyMediaLite/bin/Debug/MyMediaLite.dll
htmldoc: monodoc
	mdoc-export-html doc/monodoc/ -o website/public_html/documentation/api --template doc/doctemplate.xsl

view-htmldoc:
	x-www-browser doc/html/index.html

flyer:
	cd doc/flyer; pdflatex mymedialite-flyer.tex

edit-flyer:
	${EDITOR} doc/flyer/mymedialite-flyer.tex

.PHONY: view-flyer
view-flyer:
	${PDF_VIEWER} doc/flyer/mymedialite-flyer.pdf &

 .PHONY: website
website:
	ttree -s website/src/ -d website/public_html/ -c website/lib/ -l website/lib/ -r -f config --post_chomp -a

copy-website:
	cp -r website/public_html/* ${HOME}/homepage/public_html/mymedialite/
