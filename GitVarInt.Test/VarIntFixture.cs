using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace GitVarInt.Test
{
    [TestFixture]
    public class VarIntFixture
    {
        [Test]
        public void ZigZagEncode()
        {
            Assert.AreEqual(0, VarInt.ZigZagEncode(0));
            Assert.AreEqual(1, VarInt.ZigZagEncode(-1));
            Assert.AreEqual(2, VarInt.ZigZagEncode(1));
            Assert.AreEqual(3, VarInt.ZigZagEncode(-2));
            Assert.AreEqual(4, VarInt.ZigZagEncode(2));
            Assert.AreEqual(5, VarInt.ZigZagEncode(-3));
            Assert.AreEqual(6, VarInt.ZigZagEncode(3));
        }

        [Test]
        public void ZigZagDecode()
        {
            Assert.AreEqual(0, VarInt.ZigZagDecode(0));
            Assert.AreEqual(-1, VarInt.ZigZagDecode(1));
            Assert.AreEqual(1, VarInt.ZigZagDecode(2));
            Assert.AreEqual(-2, VarInt.ZigZagDecode(3));
            Assert.AreEqual(2, VarInt.ZigZagDecode(4));
            Assert.AreEqual(-3, VarInt.ZigZagDecode(5));
            Assert.AreEqual(3, VarInt.ZigZagDecode(6));
        }

        [TestCase("0x00", 0u)]
        [TestCase("0x01", 1u)]
        [TestCase("0x7F", 127u)]
        [TestCase("0x8000", 128u)]
        [TestCase("0x8100", 256u)]
        [TestCase("0xFF7F", 16511u)]
        [TestCase("0x808000", 16512u)]
        [TestCase("0xFFFF7F", 2113663u)]
        [TestCase("0x80808000", 2113664u)]
        [TestCase("0x8EFEFEFE7F", uint.MaxValue)]
        public void VarUInt32(string hex, uint value)
        {
            Assert.AreEqual(hex, GetHexEncoded(p => p.WriteVarInt(value)));
            Assert.AreEqual(value, GetHexDecoded(hex, p => p.ReadVarUInt32()));
        }

        private static string GetHexEncoded(Action<Stream> func)
        {
            byte[] buffer;

            using (var stream = new MemoryStream())
            {
                func(stream);

                buffer = stream.ToArray();
            }

            var sb = new StringBuilder("0x");

            foreach (byte b in buffer)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }

        private static T GetHexDecoded<T>(string hex, Func<Stream, T> func)
        {
            Assert.AreEqual("0x", hex.Substring(0, 2));

            using (var stream = new MemoryStream())
            {
                for (int i = 2; i < hex.Length; i += 2)
                {
                    stream.WriteByte(Convert.ToByte(hex.Substring(i, 2), 16));
                }

                stream.Position = 0;

                return func(stream);
            }
        }
    }
}
