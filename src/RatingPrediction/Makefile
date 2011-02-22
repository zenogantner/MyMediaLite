
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/RatingPrediction.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../MyMediaLite/bin/Debug/MyMediaLite.dll \
	../MyMediaLiteExperimental/bin/Debug/MyMediaLiteExperimental.dll
BUILD_DIR = bin/Debug

MYMEDIALITE_DLL_SOURCE=../MyMediaLite/bin/Debug/MyMediaLite.dll
MATHNET_IRIDIUM_DLL_SOURCE=../../lib/MathNet/MathNet.Iridium.dll
MYMEDIALITE_DLL_MDB_SOURCE=../MyMediaLite/bin/Debug/MyMediaLite.dll.mdb
MYMEDIALITE_DLL_MDB=$(BUILD_DIR)/MyMediaLite.dll.mdb
MYMEDIALITEEXPERIMENTAL_DLL_SOURCE=../MyMediaLiteExperimental/bin/Debug/MyMediaLiteExperimental.dll
SVM_DLL_SOURCE=../SVM/bin/Debug/SVM.dll
SVM_DLL_MDB_SOURCE=../SVM/bin/Debug/SVM.dll.mdb
SVM_DLL_MDB=$(BUILD_DIR)/SVM.dll.mdb
MYMEDIALITEEXPERIMENTAL_DLL_MDB_SOURCE=../MyMediaLiteExperimental/bin/Debug/MyMediaLiteExperimental.dll.mdb
MYMEDIALITEEXPERIMENTAL_DLL_MDB=$(BUILD_DIR)/MyMediaLiteExperimental.dll.mdb
RATINGPREDICTION_EXE_MDB_SOURCE=bin/Debug/RatingPrediction.exe.mdb
RATINGPREDICTION_EXE_MDB=$(BUILD_DIR)/RatingPrediction.exe.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+
ASSEMBLY = bin/Release/RatingPrediction.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../MyMediaLite/bin/Release/MyMediaLite.dll \
	../MyMediaLiteExperimental/bin/Release/MyMediaLiteExperimental.dll
BUILD_DIR = bin/Release

MYMEDIALITE_DLL_SOURCE=../MyMediaLite/bin/Release/MyMediaLite.dll
MATHNET_IRIDIUM_DLL_SOURCE=../../lib/MathNet/MathNet.Iridium.dll
MYMEDIALITE_DLL_MDB=
MYMEDIALITEEXPERIMENTAL_DLL_SOURCE=../MyMediaLiteExperimental/bin/Release/MyMediaLiteExperimental.dll
SVM_DLL_SOURCE=../SVM/bin/Release/SVM.dll
SVM_DLL_MDB=
MYMEDIALITEEXPERIMENTAL_DLL_MDB=
RATINGPREDICTION_EXE_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(MYMEDIALITE_DLL) \
	$(MATHNET_IRIDIUM_DLL) \
	$(MYMEDIALITE_DLL_MDB) \
	$(MYMEDIALITEEXPERIMENTAL_DLL) \
	$(SVM_DLL) \
	$(SVM_DLL_MDB) \
	$(MYMEDIALITEEXPERIMENTAL_DLL_MDB) \
	$(RATINGPREDICTION_EXE_MDB)  

BINARIES = \
	$(RATING_PREDICTION)  


RESGEN=resgen2

MYMEDIALITE_DLL = $(BUILD_DIR)/MyMediaLite.dll
MATHNET_IRIDIUM_DLL = $(BUILD_DIR)/MathNet.Iridium.dll
MYMEDIALITEEXPERIMENTAL_DLL = $(BUILD_DIR)/MyMediaLiteExperimental.dll
SVM_DLL = $(BUILD_DIR)/SVM.dll
RATING_PREDICTION = $(BUILD_DIR)/rating_prediction

FILES = \
	RatingPrediction.cs \
	AssemblyInfo.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	rating_prediction.in 

REFERENCES =  \
	System \
	System.Core \
	System.Data

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(BINARIES) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(BINARIES)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-target,MYMEDIALITE_DLL))
$(eval $(call emit-deploy-target,MATHNET_IRIDIUM_DLL))
$(eval $(call emit-deploy-target,MYMEDIALITE_DLL_MDB))
$(eval $(call emit-deploy-target,MYMEDIALITEEXPERIMENTAL_DLL))
$(eval $(call emit-deploy-target,SVM_DLL))
$(eval $(call emit-deploy-target,SVM_DLL_MDB))
$(eval $(call emit-deploy-target,MYMEDIALITEEXPERIMENTAL_DLL_MDB))
$(eval $(call emit-deploy-wrapper,RATING_PREDICTION,rating_prediction,x))


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
	$(call cp,$(MYMEDIALITEEXPERIMENTAL_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(SVM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(SVM_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(MYMEDIALITEEXPERIMENTAL_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(RATINGPREDICTION_EXE_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	mkdir -p '$(DESTDIR)$(bindir)'
	$(call cp,$(RATING_PREDICTION),$(DESTDIR)$(bindir))
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	make uninstall-satellite-assemblies prefix=$(prefix)
	$(call rm,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MYMEDIALITE_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MATHNET_IRIDIUM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MYMEDIALITE_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MYMEDIALITEEXPERIMENTAL_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(SVM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(SVM_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MYMEDIALITEEXPERIMENTAL_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(RATINGPREDICTION_EXE_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(RATING_PREDICTION),$(DESTDIR)$(bindir))
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
