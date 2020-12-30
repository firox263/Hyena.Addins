# Writer Service Sample
A more complex example of how hyena-addins can be used.

## Building
To run this sample, first build the solution, then run the `WriterServiceHost`
project. You **must build the entire solution** first, otherwise the Addin
DLL will not be generated, and therefore will not be picked up by the
addin registry.

```bash
$ dotnet build
$ dotnet run --project WriterServiceHost
```