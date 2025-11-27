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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public partial class MetadataTable
        {
            public class ParamTable : MetadataTable
            {
                List<ParamTableRow> _rows = new List<ParamTableRow>();
                public override int ID { get { return 0x08; } }
                public override uint Columns { get { return 3; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public ParamTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public ParamTableRow Add(ParamAttributes attributes, Heap.StringHeap.String name, int sequence)
                {
                    lock (this)
                    {
                        if (name == null) { throw new System.ArgumentException("Parameter name must be a valid string"); }
                        ParamTableRow row = new ParamTableRow(this, (uint)(_rows.Count + 1), attributes, name, sequence);
                        _rows.Add(row);
                        return row;
                    }
                }
                public class ParamTableRow : MetadataTableRow
                {
                    ParamTable _parent;
                    public ParamTable Parent { get { return _parent; } }
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    public override uint Length { get { return 3; } }
                    int _sequence;
                    public int Sequence { get { return _sequence; } }
                    ParamAttributes _attributes;
                    public ParamAttributes Attributes { get { return _attributes; } internal set { _attributes = value; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    public ParamTableRow(ParamTable parent, uint row, ParamAttributes attributes, Heap.StringHeap.String name, int sequence)
                    {
                        _parent = parent;
                        _sequence = sequence;
                        _name = name;
                        _row = row;
                        _attributes = attributes;
                    }
                    internal ParamTableRow(ParamTable parent, uint row)
                    {
                        _parent = parent;
                        _row = row;
                    }
                    internal void Load(Heap.StringHeap stringHeap, BinaryReader reader)
                    {
                        _attributes = (ParamAttributes)reader.ReadUInt16();
                        _sequence = reader.ReadUInt16();
                        uint nameIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                    }
#if NET6_0_OR_GREATER

                    internal void Load(Heap.StringHeap stringHeap, Span<byte> data, ref uint offset)
                    {
                        _attributes = (ParamAttributes)BitConverterLE.ToUInt16(data, offset); offset += 2;
                        _sequence = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        uint nameIndex = 0; if (stringHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                    }
#endif
                    internal void Save(BinaryWriter binaryWriter)
                    {
                        binaryWriter.Write((ushort)_attributes);
                        binaryWriter.Write((ushort)_sequence);
                        if (_name.Heap.LargeIndices) { binaryWriter.Write(_name.Index); } else { binaryWriter.Write((short)_name.Index); }
                    }
                }
                internal override void Save(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                public ParamTable()
                {
                }
                internal void Load(Heap.StringHeap stringHeap, BinaryReader reader)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(stringHeap, reader); }
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.StringHeap stringHeap, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(stringHeap, data, ref offset); }
                }
#endif
                internal ParamTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new ParamTableRow(this, (uint)(_rows.Count + 1)));
                    }
                }
            }
        }
    }
}
