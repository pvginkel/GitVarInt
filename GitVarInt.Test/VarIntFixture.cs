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
        [TestCase(0u, 0)]
        [TestCase(1u, -1)]
        [TestCase(2u, 1)]
        [TestCase(3u, -2)]
        [TestCase(4u, 2)]
        [TestCase(5u, -3)]
        [TestCase(6u, 3)]
        [TestCase(uint.MaxValue - 1, int.MaxValue)]
        [TestCase(uint.MaxValue, int.MinValue)]
        public void ZigZagInt32(uint encoded, int value)
        {
            Assert.AreEqual(encoded, VarInt.ZigZagEncode(value));
            Assert.AreEqual(value, VarInt.ZigZagDecode(encoded));
        }

        [TestCase(0u, 0)]
        [TestCase(1u, -1)]
        [TestCase(2u, 1)]
        [TestCase(3u, -2)]
        [TestCase(4u, 2)]
        [TestCase(5u, -3)]
        [TestCase(6u, 3)]
        [TestCase(ulong.MaxValue - 1, long.MaxValue)]
        [TestCase(ulong.MaxValue, long.MinValue)]
        public void ZigZagInt64(ulong encoded, long value)
        {
            Assert.AreEqual(encoded, VarInt.ZigZagEncode(value));
            Assert.AreEqual(value, VarInt.ZigZagDecode(encoded));
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

        [TestCase("0x00", 0)]
        [TestCase("0x01", -1)]
        [TestCase("0x02", 1)]
        [TestCase("0x8EFEFEFE7E", int.MaxValue)]
        [TestCase("0x8EFEFEFE7F", int.MinValue)]
        public void VarInt32(string hex, int value)
        {
            Assert.AreEqual(hex, GetHexEncoded(p => p.WriteVarInt(value)));
            Assert.AreEqual(value, GetHexDecoded(hex, p => p.ReadVarInt32()));
        }

        [TestCase("0x00", 0ul)]
        [TestCase("0x01", 1ul)]
        [TestCase("0x7F", 127ul)]
        [TestCase("0x8000", 128ul)]
        [TestCase("0x8100", 256ul)]
        [TestCase("0xFF7F", 16511ul)]
        [TestCase("0x808000", 16512ul)]
        [TestCase("0xFFFF7F", 2113663ul)]
        [TestCase("0x80808000", 2113664ul)]
        [TestCase("0x80FEFEFEFEFEFEFEFE7F", ulong.MaxValue)]
        public void VarUInt64(string hex, ulong value)
        {
            Assert.AreEqual(hex, GetHexEncoded(p => p.WriteVarInt(value)));
            Assert.AreEqual(value, GetHexDecoded(hex, p => p.ReadVarUInt64()));
        }

        [TestCase("0x00", 0l)]
        [TestCase("0x01", -1l)]
        [TestCase("0x02", 1l)]
        [TestCase("0x80FEFEFEFEFEFEFEFE7E", long.MaxValue)]
        [TestCase("0x80FEFEFEFEFEFEFEFE7F", long.MinValue)]
        public void VarInt64(string hex, long value)
        {
            Assert.AreEqual(hex, GetHexEncoded(p => p.WriteVarInt(value)));
            Assert.AreEqual(value, GetHexDecoded(hex, p => p.ReadVarInt64()));
        }

        [Test]
        public void VarInt32Roundtrip()
        {
            using (var stream = new MemoryStream())
            {
                for (int i = 0; ; i++)
                {
                    stream.Position = 0;
                    stream.WriteVarInt(i);

                    stream.Position = 0;
                    int read = stream.ReadVarInt32();

                    Assert.AreEqual(i, read);

                    if (i == int.MaxValue)
                        break;
                }
            }
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
