# Mono.Addins
A port of mono-addins to netstandard2.1

## Status
Only `Mono.Addins` is supported at present. Other
projects like Mono.Addins.Setup and the Gtk GUI
may be supported in the future.

This code has not been tested and is not suitable
for production.

## Usage
Usage is fundamentally the same as upstream
mono-addins. The samples have been ported to
the new csproj format and are easily buildable
using the dotnet cli tool.

See `Samples/` for instructions on how to run the examples.

Please note: When running in an IDE, you **must build the
entire solution first** before trying to run any samples,
otherwise the Sample Addin DLLs will not be built, and therefore
the samples will do nothing. See each sample for more details.

The original README can be seen below:

```txt
Mono.Addins is a generic framework for creating extensible applications,
and for creating libraries which extend those applications.

For for information about the library, see:
http://www.mono-project.com/Mono.Addins

Building
--------

To build the library execute:

	./configure
	make

Options:

--prefix=/path/to/prefix

	Install to the specified prefix

--enable-gui

	Include GUI support for GTK2 (requires gtk#2)

--enable-gui-gtk3

	Include GUI support for GTK3 (requires gtk#3)

--enable-tests

	Include NUnit tests (requires nunit)


Building the samples
--------------------

cd to the Samples directory and run make.

Running the NUnit tests
-----------------------

To run the NUnit tests, you need to configure the build with --enable-tests,
cd to Tests, and run 'make test'. 
```

