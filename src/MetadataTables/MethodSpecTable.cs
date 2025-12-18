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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public partial class MetadataTable
        {
            public class MethodSpecTable : MetadataTable
            {
                List<MethodSpecTableRow> _rows = new List<MethodSpecTableRow>();
                public override int ID { get { return 0x2B; } }
                public override uint Columns { get { return 2; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public MethodSpecTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public MethodSpecTableRow Add(IMethodDefOrRef method, Heap.BlobHeap.Blob instantiation)
                {
                    lock (this)
                    {
                        MethodSpecTableRow row = new MethodSpecTableRow(this, (uint)(_rows.Count + 1), method, instantiation);
                        _rows.Add(row);
                        return row;
                    }
                }
                public class MethodSpecTableRow : MetadataTableRow, IHasCustomAttribute
                {
                    MethodSpecTable _parent;
                    internal MethodSpecTable Parent { get { return _parent; } }
                    public override uint Length { get { return 2; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    IMethodDefOrRef _method;
                    public IMethodDefOrRef Method { get { return _method; } }
                    Heap.BlobHeap.Blob _instantiation;
                    public Heap.BlobHeap.Blob Instantiation { get { return _instantiation; } }
                    internal MethodSpecTableRow(MethodSpecTable parent, uint row, IMethodDefOrRef method, Heap.BlobHeap.Blob instantiation)
                    {
                        _parent = parent;
                        _row = row;
                        _method = method;
                        _instantiation = instantiation;
                    }
                    internal MethodSpecTableRow(MethodSpecTable parent, uint row)
                    {
                        _parent = parent;
                        _row = row;
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.BlobHeap? blobHeap, MethodDefTable? methodDefTable, MemberRefTable? memberRefTable, BinaryReader reader)
#else
                    internal void Load(Heap.BlobHeap blobHeap, MethodDefTable methodDefTable, MemberRefTable memberRefTable,  BinaryReader reader)
#endif
                    {
                        uint methodIndex;
                        if (MethodDefOrRefLargeIndices(methodDefTable, memberRefTable)) { methodIndex = reader.ReadUInt32(); } else { methodIndex = reader.ReadUInt16(); }
                        _method = MethodDefOrRefDecode(methodIndex, methodDefTable, memberRefTable);
                        uint blobIndex;
                        if (blobHeap.LargeIndices) { blobIndex = reader.ReadUInt32(); } else { blobIndex = reader.ReadUInt16(); }
                        _instantiation = new Heap.BlobHeap.Blob(blobHeap, blobIndex);
                    }
#if NET6_0_OR_GREATER

                    internal void Load(Heap.BlobHeap? blobHeap, MethodDefTable? methodDefTable, MemberRefTable? memberRefTable, Span<byte> data, ref uint offset)
                    {
                        uint methodIndex;
                        if (MethodDefOrRefLargeIndices(methodDefTable, memberRefTable)) { methodIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { methodIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _method = MethodDefOrRefDecode(methodIndex, methodDefTable, memberRefTable);
                        uint blobIndex;
                        if (blobHeap.LargeIndices) { blobIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { blobIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _instantiation = new Heap.BlobHeap.Blob(blobHeap, blobIndex);
                    }
#endif

#if NET6_0_OR_GREATER
                    internal void Save(MethodDefTable? methodDefTable, MemberRefTable? memberRefTable, BinaryWriter binaryWriter)
#else
                    internal void Save(MethodDefTable methodDefTable, MemberRefTable memberRefTable,  BinaryWriter binaryWriter)
#endif
                    {
                        uint methodIndex = MethodDefOrRefEncode(_method);
                        if (MethodDefOrRefLargeIndices(methodDefTable, memberRefTable)) { binaryWriter.Write((uint)methodIndex); } else { binaryWriter.Write((ushort)methodIndex); }
                        if (_instantiation.Heap.LargeIndices) { binaryWriter.Write((uint)_instantiation.Index); } else { binaryWriter.Write((ushort)_instantiation.Index); }
                    }
                }
#if NET6_0_OR_GREATER
                internal void Save(MethodDefTable? methodDefTable, MemberRefTable? memberRefTable, BinaryWriter binaryWriter)
#else
                internal void Save(MethodDefTable methodDefTable, MemberRefTable memberRefTable, BinaryWriter binaryWriter)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(methodDefTable, memberRefTable, binaryWriter);
                    }
                }
                public MethodSpecTable()
                {
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.BlobHeap blobHeap, MethodDefTable? methodDefTable, MemberRefTable? memberRefTable, BinaryReader reader)
#else
                internal void Load(Heap.BlobHeap blobHeap, MethodDefTable methodDefTable, MemberRefTable memberRefTable, BinaryReader reader)
#endif
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(blobHeap, methodDefTable, memberRefTable, reader); }
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.BlobHeap blobHeap, MethodDefTable? methodDefTable, MemberRefTable? memberRefTable, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(blobHeap, methodDefTable, memberRefTable, data, ref offset); }
                }
#endif
                internal MethodSpecTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new MethodSpecTableRow(this, (uint)(_rows.Count + 1)));
                    }
                }
            }
        }
    }
}
