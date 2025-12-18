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
            public class MethodSemanticsTable : MetadataTable
            {
                List<MethodSemanticsTableRow> _rows = new List<MethodSemanticsTableRow>();
                public override int ID { get { return 0x18; } }
                public override uint Columns { get { return 3; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public MethodSemanticsTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public MethodSemanticsTableRow Add(MethodSemanticsAttributes attributes, MethodDefTable.MethodDefTableRow method, IHasSemantics association)
                {
                    lock (this)
                    {
                        MethodSemanticsTableRow row = new MethodSemanticsTableRow(this, (uint)(_rows.Count + 1), attributes, method, association);
                        _rows.Add(row);
                        return row;
                    }
                }
                public class MethodSemanticsTableRow : MetadataTableRow, IHasCustomAttribute
                {
                    MethodSemanticsTable _parent;
                    internal MethodSemanticsTable Parent { get { return _parent; } }
                    MethodDefTable.MethodDefTableRow _method;
                    public MethodDefTable.MethodDefTableRow Method { get { return _method; } }
                    public override uint Length { get { return 3; } }
                    MethodSemanticsAttributes _attributes;
                    public MethodSemanticsAttributes Attributes { get { return _attributes; } internal set { _attributes = value; } }
                    IHasSemantics _association;
                    public IHasSemantics Association { get { return _association; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    internal MethodSemanticsTableRow(MethodSemanticsTable parent, uint row, MethodSemanticsAttributes attributes, MethodDefTable.MethodDefTableRow method, IHasSemantics association)
                    {
                        _parent = parent;
                        _row = row;
                        _attributes = attributes;
                        _method = method;
                        _association = association;
                    }
                    internal MethodSemanticsTableRow(MethodSemanticsTable parent, uint row)
                    {
                        _parent = parent;
                        _row = row;
                    }
#if NET6_0_OR_GREATER
                    internal void Load(MethodDefTable methodDefTable, EventTable? eventTable, PropertyTable? propertyTable, BinaryReader reader)
#else
                    internal void Load(MethodDefTable methodDefTable, EventTable eventTable, PropertyTable propertyTable, BinaryReader reader)
#endif
                    {
                        _attributes = (MethodSemanticsAttributes)reader.ReadUInt16();
                        uint methodIndex = 0;
                        if (methodDefTable.LargeIndices) { methodIndex = reader.ReadUInt32(); } else { methodIndex = reader.ReadUInt16(); }
                        _method = methodDefTable[methodIndex];
                        uint associationTag = 0;
                        if (HasSemanticsDecodeLargeIndices(eventTable, propertyTable)) { associationTag = reader.ReadUInt32(); } else { associationTag = reader.ReadUInt16(); }
                        _association = HasSemanticsDecode(associationTag, eventTable, propertyTable);
                    }
#if NET6_0_OR_GREATER

                    internal void Load(MethodDefTable methodDefTable, EventTable? eventTable, PropertyTable? propertyTable, Span<byte> data, ref uint offset)
                    {
                        _attributes = (MethodSemanticsAttributes)BitConverterLE.ToUInt16(data, offset); offset += 2;
                        uint methodIndex = 0;
                        if (methodDefTable.LargeIndices) { methodIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { methodIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _method = methodDefTable[methodIndex];
                        uint associationTag = 0;
                        if (HasSemanticsDecodeLargeIndices(eventTable, propertyTable)) { associationTag = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { associationTag = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _association = HasSemanticsDecode(associationTag, eventTable, propertyTable);

                    }
#endif

#if NET6_0_OR_GREATER
                    internal void Save(EventTable? eventTable, PropertyTable? propertyTable, BinaryWriter binaryWriter)
#else
                    internal void Save(EventTable eventTable, PropertyTable propertyTable, BinaryWriter binaryWriter)
#endif
                    {
                        binaryWriter.Write((ushort)_attributes);
                        if (_method.Parent.LargeIndices) { binaryWriter.Write(_method.Row); } else { binaryWriter.Write((ushort)_method.Row); }
                        uint associationTag = HasSemanticsEncode(_association);
                        if (HasSemanticsDecodeLargeIndices(eventTable, propertyTable)) { binaryWriter.Write(associationTag); } else { binaryWriter.Write((ushort)associationTag); }
                    }
                }
#if NET6_0_OR_GREATER
                internal void Save(EventTable? eventTable, PropertyTable? propertyTable, BinaryWriter binaryWriter)
#else
                internal void Save(EventTable eventTable, PropertyTable propertyTable, BinaryWriter binaryWriter)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(eventTable, propertyTable, binaryWriter);
                    }
                }
                public MethodSemanticsTable()
                {
                }
#if NET6_0_OR_GREATER
                internal void Load(MethodDefTable methodDefTable, EventTable? eventTable, PropertyTable? propertyTable, BinaryReader reader)
#else
                internal void Load(MethodDefTable methodDefTable, EventTable eventTable, PropertyTable propertyTable, BinaryReader reader)
#endif
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(methodDefTable, eventTable, propertyTable, reader); }
                }
#if NET6_0_OR_GREATER
                internal void Load(MethodDefTable methodDefTable, EventTable? eventTable, PropertyTable? propertyTable, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(methodDefTable, eventTable, propertyTable, data, ref offset); }
                }
#endif
                internal MethodSemanticsTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new MethodSemanticsTableRow(this, (uint)(_rows.Count + 1)));
                    }
                }
            }
        }
    }
}
