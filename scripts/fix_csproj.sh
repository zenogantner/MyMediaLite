#!/bin/bash

# run from project root

for f in `ls src/*/*.csproj src/*/*/*.csproj`
do
	cat ${f} | perl -npe 's{<OutputPath>bin\\(Debug|Release)</OutputPath>}{<OutputPath>bin/$1</OutputPath>}' > ${f}.new
	mv ${f}.new ${f}
done
