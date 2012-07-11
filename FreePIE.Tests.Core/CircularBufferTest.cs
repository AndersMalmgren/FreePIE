using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FreePIE.Tests.Core
{
    [TestClass]
    public class CircularBufferTest
    {
        [TestMethod]
        public void TestCircularBuffer()
        {
            var buffer = new FixedSizeStack<int>(10);

            for(int i = 0; i < 200; i++)
                buffer.Push(i);

            var sequence = buffer.ToList();

            sequence.AssertSequenceEqual(Enumerable.Range(190, 10));
        }
    }
}
