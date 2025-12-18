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
            public class ConstantTable : MetadataTable
            {
                List<ConstantTableRow> _rows = new List<ConstantTableRow>();
                public override int ID { get { return 0x0B; } }
                public override uint Columns { get { return 3; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public ConstantTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public ConstantTableRow Add(byte type, IHasConstant parent, Heap.BlobHeap.Blob value)
                {
                    lock (this)
                    {
                        ConstantTableRow row = new ConstantTableRow((uint)(_rows.Count + 1), type, parent, value);
                        _rows.Add(row);
                        return row;
                    }
                }
                public class ConstantTableRow : MetadataTableRow
                {

                    public override uint Length { get { return 3; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    byte _type;
                    public byte Type { get { return _type; } }
                    IHasConstant _parent;
                    public IHasConstant Parent { get { return _parent; } }
                    Heap.BlobHeap.Blob _value;
                    public Heap.BlobHeap.Blob Value { get { return _value; } }
                    internal ConstantTableRow(uint row, byte type, IHasConstant parent, Heap.BlobHeap.Blob value)
                    {
                        _parent = parent;
                        _row = row;
                        _type = type;
                        _parent = parent;
                        _value = value;
                    }
                    internal ConstantTableRow(uint row)
                    {
                        _row = row;
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.BlobHeap blobHeap, FieldTable? fieldTable, ParamTable? paramTable, PropertyTable? propertyTable, BinaryReader reader)
#else
                    internal void Load(Heap.BlobHeap blobHeap, FieldTable fieldTable, ParamTable paramTable, PropertyTable propertyTable, BinaryReader reader)
#endif
                    {
                        _type = reader.ReadByte(); reader.ReadByte();
                        uint parentIndex;
                        if (HasConstantLargeIndices(fieldTable, paramTable, propertyTable)) { parentIndex = reader.ReadUInt32(); } else { parentIndex = reader.ReadUInt16(); }
                        _parent = HasConstantDecode(parentIndex, fieldTable, paramTable, propertyTable);
                        uint valueIndex;
                        if (blobHeap.LargeIndices) { valueIndex = reader.ReadUInt32(); } else { valueIndex = reader.ReadUInt16(); }
                        _value = new Heap.BlobHeap.Blob(blobHeap, valueIndex);
                    }
#if NET6_0_OR_GREATER

                    internal void Load(Heap.BlobHeap blobHeap, FieldTable? fieldTable, ParamTable? paramTable, PropertyTable? propertyTable, Span<byte> data, ref uint offset)
                    {
                        _type = data[(int)offset]; offset += 2;
                        uint parentIndex;
                        if (HasConstantLargeIndices(fieldTable, paramTable, propertyTable)) { parentIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { parentIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _parent = HasConstantDecode(parentIndex, fieldTable, paramTable, propertyTable);
                        uint valueIndex;
                        if (blobHeap.LargeIndices) { valueIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { valueIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _value = new Heap.BlobHeap.Blob(blobHeap, valueIndex);
                    }
#endif

#if NET6_0_OR_GREATER
                    internal void Save(FieldTable? fieldTable, ParamTable? paramTable, PropertyTable? propertyTable, BinaryWriter binaryWriter)
#else
                    internal void Save(FieldTable fieldTable, ParamTable paramTable, PropertyTable propertyTable, BinaryWriter binaryWriter)
#endif
                    {
                        binaryWriter.Write((byte)_type);
                        binaryWriter.Write((byte)0);
                        uint parentIndex = HasConstantEncode(_parent);
                        if (HasConstantLargeIndices(fieldTable, paramTable, propertyTable)) { binaryWriter.Write(parentIndex); } else { binaryWriter.Write((ushort)parentIndex); }
                        if (_value.Heap.LargeIndices) { binaryWriter.Write(_value.Index); } else { binaryWriter.Write((ushort)_value.Index); }
                    }
                }
#if NET6_0_OR_GREATER
                internal void Save(FieldTable? fieldTable, ParamTable? paramTable, PropertyTable? propertyTable, BinaryWriter binaryWriter)
#else
                internal void Save(FieldTable fieldTable, ParamTable paramTable, PropertyTable propertyTable, BinaryWriter binaryWriter)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(fieldTable, paramTable, propertyTable, binaryWriter);
                    }
                }
                public ConstantTable()
                {
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.BlobHeap blobHeap, FieldTable? fieldTable, ParamTable? paramTable, PropertyTable? propertyTable, BinaryReader reader)
#else
                internal void Load(Heap.BlobHeap blobHeap,FieldTable fieldTable, ParamTable paramTable, PropertyTable propertyTable, BinaryReader reader)
#endif
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(blobHeap, fieldTable, paramTable, propertyTable, reader); }
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.BlobHeap blobHeap, FieldTable? fieldTable, ParamTable? paramTable, PropertyTable? propertyTable, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(blobHeap, fieldTable, paramTable, propertyTable, data, ref offset); }
                }
#endif
                internal ConstantTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new ConstantTableRow((uint)(_rows.Count + 1)));
                    }
                }
            }
        }
    }
}
