PDF_VIEWER=evince
EDITOR=editor
GENDARME_OPTIONS=--quiet --severity critical+

todo:
	ack --type=csharp TODO;  echo
	ack --type=csharp FIXME; echo
	ack --type=csharp HACK;  echo
	ack --type=csharp NotImplementedException; echo
	ack --type=csharp TODO  | wc -l
	ack --type=csharp FIXME | wc -l
	ack --type=csharp HACK  | wc -l
	ack --type=csharp NotImplementedException | wc -l

gendarme:
	gendarme ${GENDARME_OPTIONS} RatingPrediction/bin/Debug/*.exe
	gendarme ${GENDARME_OPTIONS} ItemPrediction/bin/Debug/*.exe
	gendarme ${GENDARME_OPTIONS} Mapping/bin/Debug/*.exe
	gendarme ${GENDARME_OPTIONS} RatingPrediction/bin/Debug/MyMediaLite.dll
	gendarme ${GENDARME_OPTIONS} RatingPrediction/bin/Debug/SVM.dll

monodoc:
	mdoc update -i MyMediaLite/bin/Debug/MyMediaLite.xml -o doc/monodoc/ MyMediaLite/bin/Debug/MyMediaLite.dll
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
