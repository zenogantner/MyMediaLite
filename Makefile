apidoc:
	mdoc update -i Algorithms/bin/Debug/Algorithms.xml -o doc/monodoc/ Algorithms/bin/Debug/Algorithms.dll
	mdoc-export-html doc/monodoc/ -o doc/html --template doc/doctemplate.xsl

view-apidoc:
	x-www-browser doc/html/index.html