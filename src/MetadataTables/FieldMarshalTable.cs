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
            public class FieldMarshalTable : MetadataTable
            {
                List<FieldMarshalTableRow> _rows = new List<FieldMarshalTableRow>();
                public override int ID { get { return 0x0D; } }
                public override uint Columns { get { return 2; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public FieldMarshalTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public FieldMarshalTableRow Add(IHasFieldMarshal parent, Heap.BlobHeap.Blob nativeType)
                {
                    lock (this)
                    {
                        FieldMarshalTableRow row = new FieldMarshalTableRow((uint)(_rows.Count + 1), parent, nativeType);
                        _rows.Add(row);
                        return row;
                    }
                }
                public class FieldMarshalTableRow : MetadataTableRow
                {

                    public override uint Length { get { return 2; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    IHasFieldMarshal _parent;
                    public IHasFieldMarshal Parent { get { return _parent; } }
                    Heap.BlobHeap.Blob _nativeType;
                    public Heap.BlobHeap.Blob NativeType { get { return _nativeType; } }
                    internal FieldMarshalTableRow(uint row, IHasFieldMarshal parent, Heap.BlobHeap.Blob nativeType)
                    {
                        _parent = parent;
                        _row = row;
                        _parent = parent;
                        _nativeType = nativeType;
                    }
                    internal FieldMarshalTableRow(uint row)
                    {
                        _row = row;
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.BlobHeap blobHeap, FieldTable? fieldTable, ParamTable? paramTable, BinaryReader reader)
#else
                    internal void Load(Heap.BlobHeap blobHeap, FieldTable fieldTable, ParamTable paramTable, BinaryReader reader)
#endif
                    {
                        uint parentIndex;
                        if (HasFieldMarshalLargeIndices(fieldTable, paramTable)) { parentIndex = reader.ReadUInt32(); } else { parentIndex = reader.ReadUInt16(); }
                        _parent = HasFieldMarshalDecode(parentIndex, fieldTable, paramTable);
                        uint valueIndex;
                        if (blobHeap.LargeIndices) { valueIndex = reader.ReadUInt32(); } else { valueIndex = reader.ReadUInt16(); }
                        _nativeType = new Heap.BlobHeap.Blob(blobHeap, valueIndex);
                    }
#if NET6_0_OR_GREATER

                    internal void Load(Heap.BlobHeap blobHeap, FieldTable? fieldTable, ParamTable? paramTable, Span<byte> data, ref uint offset)
                    {
                        uint parentIndex;
                        if (HasFieldMarshalLargeIndices(fieldTable, paramTable)) { parentIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { parentIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _parent = HasFieldMarshalDecode(parentIndex, fieldTable, paramTable);
                        uint valueIndex;
                        if (blobHeap.LargeIndices) { valueIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { valueIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _nativeType = new Heap.BlobHeap.Blob(blobHeap, valueIndex);
                    }
#endif

#if NET6_0_OR_GREATER
                    internal void Save(FieldTable? fieldTable, ParamTable? paramTable, BinaryWriter binaryWriter)
#else
                    internal void Save(FieldTable fieldTable, ParamTable paramTable, BinaryWriter binaryWriter)
#endif
                    {
                        uint parentIndex = HasFieldMarshalEncode(_parent);
                        if (HasFieldMarshalLargeIndices(fieldTable, paramTable)) { binaryWriter.Write(parentIndex); } else { binaryWriter.Write((ushort)parentIndex); }
                        if (_nativeType.Heap.LargeIndices) { binaryWriter.Write(_nativeType.Index); } else { binaryWriter.Write((ushort)_nativeType.Index); }
                    }
                }
#if NET6_0_OR_GREATER
                internal void Save(FieldTable? fieldTable, ParamTable? paramTable, BinaryWriter binaryWriter)
#else
                internal void Save(FieldTable fieldTable, ParamTable paramTable, BinaryWriter binaryWriter)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(fieldTable, paramTable, binaryWriter);
                    }
                }
                public FieldMarshalTable()
                {
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.BlobHeap blobHeap, FieldTable? fieldTable, ParamTable? paramTable, BinaryReader reader)
#else
                internal void Load(Heap.BlobHeap blobHeap,FieldTable fieldTable, ParamTable paramTable, BinaryReader reader)
#endif
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(blobHeap, fieldTable, paramTable, reader); }
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.BlobHeap blobHeap, FieldTable? fieldTable, ParamTable? paramTable, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(blobHeap, fieldTable, paramTable, data, ref offset); }
                }
#endif
                internal FieldMarshalTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new FieldMarshalTableRow((uint)(_rows.Count + 1)));
                    }
                }
            }
        }
    }
}
