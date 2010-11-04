PDF_VIEWER=evince
EDITOR=editor

todo:
	ack --type=csharp TODO
	ack --type=csharp FIXME
	ack --type=csharp NotImplementedException
	ack --type=csharp TODO | wc -l
	ack --type=csharp FIXME | wc -l
	ack --type=csharp NotImplementedException | wc -l

gendarme:
	gendarme --severity critical+ RatingPrediction/bin/Debug/*.dll

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
