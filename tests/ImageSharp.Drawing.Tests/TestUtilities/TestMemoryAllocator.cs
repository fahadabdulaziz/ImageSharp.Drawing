using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Memory;

namespace SixLabors.ImageSharp.Drawing.Tests.Memory
{
    internal class TestMemoryAllocator : MemoryAllocator
    {
        private List<AllocationRequest> allocationLog = new List<AllocationRequest>();

        public TestMemoryAllocator(byte dirtyValue = 42)
        {
            this.DirtyValue = dirtyValue;
        }

        /// <summary>
        /// The value to initialize the result buffer with, with non-clean options (<see cref="AllocationOptions.None"/>)
        /// </summary>
        public byte DirtyValue { get; }

        public int BufferCapacityInBytes { get; set; } = int.MaxValue;

        public IList<AllocationRequest> AllocationLog => this.allocationLog;

        protected internal override int GetBufferCapacityInBytes() => this.BufferCapacityInBytes;

        public override IMemoryOwner<T> Allocate<T>(int length, AllocationOptions options = AllocationOptions.None)
        {
            T[] array = this.AllocateArray<T>(length, options);
            return new BasicArrayBuffer<T>(array, length);
        }

        public override IManagedByteBuffer AllocateManagedByteBuffer(int length, AllocationOptions options = AllocationOptions.None)
        {
            byte[] array = this.AllocateArray<byte>(length, options);
            return new ManagedByteBuffer(array);
        }

        private T[] AllocateArray<T>(int length, AllocationOptions options)
            where T : struct
        {
            this.allocationLog.Add(AllocationRequest.Create<T>(options, length));
            var array = new T[length + 42];

            if (options == AllocationOptions.None)
            {
                Span<byte> data = MemoryMarshal.Cast<T, byte>(array.AsSpan());
                data.Fill(this.DirtyValue);
            }

            return array;
        }

        public struct AllocationRequest
        {
            private AllocationRequest(Type elementType, AllocationOptions allocationOptions, int length, int lengthInBytes)
            {
                this.ElementType = elementType;
                this.AllocationOptions = allocationOptions;
                this.Length = length;
                this.LengthInBytes = lengthInBytes;

                if (elementType == typeof(Vector4))
                {

                }
            }

            public static AllocationRequest Create<T>(AllocationOptions allocationOptions, int length)
            {
                Type type = typeof(T);
                int elementSize = Marshal.SizeOf(type);
                return new AllocationRequest(type, allocationOptions, length, length * elementSize);
            }

            public Type ElementType { get; }
            public AllocationOptions AllocationOptions { get; }
            public int Length { get; }
            public int LengthInBytes { get; }
        }


        /// <summary>
        /// Wraps an array as an <see cref="IManagedByteBuffer"/> instance.
        /// </summary>
        private class BasicArrayBuffer<T> : MemoryManager<T>
            where T : struct
        {
            private GCHandle pinHandle;

            /// <summary>
            /// Initializes a new instance of the <see cref="BasicArrayBuffer{T}"/> class
            /// </summary>
            /// <param name="array">The array</param>
            /// <param name="length">The length of the buffer</param>
            public BasicArrayBuffer(T[] array, int length)
            {
                this.Array = array;
                this.Length = length;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="BasicArrayBuffer{T}"/> class
            /// </summary>
            /// <param name="array">The array</param>
            public BasicArrayBuffer(T[] array)
                : this(array, array.Length)
            {
            }

            /// <summary>
            /// Gets the array
            /// </summary>
            public T[] Array { get; }

            /// <summary>
            /// Gets the length
            /// </summary>
            public int Length { get; }

            /// <inheritdoc />
            public override Span<T> GetSpan() => this.Array.AsSpan(0, this.Length);

            public override unsafe MemoryHandle Pin(int elementIndex = 0)
            {
                if (!this.pinHandle.IsAllocated)
                {
                    this.pinHandle = GCHandle.Alloc(this.Array, GCHandleType.Pinned);
                }

                void* ptr = (void*)this.pinHandle.AddrOfPinnedObject();
                return new MemoryHandle(ptr, this.pinHandle);
            }

            public override void Unpin()
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            protected override void Dispose(bool disposing)
            {
            }
        }

        private class ManagedByteBuffer : BasicArrayBuffer<byte>, IManagedByteBuffer
        {
            public ManagedByteBuffer(byte[] array)
                : base(array)
            {
            }
        }
    }
}
