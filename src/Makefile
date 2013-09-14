all: ../lib/mymedialite
	xbuild /p:Configuration=Debug MyMediaLite.sln
	cp -r Programs/CommandLineProgram/bin/Debug/*.dll ../lib/mymedialite
	cp -r Programs/CommandLineProgram/bin/Debug/*.exe ../lib/mymedialite
	cp -r Programs/CommandLineProgram/bin/Debug/*.mdb ../lib/mymedialite

../lib/mymedialite:
	mkdir ../lib/mymedialite

clean:
	rm -f */bin/Debug/*
	rm -f */bin/Release/*
	rm -f */*/bin/Debug/*
	rm -f */*/bin/Release/*
	rm -rf */obj
	rm -rf */*/obj

${PREFIX}/lib:
	mkdir ${PREFIX}/lib

${PREFIX}/bin:
	mkdir ${PREFIX}/bin

install: uninstall ${PREFIX}/lib ${PREFIX}/bin
	mkdir ${PREFIX}/lib/mymedialite
	cp ../lib/mymedialite/* ${PREFIX}/lib/mymedialite
	cp ../bin/* ${PREFIX}/bin

uninstall:
	rm -rf ${PREFIX}/lib/mymedialite
	rm -rf ${PREFIX}/bin/mymedialite

test:
	nunit-console --run=Tests MyMediaLite.sln --config=Debug
