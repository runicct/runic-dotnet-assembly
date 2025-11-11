/*
 * MIT License
 * 
 * Copyright (c) 2025 Runic Compiler Toolkit Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System.IO;
using System.Text;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public class MetadataRoot
        {
            public class Stream
            {
                uint _rva;
                public uint RVA
                {
                    get { return _rva; }
                    set { _rva = value; }
                }
                uint _size;
                public uint Size
                {
                    get { return _size; }
                    set { _size = value; }
                }
                string _name;
                public string Name
                {
                    get { return _name; }
                    set { _name = value; }
                }
                public Stream(uint rva, uint size, string name)
                {
                    _rva = rva;
                    _size = size;
                    _name = name;
                }
                public override string ToString()
                {
                    return _name;
                }
            }
            Stream[] _streams;
            public Stream[] Streams
            {
                get { return _streams; }
                set { _streams = value; }
            }
            uint _rva;
            public uint RVA
            {
                get { return _rva; }
                set { _rva = value; }
            }
            public uint Size
            {
                get
                {
                    return 0;
                }
            }
            string _vesion;
            public string Version
            {
                get { return _vesion; }
                set { _vesion = value; }
            }
            public MetadataRoot(uint rva, string version, Stream[] streams)
            {
                _rva = rva;
                _vesion = version;
                _streams = streams;
            }
#if NET6_0_OR_GREATER
            public static MetadataRoot Load(uint rva, Span<byte> data, uint offset)
            {
                uint magic = BitConverterLE.ToUInt32(data, offset); offset += 4;
                ushort major = BitConverterLE.ToUInt16(data, offset); offset += 2;
                ushort minor = BitConverterLE.ToUInt16(data, offset); offset += 2;
                offset += 4;
                uint versionLength = BitConverterLE.ToUInt32(data, offset); offset += 4;
                StringBuilder version = new StringBuilder();
                {
                    uint n = 0;
                    for (; n < versionLength && data[(int)offset] != 0; n++, offset++) { version.Append((char)data[(int)offset]); }
                    offset += versionLength - n;
                }
                offset += 2; // Flags Reserved, always 0
                ushort streamCount = BitConverterLE.ToUInt16(data, offset); offset += 2;
                Stream[] streams = new Stream[streamCount];
                for (int n = 0; n < streamCount; n++)
                {
                    uint streamOffset = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    uint streamLength = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    uint streamRVA = rva + streamOffset;
                    StringBuilder streamName = new StringBuilder();
                    {
                        uint x = 0;
                        for (; data[(int)offset] != 0; x++, offset++) { streamName.Append((char)data[(int)offset]); }
                        x++;
                        offset++;
                        uint padding = 4 - (x % 4);
                        if (padding != 4) { offset += padding; }
                    }
                    streams[n] = new Stream(streamRVA, streamLength, streamName.ToString());
                }

                return new MetadataRoot(rva, version.ToString(), streams);
            }
#endif

#if NET6_0_OR_GREATER
            public static MetadataRoot Load(uint rva, byte[]? data, uint offset)
#else
            public static MetadataRoot Load(uint rva, byte[] data, uint offset)
#endif
            {
                uint magic = BitConverterLE.ToUInt32(data, offset); offset += 4;
                ushort major = BitConverterLE.ToUInt16(data, offset); offset += 2;
                ushort minor = BitConverterLE.ToUInt16(data, offset); offset += 2;
                offset += 4;
                uint versionLength = BitConverterLE.ToUInt32(data, offset); offset += 4;
                StringBuilder version = new StringBuilder();
                {
                    uint n = 0;
                    for (; n < versionLength && data[offset] != 0; n++, offset++) { version.Append((char)data[offset]); }
                    offset += versionLength - n;
                }
                offset += 2; // Flags Reserved, always 0
                ushort streamCount = BitConverterLE.ToUInt16(data, offset); offset += 2;
                Stream[] streams = new Stream[streamCount];
                for (int n = 0; n < streamCount; n++)
                {
                    uint streamOffset = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    uint streamLength = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    uint streamRVA = rva + streamOffset;
                    StringBuilder streamName = new StringBuilder();
                    {
                        uint x = 0;
                        for (;data[offset] != 0; x++, offset++) { streamName.Append((char)data[offset]); }
                        x++;
                        offset++;
                        uint padding = 4 - (x % 4);
                        if (padding != 4) { offset += padding; }
                    }
                    streams[n] = new Stream(streamRVA, streamLength, streamName.ToString());
                }

                return new MetadataRoot(rva, version.ToString(), streams);
            }
            public void Save(System.IO.BinaryWriter writer)
            {
                writer.Write((uint)0x424A5342); // Magic
                writer.Write((ushort)1); // Major Version
                writer.Write((ushort)1); // Minor Version
                writer.Write((uint)0); // Reserved
                byte[] versionData = System.Text.Encoding.UTF8.GetBytes(_vesion);
                int padding = ((versionData.Length + 4) / 4) * 4 - (versionData.Length + 1);
                // Length Number of bytes allocated to hold version string (including the terminator)
                writer.Write((uint)(versionData.Length + padding + 1));
                writer.Write(versionData);
                writer.Write((byte)0);
                for (int n = 0; n < padding; n++) { writer.Write((byte)0); }
                writer.Write((short)0); // Reserved
                writer.Write((short)_streams.Length);
                for (int n = 0; n < _streams.Length; n++)
                {
                    // Memory offset to start of this stream from start of the metadata root
                    writer.Write((uint)(_streams[n].RVA - _rva));
                    writer.Write((uint)(_streams[n].Size));
                    // Name of the stream as null-terminated variable length array of ASCII characters, padded to the next 4-byte boundary with \0 characters.
                    byte[] streamName = System.Text.Encoding.UTF8.GetBytes(_streams[n].Name + "\0");
                    writer.Write(streamName);
                    padding = ((streamName.Length + 3) / 4) * 4 - streamName.Length;
                    for (int x = 0; x < padding; x++) { writer.Write((byte)0); }
                }
            }
        }
    }
}
