PDF_VIEWER=evince
EDITOR=editor
GENDARME_OPTIONS=--quiet --severity critical+
SRC_DIR=src
CONFIGURE_OPTIONS=--prefix=/usr/local --config=DEBUG
VERSION=0.06

.PHONY: add configure clean veryclean install uninstall todo gendarme monodoc htmldoc view-htmldoc flyer edit-flyer website copy-website binary-package source-package testsuite release download-movielens copy-packages-website
all: configure
	cd ${SRC_DIR} && make all

configure:
	cd ${SRC_DIR} && ./configure ${CONFIGURE_OPTIONS}

clean:
	cd ${SRC_DIR} && make clean
	rm -rf doc/monodoc/*
	rm -rf website/public_html/*
	rm -f src/*/bin/Debug/*
	rm -f src/*/bin/Release/*
	rm -rf MyMediaLite-*

veryclean: clean
	rm -f *.tar.gz

install:
	cd ${SRC_DIR} && make install

uninstall:
	cd ${SRC_DIR} && make uninstall

binary-package:
	mkdir MyMediaLite-${VERSION}
	mkdir MyMediaLite-${VERSION}/doc
	cp doc/Authors doc/Changes doc/ComponentLicenses doc/GPL-3 doc/Installation doc/Roadmap MyMediaLite-${VERSION}/doc
	cp -r examples scripts MyMediaLite-${VERSION}
	cp README MyMediaLite-${VERSION}
	#mkdir MyMediaLite-${VERSION}/data
	cp src/ItemPrediction/bin/Debug/*.exe MyMediaLite-${VERSION}
	cp src/ItemPrediction/bin/Debug/*.dll MyMediaLite-${VERSION}
	cp src/ItemPrediction/bin/Debug/*.mdb MyMediaLite-${VERSION}
	cp src/RatingPrediction/bin/Debug/*.exe MyMediaLite-${VERSION}
	cp src/MappingItemPrediction/bin/Debug/*.exe MyMediaLite-${VERSION}
	tar -cvzf MyMediaLite-${VERSION}.tar.gz MyMediaLite-${VERSION}
	rm -rf MyMediaLite-${VERSION}

source-package: clean
	mkdir MyMediaLite-${VERSION}.src
	mkdir MyMediaLite-${VERSION}.src/doc
	cp doc/Authors doc/Changes doc/CodingStandards doc/ComponentLicenses doc/GPL-3 doc/Installation doc/ReleaseChecklist doc/Roadmap MyMediaLite-${VERSION}.src/doc
	rm -rf MyMediaLite-${VERSION}.src/flyer MyMediaLite-${VERSION}.src/manual
	cp -r src examples scripts tests MyMediaLite-${VERSION}.src
	cp Makefile README MyMediaLite-${VERSION}.src
	mkdir MyMediaLite-${VERSION}.src/data
	tar -cvzf MyMediaLite-${VERSION}.src.tar.gz MyMediaLite-${VERSION}.src
	rm -rf MyMediaLite-${VERSION}.src

testsuite:
	time tests/test_rating_prediction.sh
	time tests/test_item_prediction.sh
	time tests/test_load_save.sh

release: clean all testsuite binary-package source-package
	head doc/Changes
	git status
	cat doc/ReleaseChecklist

download-movielens:
	scripts/download_movielens.sh

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

view-flyer:
	${PDF_VIEWER} doc/flyer/mymedialite-flyer.pdf &

website:
	ttree -s website/src/ -d website/public_html/ -c website/lib/ -l website/lib/ -r -f config --post_chomp -a

copy-website:
	cp -r website/public_html/* ${HOME}/homepage/public_html/mymedialite/

copy-packages-website:
	cp MyMediaLite-${VERSION}.tar.gz MyMediaLite-${VERSION}.src.tar.gz doc/Changes ${HOME}/homepage/public_html/mymedialite/