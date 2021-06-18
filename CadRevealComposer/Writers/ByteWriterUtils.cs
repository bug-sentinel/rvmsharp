namespace CadRevealComposer.Writers
{
    using Primitives;
    using System;
    using System.IO;

    public static class ByteWriterUtils
    {
        public static void WriteUint16(this Stream stream, ushort value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, sizeof(ushort));
        }

        public static void WriteUint32(this Stream stream, uint value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, sizeof(uint));
        }

        public static void WriteUint48(this Stream stream, ulong value)
        {
            for (int i = 0; i < 48; i += 8)
            {
                stream.WriteByte((byte)((value >> i) & 0xff));
            }
        }

        public static void WriteUint64(this Stream stream, ulong value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, sizeof(ulong));
        }

        public static void WriteFloat(this Stream stream, float value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, sizeof(float));
        }

        public static void WriteDouble(this Stream stream, double value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, sizeof(double));
        }

        public static void WriteFloatArray(this Stream stream, float[] value)
        {
            stream.WriteUint32((uint)value.Length);
            stream.WriteByte(sizeof(float));

            foreach (float v in value)
            {
                stream.WriteFloat(v);
            }
        }

        public static void WriteUint64Array(this Stream stream, ulong[] value)
        {
            stream.WriteUint32((uint)value.Length);
            stream.WriteByte(sizeof(ulong));

            foreach (ulong v in value)
            {
                stream.WriteUint64(v);
            }
        }

        public static void WriteRgbaArray(this Stream stream, int[][] value)
        {
            stream.WriteUint32((uint)value.Length);
            stream.WriteByte(4);

            foreach (var c in value)
            {
                stream.WriteByte((byte)c[0]);
                stream.WriteByte((byte)c[1]);
                stream.WriteByte((byte)c[2]);
                stream.WriteByte((byte)c[3]);
            }
        }

        public static void WriteNormalArray(this Stream stream, float[][] value)
        {
            stream.WriteUint32((uint)value.Length);
            stream.WriteByte(sizeof(float) * 3);

            foreach (var n in value)
            {
                stream.WriteFloat(n[0]);
                stream.WriteFloat(n[1]);
                stream.WriteFloat(n[2]);
            }
        }

        public static void WriteTextureArray(this Stream stream, TriangleMesh.Texture[] value)
        {
            stream.WriteUint32((uint)value.Length);
            stream.WriteByte(16);

            foreach (var t in value)
            {
                stream.WriteDouble(t.FileId);
                stream.WriteUint16(t.Width);
                stream.WriteUint16(t.Height);
                stream.WriteUint32(0); // reserved
            }
        }
    }
}