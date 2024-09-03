using System.Collections;

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

public class ResizableLinearJaggedArray<T> : IEnumerable<T>, IEnumerable, ICollection<T>, ICollection, IList<T>, IList, IStructuralComparable, IStructuralEquatable, ICloneable
{
    private T[][] array;

    public readonly int Rank = 1;

    public readonly int SegmentLength;
    public int NumberOfSegments { get { return array.Length; } }

    private int length = 0;
    public int Length
    {
        get { return length; }
        private set 
        {
            int _newLength = value;

            if (_newLength < 0)
                throw new ArgumentOutOfRangeException("New length can't be less than zero.");

            if (_newLength == Length)
                return;

            if (_newLength == 0)
            {
                array = new T[0][];
            }
            else
            {
                int _newNumberOfSegments = CalculateNumberOfSegments(_newLength);
                if (_newNumberOfSegments != NumberOfSegments)
                {
                    T[][] _newArray = new T[_newNumberOfSegments][];
                    for (int _segmentIndex = 0; _segmentIndex < _newNumberOfSegments && _segmentIndex < NumberOfSegments; _segmentIndex++)
                    {
                        _newArray[_segmentIndex] = array[_segmentIndex];
                    }
                    array = _newArray;
                }

                if (_newLength < Length)
                {
                    int _segmentIndex = CalculateSegmentIndex(_newLength - 1);
                    int _itemIndex = CalculateItemIndexAtSegment(_newLength - 1);
                    T[] _segment = null;
                    if (array[_segmentIndex] != null)
                    {
                        _segment = array[_segmentIndex];

                        for (int i = _itemIndex + 1; i < SegmentLength; i++)
                        {
                            _segment[i] = default(T);
                        }
                    }
                }
            }

            length = _newLength;
        }
    }

    public int MaxIndex { get { return Length - 1; } }

    public int Count { get { return Length; } }

    public bool IsReadOnly { get { return false; } }

    public bool IsSynchronized { get { return false; } }

    public object SyncRoot { get { return this; } }

    public bool IsFixedSize { get { return false; } }

    object? IList.this[int index] { get { return this[index]; } set { this[index] = (T)value; } }

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

            if(array[_segmentIndex] == null)
            {
                if (ValueEqualsDefault(value))
                    return;
                else
                    array[_segmentIndex] = new T[SegmentLength];
            }

            array[_segmentIndex][_itemIndexAtSegment] = value;
        }
    }

    /// <summary>
    /// Resizes the Length of array to a new length.
    /// </summary>
    /// <param name="_newLength">
    /// The new length of the array.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown [indside Length] when the specified new length is less than zero.
    /// </exception>
    /// <remarks>
    ///     <b>Warning:</b> Resizing to a smaller length will result in the deletion of data at higher truncated indexes.
    /// </remarks>
    public void Resize(int _newLength)
    {
        Length = _newLength;
    }

    /// <summary>
    /// Increments length of array by size.
    /// </summary>
    /// <param name="_size"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void EnglargeBySize(int _size)
    {
        if (_size < 0)
            throw new ArgumentOutOfRangeException("Can't enlarge array with minus number.");

        Resize(Length + _size);
    }

    /// <summary>
    /// Decrements length of array by size.
    /// </summary>
    /// <param name="_size"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void ShrinkBySize(int _size)
    {
        if (_size < 0)
            throw new ArgumentOutOfRangeException("Can't shrink array with minus number.");

        if (_size > Length)
            throw new ArgumentOutOfRangeException("Can't shrink array more than its actual length.");

        Resize(Length - _size);
    }

    public bool Contains(object? _item, out int _index)
    {
        _index = -1;
        for (int i = 0; i < Length; i++)
        {
            if (this[i] as object == _item)
            {
                _index = i;
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
    /// Cleanes array from empty segments.
    /// </summary>
    public void CleanEmptySegments()
    {
        if (Length == 0)
            return;

        CleanEmptySegments(0, Length);
    }

    /// <summary>
    /// Cleanes array from empty segments within given rang.
    /// </summary>
    /// <param name="_startIndex"></param>
    /// <param name="_amount"></param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void CleanEmptySegments(int _startIndex, int _amount)
    {
        if (Length == 0)
            throw new InvalidOperationException("Length of array is 0, Can't operate on an empty array.");

        if (_startIndex < 0 || _startIndex >= Length || _amount < 0)
            throw new ArgumentOutOfRangeException("length");

        if (_amount == 0)
            return;

        int _endIndex = _startIndex + _amount - 1;
        if (!IsFirstIndexInSegment(_startIndex))
        {
            _startIndex = GetFirstIndexOfNextSegment(_endIndex);
        }

        if (!IsLastIndexInSegment(_endIndex))
        {
            _endIndex = GetLastIndexOfPreviousSegment(_endIndex);
        }

        int _startSegmentIndex = CalculateSegmentIndex(_startIndex);
        int _endSegmentIndex = CalculateSegmentIndex(_endIndex);

        for (int i = _startSegmentIndex; i <= _endSegmentIndex; i++)
        {
            T[] _segment = array[i];
            if (_segment == null)
                continue;

            bool _segmentIsEmpty = true;
            for (int j = 0; j < _segment.Length; j++)
            {
                T _item = _segment[j];
                bool _itemIsExisted = !ValueEqualsDefault(_item);
                _segmentIsEmpty = !_itemIsExisted;
                if (!_segmentIsEmpty)
                    break;
            }

            if (_segmentIsEmpty)
                array[i] = null;
        }
    }

    /// <summary>
    /// Calculates the number of segments required to hold a given number of items.
    /// </summary>
    /// <param name="_length">The total number of items that should be hold in segments.</param>
    /// <returns>The number of segments needed.</returns>
    private int CalculateNumberOfSegments(int _length)
    {
        if (_length == 0)
            return 0;

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

    public bool IsFirstIndexInSegment(int _index)
    {
        return _index % SegmentLength == 0;
    }

    public bool IsLastIndexInSegment(int _index)
    {
        return _index % SegmentLength == SegmentLength - 1;
    }

    public int GetFirstIndexOfNextSegment(int _index)
    {
        int _length = _index + 1;
        _index = CalculateNumberOfSegments(_length) * SegmentLength;
        return _index;
    }

    public int GetLastIndexOfPreviousSegment(int _index)
    {
        int _length = _index + 1;
        _index = ((CalculateNumberOfSegments(_length) - 1) * SegmentLength) - 1;
        return _index;
    }

    private bool ValueEqualsDefault(object? _value)
    {
        return !(_value != null && !_value.Equals(default(T)));
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Length; i++)
        {
            int _segmentIndex = CalculateSegmentIndex(i);
            int _itemIndex = CalculateItemIndexAtSegment(i);

            if (array[_segmentIndex] != null)
            {
                yield return array[_segmentIndex][_itemIndex];
            }
            else
            {
                yield return default(T);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return (this as IEnumerable).GetEnumerator();
    }

    void ICollection<T>.Clear()
    {
        Resize(0);
    }

    bool ICollection<T>.Contains(T _item)
    {
        return Contains(_item, out _);
    }

    void ICollection.CopyTo(Array _array, int _arrayIndex)
    {
        for (int i = _arrayIndex; i < _array.Length && i < Length; i++)
        {
            _array.SetValue(this[i], i);
        }
    }
    void ICollection<T>.CopyTo(T[] _array, int _arrayIndex)
    {
        (this as ICollection).CopyTo(_array, _arrayIndex);
    }

    public void CopyTo(ResizableLinearJaggedArray<T> _array, int _arrayIndex)
    {
        for (int i = _arrayIndex; i < _array.Length && i < Length; i++)
        {
            _array[i] = this[i];
        }
    }

    bool ICollection<T>.Remove(T _item)
    {
        int _index = -1;
        for (int i = 0; i < Length; i++)
        {
            if ((object?)this[i] == (object?)_item)
            {
                _index = i;
                break;
            }
        }

        if (_index == -1)
            return false;

        for (int i = _index; i < MaxIndex; i++)
        {
            this[i] = this[i + 1];
        }
        ShrinkBySize(1);

        return true;
    }

    void IList<T>.RemoveAt(int _index)
    {
        (this as IList).RemoveAt(_index);
    }

    void IList.Remove(object? _value)
    {
        int _itemIndex = (this as IList).IndexOf(_value);
        if (_itemIndex == -1)
            throw new ArgumentException("Array doesn't contain value.");

        (this as IList).RemoveAt(_itemIndex);
    }

    void IList.RemoveAt(int _index)
    {
        if (_index <= -1 || _index > MaxIndex)
            throw new IndexOutOfRangeException();

        for (int i = _index; i < MaxIndex; i++)
        {
            this[i] = this[i + 1];
        }
        ShrinkBySize(1);
    }

    int IList<T>.IndexOf(T _item)
    {
        return Array.FindIndex(this.ToArray<T>(), (T _value) => { return (_value as object) == (_item as object); });
    }

    void IList<T>.Insert(int _index, T _item)
    {
        (this as IList).Insert(_index, _item);
    }

    int IList.Add(object? _value)
    {
        if (array == null)
        {
            throw new NullReferenceException("array is null, Index to insert item is -1");
            return -1; // this doesn't give any meaning but IList.Add made to return -1 when adding new item fails.
        }

        Add((T)_value);
        return Length - 1;
    }

    void IList.Clear()
    {
        Resize(0);
    }

    bool IList.Contains(object? _value)
    {
        return Contains(_value, out _);
    }

    int IList.IndexOf(object? _item)
    {
        int _index = -1;
        for (int i = 0; i < Length; i++)
        {
            var _arrayItem = this[i];
            if (_arrayItem == null && _item == null)
                return _index;
            else if (_arrayItem == null || _item == null)
                continue;
            else if (_arrayItem != null && _arrayItem.Equals(_item))
                return i;
        }
        return _index;
    }

    void IList.Insert(int _index, object? _value)
    {
        if (_index < 0 || _index >= Length)
            throw new ArgumentOutOfRangeException("index");

        Resize(Length + 1);

        for (int i = MaxIndex; i > _index; i--)
        {
            this[i] = this[i - 1];
        }

        this[_index] = (T)_value;
    }

    int IStructuralComparable.CompareTo(object? _other, IComparer _comparer)
    {
        if (_other == null)
            return 1;

        ResizableLinearJaggedArray<T> _array = _other as ResizableLinearJaggedArray<T>;
        if (_array == null || this.Length != _array.Length)
        {
            throw new ArgumentException("Can't compare two arrays with different lengthes.");
        }

        int _index = 0;
        ComparisonState _state = 0;
        while (_index < _array.Length && _state == ComparisonState.Equal)
        {
            _state = (ComparisonState)_comparer.Compare(this[_index], _array[_index]);
            _index++;
        }

        return (int)_state;
    }

    bool IStructuralEquatable.Equals(object? _other, IEqualityComparer _comparer)
    {
        if (_other == null)
            return false;

        ResizableLinearJaggedArray<T> _otherArray = _other as ResizableLinearJaggedArray<T>;
        if (this.Length != _otherArray.Length)
            return false;


        int _index = 0;
        while (_index < Length)
        {
            if (!_comparer.Equals(this[_index], _otherArray[_index]))
                return false;
            _index++;
        }

        return true;
    }

    int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
    {
        return this.GetHashCode();
    }

    object ICloneable.Clone()
    {
        ResizableLinearJaggedArray<T> _array = new ResizableLinearJaggedArray<T>(Length);
        for (int i = 0; i < Length; i++)
        {
            _array[i] = this[i];
        }

        return _array;
    }
}
