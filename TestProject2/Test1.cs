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

    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var list = new LinkedList<int>();
            for (int i = 0; i < 10; ++i)
            {
                list.Append(i);
            }

            foreach (var element in list)
            {
                Console.WriteLine(element);
            }
        }
    }

    public readonly ref struct Pointer<T> where T : allows ref struct
    {
        private readonly SafeHandle? handle;

        public unsafe Pointer(ref T value) //// TODO would it help to remove `ref` here?
        {
            this.handle = new SafeSocketHandle(new IntPtr(Unsafe.AsPointer(ref value)), false);
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
            this.handle?.Dispose();
        }
    }

    public ref struct LinkedList<T> where T : allows ref struct
    {
        private Pointer<Node> first;

        private Pointer<Node> current;

        private bool hasValues;

        public LinkedList()
        {
        }

        public unsafe void Append(T value)
        {
            if (!this.hasValues)
            {
                var node = new Node()
                {
                    Value = value,
                };
                var pointer = new Pointer<Node>(ref node); //// TODO add `ref` to parameter list?

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
                var pointer = new Pointer<Node>(ref node); //// TODO add `ref` to parameter list?

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
