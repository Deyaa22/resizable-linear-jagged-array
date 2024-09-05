## ResizableLinearJaggedArray - RLJArray
`ResizableLinearJaggedArray<T>` is a generic, resizable linear array with an internal jagged array structure, designed to provide Big-Oh(1) time complexity for item access as it is with the normal array.
This class allows efficient resizing by managing segments, which are fixed-length sub-arrays, and avoids the allocation of space for empty segments.

### Installation
---
To install the `ResizableLinearJaggedArray` package, use the following command in the NuGet Package Manager Console:

```bash
dotnet add package Deyaa.ExDS.ResizableLinearJaggedArray --version 0.1.1
```

Alternatively, you can add the package via the [NuGet Gallery](https://www.nuget.org/packages/Deyaa.ExDS.ResizableLinearJaggedArray) or your preferred package manager.

### How It Works
---
Unlike standard C# `Array` or `List<T>`, `RLJArray` is divided into segments that represent contiguous items.
Each segment is literally allocated only when needed, not when array enlagres,
Saving memory by not reserving space for empty segments.
The array can be resized dynamically, with the ability to enlarge and shrink in bulk, without the overhead of moving items to new array.

### Usage
---
Here's a simple example of how to use `ResizableLinearJaggedArray<T>`:
```csharp
var array = new ResizableLinearJaggedArray<int>(10, 5); // Initialize with length 10 and segment length 5
array[0] = 1;
array.Add(2);
array.EnglargeBySize(5);
array.ShrinkBySize(3);
```

### Key Features
---
- **Array-Like:** RLJArray acts the same as normal linear array, Just replace array instance with RLJArray, All operation within linear array can be done in the same way using `RLJArray` 
```csharp
// Collection initializer
var cSArray =                             new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
var rLArray = new ResizableLinearJaggedArray<int>() { 0, 1, 2, 3, 4, 5, 6, 7 };

// Assigning values using indexer 
cSArray[0] = 1; // { 1, 1, 2, 3, 4, 5, 6, 7 }
rLArray[0] = 1; // { 1, 1, 2, 3, 4, 5, 6, 7 }

// Retrieving values using indexer
int x = cSArray[2]; // x = 2 
int y = rLArray[2]; // y = 2

// checking Length of array
x = cSArray.Length; // 8
y = rLArray.Length; // 8

// Enumerating within foreach statement
foreach(var _item in cSArray)
{
    Console.WriteLine($"{_item} - "); // 1 - 1 - 2 - 3 - 4 - 5 - 6 - 7 -
}
foreach(var _item in rLArray)
{
    Console.WriteLine($"{_item} - "); // 1 - 1 - 2 - 3 - 4 - 5 - 6 - 7 -
}

// Throwing exceptions as it is in linear [single-dimensional] array
cSArray[8] = x; // throws IndexOutOfRangeException
rLArray[8] = y; // throws IndexOutOfRangeException

// Initializing array of parameterless constructor value types 
struct X { public int x = 0; public X() { x = 5; } }
var cSArray2 =                             new X[1];
var rLArray2 = new ResizableLinearJaggedArray<X>(1);
Console.WriteLine(csArray2[0]) // 0
Console.WriteLine(rLArray2[0]) // 0
cSArray2.Initialize();
rLArray2.Initialize();
Console.WriteLine(csArray2[0]) // 5
Console.WriteLine(rLArray2[0]) // 5
```
- **Efficient Resizing:** Supports bulk resizing operations without relocating items at all.
```csharp
// rLArray:                       { 1, 1, 2, 3, 4, 5, 6, 7 }
rLArray.Shrink(_amount: 4);    // { 1, 1, 2, 3 }
rLArray.Enlarge(_amount: 8);   // { 1, 1, 2, 3, 0, 0, 0, 0, 0, 0, 0, 0 }
rLArray.Resize(_newLength: 2); // { 1, 1 }
```
- **Memory Efficiency:** Avoids allocation for empty segments and can free memory by cleaning empty segments.
```csharp
rLArray.Resize(_newLength: 24); // { 1, 1, 0, ...., 0 }
Console.WriteLine(rLArray.NumberOfNullSegments); // 2
// 2 segments are null,
// which means saving (64) bytes = 4 bytes [int32] * 2 segments * 8 [default segment length] - 8 bytes [segment reference address in 64-bit systems, [4 bytes for 32-bit]]

// Note: when segment get allocated, there will be an overhead foreach segment as follows:
// 20 bytes for 64-bit systems
// 12 bytes for 32-bit systems
```
- **Flexible Segment Length:** Allows customization of segment size for optimized performance based on use case.
```csharp
// Segment Length = 8 by default
var array1 = new ResizableLinearJaggedArray<int>(_length: 16);
var array2 = new ResizableLinearJaggedArray<int>(_length: 16, _segmentLength: 16);
```

### Comparison with `List<T>`
---
**From a Usage Perspective:**
- `ResizableLinearJaggedArray<T>` allows for bulk resizing and does not require moving items to a new array, which can be more efficient than `List<T>` for certain use cases.

**From an Implementation Perspective:**
- `ResizableLinearJaggedArray<T>` avoids the need for reallocation and copying of elements when resizing, unlike `List<T>`, which doubles its capacity and moves elements when the internal array reaches full capacity. `List<T>` does not reduce the internal array size when elements are removed, leading to potential memory wastage.
- Note: Theoritically RLJArray gives more advantages over `List<T>`, But internally, optimization techniques could be done by CLR team in .Net and Kernel team in OS, Which can make the difference, This doesn't mean `List<T>` wins nor RLJArray. But RLJArray practically performence test.

### Segment Length Considerations
---
- **Large Segment Length:**
  - **Advantages:** Fewer segments, less overall space overhead of segments.
  - **Disadvantages:** More memory wastage if many segments are only partially filled.

- **Small Segment Length:**
  - **Advantages:** More granular control, better for arrays with highly changable sizes.
  - **Disadvantages:** Bigger overall space overhead of segments.

**Recommendation:** Choose segment length based on expected usage patterns and memory efficiency requirements. For most use cases, a balanced segment length (e.g., 8 to 16) provides a good trade-off.

## Add & Remove methods
---
RLJArray is designed to act like a single-dimensional array, not List<T>.
Add and Remove methods are added because of implementing ICollection interface,
And for that I kept Remove method callable,
I kept Add method puplic for collection initializer. 

### Mathematical background
---
- **Big-Oh(1) Access Time:** Provides constant-time access to elements.

- **F(x) = 1 + x Resizing Time:** Where:
Big-Oh(F(X)) < Big–Theta(n)
F(x) represents the actual number of steps for resizing,
x is number of segments,
x = Length / Segment Length, which is much much smaller than Big-Oh(n),
Even when Segment Length equals 1,
RLJArray still introduce better time,as it is moving only 8 bytes the address of segment in memory, And not moving any items at all.

### Contribute
---
Contributing means a lot to me ❤️,
Look at [CONTRIBUTING.md](CONTRIBUTING.md),
And Feel free to create issue, test and contribute.
 
### Conclusion
---
`ResizableLinearJaggedArray<T>` offers features beyond those provided by `List<T>`, such as efficient bulk resizing and memory management without reallocating or moving items. However, `List<T>` may still be preferable in scenarios where performance optimizations are handled by the .NET runtime and for applications where `List<T>`'s built-in methods are sufficient. Extensive testing is recommended to evaluate performance and suitability for specific scenarios.

### Disclaimer
---
 You are solely responsible for any damages resulting from your use of this package in any product, including logical errors and failures resulting from errors in the source code.
