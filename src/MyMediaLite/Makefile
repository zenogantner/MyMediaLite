
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
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug/

MATHNET_IRIDIUM_DLL_SOURCE=../../lib/MathNet/MathNet.Iridium.dll
MYMEDIALITE_DLL_MDB_SOURCE=bin/Debug/MyMediaLite.dll.mdb
MYMEDIALITE_DLL_MDB=$(BUILD_DIR)/MyMediaLite.dll.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+ "-define:TRACE"
ASSEMBLY = bin/Release/MyMediaLite.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release/

MATHNET_IRIDIUM_DLL_SOURCE=../../lib/MathNet/MathNet.Iridium.dll
MYMEDIALITE_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(MATHNET_IRIDIUM_DLL) \
	$(MYMEDIALITE_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(MYMEDIALITE_PC)  


RESGEN=resgen2

MATHNET_IRIDIUM_DLL = $(BUILD_DIR)/MathNet.Iridium.dll
MYMEDIALITE_PC = $(BUILD_DIR)/mymedialite.pc

FILES = \
	IItemAttributeAwareRecommender.cs \
	IUserAttributeAwareRecommender.cs \
	IIterativeModel.cs \
	IUserRelationAwareRecommender.cs \
	IItemRelationAwareRecommender.cs \
	AssemblyInfo.cs \
	Data/EntityMapping.cs \
	Data/Ratings.cs \
	DataType/Matrix.cs \
	DataType/MatrixUtils.cs \
	DataType/SparseBooleanMatrix.cs \
	DataType/SparseMatrix.cs \
	DataType/SparseVector.cs \
	DataType/VectorUtils.cs \
	Ensemble/Ensemble.cs \
	Ensemble/WeightedEnsemble.cs \
	Eval/ItemPrediction.cs \
	Eval/ItemPredictionEval.cs \
	Eval/RatingEval.cs \
	Eval/RatingPrediction.cs \
	IO/AttributeData.cs \
	IO/RelationData.cs \
	Taxonomy/EntityType.cs \
	Util/CommandLineParameters.cs \
	Util/Handlers.cs \
	Util/Pair.cs \
	Util/Random.cs \
	Util/Triple.cs \
	Util/Utils.cs \
	Correlation/CorrelationMatrix.cs \
	DataType/IMatrix.cs \
	DataType/IMatrixUtils.cs \
	DataType/SymmetricSparseMatrix.cs \
	Data/WeightedItem.cs \
	Correlation/BinaryCosine.cs \
	Correlation/Jaccard.cs \
	Correlation/BinaryDataCorrelationMatrix.cs \
	ItemRecommendation/BPR_Linear.cs \
	ItemRecommendation/BPRMF.cs \
	ItemRecommendation/IItemRecommender.cs \
	ItemRecommendation/ItemAttributeKNN.cs \
	ItemRecommendation/ItemKNN.cs \
	ItemRecommendation/KNN.cs \
	ItemRecommendation/MF.cs \
	ItemRecommendation/MostPopular.cs \
	ItemRecommendation/Random.cs \
	ItemRecommendation/UserAttributeKNN.cs \
	ItemRecommendation/UserKNN.cs \
	ItemRecommendation/WeightedItemKNN.cs \
	ItemRecommendation/WeightedUserKNN.cs \
	ItemRecommendation/WRMF.cs \
	RatingPrediction/BiasedMatrixFactorization.cs \
	RatingPrediction/IRatingPredictor.cs \
	RatingPrediction/MatrixFactorization.cs \
	RatingPrediction/EntityAverage.cs \
	RatingPrediction/GlobalAverage.cs \
	ItemRecommendation/ItemRecommender.cs \
	RatingPrediction/RatingPredictor.cs \
	IRecommender.cs \
	Util/Recommender.cs \
	Util/Memory.cs \
	DataType/SkewSymmetricSparseMatrix.cs \
	IO/ItemRecommendation.cs \
	IO/RatingPrediction.cs \
	Data/IRatings.cs \
	Data/StaticRatings.cs \
	IO/RatingPredictionStatic.cs \
	RatingPrediction/SlopeOne.cs \
	RatingPrediction/BiPolarSlopeOne.cs \
	RatingPrediction/UserAverage.cs \
	RatingPrediction/ItemAverage.cs \
	RatingPrediction/KNN.cs \
	RatingPrediction/UserItemBaseline.cs \
	RatingPrediction/ItemKNNCosine.cs \
	RatingPrediction/ItemKNN.cs \
	Correlation/Pearson.cs \
	Correlation/RatingCorrelationMatrix.cs \
	RatingPrediction/UserKNNCosine.cs \
	RatingPrediction/UserKNN.cs \
	RatingPrediction/ItemKNNPearson.cs \
	RatingPrediction/UserKNNPearson.cs \
	RatingPrediction/ItemAttributeKNN.cs \
	RatingPrediction/UserAttributeKNN.cs \
	Data/ISplit.cs \
	Data/RatingsProxy.cs \
	DataType/ListProxy.cs \
	Data/RatingCrossValidationSplit.cs \
	Data/StaticRatingsWithDateTime.cs \
	Data/RatingsWithDateTime.cs \
	IO/MovieLensRatingData.cs \
	Data/PosOnlyFeedback.cs \
	Util/GridSearch.cs \
	Data/StaticByteRatings.cs \
	Data/PosOnlyFeedbackSimpleSplit.cs \
	Data/RatingsSimpleSplit.cs \
	Data/StaticFloatRatings.cs \
	Data/CombinedRatings.cs \
	DataType/CombinedList.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	Correlation \
	Eval \
	DataType \
	Util \
	IO \
	Data \
	Taxonomy \
	Ensemble \
	ItemRecommendation \
	RatingPrediction \
	mymedialite.pc.in 

REFERENCES =  \
	System \
	System.Core \
	System.Data.DataSetExtensions \
	System.Data

DLL_REFERENCES =  \
	../../lib/MathNet/MathNet.Iridium.dll

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-target,MATHNET_IRIDIUM_DLL))
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
	$(call rm,$(MYMEDIALITE_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MYMEDIALITE_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
