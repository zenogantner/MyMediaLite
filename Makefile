PDF_VIEWER=evince
EDITOR=editor

apidoc:
	mdoc update -i Algorithms/bin/Debug/Algorithms.xml -o doc/monodoc/ Algorithms/bin/Debug/Algorithms.dll
	mdoc-export-html doc/monodoc/ -o doc/html --template doc/doctemplate.xsl

view-apidoc:
	x-www-browser doc/html/index.html

edit-apidoc-stylesheet:
	${EDITOR} doc/doctemplate.xsl

flyer:
	cd doc/flyer
	pdflatex mymedialite-flyer.tex

view-flyer:
	${PDF_VIEWER} doc/flyer/mymedialite-flyer.pdf