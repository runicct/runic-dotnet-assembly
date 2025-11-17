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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
                public class TypeDefTableRow : MetadataTableRow
                {
                    uint _row;
                    public override uint Row { get { return _row; } }
                    TypeDefTable _parent;
                    TypeAttributes _attributes;
                    public TypeAttributes Attributes { get { return _attributes; } internal set { _attributes = value; } }
#if NET6_0_OR_GREATER
                    Heap.StringHeap.String? _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    Heap.StringHeap.String? _namespace;
                    public Heap.StringHeap.String Namespace { get { return _namespace; } }
#else
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    Heap.StringHeap.String _namespace;
                    public Heap.StringHeap.String Namespace { get { return _namespace; } }

#endif
                    ushort _parentTypeRefToken;
                    public ushort ParentTypeRefToken { get { return _parentTypeRefToken; } }
                    FieldTable.FieldTableRow _fieldList;
                    public FieldTable.FieldTableRow FieldList { get { return _fieldList; } internal set { _fieldList = value; } }
                    MethodDefTable.MethodDefTableRow _methodList;
                    public MethodDefTable.MethodDefTableRow MethodList { get { return _methodList; } internal set { _methodList = value; } }
                    public override uint Length { get { return 6; } }
                    public TypeDefTableRow(TypeDefTable parent, uint row, Heap.StringHeap.String name, Heap.StringHeap.String @namespace, TypeAttributes attributes, ushort parentTypeRefToken, FieldTable.FieldTableRow fieldList, MethodDefTable.MethodDefTableRow methodList)
                    {
                        _row = row;
                        _attributes = attributes;
                        _name = name;
                        _namespace = @namespace;
                        _parent = parent;
                        _parentTypeRefToken = parentTypeRefToken;
                        _fieldList = fieldList;
                        _methodList = methodList;
                    }
                    public TypeDefTableRow(TypeDefTable parent, uint row, Heap.StringHeap stringHeap, FieldTable fieldTable, MethodDefTable methodDef, BinaryReader reader)
                    {
                        _row = row;
                        _parent = parent;
                        _attributes = (TypeAttributes)reader.ReadUInt32();
                        uint nameIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        uint namespaceIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _parentTypeRefToken = reader.ReadUInt16();
                        uint fieldList = fieldTable.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _fieldList =  fieldTable[fieldList];
                        uint methodList = methodDef.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _methodList = methodDef[fieldList];
                        if (nameIndex == 0) { _name = null; } else { _name = new Heap.StringHeap.String(stringHeap, nameIndex); }
                        if (namespaceIndex == 0) { _namespace = null; } else { _namespace = new Heap.StringHeap.String(stringHeap, namespaceIndex); }
                    }
#if NET6_0_OR_GREATER
                    public TypeDefTableRow(TypeDefTable parent, uint row, Heap.StringHeap stringHeap, FieldTable fieldTable, MethodDefTable methodDef, Span<byte> data, ref uint offset)
                    {
                        _row = row;
                        _parent = parent;
                        _attributes = (TypeAttributes)BitConverterLE.ToUInt32(data, offset); offset += 4;
                        uint nameIndex = 0; if (stringHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        uint namespaceIndex = 0; if (stringHeap.LargeIndices) { namespaceIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { namespaceIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _parentTypeRefToken = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        uint fieldList = 0; if (fieldTable.LargeIndices) { fieldList = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { fieldList = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        if (fieldList == 0 || (fieldList - 1) >= fieldTable.Rows) { _fieldList = null; } else { _fieldList = fieldTable[fieldList]; }
                        uint methodList = 0; if (methodDef.LargeIndices) { methodList = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { methodList = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        if (methodList == 0 || (methodList - 1) >= methodDef.Rows) { _methodList = null; } else { _methodList = methodDef[methodList]; }
                        if (nameIndex == 0) { _name = null; } else { _name = new Heap.StringHeap.String(stringHeap, nameIndex); }
                        if (namespaceIndex == 0) { _namespace = null; } else { _namespace = new Heap.StringHeap.String(stringHeap, namespaceIndex); }
                    }
#endif
                    internal void Save(BinaryWriter binaryWriter)
                    {
                        binaryWriter.Write((uint)_attributes);
                        if (_name != null) { binaryWriter.Write(_name.Index); } else { binaryWriter.Write((uint)0); }
                        if (_namespace != null) { binaryWriter.Write(_namespace.Index); } else { binaryWriter.Write((uint)0); }

                        binaryWriter.Write(_parentTypeRefToken);
                        uint fieldList = _fieldList != null ? _fieldList.Row : 0;
                        if (fieldList == 0)
                        {
                            if (_row < _parent.Rows)
                            {
                                fieldList = _parent[(_row + 1)]._fieldList.Row;
                            }
                        }       
                        if (_fieldList.Parent.LargeIndices) { binaryWriter.Write((uint)fieldList);}
                        else { binaryWriter.Write((ushort)fieldList); }

                        uint methodList = _methodList != null ? _methodList.Row : 0;
                        if (methodList == 0)
                        {
                            if (_row < _parent.Rows)
                            {
                                methodList = _parent[(_row + 1)]._methodList.Row;
                            }
                        }

                        if (_methodList.Parent.LargeIndices) { binaryWriter.Write((uint)methodList); }
                        else { binaryWriter.Write((ushort)methodList); }
                    }
                }
                internal override void Save(BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                public TypeDefTableRow Add(Heap.StringHeap.String name, Heap.StringHeap.String @namespace, TypeAttributes attributes, ushort parentTypeRefToken, FieldTable.FieldTableRow fieldList, MethodDefTable.MethodDefTableRow methodList)
                {
                    lock (this)
                    {
                        TypeDefTableRow row = new TypeDefTableRow(this, (uint)(_rows.Count + 1), name, @namespace, attributes, parentTypeRefToken, fieldList, methodList);
                        _rows.Add(row);
                        return row;
                    }
                }
                public TypeDefTable()
                {
                }
                internal TypeDefTable(uint rows, Heap.StringHeap stringHeap, FieldTable fieldTable, MethodDefTable methodDefTable, BinaryReader reader)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new TypeDefTableRow(this, (uint)(n + 1), stringHeap, fieldTable, methodDefTable, reader));
                    }
                }
#if NET6_0_OR_GREATER
                internal TypeDefTable(uint rows, Heap.StringHeap stringHeap, FieldTable fieldTable, MethodDefTable methodDefTable, Span<byte> data, ref uint offset)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new TypeDefTableRow(this, (uint)(n + 1), stringHeap, fieldTable, methodDefTable, data, ref offset));
                    }
                }
#endif
            }
        }
    }
}
