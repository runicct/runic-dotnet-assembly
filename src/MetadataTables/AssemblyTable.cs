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
using System.Collections.Generic;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public partial class MetadataTable
        {
            public class AssemblyTable : MetadataTable
            {
                List<AssemblyTableRow> _rows = new List<AssemblyTableRow>();
                public class AssemblyTableRow : MetadataTableRow
                {
                    uint _row;
                    public override uint Row { get { return _row; } }
                    System.Version _version;
                    public System.Version Version { get { return _version; } }
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
#if NET6_0_OR_GREATER
                    Heap.StringHeap.String? _culture;
                    public Heap.StringHeap.String? Culture { get { return _culture; } }
#else
                    Heap.StringHeap.String _culture;
                    public Heap.StringHeap.String Culture { get { return _culture; } }
#endif
                    public override uint Length { get { return 0x06; } }
#if NET6_0_OR_GREATER
                    internal AssemblyTableRow(uint row, System.Version version, Heap.StringHeap.String name, Heap.StringHeap.String? culture)
#else
                    internal AssemblyTableRow(uint row, System.Version version, Heap.StringHeap.String name, Heap.StringHeap.String culture)
#endif
                    {
                        _row = row;
                        _version = version;
                        _name = name;
                        _culture = culture;
                    }
                    internal AssemblyTableRow(uint row, Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, System.IO.BinaryReader reader)
                    {
                        _row = row;

                        uint hashAlgorithm = reader.ReadUInt32();
                        ushort assemblyVersionMajor = reader.ReadUInt16();
                        ushort assemblyVersionMinor = reader.ReadUInt16();
                        ushort assemblyVersionBuild = reader.ReadUInt16();
                        ushort assemblyVersionRevision = reader.ReadUInt16();
                        uint flags = reader.ReadUInt32();
                        uint publicKeyIndex = blobHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        uint nameIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        uint cultureIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();

                        _version = new System.Version(assemblyVersionMajor, assemblyVersionMinor, assemblyVersionBuild, assemblyVersionRevision);
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        _culture = cultureIndex == 0 ? null : new Heap.StringHeap.String(stringHeap, cultureIndex);
                    }
#if NET6_0_OR_GREATER
                    internal AssemblyTableRow(uint row, Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                    {
                        _row = row;

                        uint hashAlgorithm = BitConverterLE.ToUInt32(data, offset); offset += 4;
                        ushort assemblyVersionMajor = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        ushort assemblyVersionMinor = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        ushort assemblyVersionBuild = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        ushort assemblyVersionRevision = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        uint flags = BitConverterLE.ToUInt32(data, offset); offset += 4;
                        uint publicKeyIndex = 0; if (blobHeap.LargeIndices) { publicKeyIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { publicKeyIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        uint nameIndex = 0; if (stringHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        uint cultureIndex = 0; if (stringHeap.LargeIndices) { cultureIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { cultureIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }

                        _version = new Version(assemblyVersionMajor, assemblyVersionMinor, assemblyVersionBuild, assemblyVersionRevision);
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        _culture = cultureIndex == 0 ? null : new Heap.StringHeap.String(stringHeap, cultureIndex);
                    }
#endif
                    internal void Save(BinaryWriter binaryWriter)
                    {
                        binaryWriter.Write(0x8004 /* SHA1 */ );
                        binaryWriter.Write((ushort)_version.Major);
                        binaryWriter.Write((ushort)_version.Minor);
                        binaryWriter.Write((ushort)_version.Build);
                        binaryWriter.Write((ushort)_version.Revision);
                        binaryWriter.Write((int)0);
                        binaryWriter.Write((int)0);
                        binaryWriter.Write(_name.Index);
                        binaryWriter.Write((int)0);
                    }
                }
                public override int ID { get { return 0x20; } }
                public override uint Columns { get { return 9; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public AssemblyTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public override bool Sorted { get { return false; } }
                internal override void Save(BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
#if NET6_0_OR_GREATER
                internal uint Add(System.Version version, Heap.StringHeap.String name, Heap.StringHeap.String? culture)
#else
                internal uint Add(System.Version version, Heap.StringHeap.String name, Heap.StringHeap.String culture)
#endif
                {
                    lock (this)
                    {
                        AssemblyTableRow row = new AssemblyTableRow((uint)(_rows.Count + 1), version, name, culture);
                        _rows.Add(row);
                        return (uint)_rows.Count;
                    }
                }
                public AssemblyTable() : base()
                {
                }
                internal AssemblyTable(uint rows, Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, System.IO.BinaryReader reader) : base()
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new AssemblyTableRow((uint)(n + 1), stringHeap, blobHeap, reader));
                    }
                }
#if NET6_0_OR_GREATER

                internal AssemblyTable(uint rows, Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset) : base()
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new AssemblyTableRow((uint)(n + 1), stringHeap, blobHeap, data, ref offset));
                    }
                }
#endif
            }
        }
    }
}
