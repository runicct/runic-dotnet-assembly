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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public partial class MetadataTable
        {
            public class TypeSpecTable : MetadataTable
            {
                List<TypeSpecTableRow> _rows = new List<TypeSpecTableRow>();
                public override int ID { get { return 0x1B; } }
                public override uint Columns { get { return 1; } }
                public override uint Rows { get { lock (this) { return (uint)_rows.Count; } } }
                public override bool Sorted { get { return false; } }
                public TypeSpecTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public class TypeSpecTableRow : MetadataTableRow, ITypeDefOrRefOrSpec
                {
                    uint _row;
                    public override uint Row { get { return _row; } }
                    TypeSpecTable _parent;
                    internal TypeSpecTable Parent { get { return _parent; } }
                    Heap.BlobHeap.Blob _signature;
                    public Heap.BlobHeap.Blob Signature { get { return _signature; } }
                    public override uint Length { get { return 1; } }
                    internal TypeSpecTableRow(TypeSpecTable parent, uint row, Heap.BlobHeap.Blob signature)
                    {
                        _row = row;
                        _signature = signature;
                    }
                    internal TypeSpecTableRow(TypeSpecTable parent, uint row, Heap.BlobHeap blobHeap, BinaryReader reader)
                    {
                        _row = row;
                        _parent = parent;
                        uint signatureIndex = blobHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _signature = new Heap.BlobHeap.Blob(blobHeap, signatureIndex);
                    }
                    internal TypeSpecTableRow(TypeSpecTable parent, uint row)
                    {
                        _row = row;
                        _parent = parent;
                    }
                    internal void Load(Heap.BlobHeap blobHeap, BinaryReader reader)
                    {
                        uint signatureIndex = blobHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _signature = new Heap.BlobHeap.Blob(blobHeap, signatureIndex);
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                    {
                        uint signatureIndex = 0; if (blobHeap.LargeIndices) { signatureIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { signatureIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _signature = new Heap.BlobHeap.Blob(blobHeap, signatureIndex);
                    }
#endif

#if NET6_0_OR_GREATER
                    internal void Save(BinaryWriter binaryWriter)
#else
                    internal void Save(BinaryWriter binaryWriter)
#endif
                    {
                        if (_signature.Heap.LargeIndices) { binaryWriter.Write(_signature.Index); } else { binaryWriter.Write((ushort)_signature.Index); }
                    }
                }
                internal override void Save(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                public TypeSpecTableRow Add(Heap.BlobHeap.Blob signature)
                {
                    lock (this)
                    {
                        TypeSpecTableRow row = new TypeSpecTableRow(this, (uint)(_rows.Count + 1), signature);
                        _rows.Add(row);
                        return row;
                    }
                }
                public TypeSpecTable()
                {
                }
                internal TypeSpecTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new TypeSpecTableRow(this, (uint)(n + 1)));
                    }
                }
                internal void Load(Heap.BlobHeap blobHeap, BinaryReader reader)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Load(blobHeap, reader);
                    }
                }
#if NET6_0_OR_GREATER
                internal TypeSpecTable(uint rows, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Load(blobHeap, data, ref offset);
                    }
                }
#endif
            }
        }
    }
}
