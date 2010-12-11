
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG_X86)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = bin/Debug/MyMediaLite.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../SVM/bin/Debug/SVM.dll
BUILD_DIR = bin/Debug/

MATHNET_IRIDIUM_DLL_SOURCE=../../lib/MathNet/MathNet.Iridium.dll
SVM_DLL_SOURCE=../SVM/bin/Debug/SVM.dll
SVM_DLL_MDB_SOURCE=../SVM/bin/Debug/SVM.dll.mdb
SVM_DLL_MDB=$(BUILD_DIR)/SVM.dll.mdb
MYMEDIALITE_DLL_MDB_SOURCE=bin/Debug/MyMediaLite.dll.mdb
MYMEDIALITE_DLL_MDB=$(BUILD_DIR)/MyMediaLite.dll.mdb

endif

ifeq ($(CONFIG),RELEASE_X86)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+ "-define:TRACE"
ASSEMBLY = bin/Release/MyMediaLite.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../SVM/bin/Release/SVM.dll
BUILD_DIR = bin/Release/

MATHNET_IRIDIUM_DLL_SOURCE=../../lib/MathNet/MathNet.Iridium.dll
SVM_DLL_SOURCE=../SVM/bin/Release/SVM.dll
SVM_DLL_MDB=
MYMEDIALITE_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(MATHNET_IRIDIUM_DLL) \
	$(SVM_DLL) \
	$(SVM_DLL_MDB) \
	$(MYMEDIALITE_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(MYMEDIALITE_PC)  


RESGEN=resgen2

MATHNET_IRIDIUM_DLL = $(BUILD_DIR)/MathNet.Iridium.dll
SVM_DLL = $(BUILD_DIR)/SVM.dll
MYMEDIALITE_PC = $(BUILD_DIR)/mymedialite.pc

FILES =  \
	AssemblyInfo.cs \
	Correlation/CorrelationMatrix.cs \
	Correlation/Cosine.cs \
	Correlation/Pearson.cs \
	Data/EntityMapping.cs \
	Data/ISplit.cs \
	Data/RatingCrossValidationSplit.cs \
	Data/RatingData.cs \
	Data/RatingEvent.cs \
	Data/Ratings.cs \
	DataType/Matrix.cs \
	DataType/MatrixUtils.cs \
	DataType/SparseBooleanMatrix.cs \
	DataType/SparseMatrix.cs \
	DataType/SparseVector.cs \
	DataType/VectorUtils.cs \
	DataType/WeightedItem.cs \
	Ensemble/Ensemble.cs \
	Ensemble/WeightedEnsemble.cs \
	Eval/ItemPrediction.cs \
	Eval/ItemPredictionEval.cs \
	Eval/RatingEval.cs \
	Eval/RatingPrediction.cs \
	IItemAttributeAwareRecommender.cs \
	IItemRelationAwareRecommender.cs \
	IIterativeModel.cs \
	IO/AttributeData.cs \
	IO/ItemRecommenderData.cs \
	IO/RatingPredictionData.cs \
	IO/RelationData.cs \
	IRecommenderEngine.cs \
	ItemRecommender/BPR_Linear.cs \
	ItemRecommender/BPRMF.cs \
	ItemRecommender/IItemRecommender.cs \
	ItemRecommender/ItemAttributeKNN.cs \
	ItemRecommender/ItemAttributeSVM.cs \
	ItemRecommender/ItemKNN.cs \
	ItemRecommender/KNN.cs \
	ItemRecommender/Memory.cs \
	ItemRecommender/MF.cs \
	ItemRecommender/MostPopular.cs \
	ItemRecommender/Random.cs \
	ItemRecommender/UserAttributeKNN.cs \
	ItemRecommender/UserKNN.cs \
	ItemRecommender/WeightedItemKNN.cs \
	ItemRecommender/WeightedUserKNN.cs \
	ItemRecommender/WRMF.cs \
	IUserAttributeAwareRecommender.cs \
	IUserRelationAwareRecommender.cs \
	RatingPredictor/BiasedMatrixFactorization.cs \
	RatingPredictor/IRatingPredictor.cs \
	RatingPredictor/ItemAttributeKNN.cs \
	RatingPredictor/ItemKNN.cs \
	RatingPredictor/ItemKNNCosine.cs \
	RatingPredictor/ItemKNNPearson.cs \
	RatingPredictor/KNN.cs \
	RatingPredictor/Linear.cs \
	RatingPredictor/MatrixFactorization.cs \
	RatingPredictor/Memory.cs \
	RatingPredictor/SocialMF.cs \
	RatingPredictor/UserAttributeKNN.cs \
	RatingPredictor/UserItemBaseline.cs \
	RatingPredictor/UserKNN.cs \
	RatingPredictor/UserKNNCosine.cs \
	RatingPredictor/UserKNNPearson.cs \
	Taxonomy/EntityType.cs \
	Util/CommandLineParameters.cs \
	Util/Engine.cs \
	Util/Handlers.cs \
	Util/Pair.cs \
	Util/Random.cs \
	Util/Triple.cs \
	Util/Utils.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	Correlation \
	ItemRecommender \
	RatingPredictor \
	Eval \
	DataType \
	Util \
	IO \
	Data \
	Taxonomy \
	Ensemble \
	mymedialite.pc.in 

REFERENCES =  \
	../../lib/MathNet/MathNet.Iridium.dll \
	../SVM/bin/Debug/SVM.dll \
	System \
	System.Core \
	System.Data \
	System.Data.DataSetExtensions

DLL_REFERENCES =  \
	../../lib/MathNet/MathNet.Iridium.dll

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-target,MATHNET_IRIDIUM_DLL))
$(eval $(call emit-deploy-target,SVM_DLL))
$(eval $(call emit-deploy-target,SVM_DLL_MDB))
$(eval $(call emit-deploy-wrapper,MYMEDIALITE_PC,mymedialite.pc))


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
	$(call cp,$(MATHNET_IRIDIUM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(SVM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(SVM_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(MYMEDIALITE_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	mkdir -p '$(DESTDIR)$(libdir)/pkgconfig'
	$(call cp,$(MYMEDIALITE_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	make uninstall-satellite-assemblies prefix=$(prefix)
	$(call rm,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MATHNET_IRIDIUM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(SVM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(SVM_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MYMEDIALITE_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MYMEDIALITE_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
