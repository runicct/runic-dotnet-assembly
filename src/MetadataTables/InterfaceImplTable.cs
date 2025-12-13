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
            public class InterfaceImplTable : MetadataTable
            {
                List<InterfaceImplTableRow> _rows = new List<InterfaceImplTableRow>();
                public override int ID { get { return 0x09; } }
                public override uint Columns { get { return 2; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public InterfaceImplTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public InterfaceImplTableRow Add(TypeDefTable.TypeDefTableRow @class, ITypeDefOrRefOrSpec @interface)
                {
                    lock (this)
                    {
                        InterfaceImplTableRow row = new InterfaceImplTableRow(this, (uint)(_rows.Count + 1), @class, @interface);
                        _rows.Add(row);
                        return row;
                    }
                }
                public class InterfaceImplTableRow : MetadataTableRow, IHasCustomAttribute
                {
                    InterfaceImplTable _parent;
                    internal InterfaceImplTable Parent { get { return _parent; } }
                    public override uint Length { get { return 2; } }
                    TypeDefTable.TypeDefTableRow _class;
                    public TypeDefTable.TypeDefTableRow Class { get { return _class; } }
                    ITypeDefOrRefOrSpec _interface;
                    public ITypeDefOrRefOrSpec Interface { get { return _interface; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    internal InterfaceImplTableRow(InterfaceImplTable parent, uint row, TypeDefTable.TypeDefTableRow cls, ITypeDefOrRefOrSpec @interface)
                    {
                        _parent = parent;
                        _class = cls;
                        _interface = @interface;
                        _row = row;
                    }
                    internal InterfaceImplTableRow(InterfaceImplTable parent, uint row)
                    {
                        _parent = parent;
                        _row = row;
                    }
#if NET6_0_OR_GREATER
                    internal void Load(TypeDefTable typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, BinaryReader reader)
#else
                    internal void Load(TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable, BinaryReader reader)
#endif
                    {
                        uint classIndex = 0;
                        if (typeDefTable.LargeIndices) { classIndex = reader.ReadUInt32(); } else { classIndex = reader.ReadUInt16(); }
                        _class = typeDefTable[classIndex];
                        uint interfaceIndex = 0;
                        if (TypeDefOrRefOrSpecLargeIndices(typeDefTable, typeRefTable, typeSpecTable)) { interfaceIndex = reader.ReadUInt32(); } else { interfaceIndex = reader.ReadUInt16(); }
                        _interface = TypeDefOrRefOrSpecDecode(interfaceIndex, typeDefTable, typeRefTable, typeSpecTable);
                    }
#if NET6_0_OR_GREATER

                    internal void Load(TypeDefTable typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, Span<byte> data, ref uint offset)
                    {
                        uint classIndex = 0;
                        if (typeDefTable.LargeIndices) { classIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { classIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _class = typeDefTable[classIndex];
                        uint interfaceIndex = 0;
                        if (TypeDefOrRefOrSpecLargeIndices(typeDefTable, typeRefTable, typeSpecTable)) { interfaceIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { interfaceIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _interface = TypeDefOrRefOrSpecDecode(interfaceIndex, typeDefTable, typeRefTable, typeSpecTable);
                    }
#endif

#if NET6_0_OR_GREATER
                    internal void Save(TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, BinaryWriter binaryWriter)
#else
                    internal void Save(TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable, BinaryWriter binaryWriter)
#endif
                    {
                        if (_class.Parent.LargeIndices) { binaryWriter.Write((uint)_class.Row); } else { binaryWriter.Write((ushort)_class.Row); }
                        uint interfaceIndex = TypeDefOrRefOrSpecEncode(_interface);
                        if (TypeDefOrRefOrSpecLargeIndices(typeDefTable, typeRefTable, typeSpecTable)) { binaryWriter.Write((uint)interfaceIndex); } else { binaryWriter.Write((ushort)interfaceIndex); }
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
                public InterfaceImplTable()
                {
                }
#if NET6_0_OR_GREATER
                internal void Load(TypeDefTable typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, BinaryReader reader)
#else
                internal void Load(TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable, BinaryReader reader)
#endif
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(typeDefTable, typeRefTable, typeSpecTable, reader); }
                }
#if NET6_0_OR_GREATER
                internal void Load(TypeDefTable typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(typeDefTable, typeRefTable, typeSpecTable, data, ref offset); }
                }
#endif
                internal InterfaceImplTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new InterfaceImplTableRow(this, (uint)(_rows.Count + 1)));
                    }
                }
            }
        }
    }
}
