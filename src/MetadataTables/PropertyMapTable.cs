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
            public class PropertyMapTable : MetadataTable
            {
                List<PropertyMapTableRow> _rows = new List<PropertyMapTableRow>();
                public override int ID { get { return 0x15; } }
                public override uint Columns { get { return 2; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public PropertyMapTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public PropertyMapTableRow Add(TypeDefTable.TypeDefTableRow parent, PropertyTable.PropertyTableRow propertyList)
                {
                    lock (this)
                    {
                        PropertyMapTableRow row = new PropertyMapTableRow(parent, propertyList);
                        _rows.Add(row);
                        return row;
                    }
                }
                public class PropertyMapTableRow : MetadataTableRow
                {
                    TypeDefTable.TypeDefTableRow _parent;
                    public TypeDefTable.TypeDefTableRow Parent { get { return _parent; } }
                    PropertyTable.PropertyTableRow _propertyList;
                    public PropertyTable.PropertyTableRow PropertyList { get { return _propertyList; } internal set { _propertyList = value; } }
                    public override uint Length { get { return 2; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    internal PropertyMapTableRow(TypeDefTable.TypeDefTableRow parent, PropertyTable.PropertyTableRow propertyList)
                    {
                        _parent = parent;
                        _propertyList = propertyList;
                    }
                    internal PropertyMapTableRow(uint row)
                    {
                        _row = row;
                    }
                    internal void Load(TypeDefTable typeDefTable, PropertyTable propertyTable, BinaryReader reader)
                    {
                        uint parentIndex = 0;
                        if (typeDefTable.LargeIndices) { parentIndex = reader.ReadUInt32(); } else { parentIndex = reader.ReadUInt16(); }
                        _parent = typeDefTable[(uint)parentIndex];
                        uint propertyListIndex = 0;
                        if (propertyTable.LargeIndices) { propertyListIndex = reader.ReadUInt32(); } else { propertyListIndex = reader.ReadUInt16(); }
                        _propertyList = propertyTable[(uint)propertyListIndex];
                    }
#if NET6_0_OR_GREATER

                    internal void Load(TypeDefTable typeDefTable, PropertyTable propertyTable, Span<byte> data, ref uint offset)
                    {
                        uint parentIndex = 0;
                        if (typeDefTable.LargeIndices) { parentIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { parentIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _parent = typeDefTable[(uint)parentIndex];
                        uint propertyListIndex = 0;
                        if (propertyTable.LargeIndices) { propertyListIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { propertyListIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _propertyList = propertyTable[(uint)propertyListIndex];
                    }
#endif
                    internal void Save(TypeDefTable typeDefTable, PropertyTable propertyTable, BinaryWriter binaryWriter)
                    {
                        if (typeDefTable.LargeIndices) { binaryWriter.Write((uint)_parent.Row); } else { binaryWriter.Write((ushort)_parent.Row); }
                        if (propertyTable.LargeIndices) { binaryWriter.Write((uint)_propertyList.Row); } else { binaryWriter.Write((ushort)_propertyList.Row); }
                    }
                }
                internal void Save(TypeDefTable typeDefTable, PropertyTable propertyTable, BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(typeDefTable, propertyTable, binaryWriter);
                    }
                }
                public PropertyMapTable()
                {
                }
                internal void Load(TypeDefTable typeDefTable, PropertyTable propertyTable, BinaryReader reader)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(typeDefTable, propertyTable, reader); }
                }
#if NET6_0_OR_GREATER
                internal void Load(TypeDefTable typeDefTable, PropertyTable propertyTable, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(typeDefTable, propertyTable, data, ref offset); }
                }
#endif
                internal PropertyMapTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new PropertyMapTableRow((uint)(_rows.Count + 1)));
                    }
                }
            }
        }
    }
}
