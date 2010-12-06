PDF_VIEWER=evince
EDITOR=editor
GENDARME_OPTIONS=--quiet --severity critical+
SRC_DIR=src
CONFIGURE_OPTIONS=--prefix=/usr/local
VERSION=0.08
HTML_MDOC_DIR=website/public_html/documentation/mdoc
HTML_DOXYGEN_DIR=website/public_html/documentation/doxygen
HTML_IMMDOC_DIR=website/public_html/documentation/immdoc

.PHONY: add configure clean veryclean install uninstall todo gendarme monodoc htmldoc view-htmldoc flyer edit-flyer website copy-website binary-package source-package test release download-movielens copy-packages-website
all: configure
	cd ${SRC_DIR} && make all

configure:
	cd ${SRC_DIR} && ./configure ${CONFIGURE_OPTIONS}

clean:
	cd ${SRC_DIR} && make clean
	cd examples/csharp && make clean
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

binary-package: all
	mkdir MyMediaLite-${VERSION}
	mkdir MyMediaLite-${VERSION}/doc
	cp doc/Authors doc/Changes doc/ComponentLicenses doc/GPL-3 doc/Installation doc/Roadmap MyMediaLite-${VERSION}/doc
	cp -r examples scripts MyMediaLite-${VERSION}
	cp README MyMediaLite-${VERSION}
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

test: all
	time tests/test_rating_prediction.sh
	time tests/test_item_prediction.sh
	time tests/test_load_save.sh

release: test binary-package source-package
	head doc/Changes
	git status
	cp doc/Changes website/src/download
	cat doc/ReleaseChecklist

data:
	mkdir data/

download-movielens: data
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
	gendarme ${GENDARME_OPTIONS} ${SRC_DIR}/MappingRatingPrediction/bin/Debug/*.exe
	gendarme ${GENDARME_OPTIONS} ${SRC_DIR}/MappingItemPrediction/bin/Debug/*.exe
	gendarme ${GENDARME_OPTIONS} ${SRC_DIR}/MyMediaLite/bin/Debug/MyMediaLite.dll
	gendarme ${GENDARME_OPTIONS} ${SRC_DIR}/MyMediaLite/bin/Debug/SVM.dll

monodoc:
	mdoc update -i ${SRC_DIR}/MyMediaLite/bin/Debug/MyMediaLite.xml -o doc/monodoc/ ${SRC_DIR}/MyMediaLite/bin/Debug/MyMediaLite.dll

mdoc-html: monodoc
	mdoc-export-html doc/monodoc/ -o ${HTML_MDOC_DIR} --template=doc/htmldoc-template.xsl
	perl -e "use File::Slurp; \$$f = read_file '${HTML_MDOC_DIR}/index.html'; \$$f =~ s/\n.+?\n.+?experimental.+?\n.+?\n.+?\n.+?\n.+//; print \$$f;" > tmp.html && cat tmp.html > ${HTML_MDOC_DIR}/index.html && rm tmp.html

view-mdoc:
	x-www-browser file://${HTML_MDOC_DIR}/index.html

doxygen:
	cd doc/ && doxygen
	cp -r doc/doxygen/html/* ${HTML_DOXYGEN_DIR}

view-doxygen:
	x-www-browser file://${HTML_DOXYGEN_DIR}/index.html

immdoc:
	mono --debug ~/Desktop/ImmDocNet.exe -vl:3 -ForceDelete -IncludePrivateMembers -pn:MyMediaLite -od:doc/immdoc ${SRC_DIR}/MyMediaLite/bin/Debug/MyMediaLite.xml ${SRC_DIR}/MyMediaLite/bin/Debug/MyMediaLite.dll
	cp -r doc/immdoc/* ${HTML_IMMDOC_DIR}

view-immdoc:
	x-www-browser file://${HTML_IMMDOC_DIR}/index.html

flyer:
	cd doc/flyer; pdflatex mymedialite-flyer.tex

edit-flyer:
	${EDITOR} doc/flyer/mymedialite-flyer.tex

view-flyer:
	${PDF_VIEWER} doc/flyer/mymedialite-flyer.pdf &

website:
	ttree -s website/src/ -d website/public_html/ -c website/lib/ -l website/lib/ -r -f config --post_chomp -a

copy-website: website
	cp -r website/public_html/* ${HOME}/homepage/public_html/mymedialite/

copy-packages-website:
	cp MyMediaLite-${VERSION}.tar.gz MyMediaLite-${VERSION}.src.tar.gz ${HOME}/homepage/public_html/mymedialite/download
