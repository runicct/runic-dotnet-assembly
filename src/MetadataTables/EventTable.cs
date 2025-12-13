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
using System.Diagnostics.Tracing;
using System.IO;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public partial class MetadataTable
        {
            public class EventTable : MetadataTable
            {
                List<EventTableRow> _rows = new List<EventTableRow>();
                public override int ID { get { return 0x14; } }
                public override uint Columns { get { return 3; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public EventTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public EventTableRow Add(EventAttributes attributes, Heap.StringHeap.String name, ITypeDefOrRefOrSpec type)
                {
                    lock (this)
                    {
                        EventTableRow row = new EventTableRow(this, (uint)(_rows.Count + 1), attributes, name, type);
                        _rows.Add(row);
                        return row;
                    }
                }
                public class EventTableRow : MetadataTableRow, IHasCustomAttribute, IHasSemantics
                {
                    EventTable _parent;
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    ITypeDefOrRefOrSpec _type;
                    public ITypeDefOrRefOrSpec Type { get { return _type; } }
                    public override uint Length { get { return 3; } }
                    EventAttributes _attributes;
                    public EventAttributes Attributes { get { return _attributes; } internal set { _attributes = value; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    internal EventTableRow(EventTable parent, uint row, EventAttributes attributes, Heap.StringHeap.String name, ITypeDefOrRefOrSpec type)
                    {
                        _parent = parent;
                        _name = name;
                        _row = row;
                        _attributes = attributes;
                        _type = type;
                    }
                    internal EventTableRow(EventTable parent, uint row)
                    {
                        _parent = parent;
                        _row = row;
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.StringHeap stringHeap, TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, BinaryReader reader)
#else
                    internal void Load(Heap.StringHeap stringHeap, TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable, BinaryReader reader)
#endif
                    {
                        _attributes = (EventAttributes)reader.ReadUInt16();
                        uint nameIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        uint typeIndex = 0;
                        if (TypeDefOrRefOrSpecLargeIndices(typeDefTable, typeRefTable, typeSpecTable)) { typeIndex = reader.ReadUInt32(); } else { typeIndex = reader.ReadUInt16(); }
                        _type = TypeDefOrRefOrSpecDecode(typeIndex, typeDefTable, typeRefTable, typeSpecTable);
                    }
#if NET6_0_OR_GREATER

                    internal void Load(Heap.StringHeap stringHeap, TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, Span<byte> data, ref uint offset)
                    {
                        _attributes = (EventAttributes)BitConverterLE.ToUInt16(data, offset); offset += 2;
                        uint nameIndex = 0; if (stringHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        uint typeIndex = 0; if (TypeDefOrRefOrSpecLargeIndices(typeDefTable, typeRefTable, typeSpecTable)) { typeIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { typeIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _type = TypeDefOrRefOrSpecDecode(typeIndex, typeDefTable, typeRefTable, typeSpecTable);
                    }
#endif
#if NET6_0_OR_GREATER
                    internal void Save(TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, BinaryWriter binaryWriter)
#else
                    internal void Save(TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable, BinaryWriter binaryWriter)
#endif
                    {
                        binaryWriter.Write((ushort)_attributes);
                        if (_name.Heap.LargeIndices) { binaryWriter.Write(_name.Index); } else { binaryWriter.Write((short)_name.Index); }
                        uint typeIndex = TypeDefOrRefOrSpecEncode(_type);
                        if (TypeDefOrRefOrSpecLargeIndices(typeDefTable, typeRefTable, typeSpecTable)) { binaryWriter.Write(typeIndex); } else { binaryWriter.Write((ushort)typeIndex); }
                    }
                }
#if NET6_0_OR_GREATER
                internal void Save(TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, BinaryWriter binaryWriter)
#else
                internal void Save(TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable, BinaryWriter binaryWriter)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(typeDefTable, typeRefTable, typeSpecTable, binaryWriter);
                    }
                }
                public EventTable()
                {
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.StringHeap stringHeap, TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, BinaryReader reader)
#else
                internal void Load(Heap.StringHeap stringHeap, TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable, BinaryReader reader)
#endif
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(stringHeap, typeDefTable, typeRefTable, typeSpecTable, reader); }
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.StringHeap stringHeap, TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(stringHeap, typeDefTable, typeRefTable, typeSpecTable, data, ref offset); }
                }
#endif
                internal EventTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new EventTableRow(this, (uint)(_rows.Count + 1)));
                    }
                }
            }
        }
    }
}
