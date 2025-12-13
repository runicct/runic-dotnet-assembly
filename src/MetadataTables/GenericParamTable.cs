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
            public class GenericParamTable : MetadataTable
            {
                List<GenericParamTableRow> _rows = new List<GenericParamTableRow>();
                public override int ID { get { return 0x2A; } }
                public override uint Columns { get { return 4; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public GenericParamTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public GenericParamTableRow Add(ushort number, GenericParamAttributes flags, ITypeDefOrMethodDef owner, Heap.StringHeap.String name)
                {
                    lock (this)
                    {
                        GenericParamTableRow row = new GenericParamTableRow(this, (uint)(_rows.Count + 1), number, flags, owner, name);
                        _rows.Add(row);
                        return row;
                    }
                }
                public class GenericParamTableRow : MetadataTableRow, IHasCustomAttribute
                {
                    GenericParamTable _parent;
                    internal GenericParamTable Parent { get { return _parent; } }
                    public override uint Length { get { return 4; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    ushort _number;
                    public ushort Number { get { return _number; } }
                    ITypeDefOrMethodDef _owner;
                    public ITypeDefOrMethodDef Owner { get { return _owner; } }
                    GenericParamAttributes _flags;
                    public GenericParamAttributes Flags { get { return _flags; } }
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    internal GenericParamTableRow(GenericParamTable parent, uint row, ushort number, GenericParamAttributes flags, ITypeDefOrMethodDef owner, Heap.StringHeap.String name)
                    {
                        _parent = parent;
                        _row = row;
                        _number = number;
                        _flags = flags;
                        _owner = owner;
                        _name = name;
                    }
                    internal GenericParamTableRow(GenericParamTable parent, uint row)
                    {
                        _parent = parent;
                        _row = row;
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.StringHeap? stringHeap, TypeDefTable? typeDefTable, MethodDefTable? methodDefTable, BinaryReader reader)
#else
                    internal void Load(Heap.StringHeap stringHeap, TypeDefTable typeDefTable, MethodDefTable methodDefTable, BinaryReader reader)
#endif
                    {
                        _number = reader.ReadUInt16();
                        _flags = (GenericParamAttributes)reader.ReadUInt16();
                        uint ownerIndex;
                        if (TypeDefOrMethodDefLargeIndices(typeDefTable, methodDefTable)) { ownerIndex = reader.ReadUInt32(); } else { ownerIndex = reader.ReadUInt16(); }
                        _owner = TypeDefOrMethodDefDecode(ownerIndex, typeDefTable, methodDefTable);
                        uint nameIndex;
                        if (stringHeap.LargeIndices) { nameIndex = reader.ReadUInt32(); } else { nameIndex = reader.ReadUInt16(); }
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                    }
#if NET6_0_OR_GREATER

                    internal void Load(Heap.StringHeap? stringHeap, TypeDefTable? typeDefTable, MethodDefTable? methodDefTable, Span<byte> data, ref uint offset)
                    {
                        _number = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        _flags = (GenericParamAttributes)BitConverterLE.ToUInt16(data, offset); offset += 2;
                        uint ownerIndex;
                        if (TypeDefOrMethodDefLargeIndices(typeDefTable, methodDefTable)) { ownerIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { ownerIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _owner = TypeDefOrMethodDefDecode(ownerIndex, typeDefTable, methodDefTable);
                        uint nameIndex;
                        if (stringHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                    }
#endif

#if NET6_0_OR_GREATER
                    internal void Save(TypeDefTable? typeDefTable, MethodDefTable? methodDefTable, BinaryWriter binaryWriter)
#else
                    internal void Save(TypeDefTable typeDefTable, MethodDefTable methodDefTable, BinaryWriter binaryWriter)
#endif
                    {
                         binaryWriter.Write((ushort)_number);
                         binaryWriter.Write((ushort)_flags);
                         uint ownerIndex = TypeDefOrMethodDefEncode(_owner);
                         if (TypeDefOrMethodDefLargeIndices(typeDefTable, methodDefTable)) { binaryWriter.Write((uint)ownerIndex); } else { binaryWriter.Write((ushort)ownerIndex); }
                         if (_name.Heap.LargeIndices) { binaryWriter.Write((uint)_name.Index); } else { binaryWriter.Write((ushort)_name.Index); }
                    }
                }
#if NET6_0_OR_GREATER
                internal void Save(TypeDefTable? typeDefTable, MethodDefTable? methodDefTable, BinaryWriter binaryWriter)
#else
                internal void Save(TypeDefTable typeDefTable, MethodDefTable methodDefTable, BinaryWriter binaryWriter)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(typeDefTable, methodDefTable, binaryWriter);
                    }
                }
                public GenericParamTable()
                {
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.StringHeap stringHeap, TypeDefTable? typeDefTable, MethodDefTable? methodDefTable, BinaryReader reader)
#else
                internal void Load(Heap.StringHeap stringHeap, TypeDefTable typeDefTable, MethodDefTable methodDefTable, BinaryReader reader)
#endif
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(stringHeap, typeDefTable, methodDefTable, reader); }
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.StringHeap stringHeap, TypeDefTable? typeDefTable, MethodDefTable? methodDefTable, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(stringHeap, typeDefTable, methodDefTable, data, ref offset); }
                }
#endif
                internal GenericParamTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new GenericParamTableRow(this, (uint)(_rows.Count + 1)));
                    }
                }
            }
        }
    }
}
