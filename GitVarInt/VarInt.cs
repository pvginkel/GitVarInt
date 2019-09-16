using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitVarInt
{
    public static class VarInt
    {
        public static int ReadVarInt32(this Stream stream)
        {
            return ZigZagDecode(stream.ReadVarUInt32());
        }

        public static uint ReadVarUInt32(this Stream stream)
        {
            byte b = ReadByte(stream);
            uint value = (uint)(b & 127);

            while ((b & 128) != 0)
            {
                value += 1;
                if (value == 0 || (value & 0xfe000000u) != 0)
                    throw new VarIntException("Decode overflow");
                b = ReadByte(stream);
                value = (value << 7) + (uint)(b & 127);
            }

            return value;
        }

        public static void WriteVarInt(this Stream stream, int value)
        {
            stream.WriteVarInt(ZigZagEncode(value));
        }

        public static void WriteVarInt(this Stream stream, uint value)
        {
            var buf = GetVarIntBuffer(value);
            stream.Write(buf, 0, buf.Length);
        }

        private static byte[] GetVarIntBuffer(uint value)
        {
            int length = GetVarIntLength(value);
            var buffer = new byte[length];
            int offset = length;

            buffer[--offset] = (byte)(value & 0x7F);

            while ((value >>= 7) != 0)
            {
                buffer[--offset] = (byte)(0x80 | (--value & 0x7F));
            }

            return buffer;
        }

        private static int GetVarIntLength(long value)
        {
            int length = 1;

            for (; (value >>= 7) != 0; length++)
            {
                --value;
            }

            return length;
        }

        public static uint ZigZagEncode(int value)
        {
            return ((uint)value << 1) ^ (uint)-((uint)value >> 31);
        }

        public static int ZigZagDecode(uint value)
        {
            return (int)((value >> 1) ^ -(value & 0x1));
        }

        private static byte ReadByte(Stream stream)
        {
            int b = stream.ReadByte();
            if (b == -1)
                throw new VarIntException("Unexpected end");

            return (byte)b;
        }
    }
}
