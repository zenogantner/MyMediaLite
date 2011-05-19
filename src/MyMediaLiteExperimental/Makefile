
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = bin/Debug/MyMediaLiteExperimental.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../MyMediaLite/bin/Debug/MyMediaLite.dll \
	../SVM/bin/Debug/SVM.dll
BUILD_DIR = bin/Debug

MYMEDIALITEEXPERIMENTAL_DLL_MDB_SOURCE=bin/Debug/MyMediaLiteExperimental.dll.mdb
MYMEDIALITEEXPERIMENTAL_DLL_MDB=$(BUILD_DIR)/MyMediaLiteExperimental.dll.mdb
MYMEDIALITE_DLL_SOURCE=../MyMediaLite/bin/Debug/MyMediaLite.dll
MATHNET_IRIDIUM_DLL_SOURCE=../../lib/MathNet/MathNet.Iridium.dll
MYMEDIALITE_DLL_MDB_SOURCE=../MyMediaLite/bin/Debug/MyMediaLite.dll.mdb
MYMEDIALITE_DLL_MDB=$(BUILD_DIR)/MyMediaLite.dll.mdb
SVM_DLL_SOURCE=../SVM/bin/Debug/SVM.dll
SVM_DLL_MDB_SOURCE=../SVM/bin/Debug/SVM.dll.mdb
SVM_DLL_MDB=$(BUILD_DIR)/SVM.dll.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-define:TRACE"
ASSEMBLY = bin/Release/MyMediaLiteExperimental.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../MyMediaLite/bin/Release/MyMediaLite.dll \
	../SVM/bin/Release/SVM.dll
BUILD_DIR = bin/Release

MYMEDIALITEEXPERIMENTAL_DLL_MDB=
MYMEDIALITE_DLL_SOURCE=../MyMediaLite/bin/Release/MyMediaLite.dll
MATHNET_IRIDIUM_DLL_SOURCE=../../lib/MathNet/MathNet.Iridium.dll
MYMEDIALITE_DLL_MDB=
SVM_DLL_SOURCE=../SVM/bin/Release/SVM.dll
SVM_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(MYMEDIALITEEXPERIMENTAL_DLL_MDB) \
	$(MYMEDIALITE_DLL) \
	$(MATHNET_IRIDIUM_DLL) \
	$(MYMEDIALITE_DLL_MDB) \
	$(SVM_DLL) \
	$(SVM_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(MYMEDIALITEEXPERIMENTAL_PC)  


RESGEN=resgen2

MYMEDIALITE_DLL = $(BUILD_DIR)/MyMediaLite.dll
MATHNET_IRIDIUM_DLL = $(BUILD_DIR)/MathNet.Iridium.dll
SVM_DLL = $(BUILD_DIR)/SVM.dll
MYMEDIALITEEXPERIMENTAL_PC = $(BUILD_DIR)/mymedialiteexperimental.pc

FILES = \
	AttrToFactor/BPRMF_Mapping.cs \
	AttrToFactor/BPRMF_ItemMapping.cs \
	AttrToFactor/BPRMF_ItemMapping_Complex.cs \
	AttrToFactor/BPRMF_ItemMapping_Optimal.cs \
	AttrToFactor/BPRMF_UserMapping.cs \
	AttrToFactor/BPRMF_UserMapping_Optimal.cs \
	AttrToFactor/MF_Mapping.cs \
	AttrToFactor/MF_ItemMapping.cs \
	AttrToFactor/MF_Item_Mapping_Optimal.cs \
	ItemRecommender/ItemAttributeSVM.cs \
	Diversification/SequentialDiversification.cs \
	AttrToFactor/BPRMF_ItemMappingKNN.cs \
	AttrToFactor/BPRMF_ItemMappingSVR.cs \
	IO/KDDCup2011/Track2Candidates.cs \
	Eval/KDDCup.cs \
	Data/KDDCupItems.cs \
	Taxonomy/KDDCupItemType.cs \
	IO/KDDCup2011/Items.cs \
	IO/KDDCup2011/Ratings.cs \
	Eval/Track2Validation.cs \
	RatingPrediction/SocialMF.cs \
	RatingPrediction/NewKNN.cs \
	IKDDCupRecommender.cs \
	IO/KDDCup2011/Track2Items.cs \
	ItemRecommender/ISemiSupervisedItemRecommender.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	AttrToFactor \
	ItemRecommender \
	Util \
	Diversification \
	IO \
	IO/KDDCup2011 \
	Eval \
	Data \
	Taxonomy \
	Correlation \
	RatingPrediction \
	mymedialiteexperimental.pc.in 

REFERENCES =  \
	System.Core \
	System \
	System.Data

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-target,MYMEDIALITE_DLL))
$(eval $(call emit-deploy-target,MATHNET_IRIDIUM_DLL))
$(eval $(call emit-deploy-target,MYMEDIALITE_DLL_MDB))
$(eval $(call emit-deploy-target,SVM_DLL))
$(eval $(call emit-deploy-target,SVM_DLL_MDB))
$(eval $(call emit-deploy-wrapper,MYMEDIALITEEXPERIMENTAL_PC,mymedialiteexperimental.pc))


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
	$(call cp,$(MYMEDIALITEEXPERIMENTAL_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(MYMEDIALITE_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(MATHNET_IRIDIUM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(MYMEDIALITE_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(SVM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(SVM_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	mkdir -p '$(DESTDIR)$(libdir)/pkgconfig'
	$(call cp,$(MYMEDIALITEEXPERIMENTAL_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	make uninstall-satellite-assemblies prefix=$(prefix)
	$(call rm,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MYMEDIALITEEXPERIMENTAL_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MYMEDIALITE_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MATHNET_IRIDIUM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MYMEDIALITE_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(SVM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(SVM_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MYMEDIALITEEXPERIMENTAL_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
