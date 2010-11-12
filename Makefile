PDF_VIEWER=evince
EDITOR=editor
GENDARME_OPTIONS=--quiet --severity critical+
SRC_DIR=src

all:
	cd ${SRC_DIR} && make all

clean:
	cd ${SRC_DIR} && make clean
	rm -rf doc/monodoc/*
	rm -rf website/public_html/*

install:
	cd ${SRC_DIR} && make install

install:
	cd ${SRC_DIR} && make uninstall

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
htmldoc:
	mdoc-export-html doc/monodoc/ -o website/public_html/documentation/api --template doc/doctemplate.xsl

.PHONY: apidoc
apidoc: monodoc htmldoc

view-apidoc:
	x-www-browser doc/html/index.html

edit-apidoc-stylesheet:
	${EDITOR} doc/doctemplate.xsl

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
