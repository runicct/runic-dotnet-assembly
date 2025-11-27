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

using System.Collections.Generic;
using System.IO;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public partial class MetadataTable
        {
            public class LocalConstantTable : MetadataTable
            {
                List<LocalConstantTableRow> _rows = new List<LocalConstantTableRow>();
                public class LocalConstantTableRow : MetadataTableRow
                {
                    uint _row;
                    public override uint Row { get { return _row; } }
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    Heap.BlobHeap.Blob _signature;
                    public Heap.BlobHeap.Blob Signature { get { return _signature; } }
                    public override uint Length { get { return 0x02; } }
                    LocalConstantTable _parent;
                    internal LocalConstantTable Parent { get { return _parent; } }
                    internal LocalConstantTableRow(LocalConstantTable parent, uint row, Heap.StringHeap.String name, Heap.BlobHeap.Blob signature)
                    {
                        _row = row;
                        _name = name;
                        _signature = signature;
                        _parent = parent;
                    }
                    internal LocalConstantTableRow(LocalConstantTable parent, uint row)
                    {
                        _row = row;
                        _parent = parent;
                    }
                    internal void Load(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, System.IO.BinaryReader reader)
                    {
                        _name = new Heap.BlobHeap.StringHeap.String(stringHeap, stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16());
                        _signature = new Heap.BlobHeap.Blob(blobHeap, blobHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16());
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                    {
                        uint nameIndex = 0; if (stringHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        uint signatureIndex = 0; if (blobHeap.LargeIndices) { signatureIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { signatureIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _signature = new Heap.BlobHeap.Blob(blobHeap, signatureIndex);
                    }
#endif
                    internal void Save(BinaryWriter binaryWriter)
                    {
                        if (_name.Heap.LargeIndices) { binaryWriter.Write((uint)_name.Index); } else { binaryWriter.Write((ushort)_name.Index); }
                        if (_signature.Heap.LargeIndices) { binaryWriter.Write((uint)_signature.Index); } else { binaryWriter.Write((ushort)_signature.Index); }
                    }
                }
                public override int ID { get { return 0x34; } }
                public override uint Columns { get { return 2; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public LocalConstantTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public override bool Sorted { get { return false; } }
                internal override void Save(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                public LocalConstantTableRow Add(Heap.StringHeap.String name, Heap.BlobHeap.Blob signature)
                {
                    lock (this)
                    {
                        LocalConstantTableRow row = new LocalConstantTableRow(this, (uint)(_rows.Count + 1), name, signature);
                        _rows.Add(row);
                        return row;
                    }
                }
                public LocalConstantTable() : base()
                {

                }
                internal LocalConstantTable(uint rows) : base()
                {
                    _rows = new List<LocalConstantTableRow>();
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new LocalConstantTableRow(this, (uint)n));
                    }
                }
                internal void Load(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, System.IO.BinaryReader reader)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Load(stringHeap, blobHeap, reader);
                    }
                }
#if NET6_0_OR_GREATER

                internal void Load(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Load(stringHeap, blobHeap, data, ref offset);
                    }
                }
#endif
            }
        }
    }
}
