using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;

namespace FreePIE.Core.Plugins.MemoryMapping
{
    public class MappedMemory<TRootObject> : IDisposable where TRootObject : struct
    {
        private readonly MemoryMappedFile memory;
        private readonly MemoryMappedViewAccessor view;

        public MappedMemory(string sharedMemoryName)
        {
            memory = MemoryMappedFile.CreateOrOpen(sharedMemoryName, Marshal.SizeOf(typeof(TRootObject)));
            view = memory.CreateViewAccessor();
        }

        private long GetFieldOffsetFromExpression<TValue>(Expression<Func<TRootObject, TValue>> getter) where TValue : struct
        {
            var member = getter.Body as MemberExpression;

            if (member == null)
                throw new Exception("Invalid expression - must be a single level expression");

            return Marshal.OffsetOf(typeof(TRootObject), member.Member.Name).ToInt64();
        }

        public void Write<TValue>(Expression<Func<TRootObject, TValue>> getter, TValue value) where TValue : struct
        {
            view.Write(GetFieldOffsetFromExpression(getter), ref value);
        }

        public void Write(TRootObject value)
        {
            view.Write(0, ref value);
        }

        public TValue Read<TValue>(Func<TRootObject, TValue> getter) where TValue : struct
        {
            return getter(Read());
        }

        public TRootObject Read()
        {
            TRootObject value;
            view.Read(0, out value);
            return value;
        }

        public void Dispose()
        {
            view.Dispose();
            memory.Dispose();
        }
    }
}
