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
            public class NestedClassTable : MetadataTable
            {
                List<NestedClassTableRow> _rows = new List<NestedClassTableRow>();
                public override int ID { get { return 0x29; } }
                public override uint Columns { get { return 2; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public NestedClassTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public class NestedClassTableRow : MetadataTableRow
                {
                    public override uint Length { get { return 1; } }
                    TypeDefTable.TypeDefTableRow _nestedClass;
                    public TypeDefTable.TypeDefTableRow NestedClass { get { return _nestedClass; } }
                    TypeDefTable.TypeDefTableRow _enclosingClass;
                    public TypeDefTable.TypeDefTableRow EnclosingClass { get { return _enclosingClass; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    internal NestedClassTableRow(uint row, TypeDefTable.TypeDefTableRow nestedClass, TypeDefTable.TypeDefTableRow enclosingClass)
                    {
                        _row = row;
                        _nestedClass = nestedClass;
                        _enclosingClass = enclosingClass;
                    }
                    internal NestedClassTableRow(uint row, TypeDefTable typeDefTable, System.IO.BinaryReader reader)
                    {
                        _row = row;
                        uint nestedClassIndex = 0; if (typeDefTable.LargeIndices) { nestedClassIndex = reader.ReadUInt32(); } else { nestedClassIndex = reader.ReadUInt16(); }
                        uint enclosingClassIndex = 0; if (typeDefTable.LargeIndices) { enclosingClassIndex = reader.ReadUInt32(); } else { enclosingClassIndex = reader.ReadUInt16(); }
                        _nestedClass = typeDefTable[nestedClassIndex];
                        _enclosingClass = typeDefTable[enclosingClassIndex];
                    }
#if NET6_0_OR_GREATER
                    internal NestedClassTableRow(uint row, TypeDefTable typeDefTable, Span<byte> data, ref uint offset)
                    {
                        _row = row;
                        uint nestedClassIndex = 0; if (typeDefTable.LargeIndices) { nestedClassIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nestedClassIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        uint enclosingClassIndex = 0; if (typeDefTable.LargeIndices) { enclosingClassIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { enclosingClassIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _nestedClass = typeDefTable[nestedClassIndex];
                        _enclosingClass = typeDefTable[enclosingClassIndex];
                    }
#endif
                    internal void Serialize(BinaryWriter binaryWriter)
                    {
                        if (_nestedClass.Parent.LargeIndices) { binaryWriter.Write(_nestedClass.Row); } else { binaryWriter.Write((ushort)_nestedClass.Row); }
                        if (_enclosingClass.Parent.LargeIndices) { binaryWriter.Write(_enclosingClass.Row); } else { binaryWriter.Write((ushort)_enclosingClass.Row); }
                    }
                }
                public NestedClassTableRow Add(TypeDefTable.TypeDefTableRow nestedClass, TypeDefTable.TypeDefTableRow enclosingClass)
                {
                    lock (this)
                    {
                        NestedClassTableRow row = new NestedClassTableRow((uint)(_rows.Count + 1), nestedClass, enclosingClass);
                        _rows.Add(row);
                        return row;
                    }
                }
                internal override void Save(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Serialize(binaryWriter);
                    }
                }
                public NestedClassTable()
                {
                }
                internal NestedClassTable(uint rows, TypeDefTable typeDefTable, System.IO.BinaryReader reader)
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new NestedClassTableRow((uint)(n + 1), typeDefTable, reader));
                    }
                }
#if NET6_0_OR_GREATER
                internal NestedClassTable(uint rows, TypeDefTable typeDefTable, Span<byte> data, ref uint offset)
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new NestedClassTableRow((uint)(n + 1), typeDefTable, data, ref offset));
                    }
                }
#endif
            }
        }
    }
}
