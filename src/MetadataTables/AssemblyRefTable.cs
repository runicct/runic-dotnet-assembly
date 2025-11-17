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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public partial class MetadataTable
        {
            public class AssemblyRefTable : MetadataTable
            {
                List<AssemblyRefTableRow> _rows = new List<AssemblyRefTableRow>();
                public class AssemblyRefTableRow : MetadataTableRow
                {
                    System.Version _version;
                    public System.Version Version { get { return _version; } }
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
#if NET6_0_OR_GREATER
                    Heap.StringHeap.String? _culture;
                    public Heap.StringHeap.String? Culture { get { return _culture; } }
                    Heap.BlobHeap.Blob? _publicKey;
                    public Heap.BlobHeap.Blob? PublicKey { get { return _publicKey; } }
#else
                    Heap.StringHeap.String _culture;
                    public Heap.StringHeap.String Culture { get { return _culture; } }
                    Heap.BlobHeap.Blob _publicKey;
                    public Heap.BlobHeap.Blob PublicKey { get { return _publicKey; } }
#endif
                    public override uint Length { get { return 9; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    internal AssemblyRefTableRow(uint row, System.Version version, Heap.StringHeap.String name, Heap.BlobHeap.Blob publicKey)
                    {
                        _row = row;
                        _version = version;
                        _name = name;
                        _publicKey = publicKey;
                    }
                    internal AssemblyRefTableRow(uint row, Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, System.IO.BinaryReader reader)
                    {
                        _row = row;
                        ushort assemblyVersionMajor = reader.ReadUInt16();
                        ushort assemblyVersionMinor = reader.ReadUInt16();
                        ushort assemblyVersionBuild = reader.ReadUInt16();
                        ushort assemblyVersionRevision = reader.ReadUInt16();
                        _version = new Version(assemblyVersionMajor, assemblyVersionMinor, assemblyVersionBuild, assemblyVersionRevision);

                        uint flags = reader.ReadUInt32();
                        uint publicKeyIndex = blobHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        uint nameKeyIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        uint cultureKeyIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();

                        _publicKey = publicKeyIndex == 0 ? null : new Heap.BlobHeap.Blob(blobHeap, publicKeyIndex);
                        _name = new Heap.StringHeap.String(stringHeap, nameKeyIndex);
                        _culture = cultureKeyIndex == 0 ? null : new Heap.StringHeap.String(stringHeap, cultureKeyIndex);
                    }
#if NET6_0_OR_GREATER
                    internal AssemblyRefTableRow(uint row, Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                    {
                        _row = row;
                        ushort assemblyVersionMajor = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        ushort assemblyVersionMinor = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        ushort assemblyVersionBuild = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        ushort assemblyVersionRevision = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        _version = new Version(assemblyVersionMajor, assemblyVersionMinor, assemblyVersionBuild, assemblyVersionRevision);
                        uint flags = 0;  flags = BitConverterLE.ToUInt32(data, offset); offset += 4;
                        uint publicKeyIndex = 0; if (blobHeap.LargeIndices) { publicKeyIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { publicKeyIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        uint nameKeyIndex = 0; if (stringHeap.LargeIndices) { nameKeyIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameKeyIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        uint cultureKeyIndex = 0; if (stringHeap.LargeIndices) { cultureKeyIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { cultureKeyIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _publicKey = publicKeyIndex == 0 ? null : new Heap.BlobHeap.Blob(blobHeap, publicKeyIndex);
                        _name = new Heap.StringHeap.String(stringHeap, nameKeyIndex);
                        _culture = cultureKeyIndex == 0 ? null : new Heap.StringHeap.String(stringHeap, cultureKeyIndex);
                    }
#endif
                    internal void Save(BinaryWriter binaryWriter)
                    {
                        binaryWriter.Write((ushort)_version.Major);
                        binaryWriter.Write((ushort)_version.Minor);
                        binaryWriter.Write((ushort)_version.Build);
                        binaryWriter.Write((ushort)_version.Revision);
                        binaryWriter.Write(0);
                        if (_publicKey != null)
                        {
                            binaryWriter.Write(_publicKey.Index);
                        }
                        else
                        {
                            binaryWriter.Write(0);
                        }
                        binaryWriter.Write(_name.Index);
                        binaryWriter.Write(0);
                        binaryWriter.Write(0);
                    }
                }
                public override int ID { get { return 0x23; } }
                public override uint Columns { get { return 9; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public AssemblyRefTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                internal override void Save(BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                public AssemblyRefTableRow Add(System.Version version, Heap.StringHeap.String name, Heap.BlobHeap.Blob publicKey)
                {
                    lock (this)
                    {
                        AssemblyRefTableRow row = new AssemblyRefTableRow((uint)(_rows.Count + 1), version, name, publicKey);
                        _rows.Add(row);
                        return row;
                    }
                }
                internal AssemblyRefTable()
                {
                }
                internal AssemblyRefTable(uint rows, Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, System.IO.BinaryReader reader)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new AssemblyRefTableRow(rows, stringHeap, blobHeap, reader));
                    }
                }
#if NET6_0_OR_GREATER

                internal AssemblyRefTable(uint rows, Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new AssemblyRefTableRow(rows, stringHeap, blobHeap, data, ref offset));
                    }
                }
#endif
            }
        }
    }
}
