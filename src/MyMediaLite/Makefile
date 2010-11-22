
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

SVM_DLL_SOURCE=../SVM/bin/Release/SVM.dll
SVM_DLL_MDB=
MYMEDIALITE_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SVM_DLL) \
	$(SVM_DLL_MDB) \
	$(MYMEDIALITE_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(MYMEDIALITE_PC)  


RESGEN=resgen2

SVM_DLL = $(BUILD_DIR)/SVM.dll
MYMEDIALITE_PC = $(BUILD_DIR)/mymedialite.pc

FILES =  \
	AssemblyInfo.cs \
	correlation/CorrelationMatrix.cs \
	correlation/Cosine.cs \
	correlation/Pearson.cs \
	data/EntityMapping.cs \
	data/RatingData.cs \
	data/RatingEvent.cs \
	data/Ratings.cs \
	data_type/Matrix.cs \
	data_type/MatrixUtils.cs \
	data_type/SparseBooleanMatrix.cs \
	data_type/SparseMatrix.cs \
	data_type/SparseVector.cs \
	data_type/VectorUtils.cs \
	data_type/WeightedItem.cs \
	ensemble/Ensemble.cs \
	ensemble/WeightedEnsemble.cs \
	eval/ItemPrediction.cs \
	eval/ItemPredictionEval.cs \
	eval/RatingEval.cs \
	eval/RatingPrediction.cs \
	experimental/attr_to_feature/BPRMF_ItemMapping.cs \
	experimental/attr_to_feature/BPRMF_ItemMapping_Complex.cs \
	experimental/attr_to_feature/BPRMF_ItemMapping_kNN.cs \
	experimental/attr_to_feature/BPRMF_ItemMapping_Optimal.cs \
	experimental/attr_to_feature/BPRMF_ItemMapping_SVR.cs \
	experimental/attr_to_feature/BPRMF_Mapping.cs \
	experimental/attr_to_feature/BPRMF_UserMapping.cs \
	experimental/attr_to_feature/BPRMF_UserMapping_Optimal.cs \
	experimental/attr_to_feature/MF_ItemMapping.cs \
	experimental/attr_to_feature/MF_Mapping.cs \
	IItemAttributeAwareRecommender.cs \
	IItemRelationAwareRecommender.cs \
	IIterativeModel.cs \
	ILatentFactorModel.cs \
	io/AttributeData.cs \
	io/ItemRecommenderData.cs \
	io/RatingPredictionData.cs \
	io/RelationData.cs \
	IRecommenderEngine.cs \
	item_recommender/BPR_Linear.cs \
	item_recommender/BPRMF.cs \
	item_recommender/IItemRecommender.cs \
	item_recommender/ItemAttributeKNN.cs \
	item_recommender/ItemAttributeSVM.cs \
	item_recommender/ItemKNN.cs \
	item_recommender/KNN.cs \
	item_recommender/Memory.cs \
	item_recommender/MF.cs \
	item_recommender/MostPopular.cs \
	item_recommender/Random.cs \
	item_recommender/UserAttributeKNN.cs \
	item_recommender/UserKNN.cs \
	item_recommender/WeightedItemKNN.cs \
	item_recommender/WeightedUserKNN.cs \
	item_recommender/WRMF.cs \
	IUserAttributeAwareRecommender.cs \
	IUserRelationAwareRecommender.cs \
	rating_predictor/BiasedMatrixFactorization.cs \
	rating_predictor/ItemAttributeKNN.cs \
	rating_predictor/ItemKNN.cs \
	rating_predictor/ItemKNNCosine.cs \
	rating_predictor/ItemKNNPearson.cs \
	rating_predictor/KNN.cs \
	rating_predictor/Linear.cs \
	rating_predictor/MatrixFactorization.cs \
	rating_predictor/Memory.cs \
	rating_predictor/RatingPredictor.cs \
	rating_predictor/SocialMF.cs \
	rating_predictor/UserAttributeKNN.cs \
	rating_predictor/UserItemBaseline.cs \
	rating_predictor/UserKNN.cs \
	rating_predictor/UserKNNCosine.cs \
	rating_predictor/UserKNNPearson.cs \
	taxonomy/EntityType.cs \
	util/CommandLineParameters.cs \
	util/Engine.cs \
	util/Handlers.cs \
	util/Pair.cs \
	util/Random.cs \
	util/Triple.cs \
	util/Utils.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	item_recommender \
	rating_predictor \
	eval \
	data_type \
	util \
	io \
	data \
	taxonomy \
	experimental \
	experimental/attr_to_feature \
	ensemble \
	experimental/item_prediction \
	mymedialite.pc.in 

REFERENCES =  \
	../../lib/MathNet/MathNet.Iridium.dll \
	../SVM/bin/Debug/SVM.dll \
	System \
	System.Core \
	System.Data \
	System.Data.DataSetExtensions

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



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
	$(call rm,$(SVM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(SVM_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MYMEDIALITE_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MYMEDIALITE_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
