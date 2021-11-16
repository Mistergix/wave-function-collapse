using System;

public interface IHeapItem<in T> : IComparable<T> 
{ 
    int HeapIndex { get; set; }
}