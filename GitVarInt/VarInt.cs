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

        public static long ReadVarInt64(this Stream stream)
        {
            return ZigZagDecode(stream.ReadVarUInt64());
        }

        public static ulong ReadVarUInt64(this Stream stream)
        {
            byte b = ReadByte(stream);
            ulong value = (ulong)(b & 127);

            while ((b & 128) != 0)
            {
                value += 1;
                if (value == 0 || (value & 0xfe00000000000000ul) != 0)
                    throw new VarIntException("Decode overflow");
                b = ReadByte(stream);
                value = (value << 7) + (ulong)(b & 127);
            }

            return value;
        }

        public static void WriteVarInt(this Stream stream, int value)
        {
            stream.WriteVarInt(ZigZagEncode(value));
        }

        public static void WriteVarInt(this Stream stream, uint value)
        {
            unsafe
            {
                var buffer = stackalloc byte[5];
                int offset = 5;

                buffer[--offset] = (byte)(value & 0x7F);

                while ((value >>= 7) != 0)
                {
                    buffer[--offset] = (byte)(0x80 | (--value & 0x7F));
                }

                while (offset < 5)
                {
                    stream.WriteByte(buffer[offset++]);
                }
            }
        }

        public static void WriteVarInt(this Stream stream, long value)
        {
            stream.WriteVarInt(ZigZagEncode(value));
        }

        public static void WriteVarInt(this Stream stream, ulong value)
        {
            unsafe
            {
                var buffer = stackalloc byte[10];
                int offset = 10;

                buffer[--offset] = (byte)(value & 0x7F);

                while ((value >>= 7) != 0)
                {
                    buffer[--offset] = (byte)(0x80 | (--value & 0x7F));
                }

                while (offset < 10)
                {
                    stream.WriteByte(buffer[offset++]);
                }
            }
        }

        public static uint ZigZagEncode(int value)
        {
            return ((uint)value << 1) ^ (uint)-(int)((uint)value >> 31);
        }

        public static ulong ZigZagEncode(long value)
        {
            return ((ulong)value << 1) ^ (ulong)-(long)((ulong)value >> 63);
        }

        public static int ZigZagDecode(uint value)
        {
            return (int)(value >> 1) ^ -((int)value & 0x1);
        }

        public static long ZigZagDecode(ulong value)
        {
            return (long)(value >> 1) ^ -((long)value & 0x1);
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
