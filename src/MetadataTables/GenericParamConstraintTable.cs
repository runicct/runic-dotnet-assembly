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
            public class GenericParamConstraintTable : MetadataTable
            {
                List<GenericParamConstraintTableRow> _rows = new List<GenericParamConstraintTableRow>();
                public override int ID { get { return 0x2C; } }
                public override uint Columns { get { return 2; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public GenericParamConstraintTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public GenericParamConstraintTableRow Add(GenericParamTable.GenericParamTableRow owner, ITypeDefOrRefOrSpec constraint)
                {
                    lock (this)
                    {
                        GenericParamConstraintTableRow row = new GenericParamConstraintTableRow(this, (uint)(_rows.Count + 1), owner, constraint);
                        _rows.Add(row);
                        return row;
                    }
                }
                public class GenericParamConstraintTableRow : MetadataTableRow, IHasCustomAttribute
                {
                    GenericParamConstraintTable _parent;
                    internal GenericParamConstraintTable Parent { get { return _parent; } }
                    public override uint Length { get { return 2; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    ushort _number;
                    public ushort Number { get { return _number; } }
                    GenericParamTable.GenericParamTableRow _owner;
                    public GenericParamTable.GenericParamTableRow Owner { get { return _owner; } }
                    ITypeDefOrRefOrSpec _constraint;
                    public ITypeDefOrRefOrSpec Constraint { get { return _constraint; } }
                    internal GenericParamConstraintTableRow(GenericParamConstraintTable parent, uint row, GenericParamTable.GenericParamTableRow owner, ITypeDefOrRefOrSpec constraint)
                    {
                        _parent = parent;
                        _row = row;
                        _owner = owner;
                        _constraint = constraint;
                    }
                    internal GenericParamConstraintTableRow(GenericParamConstraintTable parent, uint row)
                    {
                        _parent = parent;
                        _row = row;
                    }
#if NET6_0_OR_GREATER
                    internal void Load(GenericParamTable genericParamTable, TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, BinaryReader reader)
#else
                    internal void Load(GenericParamTable genericParamTable, TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable, BinaryReader reader)
#endif
                    {
                        uint ownerIndex;
                        if (genericParamTable.LargeIndices) { ownerIndex = reader.ReadUInt32(); } else { ownerIndex = reader.ReadUInt16(); }
                        _owner = genericParamTable[(uint)ownerIndex];
                        uint constraintIndex;
                        if (TypeDefOrRefOrSpecLargeIndices(typeDefTable, typeRefTable, typeSpecTable)) { constraintIndex = reader.ReadUInt32(); } else { constraintIndex = reader.ReadUInt16(); }
                        _constraint = TypeDefOrRefOrSpecDecode(constraintIndex, typeDefTable, typeRefTable, typeSpecTable);
                    }
#if NET6_0_OR_GREATER

                    internal void Load(GenericParamTable genericParamTable, TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, Span<byte> data, ref uint offset)
                    {
                        uint ownerIndex;
                        if (genericParamTable.LargeIndices) { ownerIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { ownerIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _owner = genericParamTable[(uint)ownerIndex];
                        uint constraintIndex;
                        if (TypeDefOrRefOrSpecLargeIndices(typeDefTable, typeRefTable, typeSpecTable)) { constraintIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { constraintIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _constraint = TypeDefOrRefOrSpecDecode(constraintIndex, typeDefTable, typeRefTable, typeSpecTable);
                    }
#endif

#if NET6_0_OR_GREATER
                    internal void Save(TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, BinaryWriter binaryWriter)
#else
                    internal void Save( TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable,  BinaryWriter binaryWriter)
#endif
                    {
                        if (_owner.Parent.LargeIndices) { binaryWriter.Write((uint)_owner.Row); } else { binaryWriter.Write((ushort)_owner.Row); }
                        uint constraintIndex = TypeDefOrRefOrSpecEncode(_constraint);
                        if (TypeDefOrRefOrSpecLargeIndices(typeDefTable, typeRefTable, typeSpecTable)) { binaryWriter.Write((uint)constraintIndex); } else { binaryWriter.Write((ushort)constraintIndex); }
                    }
                }
#if NET6_0_OR_GREATER
                internal void Save(TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, BinaryWriter binaryWriter)
#else
                internal void Save(TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable,BinaryWriter binaryWriter)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(typeDefTable, typeRefTable, typeSpecTable, binaryWriter);
                    }
                }
                public GenericParamConstraintTable()
                {
                }
#if NET6_0_OR_GREATER
                internal void Load(GenericParamTable genericParamTable, TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, BinaryReader reader)
#else
                internal void Load(GenericParamTable genericParamTable, TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable,  BinaryReader reader)
#endif
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(genericParamTable, typeDefTable, typeRefTable, typeSpecTable, reader); }
                }
#if NET6_0_OR_GREATER
                internal void Load(GenericParamTable genericParamTable, TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(genericParamTable, typeDefTable, typeRefTable, typeSpecTable, data, ref offset); }
                }
#endif
                internal GenericParamConstraintTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new GenericParamConstraintTableRow(this, (uint)(_rows.Count + 1)));
                    }
                }
            }
        }
    }
}
