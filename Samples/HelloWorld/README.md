# Hello World Sample
This sample illustrates a simple 'Hello World' program, but
with the "Hello World!" printed by an addin rather than
the main process.

## Building
To run this sample, first build the solution, then run the `HelloWorld`
project. You **must build the entire solution** first, otherwise the Addin
DLL will not be generated, and therefore will not be picked up by the
addin registry.

```bash
$ dotnet build
$ dotnet run --project HelloWorld
```