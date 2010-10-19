PDF_VIEWER=evince
EDITOR=editor

monodoc:
	mdoc update -i Algorithms/bin/Debug/Algorithms.xml -o doc/monodoc/ Algorithms/bin/Debug/Algorithms.dll
htmldoc:
	mdoc-export-html doc/monodoc/ -o doc/html --template doc/doctemplate.xsl

.PHONY: apidoc
apidoc: monodoc htmldoc

view-apidoc:
	x-www-browser doc/html/index.html

edit-apidoc-stylesheet:
	${EDITOR} doc/doctemplate.xsl

flyer:
	cd doc/flyer
	pdflatex mymedialite-flyer.tex

.PHONY: view-flyer
view-flyer:
	${PDF_VIEWER} doc/flyer/mymedialite-flyer.pdf

 .PHONY: website
website:
	ttree -s website/src/ -d website/public_html/ -c website/lib/ -l website/lib/ -r -f config --post_chomp -a
