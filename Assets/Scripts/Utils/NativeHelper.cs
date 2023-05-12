using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace OptIn.Voxel.Utils
{
    public static class NativeHelper
    {
        public static unsafe void NativeAddRange<T>(this List<T> list, NativeSlice<T> nativeSlice) where T : struct
        {
            var index = list.Count;
            var newLength = index + nativeSlice.Length;
 
            // Resize our list if we require
            if (list.Capacity < newLength)
            {
                list.Capacity = newLength;
            }
 
            var items = NoAllocHelpers.ExtractArrayFromListT(list);
            var size = UnsafeUtility.SizeOf<T>();
 
            // Get the pointer to the end of the list
            var bufferStart = (IntPtr) UnsafeUtility.AddressOf(ref items[0]);
            var buffer = (byte*)(bufferStart + (size * index));
 
            UnsafeUtility.MemCpy(buffer, nativeSlice.GetUnsafePtr(), nativeSlice.Length * (long) size);
 
            NoAllocHelpers.ResizeList(list, newLength);
        }

        public static unsafe void ManagedToNative<T>(this NativeArray<T> nativeArray, T[] array) where T : unmanaged
        {
            fixed (void* voxelPointer = array)
            {
                UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nativeArray), voxelPointer, array.Length * (long) UnsafeUtility.SizeOf<T>());
            }
        }

        public static unsafe void NativeToManaged<T>(this T[] array, NativeArray<T> nativeArray) where T : unmanaged
        {
            fixed (void* voxelPointer = array)
            {
                UnsafeUtility.MemCpy(voxelPointer, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nativeArray), array.Length * (long) UnsafeUtility.SizeOf<T>());
            }
        }
    }
}