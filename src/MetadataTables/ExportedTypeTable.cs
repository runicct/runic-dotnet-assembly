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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public partial class MetadataTable
        {
            public class ExportedTypeTable : MetadataTable
            {
                List<ExportedTypeTableRow> _rows = new List<ExportedTypeTableRow>();
                public override int ID { get { return 0x27; } }
                public override uint Columns { get { return 5; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public ExportedTypeTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
#if NET6_0_OR_GREATER
                public ExportedTypeTableRow Add(TypeAttributes flags, TypeDefTable.TypeDefTableRow? typeDefId, Heap.StringHeap.String typeName, Heap.StringHeap.String typeNamespace, IImplementation implementation)
#else
                public ExportedTypeTableRow Add(TypeAttributes flags, TypeDefTable.TypeDefTableRow typeDefId, Heap.StringHeap.String typeName, Heap.StringHeap.String typeNamespace, IImplementation implementation)
#endif
                {
                    lock (this)
                    {
                        ExportedTypeTableRow row = new ExportedTypeTableRow(this, (uint)(_rows.Count + 1), flags, typeDefId, typeName, typeNamespace, implementation);
                        _rows.Add(row);
                        return row;
                    }
                }
                public class ExportedTypeTableRow : MetadataTableRow, IHasCustomAttribute, IImplementation
                {
                    ExportedTypeTable _parent;
                    internal ExportedTypeTable Parent { get { return _parent; } }
                    public override uint Length { get { return 5; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    TypeAttributes _flags;
                    public TypeAttributes Flags { get { return _flags; } }
#if NET6_0_OR_GREATER
                    TypeDefTable.TypeDefTableRow? _typeDefId;
                    public TypeDefTable.TypeDefTableRow? TypeDefId { get { return _typeDefId; } }
#else
                    TypeDefTable.TypeDefTableRow _typeDefId;
                    public TypeDefTable.TypeDefTableRow TypeDefId { get { return _typeDefId; } }
#endif
                    Heap.StringHeap.String _typeName;
                    public Heap.StringHeap.String TypeName { get { return _typeName; } }
                    Heap.StringHeap.String _typeNamespace;
                    public Heap.StringHeap.String TypeNamespace { get { return _typeNamespace; } }
                    IImplementation _implementation;
                    public IImplementation Implementation { get { return _implementation; } }
#if NET6_0_OR_GREATER
                    internal ExportedTypeTableRow(ExportedTypeTable parent, uint row, TypeAttributes flags, TypeDefTable.TypeDefTableRow? typeDefId, Heap.StringHeap.String typeName, Heap.StringHeap.String typeNamespace, IImplementation implementation)
#else
                    internal ExportedTypeTableRow(ExportedTypeTable parent, uint row, TypeAttributes flags, TypeDefTable.TypeDefTableRow typeDefId, Heap.StringHeap.String typeName, Heap.StringHeap.String typeNamespace, IImplementation implementation)
#endif
                    {
                        _parent = parent;
                        _row = row;
                        _flags = flags;
                        _typeDefId = typeDefId;
                        _typeName = typeName;
                        _typeNamespace = typeNamespace;
                        _implementation = implementation;
                    }
                    internal ExportedTypeTableRow(ExportedTypeTable parent, uint row)
                    {
                        _parent = parent;
                        _row = row;
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.StringHeap stringHeap, TypeDefTable? typeDefTable, FileTable? fileTable, AssemblyRefTable? assemblyRefTable, BinaryReader reader)
#else
                    internal void Load(Heap.StringHeap stringHeap, TypeDefTable typeDefTable, FileTable fileTable, AssemblyRefTable assemblyRefTable, BinaryReader reader)
#endif
                    {
                        _flags = (TypeAttributes)reader.ReadUInt32();
                        uint typeDefIdIndex = reader.ReadUInt32();
                        if (typeDefIdIndex == 0 || typeDefTable == null) { _typeDefId = null; } else { _typeDefId = typeDefTable[typeDefIdIndex]; }
                        uint typeNameIndex;
                        if (stringHeap.LargeIndices) { typeNameIndex = reader.ReadUInt32(); } else { typeNameIndex = reader.ReadUInt16(); }
                        _typeName = new Heap.StringHeap.String(stringHeap, typeNameIndex);
                        uint typeNamespaceIndex;
                        if (stringHeap.LargeIndices) { typeNamespaceIndex = reader.ReadUInt32(); } else { typeNamespaceIndex = reader.ReadUInt16(); }
                        _typeNamespace = new Heap.StringHeap.String(stringHeap, typeNamespaceIndex);
                        uint implementationIndex = 0;
                        if (ImplementationLargeIndices(fileTable, assemblyRefTable, _parent)) { implementationIndex = reader.ReadUInt32(); } else { implementationIndex = reader.ReadUInt16(); }
                        _implementation = ImplementationDecode(implementationIndex, fileTable, assemblyRefTable, _parent);
                    }
#if NET6_0_OR_GREATER

                    internal void Load(Heap.StringHeap stringHeap, TypeDefTable? typeDefTable, FileTable? fileTable, AssemblyRefTable? assemblyRefTable, Span<byte> data, ref uint offset)
                    {
                        _flags = (TypeAttributes)BitConverterLE.ToUInt32(data, offset); offset += 4;
                        uint typeDefIdIndex = BitConverterLE.ToUInt32(data, offset); offset += 4;
                        if (typeDefIdIndex == 0 || typeDefTable == null) { _typeDefId = null; } else { _typeDefId = typeDefTable[typeDefIdIndex]; }
                        uint typeNameIndex;
                        if (stringHeap.LargeIndices) { typeNameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { typeNameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2;  }
                        _typeName = new Heap.StringHeap.String(stringHeap, typeNameIndex);
                        uint typeNamespaceIndex;
                        if (stringHeap.LargeIndices) { typeNamespaceIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { typeNamespaceIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _typeNamespace = new Heap.StringHeap.String(stringHeap, typeNamespaceIndex);
                        uint implementationIndex = 0;
                        if (ImplementationLargeIndices(fileTable, assemblyRefTable, _parent)) { implementationIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { implementationIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _implementation = ImplementationDecode(implementationIndex, fileTable, assemblyRefTable, _parent);
                    }
#endif

#if NET6_0_OR_GREATER
                    internal void Save(TypeDefTable? typeDefTable, FileTable? fileTable, AssemblyRefTable? assemblyRefTable, BinaryWriter binaryWriter)
#else
                    internal void Save(TypeDefTable typeDefTable, FileTable fileTable, AssemblyRefTable assemblyRefTable,  BinaryWriter binaryWriter)
#endif
                    {
                        binaryWriter.Write((uint)_flags);
                        if (_typeDefId == null || typeDefTable == null) { binaryWriter.Write((uint)0); } else { binaryWriter.Write((uint)_typeDefId.Row); }
                        if (_typeName.Heap.LargeIndices) { binaryWriter.Write((uint)_typeName.Index); } else { binaryWriter.Write((ushort)_typeName.Index); }
                        if (_typeNamespace.Heap.LargeIndices) { binaryWriter.Write((uint)_typeNamespace.Index); } else { binaryWriter.Write((ushort)_typeNamespace.Index); }
                        uint implementationIndex = ImplementationEncode(_implementation);
                        if (ImplementationLargeIndices(fileTable, assemblyRefTable, _parent)) { binaryWriter.Write((uint)implementationIndex); } else { binaryWriter.Write((ushort)implementationIndex); }
                    }
                }
#if NET6_0_OR_GREATER
                internal void Save(TypeDefTable? typeDefTable, FileTable? fileTable, AssemblyRefTable? assemblyRefTable, BinaryWriter binaryWriter)
#else
                internal void Save(TypeDefTable typeDefTable, FileTable fileTable, AssemblyRefTable assemblyRefTable, BinaryWriter binaryWriter)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(typeDefTable, fileTable, assemblyRefTable, binaryWriter);
                    }
                }
                public ExportedTypeTable()
                {
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.StringHeap stringHeap, TypeDefTable? typeDefTable, FileTable? fileTable, AssemblyRefTable? assemblyRefTable, BinaryReader reader)
#else
                internal void Load(Heap.StringHeap stringHeap, TypeDefTable typeDefTable, FileTable fileTable, AssemblyRefTable assemblyRefTable, BinaryReader reader)
#endif
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(stringHeap, typeDefTable, fileTable, assemblyRefTable, reader); }
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.StringHeap stringHeap, TypeDefTable? typeDefTable, FileTable? fileTable, AssemblyRefTable? assemblyRefTable, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(stringHeap, typeDefTable, fileTable, assemblyRefTable, data, ref offset); }
                }
#endif
                internal ExportedTypeTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new ExportedTypeTableRow(this, (uint)(_rows.Count + 1)));
                    }
                }
            }
        }
    }
}
