# Copilot Instructions for Collections.Pooled.V2

## Build, Test, and Benchmark Commands

```bash
# Restore and build the entire solution
dotnet build Collections.Pooled.sln

# Run the full test suite (~27,500 tests, xUnit v3)
dotnet test Collections.Pooled.Tests/Collections.Pooled.Tests.csproj

# Run a single test class
dotnet test Collections.Pooled.Tests/Collections.Pooled.Tests.csproj --filter "FullyQualifiedName~List_Generic_Tests_int"

# Run a single test method
dotnet test Collections.Pooled.Tests/Collections.Pooled.Tests.csproj --filter "FullyQualifiedName~List_Generic_Tests_int.Add_ToReadOnlyThrows"

# Run benchmarks (interactive picker)
dotnet run -c Release --project Collections.Pooled.Benchmarks/Collections.Pooled.Benchmarks.csproj

# Run a specific benchmark class
dotnet run -c Release --project Collections.Pooled.Benchmarks/Collections.Pooled.Benchmarks.csproj -- --filter "*List_Add*"
```

## Architecture

This library provides drop-in replacements for `System.Collections.Generic` types that use `ArrayPool<T>` to minimize heap allocations. The source is derived from the .NET runtime (corefx) implementations.

**Projects:**
- `Collections.Pooled` — the library itself, targeting `netstandard2.1`. Published as the `Collections.Pooled.V2` NuGet package.
- `Collections.Pooled.Tests` — xUnit v3 tests targeting `net10.0`, ported from corefx.
- `Collections.Pooled.Benchmarks` — BenchmarkDotNet benchmarks targeting `net8.0;net10.0`.

**Pooled collection types** (each in its own file under `Collections.Pooled/`):
- `PooledList<T>`, `PooledDictionary<TKey, TValue>`, `PooledSet<T>`, `PooledStack<T>`, `PooledQueue<T>`, `PooledMemory<T>`

All pooled types implement `IDisposable`. Disposing returns rented arrays to the pool. Forgetting to dispose is safe but loses the pooling benefit.

**ClearMode** controls whether data is zeroed before returning arrays to the pool: `Auto` (default, clears reference types), `Always`, or `Never`.

## Key Conventions

- **ThrowHelper pattern**: Exceptions are thrown via static `ThrowHelper` methods, not inline `throw` statements. This keeps IL small for generic types — follow this pattern when adding or modifying error paths.
- **corefx-derived code** retains the original `.NET Foundation` license header and field-naming comments like `// Do not rename (binary serialization)`. Preserve these.
- **`PooledMemory<T>`** is the one type not derived from corefx — it was written for this project. It uses the same `ClearMode` and `ArrayPool` patterns as the other types.
- **Test structure** mirrors xUnit trait tests from corefx: abstract generic base classes (`ICollection.Generic.Tests`, `IList.Generic.Tests`, etc.) are specialized by concrete classes per collection (e.g., `List_Generic_Tests_string`). Add new tests by subclassing or extending the existing hierarchy under the appropriate `Pooled*` subfolder.
- **Benchmark structure**: Each benchmark class inherits a `*Base` class that provides helper methods for creating test data. Benchmarks compare the standard BCL type (marked `Baseline = true`) against the pooled equivalent, using `[MemoryDiagnoser]` and `[SimpleJob]` attributes.
- **Versioning** uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) (`version.json`). Do not set versions manually in csproj files.
- **Extension methods** (`PooledExtensions.cs`) provide `ToPooled*()` conversions. When adding a new pooled type, add corresponding extension methods here.
