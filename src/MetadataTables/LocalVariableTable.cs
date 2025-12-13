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
            public class LocalVariableTable : MetadataTable
            {
                List<LocalVariableTableRow> _rows = new List<LocalVariableTableRow>();
                public class LocalVariableTableRow : MetadataTableRow
                {
                    uint _row;
                    public override uint Row { get { return _row; } }
                    LocalVariableAttributes _attributes = LocalVariableAttributes.None;
                    public LocalVariableAttributes Attributes { get { return _attributes; } }
                    ushort _index = 0;
                    public ushort Index { get { return _index; } }
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    public override uint Length { get { return 0x03; } }
                    LocalVariableTable _parent;
                    internal LocalVariableTable Parent { get { return _parent; } set { _parent = value; } }
                    internal LocalVariableTableRow(LocalVariableTable parent, uint row, LocalVariableAttributes attributes, ushort index, Heap.StringHeap.String name)
                    {
                        _row = row;
                        _parent = parent;
                        _attributes = attributes;
                        _index = index;
                        _name = name;
                    }
                    internal LocalVariableTableRow(LocalVariableTable parent, uint row)
                    {
                        _row = row;
                        _parent = parent;
                    }
                    internal void Load( Heap.StringHeap stringHeap, System.IO.BinaryReader reader)
                    {
                        _attributes = (LocalVariableAttributes)reader.ReadUInt16();
                        _index = reader.ReadUInt16();
                        _name = new Heap.BlobHeap.StringHeap.String(stringHeap, stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16());
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.StringHeap stringHeap, Span<byte> data, ref uint offset)
                    {
                        _attributes = (LocalVariableAttributes)BitConverterLE.ToUInt16(data, offset); offset += 2;
                        _index = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        uint nameIndex = 0; if (stringHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                    }
#endif
                    internal void Save(BinaryWriter binaryWriter)
                    {
                        binaryWriter.Write((ushort)_attributes);
                        binaryWriter.Write((ushort)_index);
                        if (_name.Heap.LargeIndices) { binaryWriter.Write((uint)_name.Index); } else { binaryWriter.Write((ushort)_name.Index); }
                    }
                }
                public override int ID { get { return 0x33; } }
                public override uint Columns { get { return 3; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public LocalVariableTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public override bool Sorted { get { return false; } }
                internal void Save(BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                public LocalVariableTableRow Add(LocalVariableAttributes attributes, ushort index, Heap.StringHeap.String name)
                {
                    lock (this)
                    {
                        LocalVariableTableRow row = new LocalVariableTableRow(this, (uint)(_rows.Count + 1), attributes, index, name);
                        _rows.Add(row);
                        return row;
                    }
                }
                public LocalVariableTable() : base()
                {

                }
                internal LocalVariableTable(uint rows) : base()
                {
                    _rows = new List<LocalVariableTableRow>();
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new LocalVariableTableRow(this, (uint)n));
                    }
                }
                internal void Load(Heap.StringHeap stringHeap, System.IO.BinaryReader reader)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Load(stringHeap, reader);
                    }
                }
#if NET6_0_OR_GREATER

                internal void Load(Heap.StringHeap stringHeap, Span<byte> data, ref uint offset)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Load(stringHeap, data, ref offset);
                    }
                }
#endif
            }
        }
    }
}
