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
            public class EventMapTable : MetadataTable
            {
                List<EventMapTableRow> _rows = new List<EventMapTableRow>();
                public override int ID { get { return 0x12; } }
                public override uint Columns { get { return 2; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public EventMapTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public EventMapTableRow Add(TypeDefTable.TypeDefTableRow parent, EventTable.EventTableRow eventList)
                {
                    lock (this)
                    {
                        EventMapTableRow row = new EventMapTableRow(parent, eventList);
                        _rows.Add(row);
                        return row;
                    }
                }
                public class EventMapTableRow : MetadataTableRow
                {
                    TypeDefTable.TypeDefTableRow _parent;
                    public TypeDefTable.TypeDefTableRow Parent { get { return _parent; } }
                    EventTable.EventTableRow _eventList;
                    public EventTable.EventTableRow EventList { get { return _eventList; } internal set { _eventList = value; } }
                    public override uint Length { get { return 2; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    internal EventMapTableRow(TypeDefTable.TypeDefTableRow parent, EventTable.EventTableRow eventList)
                    {
                        _parent = parent;
                        _eventList = eventList;
                    }
                    internal EventMapTableRow(uint row)
                    {
                        _row = row;
                    }
                    internal void Load(TypeDefTable typeDefTable, EventTable eventTable, BinaryReader reader)
                    {
                        uint parentIndex = 0;
                        if (typeDefTable.LargeIndices) { parentIndex = reader.ReadUInt32(); } else { parentIndex = reader.ReadUInt16(); }
                        _parent = typeDefTable[(uint)parentIndex];
                        uint eventListIndex = 0;
                        if (eventTable.LargeIndices) { eventListIndex = reader.ReadUInt32(); } else { eventListIndex = reader.ReadUInt16(); }
                        _eventList = eventTable[(uint)eventListIndex];
                    }
#if NET6_0_OR_GREATER

                    internal void Load(TypeDefTable typeDefTable, EventTable eventTable, Span<byte> data, ref uint offset)
                    {
                        uint parentIndex = 0;
                        if (typeDefTable.LargeIndices) { parentIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { parentIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _parent = typeDefTable[(uint)parentIndex];
                        uint eventListIndex = 0;
                        if (eventTable.LargeIndices) { eventListIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { eventListIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _eventList = eventTable[(uint)eventListIndex];
                    }
#endif
                    internal void Save(TypeDefTable typeDefTable, EventTable eventTable, BinaryWriter binaryWriter)
                    {
                        if (typeDefTable.LargeIndices) { binaryWriter.Write((uint)_parent.Row); } else { binaryWriter.Write((ushort)_parent.Row); }
                        if (eventTable.LargeIndices) { binaryWriter.Write((uint)_eventList.Row); } else { binaryWriter.Write((ushort)_eventList.Row); }
                    }
                }
                internal void Save(TypeDefTable typeDefTable, EventTable eventTable, BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(typeDefTable, eventTable, binaryWriter);
                    }
                }
                public EventMapTable()
                {
                }
                internal void Load(TypeDefTable typeDefTable, EventTable eventTable, BinaryReader reader)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(typeDefTable, eventTable, reader); }
                }
#if NET6_0_OR_GREATER
                internal void Load(TypeDefTable typeDefTable, EventTable eventTable, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(typeDefTable, eventTable, data, ref offset); }
                }
#endif
                internal EventMapTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new EventMapTableRow((uint)(_rows.Count + 1)));
                    }
                }
            }
        }
    }
}
