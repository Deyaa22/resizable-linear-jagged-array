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
    public int NumberOfSegments { get { return array.Length; } }

    private int length = 0;
    public int Length
    {
        get { return length; }
        private set 
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException("New length can't be less than zero.");

            if (value == Length)
                return;

            int _newNumberOfSegments = CalculateNumberOfSegments(value);
            if (_newNumberOfSegments != NumberOfSegments)
            {
                T[][] _newArray = new T[_newNumberOfSegments][];
                for (int _segmentIndex = 0; _segmentIndex < _newNumberOfSegments && _segmentIndex < NumberOfSegments; _segmentIndex++)
                {
                    _newArray[_segmentIndex] = array[_segmentIndex];
                }
                array = _newArray;
            }

            length = value;
        }
    }

    public int MaxIndex { get { return Length - 1; } }

    public ResizableLinearJaggedArray(int _length = 0, int _segmentLength = 8)
    {
        if (_length < 0)
            throw new ArgumentOutOfRangeException("Length of array can't be less than zero.");

        if (_segmentLength <= 0)
            throw new ArgumentOutOfRangeException("Segment length can't be zero nor less.");

        SegmentLength = _segmentLength;
        int _numberOfSegment = CalculateNumberOfSegments(_length);
        array = new T[_numberOfSegment][];
        Length = _length;
    }

    public T this[int _index]
    {
        get
        {
            if (_index >= Length || _index < 0)
                throw new IndexOutOfRangeException();

            T _item = default(T);

            int _segmentIndex = CalculateSegmentIndex(_index);
            int _itemIndexAtSegment = CalculateItemIndexAtSegment(_index);

            if (array[_segmentIndex] != null)
                _item = array[_segmentIndex][_itemIndexAtSegment];

            return _item;
        }
        set
        {
            if (_index >= Length || _index < 0)
                throw new IndexOutOfRangeException();

            int _segmentIndex = CalculateSegmentIndex(_index);
            int _itemIndexAtSegment = CalculateItemIndexAtSegment(_index);

            if (array[_segmentIndex] == null)
                array[_segmentIndex] = new T[SegmentLength];

            array[_segmentIndex][_itemIndexAtSegment] = value;
        }
    }


    /// <summary>
    /// Collection Initializer.
    /// Adds new item on top of array,
    /// Method increments length by one.
    /// </summary>
    /// <param name="_item"></param>
    /// <remarks>
    ///     <b>Warning:</b> For adding items repeatedly, It's better to resize the array, then assign items.
    /// </remarks>
    public void Add(T _item)
    {
        Length++;
        this[MaxIndex] = _item;
    }

    /// <summary>
    /// Calculates the number of segments required to hold a given number of items.
    /// </summary>
    /// <param name="_length">The total number of items that should be hold in segments.</param>
    /// <returns>The number of segments needed.</returns>
    private int CalculateNumberOfSegments(int _length)
    {
        int _maxIndex = _length - 1;
        int _maxSegmentIndex = CalculateSegmentIndex(_maxIndex);
        int _numberOfSegments = _maxSegmentIndex + 1;
        return _numberOfSegments;
    }

    /// <summary>
    /// Calculates the index of a segment which holds index"
    /// </summary>
    /// <param name="_index">The index wich should be contained in a segment.</param>
    private int CalculateSegmentIndex(int _index)
    {
        return _index / SegmentLength;
    }

    /// <summary>
    /// Calculate the index in segment using the given item index.
    /// </summary>
    /// <param name="_index">The index in the array.</param>
    private int CalculateItemIndexAtSegment(int _index)
    {
        return _index % SegmentLength;
    }
}
