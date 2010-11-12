
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
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

ifeq ($(CONFIG),RELEASE)
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

FILES = \
	correlation/Pearson.cs \
	correlation/CorrelationMatrix.cs \
	correlation/Cosine.cs \
	data_type/Matrix.cs \
	data_type/SparseBooleanMatrix.cs \
	data_type/VectorUtils.cs \
	data_type/WeightedItem.cs \
	item_recommender/BPR_Linear.cs \
	rating_predictor/ItemAttributeKNN.cs \
	item_recommender/BPRMF.cs \
	item_recommender/Random.cs \
	item_recommender/ItemAttributeKNN.cs \
	item_recommender/ItemKNN.cs \
	item_recommender/KNN.cs \
	item_recommender/Memory.cs \
	item_recommender/MF.cs \
	item_recommender/MostPopular.cs \
	item_recommender/UserKNN.cs \
	item_recommender/WRMF.cs \
	rating_predictor/MatrixFactorization.cs \
	rating_predictor/ItemKNN.cs \
	rating_predictor/KNN.cs \
	rating_predictor/Linear.cs \
	rating_predictor/Memory.cs \
	rating_predictor/UserAttributeKNN.cs \
	rating_predictor/UserItemBaseline.cs \
	rating_predictor/UserKNN.cs \
	util/Random.cs \
	item_recommender/UserAttributeKNN.cs \
	util/CommandLineParameters.cs \
	util/Handlers.cs \
	util/Utils.cs \
	io/ItemRecommenderData.cs \
	util/Pair.cs \
	data/RatingData.cs \
	io/AttributeData.cs \
	io/RatingPredictionData.cs \
	taxonomy/EntityType.cs \
	rating_predictor/RatingPredictor.cs \
	experimental/attr_to_feature/BPRMF_Mapping.cs \
	util/Triple.cs \
	eval/RatingEval.cs \
	data/RatingEvent.cs \
	data/Ratings.cs \
	rating_predictor/BiasedMatrixFactorization.cs \
	data/EntityMapping.cs \
	eval/ItemPrediction.cs \
	eval/RatingPrediction.cs \
	rating_predictor/ItemKNNCosine.cs \
	rating_predictor/ItemKNNPearson.cs \
	rating_predictor/UserKNNPearson.cs \
	rating_predictor/UserKNNCosine.cs \
	item_recommender/WeightedUserKNN.cs \
	eval/ItemPredictionEval.cs \
	ensemble/Ensemble.cs \
	ensemble/WeightedEnsemble.cs \
	item_recommender/ItemAttributeSVM.cs \
	data_type/SparseMatrix.cs \
	data_type/SparseVector.cs \
	rating_predictor/SocialMF.cs \
	io/RelationData.cs \
	data_type/data_type_test.cs \
	data/data_test.cs \
	IItemAttributeAwareRecommender.cs \
	IUserAttributeAwareRecommender.cs \
	IRecommenderEngine.cs \
	IIterativeModel.cs \
	IUserRelationAwareRecommender.cs \
	IItemRelationAwareRecommender.cs \
	util/Engine.cs \
	ILatentFactorModel.cs \
	AssemblyInfo.cs \
	item_recommender/IItemRecommender.cs \
	experimental/attr_to_feature/BPRMF_ItemMapping.cs \
	experimental/attr_to_feature/BPRMF_ItemMapping_Complex.cs \
	experimental/attr_to_feature/BPRMF_ItemMapping_kNN.cs \
	experimental/attr_to_feature/BPRMF_ItemMapping_Optimal.cs \
	experimental/attr_to_feature/BPRMF_ItemMapping_SVR.cs \
	experimental/attr_to_feature/BPRMF_UserMapping.cs \
	experimental/attr_to_feature/BPRMF_UserMapping_Optimal.cs 

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
	experimental/rating_prediction \
	experimental/item_prediction \
	mymedialite.pc.in 

REFERENCES =  \
	System \
	System.Core \
	System.Data.DataSetExtensions \
	System.Data \
	-pkg:mono-nunit

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
