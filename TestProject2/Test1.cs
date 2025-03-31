namespace TestProject2
{
    using Microsoft.Testing.Platform.Extensions.Messages;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using static System.Net.Mime.MediaTypeNames;
    using System.Xml.Linq;
    using System.Diagnostics.CodeAnalysis;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.VisualBasic;

    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var list = new LinkedList<int>(stackalloc byte[0]);
            for (int i = 0; i < 10; ++i)
            {
                Span<byte> memory = stackalloc byte[list.MemorySize];
                list.Append(i, memory);
            }

            foreach (var element in list)
            {
                Console.WriteLine(element);
            }
        }

        public sealed class Collector
        {
            public Collector()
            {
            }

            ~Collector()
            {
                Interlocked.Increment(ref Finalized);
            }
        }

        public static int Finalized = 0;

        [TestMethod]
        public void Test2()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Finalized = 0;

            var list = new List<Pointer<Collector>>();
            for (int i = 0; i < 10; ++i)
            {
                var collector = new Collector();
                var pointer = new Pointer<Collector>(ref collector);
                list.Add(pointer);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.AreEqual(0, Finalized);
        }

        [TestMethod]
        public void Test3()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Finalized = 0;

            Test3Helper();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.AreEqual(0, Finalized);
        }

        private static unsafe void Test3Helper()
        {
            var collector = new Collector();

            Collector* pointer = &collector;
            var safePointer = new IntPtr(pointer);
            var handle = GCHandle.FromIntPtr(safePointer);
        }

        public struct Test4Structure
        {
            public Test4Structure(Collector first, Collector second, Collector third)
            {
                First = first;
                Second = second;
                Third = third;
            }

            public Collector First { get; }
            public Collector Second { get; }
            public Collector Third { get; }
        }

        [TestMethod]
        public unsafe void Test4()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Finalized = 0;

            var length = 10;
            byte* bytes = stackalloc byte[Unsafe.SizeOf<Test4Structure>() * length];
            var span = new Span<Test4Structure>(bytes, length);
            for (int i = 0; i < length; ++i)
            {
                var structure = new Test4Structure(
                    new Collector(),
                    new Collector(),
                    new Collector());
                span[i] = structure;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.AreEqual(0, Finalized);
        }
    }

    public readonly struct Pointer<T> where T : allows ref struct
    {
        private readonly SafeHandle? handle;

        private readonly GCHandle gcHandle;

        public unsafe Pointer(ref T value) //// TODO would it help to remove `ref` here?
        {
            var pointer = new IntPtr(Unsafe.AsPointer(ref value));
            this.handle = new SafeSocketHandle(pointer, false);

            this.gcHandle = GCHandle.FromIntPtr(pointer);
        }

        public unsafe ref T Value
        {
            get
            {
                if (this.IsNull)
                {
                    throw new NullReferenceException("TODO");
                }

                var dangerousHandle = this.handle.DangerousGetHandle();
                ref T value = ref Unsafe.AsRef<T>(dangerousHandle.ToPointer());
                return ref value;
            }
        }

        [MemberNotNullWhen(false, nameof(this.handle))]
        public bool IsNull
        {
            get
            {
                if (this.handle == null)
                {
                    return true;
                }

                var dangerousHandle = this.handle.DangerousGetHandle();
                if (dangerousHandle == IntPtr.Zero)
                {
                    return true;
                }

                return false;
            }
        }

        public void Dispose()
        {
            this.gcHandle.Free();
            this.handle?.Dispose();
        }
    }

    public static class MemoryMarshal2
    {
        public static unsafe void Write<T>(Span<byte> destination, in T value) where T : allows ref struct
        {
            var typeSize = Unsafe.SizeOf<T>();
            if (destination.Length != typeSize)
            {
                var message = $"The length of '{nameof(destination)}' must be the same as the size of the type of '{nameof(value)}'. The length of '{nameof(destination)}' was '{destination.Length}'. The type of '{nameof(value)}' was '{typeof(T).FullName}'; its size was '{typeSize}'.";
                throw new ArgumentOutOfRangeException(message, (Exception?)null);
            }

            fixed (byte* pointer = destination)
            {
                Unsafe.Copy(pointer, in value);
            }
        }
        public static ref T AsRef<T>(Span<byte> memory) where T : allows ref struct
        {
            var typeSize = Unsafe.SizeOf<T>();
            if (memory.Length != typeSize)
            {
                var message = $"The length of '{nameof(memory)}' must be the same as the size of the type to get a reference to. The length of '{nameof(memory)}' was '{memory.Length}'. The type was '{typeof(T).FullName}'; its size was '{typeSize}'.";
                throw new ArgumentOutOfRangeException(message, (Exception?)null);
            }

            return ref Unsafe.As<byte, T>(ref memory.GetPinnableReference());
        }
    }

    public ref struct LinkedList<T> where T : allows ref struct
    {
        private Pointer<Node> first;

        private Pointer<Node> current;

        private bool hasValues;

        public LinkedList(Span<byte> empty)
        {
            if (empty.Length != 0)
            {
                throw new Exception("TODO");
            }
        }

        public int MemorySize { get; } = System.Runtime.CompilerServices.Unsafe.SizeOf<Node>();

        public unsafe void Append(T value, Span<byte> memory)
        {
            if (!this.hasValues)
            {
                var node = new Node()
                {
                    Value = value,
                };

                MemoryMarshal2.Write(memory, node);
                var copied = MemoryMarshal2.AsRef<Node>(memory);

                var pointer = new Pointer<Node>(ref copied); //// TODO add `ref` to parameter list?

                this.first = pointer;
                this.current = first;

                this.hasValues = true;
            }
            else
            {
                var node = new Node()
                {
                    Value = value,
                };

                MemoryMarshal2.Write(memory, node);
                var copied = MemoryMarshal2.AsRef<Node>(memory);

                var pointer = new Pointer<Node>(ref copied); //// TODO add `ref` to parameter list?

                this.current.Value.Next = pointer;
                this.current = pointer;
            }
        }

        public void Dispose()
        {
            var current = this.first;
            while (!current.Value.Next.IsNull)
            {
                current.Dispose();
                current = current.Value.Next;
            }

            current.Dispose();
        }

        private ref struct Node
        {
            public T Value;

            public Pointer<Node> Next;

            public void Dispose()
            {
                this.Next.Dispose();
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public ref struct Enumerator
        {
            private Pointer<Node> current;

            private bool moved;

            private bool hasValues;

            public Enumerator(LinkedList<T> list)
            {
                this.current = list.first;

                this.moved = false;

                this.hasValues = list.hasValues;
            }

            public T Current
            {
                get
                {
                    if (!this.hasValues || !this.moved)
                    {
                        throw new System.Exception("TODO");
                    }

                    return current.Value.Value;
                }
            }

            public unsafe bool MoveNext()
            {
                if (!this.hasValues)
                {
                    return false;
                }

                if (!this.moved)
                {
                    this.moved = true;
                    return true;
                }

                if (this.current.Value.Next.IsNull)
                {
                    return false;
                }

                this.current = this.current.Value.Next;
                return true;
            }
        }
    }
}
