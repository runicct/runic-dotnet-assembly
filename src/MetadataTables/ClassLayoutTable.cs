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
            public class ClassLayoutTable : MetadataTable
            {
                List<ClassLayoutTableRow> _rows = new List<ClassLayoutTableRow>();
                public override int ID { get { return 0x0F; } }
                public override uint Columns { get { return 3; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public ClassLayoutTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public ClassLayoutTableRow Add(ushort packingSize, uint classSize, TypeDefTable.TypeDefTableRow parent)
                {
                    lock (this)
                    {
                        ClassLayoutTableRow row = new ClassLayoutTableRow((uint)(_rows.Count + 1), packingSize, classSize, parent);
                        _rows.Add(row);
                        return row;
                    }
                }
                public class ClassLayoutTableRow : MetadataTableRow
                {

                    public override uint Length { get { return 3; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    TypeDefTable.TypeDefTableRow _parent;
                    public TypeDefTable.TypeDefTableRow Parent { get { return _parent; } }
                    ushort _packingSize;
                    public ushort PackingSize { get { return _packingSize; } }
                    uint _classSize;
                    public uint ClassSize { get { return _classSize; } }
                    internal ClassLayoutTableRow(uint row, ushort packingSize, uint classSize, TypeDefTable.TypeDefTableRow parent)
                    {
                        _parent = parent;
                        _row = row;
                        _packingSize = packingSize;
                        _classSize = classSize;
                        _parent = parent;
                    }
                    internal ClassLayoutTableRow(uint row)
                    {
                        _row = row;
                    }
#if NET6_0_OR_GREATER
                    internal void Load(TypeDefTable? typeDefTable, BinaryReader reader)
#else
                    internal void Load(TypeDefTable typeDefTable, BinaryReader reader)
#endif
                    {
                        _packingSize = reader.ReadUInt16();
                        _classSize = reader.ReadUInt32();
                        uint parentIndex;
                        if (typeDefTable != null)
                        {
                            if (typeDefTable.LargeIndices) { parentIndex = reader.ReadUInt32(); } else { parentIndex = reader.ReadUInt16(); }
                            _parent = typeDefTable[(uint)parentIndex];
                        }
                        else
                        {
                            throw new InvalidDataException("TypeDef table is required to load ClassLayout table.");
                        }
                    }
#if NET6_0_OR_GREATER

                    internal void Load(TypeDefTable? typeDefTable, Span<byte> data, ref uint offset)
                    {
                        _packingSize = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        _classSize = BitConverterLE.ToUInt32(data, offset); offset += 4;
                        uint parentIndex;
                        if (typeDefTable != null)
                        {
                            if (typeDefTable.LargeIndices) { parentIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { parentIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                            _parent = typeDefTable[(uint)parentIndex];
                        }
                        else
                        {
                            throw new InvalidDataException("TypeDef table is required to load ClassLayout table.");
                        }
                    }
#endif

#if NET6_0_OR_GREATER
                    internal void Save(BinaryWriter binaryWriter)
#else
                    internal void Save(BinaryWriter binaryWriter)
#endif
                    {
                        binaryWriter.Write(_packingSize);
                        binaryWriter.Write(_classSize);
                        if (_parent.Parent.LargeIndices) { binaryWriter.Write((uint)_parent.Row); } else { binaryWriter.Write((ushort)_parent.Row); }
                    }
                }
#if NET6_0_OR_GREATER
                internal void Save(BinaryWriter binaryWriter)
#else
                internal void Save(BinaryWriter binaryWriter)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                public ClassLayoutTable()
                {
                }
#if NET6_0_OR_GREATER
                internal void Load(TypeDefTable? typeDefTable, BinaryReader reader)
#else
                internal void Load(TypeDefTable typeDefTable, BinaryReader reader)
#endif
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(typeDefTable, reader); }
                }
#if NET6_0_OR_GREATER
                internal void Load(TypeDefTable? typeDefTable, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(typeDefTable, data, ref offset); }
                }
#endif
                internal ClassLayoutTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new ClassLayoutTableRow((uint)(_rows.Count + 1)));
                    }
                }
            }
        }
    }
}
