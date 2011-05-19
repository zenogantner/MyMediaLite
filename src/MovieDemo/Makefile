
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG" "-main:MovieDemo.MainClass"
ASSEMBLY = bin/Debug/MovieDemo.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../MyMediaLite/bin/Debug/MyMediaLite.dll
BUILD_DIR = bin/Debug

MOVIEDEMO_EXE_MDB_SOURCE=bin/Debug/MovieDemo.exe.mdb
MOVIEDEMO_EXE_MDB=$(BUILD_DIR)/MovieDemo.exe.mdb
MYMEDIALITE_DLL_SOURCE=../MyMediaLite/bin/Debug/MyMediaLite.dll
MATHNET_IRIDIUM_DLL_SOURCE=../../lib/MathNet/MathNet.Iridium.dll
MYMEDIALITE_DLL_MDB_SOURCE=../MyMediaLite/bin/Debug/MyMediaLite.dll.mdb
MYMEDIALITE_DLL_MDB=$(BUILD_DIR)/MyMediaLite.dll.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- "-main:MovieDemo.MainClass"
ASSEMBLY = bin/Release/MovieDemo.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../MyMediaLite/bin/Release/MyMediaLite.dll
BUILD_DIR = bin/Release

MOVIEDEMO_EXE_MDB=
MYMEDIALITE_DLL_SOURCE=../MyMediaLite/bin/Release/MyMediaLite.dll
MATHNET_IRIDIUM_DLL_SOURCE=../../lib/MathNet/MathNet.Iridium.dll
MYMEDIALITE_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(MOVIEDEMO_EXE_MDB) \
	$(MYMEDIALITE_DLL) \
	$(MATHNET_IRIDIUM_DLL) \
	$(MYMEDIALITE_DLL_MDB)  

BINARIES = \
	$(MOVIEDEMO)  


RESGEN=resgen2

MYMEDIALITE_DLL = $(BUILD_DIR)/MyMediaLite.dll
MATHNET_IRIDIUM_DLL = $(BUILD_DIR)/MathNet.Iridium.dll
MOVIEDEMO = $(BUILD_DIR)/moviedemo

FILES = \
	gtk-gui/generated.cs \
	MainWindow.cs \
	gtk-gui/MainWindow.cs \
	Main.cs \
	AssemblyInfo.cs \
	MovieLensMovieInfo.cs \
	MovieLensGenres.cs \
	IMDBAkaTitles.cs \
	GtkSharpUtils.cs \
	Locale.cs \
	UserNameInput.cs \
	gtk-gui/MovieDemo.UserNameInput.cs 

DATA_FILES = 

RESOURCES = \
	gtk-gui/gui.stetic 

EXTRAS = \
	app.desktop \
	moviedemo.in 

REFERENCES =  \
	System \
	-pkg:gtk-sharp-2.0 \
	-pkg:glib-sharp-2.0 \
	-pkg:glade-sharp-2.0 \
	Mono.Posix \
	System.Core

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(BINARIES) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(BINARIES)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-target,MYMEDIALITE_DLL))
$(eval $(call emit-deploy-target,MATHNET_IRIDIUM_DLL))
$(eval $(call emit-deploy-target,MYMEDIALITE_DLL_MDB))
$(eval $(call emit-deploy-wrapper,MOVIEDEMO,moviedemo,x))


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
	$(call cp,$(MOVIEDEMO_EXE_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(MYMEDIALITE_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(MATHNET_IRIDIUM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(MYMEDIALITE_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	mkdir -p '$(DESTDIR)$(bindir)'
	$(call cp,$(MOVIEDEMO),$(DESTDIR)$(bindir))
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	make uninstall-satellite-assemblies prefix=$(prefix)
	$(call rm,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MOVIEDEMO_EXE_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MYMEDIALITE_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MATHNET_IRIDIUM_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MYMEDIALITE_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MOVIEDEMO),$(DESTDIR)$(bindir))
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
