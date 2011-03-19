
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/MyMediaLiteTest.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../MyMediaLite/bin/Debug/MyMediaLite.dll
BUILD_DIR = bin/Debug

MYMEDIALITE_DLL_SOURCE=../MyMediaLite/bin/Debug/MyMediaLite.dll
MATHNET_IRIDIUM_DLL_SOURCE=../../lib/MathNet/MathNet.Iridium.dll
MYMEDIALITE_DLL_MDB_SOURCE=../MyMediaLite/bin/Debug/MyMediaLite.dll.mdb
MYMEDIALITE_DLL_MDB=$(BUILD_DIR)/MyMediaLite.dll.mdb
MYMEDIALITETEST_DLL_MDB_SOURCE=bin/Debug/MyMediaLiteTest.dll.mdb
MYMEDIALITETEST_DLL_MDB=$(BUILD_DIR)/MyMediaLiteTest.dll.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/MyMediaLiteTest.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../MyMediaLite/bin/Release/MyMediaLite.dll
BUILD_DIR = bin/Release

MYMEDIALITE_DLL_SOURCE=../MyMediaLite/bin/Release/MyMediaLite.dll
MATHNET_IRIDIUM_DLL_SOURCE=../../lib/MathNet/MathNet.Iridium.dll
MYMEDIALITE_DLL_MDB=
MYMEDIALITETEST_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(MYMEDIALITE_DLL) \
	$(MATHNET_IRIDIUM_DLL) \
	$(MYMEDIALITE_DLL_MDB) \
	$(MYMEDIALITETEST_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(MYMEDIALITETEST_PC)  


RESGEN=resgen2

MYMEDIALITE_DLL = $(BUILD_DIR)/MyMediaLite.dll
MATHNET_IRIDIUM_DLL = $(BUILD_DIR)/MathNet.Iridium.dll
MYMEDIALITETEST_PC = $(BUILD_DIR)/mymedialitetest.pc

FILES =  \
	Correlation/BinaryCosineTest.cs \
	Correlation/CorrelationMatrixTest.cs \
	Correlation/PearsonTest.cs \
	Data/EntityMappingTest.cs \
	Data/RatingsTest.cs \
	Data/StaticByteRatingsTest.cs \
	Data/StaticFloatRatingsTest.cs \
	Data/StaticRatingsTest.cs \
	DataType/ListProxyTest.cs \
	DataType/MatrixTest.cs \
	DataType/MatrixUtilsTest.cs \
	DataType/SkewSymmetricSparseMatrixTest.cs \
	DataType/SparseBooleanMatrixTest.cs \
	DataType/SparseMatrixTest.cs \
	DataType/SymmetricSparseMatrixTest.cs \
	DataType/VectorUtilsTest.cs \
	RatingPrediction/BiPolarSlopeOneTest.cs \
	RatingPrediction/SlopeOneTest.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	DataType \
	Data \
	Correlation \
	RatingPrediction \
	mymedialitetest.pc.in 

REFERENCES =  \
	../MyMediaLite/bin/Debug/MyMediaLite.dll \
	-pkg:mono-nunit \
	System.Core

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-target,MYMEDIALITE_DLL))
$(eval $(call emit-deploy-target,MATHNET_IRIDIUM_DLL))
$(eval $(call emit-deploy-target,MYMEDIALITE_DLL_MDB))
$(eval $(call emit-deploy-wrapper,MYMEDIALITETEST_PC,mymedialitetest.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'


$(ASSEMBLY_MDB): $(ASSEMBLY)
$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	make pre-all-local-hook prefix=$(prefix)
	mkdir -p $(shell dirname $(ASSEMBLY))
	make $(CONFIG)_BeforeBuild
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
	make $(CONFIG)_AfterBuild
	make post-all-local-hook prefix=$(prefix)

install-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-install-local-hook prefix=$(prefix)
	make install-satellite-assemblies prefix=$(prefix)
	mkdir -p '$(DESTDIR)$(libdir)/$(PACKAGE)'
	$(call cp,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(MYMEDIALITE_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(MATHNET_IRIDIUM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(MYMEDIALITE_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(MYMEDIALITETEST_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	mkdir -p '$(DESTDIR)$(libdir)/pkgconfig'
	$(call cp,$(MYMEDIALITETEST_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	make uninstall-satellite-assemblies prefix=$(prefix)
	$(call rm,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MYMEDIALITE_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MATHNET_IRIDIUM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MYMEDIALITE_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MYMEDIALITETEST_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MYMEDIALITETEST_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
