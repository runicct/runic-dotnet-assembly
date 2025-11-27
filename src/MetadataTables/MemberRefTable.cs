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
            public class MemberRefTable : MetadataTable
            {
                public class MemberRefTableRow : MetadataTableRow
                {
                    public override uint Length { get { return 5; } }
                    ushort _parentToken;
                    public ushort ParentToken { get { return _parentToken; } }
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    Heap.BlobHeap.Blob _signature;
                    public Heap.BlobHeap.Blob Signature { get { return _signature; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    internal MemberRefTableRow(uint row, Heap.StringHeap.String name, Heap.BlobHeap.Blob signature, ushort parentToken)
                    {
                        _row = row;
                        _parentToken = parentToken;
                        _signature = signature;
                        _name = name;
                    }
                    internal MemberRefTableRow(uint row, Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, System.IO.BinaryReader reader)
                    {
                        _row = row;
                        _parentToken = reader.ReadUInt16();
                        uint nameIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        uint signatureIndex = blobHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        _signature = new Heap.BlobHeap.Blob(blobHeap, signatureIndex);
                    }
#if NET6_0_OR_GREATER
                    internal MemberRefTableRow(uint row, Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                    {
                        _row = row;
                        _parentToken = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        uint nameIndex = 0; if (stringHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        uint signatureIndex = 0; if (blobHeap.LargeIndices) { signatureIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { signatureIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        _signature = new Heap.BlobHeap.Blob(blobHeap, signatureIndex);
                    }
#endif
                    internal void Save(BinaryWriter binaryWriter)
                    {
                        binaryWriter.Write(_parentToken);
                        if (_name.Heap.LargeIndices) { binaryWriter.Write(_name.Index); } else { binaryWriter.Write((ushort)_name.Index); }
                        if (_signature.Heap.LargeIndices) { binaryWriter.Write(_signature.Index); } else { binaryWriter.Write((ushort)_signature.Index); }
                    }
                }
                public override int ID { get { return 0x0A; } }
                public override uint Columns { get { return 3; } }
                public override uint Rows { get { lock (this) { return (uint)_rows.Count; } } }
                public override bool Sorted { get { return false; } }
                internal override void Save(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                List<MemberRefTableRow> _rows = new List<MemberRefTableRow>();
                public MemberRefTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public MemberRefTableRow Add(Heap.StringHeap.String name, Heap.BlobHeap.Blob signature, ushort parentToken)
                {
                    lock (this)
                    {
                        MemberRefTableRow row = new MemberRefTableRow((uint)(_rows.Count + 1), name, signature, parentToken);
                        _rows.Add(row);
                        return row;
                    }
                }
                public MemberRefTable()
                {
                }

                internal MemberRefTable(uint rows, Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, System.IO.BinaryReader reader)
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new MemberRefTableRow(n + 1, stringHeap, blobHeap, reader));
                    }
                }
#if NET6_0_OR_GREATER
                internal MemberRefTable(uint rows, Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new MemberRefTableRow(n + 1, stringHeap, blobHeap, data, ref offset));
                    }
                }
#endif
            }
        }
    }
}
