namespace ResizableLinearJaggedArray.Generics;

/// <summary>
/// RLJArray: A generic resizable linear array with a jagged array as internal structure,
/// Provides Big-Oh (1) of Time Complexity for accessing items.
/// 
/// The array is divided into segments,
/// Each segment represents a fixed number of contiguous items,
/// This design saves memory by avoiding the allocation of space for empty segments.
/// 
/// RLJArray gives the ability to enlarge and shrink the array as needed.
/// 
/// Segment is conceptual term which expresses a contiguous elements in memory with fixed size similar to a standard array.
/// 
/// Technically, a segment is a sub array with a fixed length,
/// but it remains null until at least one element is assigned.
/// 
/// The nullability of segments does not affect data retrieval,
/// Accessing elements returns the default value; if the segment is null,
/// This approach conserves memory by avoiding the allocation of space of sub array for empty segments.
/// 
/// User can get benefit of manually freeing space of memory of segments which get empty by calling the [CleanEmptySegments()] method.
/// </summary>
/// <typeparam name="T"></typeparam>

public class ResizableLinearJaggedArray<T>
{
    private T[][] array;

    public readonly int SegmentLength;

    private int length = 0;
    public int Length
    {
        get { return length; }
        private set { throw new NotImplementedException(); }
    }

    public ResizableLinearJaggedArray(int _length = 0, int _segmentLength = 8)
    {
        if (_length < 0)
            throw new ArgumentOutOfRangeException("Length of array can't be less than zero.");

        if (_segmentLength <= 0)
            throw new ArgumentOutOfRangeException("Segment length can't be zero nor less.");

        throw new NotImplementedException();
    }

    /// <summary>
    /// Calculates the number of segments required to hold a given number of items.
    /// </summary>
    /// <param name="_length">The total number of items that should be hold in segments.</param>
    /// <returns>The number of segments needed.</returns>
    private int CalculateNumberOfSegments(int _length)
    {
        int _maxIndex = _length - 1;
        int _maxSegmentIndex = _maxIndex / SegmentLength;
        int _numberOfSegments = _maxSegmentIndex + 1;
        return _numberOfSegments;
    }
}
