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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public partial class MetadataTable
        {
            public class TypeDefTable : MetadataTable
            {
                List<TypeDefTableRow> _rows = new List<TypeDefTableRow>();
                public override int ID { get { return 0x02; } }
                public override uint Columns { get { return 6; } }
                public override uint Rows { get { lock (this) { return (uint)_rows.Count; } } }
                public override bool Sorted { get { return false; } }
                public TypeDefTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public class TypeDefTableRow : MetadataTableRow, ITypeDefOrRefOrSpec, IMemberRefParent, IHasCustomAttribute, ITypeDefOrMethodDef, IHasDeclSecurity
                {
                    uint _row;
                    public override uint Row { get { return _row; } }
                    TypeDefTable _parent;
                    internal TypeDefTable Parent { get { return _parent; } }
                    TypeAttributes _attributes;
                    public TypeAttributes Attributes { get { return _attributes; } internal set { _attributes = value; } }
#if NET6_0_OR_GREATER
                    ITypeDefOrRefOrSpec? _parentType = null;
                    public ITypeDefOrRefOrSpec? ParentType { get { return _parentType; } internal set { _parentType = value; } }
                    Heap.StringHeap.String? _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    Heap.StringHeap.String? _namespace;
                    public Heap.StringHeap.String Namespace { get { return _namespace; } }
#else
                    ITypeDefOrRefOrSpec _parentType = null;
                    public ITypeDefOrRefOrSpec ParentType { get { return _parentType; } internal set { _parentType = value; } }
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    Heap.StringHeap.String _namespace;
                    public Heap.StringHeap.String Namespace { get { return _namespace; } }

#endif
                    FieldTable.FieldTableRow _fieldList;
                    public FieldTable.FieldTableRow FieldList { get { return _fieldList; } internal set { _fieldList = value; } }
                    /// <summary>
                    /// Return the next FieldTableRow after this type's last field, or null if this is the last type with fields.
                    /// </summary>
#if NET6_0_OR_GREATER
                    public FieldTable.FieldTableRow? FieldListEnd
#else
                    public FieldTable.FieldTableRow FieldListEnd
#endif
                    {
                        get
                        {
                            for (uint n = _row + 1; n <= _parent.Rows; n++)
                            {
                                TypeDefTableRow typeDefRow = _parent[n];
                                if (typeDefRow.FieldList != null)
                                {
                                    return typeDefRow.FieldList;
                                }
                            }
                            return null;
                        }
                    }
                    MethodDefTable.MethodDefTableRow _methodList;
                    public MethodDefTable.MethodDefTableRow MethodList { get { return _methodList; } internal set { _methodList = value; } }
                    /// <summary>
                    /// Return the next MethodDefTableRow after this type's last method, or null if this is the last type with methods.
                    /// </summary>
#if NET6_0_OR_GREATER
                    public MethodDefTable.MethodDefTableRow? MethodListEnd
#else
                    public MethodDefTable.MethodDefTableRow MethodListEnd
#endif
                    {
                        get
                        {
                            for (uint n = _row + 1; n <= _parent.Rows; n++)
                            {
                                TypeDefTableRow typeDefRow = _parent[n];
                                if (typeDefRow.MethodList != null)
                                {
                                    return typeDefRow.MethodList;
                                }
                            }
                            return null;
                        }
                    }
                    public override uint Length { get { return 6; } }
                    internal TypeDefTableRow(TypeDefTable parent, uint row, Heap.StringHeap.String name, Heap.StringHeap.String @namespace, TypeAttributes attributes, ITypeDefOrRefOrSpec parentType, FieldTable.FieldTableRow fieldList, MethodDefTable.MethodDefTableRow methodList)
                    {
                        _row = row;
                        _attributes = attributes;
                        _name = name;
                        _namespace = @namespace;
                        _parent = parent;
                        _parentType = parentType;
                        _fieldList = fieldList;
                        _methodList = methodList;
                    }
                    internal TypeDefTableRow(TypeDefTable parent, uint row)
                    {
                        _row = row;
                        _parent = parent;
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.StringHeap stringHeap, FieldTable fieldTable, MethodDefTable methodDef, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, BinaryReader reader)
#else
                    internal void Load(Heap.StringHeap stringHeap, FieldTable fieldTable, MethodDefTable methodDef, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable, BinaryReader reader)
#endif
                    {
                        _attributes = (TypeAttributes)reader.ReadUInt32();
                        uint nameIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        uint namespaceIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        uint typeDefOrTypeRefTag = 0;
                        if (TypeDefOrRefOrSpecLargeIndices(_parent, typeRefTable, typeSpecTable)) { typeDefOrTypeRefTag = reader.ReadUInt32(); } else { typeDefOrTypeRefTag = reader.ReadUInt16(); }
                        if (typeDefOrTypeRefTag == 0) { _parentType = null; }
                        else
                        {
                            _parentType = TypeDefOrRefOrSpecDecode(typeDefOrTypeRefTag, _parent, typeRefTable, typeSpecTable);
                        }
                        uint fieldList = fieldTable.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _fieldList =  fieldTable[fieldList];
                        uint methodList = methodDef.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _methodList = methodDef[fieldList];
                        if (nameIndex == 0) { _name = null; } else { _name = new Heap.StringHeap.String(stringHeap, nameIndex); }
                        if (namespaceIndex == 0) { _namespace = null; } else { _namespace = new Heap.StringHeap.String(stringHeap, namespaceIndex); }
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.StringHeap stringHeap, FieldTable fieldTable, MethodDefTable methodDef, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, Span<byte> data, ref uint offset)
                    {
                        _attributes = (TypeAttributes)BitConverterLE.ToUInt32(data, offset); offset += 4;
                        uint nameIndex = 0; if (stringHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        uint namespaceIndex = 0; if (stringHeap.LargeIndices) { namespaceIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { namespaceIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        uint typeDefOrTypeRefTag = 0;
                        if (TypeDefOrRefOrSpecLargeIndices(_parent, typeRefTable, typeSpecTable)) { typeDefOrTypeRefTag = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { typeDefOrTypeRefTag = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        if (typeDefOrTypeRefTag == 0) { _parentType = null; }
                        else
                        {
                            _parentType = TypeDefOrRefOrSpecDecode(typeDefOrTypeRefTag, _parent, typeRefTable, typeSpecTable);
                        }
                        uint fieldList = 0; if (fieldTable.LargeIndices) { fieldList = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { fieldList = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        if (fieldList == 0 || (fieldList - 1) >= fieldTable.Rows) { _fieldList = null; } else { _fieldList = fieldTable[fieldList]; }
                        uint methodList = 0; if (methodDef.LargeIndices) { methodList = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { methodList = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        if (methodList == 0 || (methodList - 1) >= methodDef.Rows) { _methodList = null; } else { _methodList = methodDef[methodList]; }
                        if (nameIndex == 0) { _name = null; } else { _name = new Heap.StringHeap.String(stringHeap, nameIndex); }
                        if (namespaceIndex == 0) { _namespace = null; } else { _namespace = new Heap.StringHeap.String(stringHeap, namespaceIndex); }
                    }
#endif

#if NET6_0_OR_GREATER
                    internal void Save(FieldTable? fieldTable, MethodDefTable? methodDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, BinaryWriter binaryWriter)
#else
                    internal void Save(FieldTable fieldTable, MethodDefTable methodDefTable,  TypeRefTable typeRefTable, TypeSpecTable typeSpecTable, BinaryWriter binaryWriter)
#endif
                    {
                        binaryWriter.Write((uint)_attributes);
                        if (_name.Heap.LargeIndices) { binaryWriter.Write(_name.Index); } else { binaryWriter.Write((ushort)_name.Index); }
                        if (_namespace.Heap.LargeIndices) { binaryWriter.Write(_namespace.Index); } else { binaryWriter.Write((ushort)_namespace.Index); }
                        uint index = TypeDefOrRefOrSpecEncode(_parentType);
                        if (TypeDefOrRefOrSpecLargeIndices(_parent, typeRefTable, typeSpecTable)) { binaryWriter.Write(index); } else { binaryWriter.Write((ushort)index); }
                        if (_fieldList == null)
                        {
#if NET6_0_OR_GREATER
                            FieldTable.FieldTableRow? fieldListEnd = FieldListEnd;
#else
                            FieldTable.FieldTableRow fieldListEnd = FieldListEnd;
#endif
                            if (fieldListEnd != null)
                            {
                                if (fieldListEnd.Parent.LargeIndices) { binaryWriter.Write((uint)(fieldListEnd.Row)); } else { binaryWriter.Write((ushort)(fieldListEnd.Row)); }
                            }
                            else
                            {
                                if (fieldTable == null) { binaryWriter.Write((ushort)(1)); }
                                else if (fieldTable.LargeIndices) { binaryWriter.Write((uint)(fieldTable.Rows + 1)); } else { binaryWriter.Write((ushort)(fieldTable.Rows + 1)); }
                            }
                        }
                        else
                        {
                            if (_fieldList.Parent.LargeIndices) { binaryWriter.Write((uint)_fieldList.Row); }
                            else { binaryWriter.Write((ushort)_fieldList.Row); }
                        }

                        if (_methodList == null)
                        {
#if NET6_0_OR_GREATER
                            MethodDefTable.MethodDefTableRow? methodDefTableRow = MethodListEnd;
#else
                            MethodDefTable.MethodDefTableRow methodDefTableRow = MethodListEnd;
#endif
                            if (methodDefTableRow != null)
                            {
                                if (methodDefTableRow.Parent.LargeIndices) { binaryWriter.Write((uint)(methodDefTableRow.Row)); } else { binaryWriter.Write((ushort)(methodDefTableRow.Row)); }
                            }
                            else
                            {
                                if (methodDefTable == null) { binaryWriter.Write((ushort)(1)); }
                                else if (methodDefTable.LargeIndices) { binaryWriter.Write((uint)(methodDefTable.Rows + 1)); } else { binaryWriter.Write((ushort)(methodDefTable.Rows + 1)); }
                            }
                        }
                        else
                        {
                            if (_methodList.Parent.LargeIndices) { binaryWriter.Write((uint)_methodList.Row); } else { binaryWriter.Write((ushort)_methodList.Row); }
                        }
                    }
                }
#if NET6_0_OR_GREATER
                internal void Save(FieldTable? fieldTable, MethodDefTable? methodDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, BinaryWriter binaryWriter)
#else
                internal void Save(FieldTable fieldTable, MethodDefTable methodDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable,  BinaryWriter binaryWriter)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(fieldTable, methodDefTable, typeRefTable, typeSpecTable, binaryWriter);
                    }
                }
                public TypeDefTableRow Add(Heap.StringHeap.String name, Heap.StringHeap.String @namespace, TypeAttributes attributes, ITypeDefOrRefOrSpec parentType, FieldTable.FieldTableRow fieldList, MethodDefTable.MethodDefTableRow methodList)
                {
                    lock (this)
                    {
                        TypeDefTableRow row = new TypeDefTableRow(this, (uint)(_rows.Count + 1), name, @namespace, attributes, parentType, fieldList, methodList);
                        _rows.Add(row);
                        return row;
                    }
                }
                public TypeDefTable()
                {
                }
#if NET6_0_OR_GREATER
                internal TypeDefTable(uint rows, Heap.StringHeap stringHeap, FieldTable fieldTable, MethodDefTable methodDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, BinaryReader reader)
#else
                internal TypeDefTable(uint rows, Heap.StringHeap stringHeap, FieldTable fieldTable, MethodDefTable methodDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable, BinaryReader reader)
#endif
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new TypeDefTableRow(this, (uint)(n + 1)));
                    }
                    for (int n = 0; n < rows; n++)
                    {
                        _rows[n].Load(stringHeap, fieldTable, methodDefTable, typeRefTable, typeSpecTable, reader);
                    }
                }
#if NET6_0_OR_GREATER
                internal TypeDefTable(uint rows, Heap.StringHeap stringHeap, FieldTable fieldTable, MethodDefTable methodDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, Span<byte> data, ref uint offset)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new TypeDefTableRow(this, (uint)(n + 1)));
                    }
                    for (int n = 0; n < rows; n++)
                    {
                        _rows[n].Load(stringHeap, fieldTable, methodDefTable, typeRefTable, typeSpecTable, data, ref offset);
                    }
                }
#endif
        }
    }
    }
}
