using System;
using System.Collections;
using System.Collections.Generic;

namespace Deyaa.Collections.Generics;

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
/// Segment is a conceptual term which expresses a contiguous elements in memory with fixed size similar to a standard array.
/// 
/// Technically, a segment is a sub array with a fixed length,
/// but it remains null until at least one element gets assigned.
/// 
/// The nullability of segments does not affect data retrieval,
/// Accessing elements returns the default value; if the segment is null,
/// This approach conserves memory by avoiding the allocation of space of sub array for empty segments.
/// 
/// User can get benefit of manually freeing space of memory of segments which get empty by calling the 'CleanEmptySegments()' method.
/// </summary>
/// <typeparam name="T"></typeparam>

public class RLJArray<T> : IEnumerable<T>, IEnumerable, ICollection<T>, ICollection, IList<T>, IList, IStructuralComparable, IStructuralEquatable, ICloneable
{
    private T[][] _array;

    public readonly int Rank = 1;

    public readonly int SegmentLength;

    /// <summary>
    /// Gets the total number of segments used in the array.
    /// </summary>
    public int TotalNumberOfSegments { get { return _array.Length; } }
    /// <summary>
    /// Gets the number of segments that are null in memory.
    /// </summary>
    /// <remarks>
    ///     <b>Note:</b> Null segments do not affect data assignment or retrieval, but they help conserve memory.
    /// </remarks>
    public int NumberOfNullSegments
    {
        get
        {
            int counter = 0;
            for (int i = 0; i < TotalNumberOfSegments; i++)
                if (_array[i] == null)
                    counter++;
            return counter;
        }
    }

    /// <summary>
    /// Gets the number of segments that contain only default values in memory, but are not null.
    /// </summary>
    /// <remarks>
    ///     <b>Note:</b> It is recommended to clean empty segments using the 'CleanEmptySegments' method to save memory.
    /// </remarks>
    public int NumberOfEmptySegments
    {
        get
        {
            int counter = 0;
            for (int i = 0; i < TotalNumberOfSegments; i++)
            {
                for (int j = 0; _array[i] != null && j < _array[i].Length; j++)
                {
                    if (!EqualsDefault(_array[i][j]))
                        break;

                    if (j == SegmentLength - 1)
                        counter++;
                }
            }
            return counter;
        }
    }

    private int _length = 0;
    public int Length
    {
        get { return _length; }
        private set
        {
            int newLength = value;

            if (newLength < 0)
                throw new ArgumentOutOfRangeException("New length can't be less than zero.");

            if (newLength == Length)
                return;

            if (newLength == 0)
            {
                _array = new T[0][];
            }
            else
            {
                int newNumberOfSegments = CalculateNumberOfSegments(newLength);
                if (newNumberOfSegments != TotalNumberOfSegments)
                {
                    T[][] newArray = new T[newNumberOfSegments][];
                    for (int segmentIndex = 0; segmentIndex < newNumberOfSegments && segmentIndex < TotalNumberOfSegments; segmentIndex++)
                    {
                        newArray[segmentIndex] = _array[segmentIndex];
                    }
                    _array = newArray;
                }

                if (newLength < Length)
                {
                    int segmentIndex = CalculateSegmentIndex(newLength - 1);
                    int itemIndex = CalculateItemIndexAtSegment(newLength - 1);
                    T[] segment = null;
                    if (_array[segmentIndex] != null)
                    {
                        segment = _array[segmentIndex];

                        for (int i = itemIndex + 1; i < SegmentLength; i++)
                        {
                            segment[i] = default(T);
                        }
                    }
                }
            }

            _length = newLength;
        }
    }

    public int GetLength(int dimension = 0)
    {
        if (dimension != 0)
            throw new ArgumentOutOfRangeException("RLJArray acts like a normal linear array, RLJArray doesn't support multiple dimensions");

        return Length;
    }

    public long LongLength
    {
        get
        {
            return (long)this.GetLength();
        }
    }

    public long GetLongLength(int dimension = 0)
    {
        return (long)this.GetLength(dimension);
    }


    private int MaxIndex { get { return Length - 1; } }

    /// <summary>
    /// Return  The last index [<see cref="MaxIndex"/>] in array.
    /// </summary>
    /// <param name="dimension"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when non zero value entered for <param name="dimension"></param>.</exception>
    public int GetUpperBound(int dimension = 0)
    {
        if (dimension != 0)
            throw new ArgumentOutOfRangeException("RLJArray acts like a normal linear array, RLJArray doesn't support multiple dimensions");

        return MaxIndex;
    }

    public int Count { get { return Length; } }

    public bool IsReadOnly { get { return false; } }

    public bool IsSynchronized { get { return false; } }

    public bool IsFixedSize { get { return false; } }

    public object SyncRoot { get { return this; } }

    public RLJArray(int length = 0, int segmentLength = 8)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException("Length of array can't be less than zero.");

        if (segmentLength <= 0)
            throw new ArgumentOutOfRangeException("Segment length can't be zero nor less.");

        SegmentLength = segmentLength;
        int numberOfSegment = CalculateNumberOfSegments(length);
        _array = new T[numberOfSegment][];
        Resize(length);
    }

    public T this[int index]
    {
        get
        {
            if (index >= Length || index < 0)
                throw new IndexOutOfRangeException();

            T item = default(T);

            int segmentIndex = CalculateSegmentIndex(index);
            int itemIndexAtSegment = CalculateItemIndexAtSegment(index);

            if (_array[segmentIndex] != null)
                item = _array[segmentIndex][itemIndexAtSegment];

            return item;
        }
        set
        {
            if (index >= Length || index < 0)
                throw new IndexOutOfRangeException();

            int segmentIndex = CalculateSegmentIndex(index);
            int itemIndexAtSegment = CalculateItemIndexAtSegment(index);

            if (_array[segmentIndex] == null)
            {
                if (EqualsDefault(value))
                    return;
                else
                    _array[segmentIndex] = new T[SegmentLength];
            }

            _array[segmentIndex][itemIndexAtSegment] = value;
        }
    }

    object? IList.this[int index] { get { return this[index]; } set { this[index] = (T)value; } }

    /// <summary>
    /// Resizes the Length of array to a new length.
    /// </summary>
    /// <param name="newLength">
    /// The new length of the array.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown [inside Length] when the specified new length is less than zero.
    /// </exception>
    /// <remarks>
    ///     <b>Warning:</b> Resizing to a smaller length will result in the deletion of data at higher truncated indexes.
    /// </remarks>
    public void Resize(int newLength)
    {
        Length = newLength;
    }

    /// <summary>
    /// Increments length of array by amount.
    /// </summary>
    /// <param name="amount"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void Enlarge(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException("Can't enlarge array with minus number.");

        Resize(Length + amount);
    }

    /// <summary>
    /// Decrements length of array by amount.
    /// </summary>
    /// <param name="amount"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void Shrink(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException("Can't shrink array with minus number.");

        if (amount > Length)
            throw new ArgumentOutOfRangeException("Can't shrink array more than its actual length.");

        Resize(Length - amount);
    }

    public bool Contains(object? item, out int index)
    {
        index = -1;
        for (int i = 0; i < Length; i++)
        {
            var arrayItem = this[i] as object;
            if (AreEqual(arrayItem, item))
            {
                index = i;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Collection Initializer.
    /// Adds new item on top of array,
    /// Method increments length by one.
    /// </summary>
    /// <param name="item"></param>
    /// <remarks>
    ///     <b>Warning:</b> For adding items repeatedly, It's better to resize the array, then assign items.
    /// </remarks>
    public void Add(T item)
    {
        Resize(Length + 1);
        this[MaxIndex] = item;
    }

    /// <summary>
    /// Cleanes array from empty segments.
    /// </summary>
    public void CleanEmptySegments()
    {
        if (Length == 0)
            return;

        CleanEmptySegments(0, Length);
    }

    /// <summary>
    /// Cleanes array from empty segments within given range.
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="amount"></param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void CleanEmptySegments(int startIndex, int amount)
    {
        if (Length == 0)
            throw new InvalidOperationException("Length of array is 0, Can't operate on an empty array.");

        if (startIndex < 0 || startIndex >= Length || amount < 0)
            throw new ArgumentOutOfRangeException("length");

        if (amount == 0)
            return;

        int endIndex = startIndex + amount - 1;
        if (!IsFirstIndexInSegment(startIndex))
        {
            startIndex = GetFirstIndexOfNextSegment(endIndex);
        }

        if (!IsLastIndexInSegment(endIndex))
        {
            endIndex = GetLastIndexOfPreviousSegment(endIndex);
        }

        int startSegmentIndex = CalculateSegmentIndex(startIndex);
        int endSegmentIndex = CalculateSegmentIndex(endIndex);

        for (int i = startSegmentIndex; i <= endSegmentIndex; i++)
        {
            T[] segment = _array[i];
            if (segment == null)
                continue;

            bool segmentIsEmpty = true;
            for (int j = 0; j < segment.Length; j++)
            {
                T item = segment[j];
                bool itemIsExisted = !EqualsDefault(item);
                segmentIsEmpty = !itemIsExisted;
                if (!segmentIsEmpty)
                    break;
            }

            if (segmentIsEmpty)
                _array[i] = null;
        }
    }
    /// <summary>
    /// Calculates the number of segments required to hold a given number of items.
    /// </summary>
    /// <param name="length">The total number of items to be held in segments.</param>
    /// <returns>The number of segments needed.</returns>
    private int CalculateNumberOfSegments(int length)
    {
        if (length == 0)
            return 0;

        int maxIndex = length - 1;
        int maxSegmentIndex = CalculateSegmentIndex(maxIndex);
        return maxSegmentIndex + 1;
    }

    /// <summary>
    /// Calculates the index of the segment that holds the given index.
    /// </summary>
    /// <param name="index">The index to be contained in a segment.</param>
    private int CalculateSegmentIndex(int index)
    {
        return index / SegmentLength;
    }

    /// <summary>
    /// Calculates the index within the segment using the given item index.
    /// </summary>
    /// <param name="index">The index in the array.</param>
    private int CalculateItemIndexAtSegment(int index)
    {
        return index % SegmentLength;
    }

    /// <summary>
    /// Determines whether the given index is the first index in a segment.
    /// </summary>
    /// <param name="index">The index to check.</param>
    /// <returns>True if the index is the first index in a segment, otherwise false.</returns>
    public bool IsFirstIndexInSegment(int index)
    {
        return index % SegmentLength == 0;
    }

    /// <summary>
    /// Determines whether the given index is the last index in a segment.
    /// </summary>
    /// <param name="index">The index to check.</param>
    /// <returns>True if the index is the last index in a segment, otherwise false.</returns>
    public bool IsLastIndexInSegment(int index)
    {
        return index % SegmentLength == SegmentLength - 1;
    }

    /// <summary>
    /// Returns the first index of the next segment.
    /// </summary>
    /// <param name="index">The current index.</param>
    /// <returns>The first index of the next segment.</returns>
    public int GetFirstIndexOfNextSegment(int index)
    {
        int length = index + 1;
        return CalculateNumberOfSegments(length) * SegmentLength;
    }

    /// <summary>
    /// Returns the last index of the previous segment.
    /// </summary>
    /// <param name="index">The current index.</param>
    /// <returns>The last index of the previous segment.</returns>
    public int GetLastIndexOfPreviousSegment(int index)
    {
        int length = index + 1;
        return ((CalculateNumberOfSegments(length) - 1) * SegmentLength) - 1;
    }

    /// <summary>
    /// Checks if the given value is equal to the default value of the type.
    /// </summary>
    /// <param name="value">The value to compare.</param>
    /// <returns>True if the value is equal to the default value of the type, otherwise false.</returns>
    private bool EqualsDefault(object? value)
    {
        return AreEqual(value, default(T));
    }

    /// <summary>
    /// Compares two objects for equality.
    /// </summary>
    /// <param name="item1">The first object.</param>
    /// <param name="item2">The second object.</param>
    /// <returns>True if the objects are equal, otherwise false.</returns>
    private bool AreEqual(object? _item1, object? _item2)
    {
        bool res = false;
        if (_item1 == null && _item2 == null)
            res = true;
        else if (_item1 == null || _item2 == null)
            res = false;
        else
            res = _item1.Equals(_item2);
        return res;
    }

    /// <summary>
    /// Gets an enumerator to iterate over the collection.
    /// </summary>
    /// <returns>An enumerator for the collection.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Length; i++)
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return (this as IEnumerable).GetEnumerator();
    }

    /// <summary>
    /// Clears the collection.
    /// </summary>
    void ICollection<T>.Clear()
    {
        Resize(0);
    }

    /// <summary>
    /// Checks if the collection contains a specific item.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>True if the item is found, otherwise false.</returns>
    bool ICollection<T>.Contains(T item)
    {
        return Contains(item, out _);
    }

    /// <summary>
    /// Copies the collection to the specified array starting at the specified index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The index to start copying at.</param>
    public void CopyTo(Array array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (array.Rank != Rank)
            throw new ArgumentException("Only single dimensional arrays are supported for the requested action.");

        for (int i = arrayIndex; i < array.Length && i < Length; i++)
        {
            array.SetValue(this[i], i);
        }
    }

    /// <summary>
    /// Copies the collection to the specified array starting at the specified index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The index to start copying at.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        CopyTo((Array)array, arrayIndex);
    }

    /// <summary>
    /// Copies the collection to the specified RLJArray starting at the specified index.
    /// </summary>
    /// <param name="array">The destination RLJArray.</param>
    /// <param name="arrayIndex">The index to start copying at.</param>
    public void CopyTo(RLJArray<T> array, int arrayIndex)
    {
        for (int i = arrayIndex; i < array.Length && i < Length; i++)
        {
            array[i] = this[i];
        }
    }

    /// <summary>
    /// Copies the collection to the specified array starting at the specified index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="index">The starting index.</param>
    public void CopyTo(Array array, long index)
    {
        if (index > int.MaxValue || index < int.MinValue)
            throw new ArgumentOutOfRangeException(nameof(index), "Arrays larger than 2GB are not supported.");
        this.CopyTo(array, (int)index);
    }

    /// <summary>
    /// Removes the first occurrence of the specified item from the collection.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was removed, otherwise false.</returns>
    bool ICollection<T>.Remove(T item)
    {
        int index = -1;
        for (int i = 0; i < Length; i++)
        {
            T _arrayItem = this[i];
            if (AreEqual(_arrayItem, item))
            {
                index = i;
                break;
            }
        }

        if (index == -1)
            return false;

        for (int i = index; i < MaxIndex; i++)
        {
            this[i] = this[i + 1];
        }

        Shrink(1);
        return true;
    }

    /// <summary>
    /// Removes the item at the specified index.
    /// </summary>
    /// <param name="index">The index to remove the item from.</param>
    void IList<T>.RemoveAt(int index)
    {
        (this as IList).RemoveAt(index);
    }

    /// <summary>
    /// Removes the specified item from the collection.
    /// </summary>
    /// <param name="value">The item to remove.</param>
    void IList.Remove(object? value)
    {
        int itemIndex = (this as IList).IndexOf(value);
        if (itemIndex == -1)
            throw new ArgumentException("Array doesn't contain value.");

        (this as IList).RemoveAt(itemIndex);
    }

    /// <summary>
    /// Removes the item at the specified index.
    /// </summary>
    /// <param name="index">The index to remove the item from.</param>
    void IList.RemoveAt(int index)
    {
        if (index < 0 || index > MaxIndex)
            throw new IndexOutOfRangeException();

        for (int i = index; i < MaxIndex; i++)
        {
            this[i] = this[i + 1];
        }

        Shrink(1);
    }

    /// <summary>
    /// Finds the index of the specified item.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <returns>The index of the item if found, otherwise -1.</returns>
    int IList<T>.IndexOf(T item)
    {
        return (this as IList).IndexOf(item);
    }

    /// <summary>
    /// Inserts an item at the specified index.
    /// </summary>
    /// <param name="index">The index to insert the item at.</param>
    /// <param name="item">The item to insert.</param>
    void IList<T>.Insert(int index, T item)
    {
        (this as IList).Insert(index, item);
    }

    /// <summary>
    /// Adds an item to the collection.
    /// </summary>
    /// <param name="value">The item to add.</param>
    /// <returns>The index where the item was added.</returns>
    int IList.Add(object? value)
    {
        if (_array == null)
        {
            throw new NullReferenceException("Array is null, cannot insert item.");
            return -1;
        }

        Add((T)value);
        return MaxIndex;
    }

    /// <summary>
    /// Clears the collection.
    /// </summary>
    void IList.Clear()
    {
        Resize(0);
    }

    /// <summary>
    /// Checks if the collection contains a specific item.
    /// </summary>
    /// <param name="value">The item to check.</param>
    /// <returns>True if the item is found, otherwise false.</returns>
    bool IList.Contains(object? value)
    {
        return Contains(value, out _);
    }

    /// <summary>
    /// Finds the index of the specified item.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <returns>The index of the item if found, otherwise -1.</returns>
    int IList.IndexOf(object? item)
    {
        int index = -1;
        for (int i = 0; i < Length; i++)
        {
            var arrayItem = this[i];
            if (AreEqual(arrayItem, item))
            {
                index = i;
                break;
            }
        }
        return index;
    }
    void IList.Insert(int index, object? value)
    {
        if (index < 0 || index >= Length)
            throw new ArgumentOutOfRangeException("index");

        Resize(Length + 1);

        for (int i = MaxIndex; i > index; i--)
        {
            this[i] = this[i - 1];
        }

        this[index] = (T)value;
    }

    int IStructuralComparable.CompareTo(object? other, IComparer comparer)
    {
        if (other == null)
            return 1;

        RLJArray<T> array = other as RLJArray<T>;
        if (array == null || this.Length != array.Length)
        {
            throw new ArgumentException("Can't compare two arrays with different lengths.");
        }

        int index = 0;
        ComparisonState state = ComparisonState.Equal;
        while (index < array.Length && state == ComparisonState.Equal)
        {
            state = (ComparisonState)comparer.Compare(this[index], array[index]);
            index++;
        }

        return (int)state;
    }

    bool IStructuralEquatable.Equals(object? other, IEqualityComparer comparer)
    {
        if (other == null)
            return false;

        RLJArray<T> otherArray = other as RLJArray<T>;
        if (this.Length != otherArray.Length)
            return false;

        int index = 0;
        while (index < Length)
        {
            if (!comparer.Equals(this[index], otherArray[index]))
                return false;
            index++;
        }

        return true;
    }

    int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
    {
        return this.GetHashCode();
    }

    /// <summary>
    /// Return new object of type <see cref="RLJArray{T}"/> with same data and size of this instance.
    /// </summary>
    /// <remarks>
    /// <b>Note:</b> Method returns an object of type <see cref="RLJArray{T}"/>
    /// </remarks>
    /// <returns>
    /// An object of type <see cref="RLJArray{T}"/>.
    /// </returns>
    public object Clone()
    {
        RLJArray<T> array = new RLJArray<T>(Length, SegmentLength);
        for (int i = 0; i < Length; i++)
        {
            array[i] = this[i];
        }

        return array;
    }

    /// <summary>
    /// Initializes every element of the value-type System.Array by calling the parameterless constructor of the value type.
    /// </summary>
    public void Initialize()
    {
        if (typeof(T).IsValueType && typeof(T).GetConstructor(Type.EmptyTypes) != null)
        {
            for (int i = 0; i < Length; i++)
            {
                this[i] = (T)Activator.CreateInstance(typeof(T));
            }
        }
    }

    public object GetValue(long index)
    {
        if (index > int.MaxValue || index < int.MinValue)
        {
            throw new ArgumentOutOfRangeException("index", "Arrays larger than 2GB are not supported.");
        }

        return this.GetValue((int)index);
    }

    public object GetValue(long index1, long index2)
    {
        throw new NotSupportedException(Constants.RLJARRAY_IS_SINGLE_DIMENSIONAL_ARRAY);
    }

    public object GetValue(long index1, long index2, long index3)
    {
        throw new NotSupportedException(Constants.RLJARRAY_IS_SINGLE_DIMENSIONAL_ARRAY);
    }

    public object GetValue(params long[] indices)
    {
        if (indices == null)
        {
            throw new ArgumentNullException("indices");
        }
        if (indices.Length != this.Rank)
        {
            throw new ArgumentException("Indices length does not match the array rank.");
        }

        return this[(int)indices[0]];
    }

    public void SetValue(object value, long index)
    {
        if (index > int.MaxValue || index < int.MinValue)
        {
            throw new ArgumentOutOfRangeException("index", "Arrays larger than 2GB are not supported.");
        }

        this.SetValue(value, (int)index);
    }

    public void SetValue(object value, long index1, long index2)
    {
        throw new NotSupportedException(Constants.RLJARRAY_IS_SINGLE_DIMENSIONAL_ARRAY);
    }

    public void SetValue(object value, long index1, long index2, long index3)
    {
        throw new NotSupportedException(Constants.RLJARRAY_IS_SINGLE_DIMENSIONAL_ARRAY);
    }

    public void SetValue(object value, params long[] indices)
    {
        if (indices == null)
        {
            throw new ArgumentNullException("indices");
        }
        if (indices.Length != this.Rank)
        {
            throw new ArgumentException("Indices length does not match the array rank.");
        }

        this[(int)indices[0]] = (T)value;
    }

    public object GetValue(int index)
    {
        return this[index];
    }

    public object GetValue(int index1, int index2)
    {
        throw new NotSupportedException(Constants.RLJARRAY_IS_SINGLE_DIMENSIONAL_ARRAY);
    }

    public object GetValue(int index1, int index2, int index3)
    {
        throw new NotSupportedException(Constants.RLJARRAY_IS_SINGLE_DIMENSIONAL_ARRAY);
    }

    public void SetValue(object value, int index)
    {
        this[index] = (T)value;
    }

    public void SetValue(object value, int index1, int index2)
    {
        throw new NotSupportedException(Constants.RLJARRAY_IS_SINGLE_DIMENSIONAL_ARRAY);
    }

    public void SetValue(object value, int index1, int index2, int index3)
    {
        throw new NotSupportedException(Constants.RLJARRAY_IS_SINGLE_DIMENSIONAL_ARRAY);
    }

    private class Constants
    {
        private Constants() { }

        public const string RLJARRAY_IS_SINGLE_DIMENSIONAL_ARRAY = "RLJArray is just a single-dimensional array.";
        public const string ARRAY_2GB_IS_NOT_SUPPORTED = "Arrays larger than 2GB are not supported.";
    }
}
