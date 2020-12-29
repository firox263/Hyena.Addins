# Addin Manifest Sample
The same as the 'Hello World' sample, but using an xml
manifest to define extension points and nodes.

## Building
To run this sample, first build the solution, then run the `HelloWorld`
project. You **must build the entire solution** first, otherwise the Addin
DLL will not be generated, and therefore will not be picked up by the
addin registry.

```bash
$ dotnet build
$ dotnet run --project HelloWorld
```