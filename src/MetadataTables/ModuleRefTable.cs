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
            public class ModuleRefTable : MetadataTable
            {
                List<ModuleRefTableRow> _rows = new List<ModuleRefTableRow>();
                public override int ID { get { return 0x1A; } }
                public override uint Columns { get { return 1; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public ModuleRefTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public class ModuleRefTableRow : MetadataTableRow, IResolutionScope, IMemberRefParent, IHasCustomAttribute
                {
                    public override uint Length { get { return 1; } }
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    internal ModuleRefTableRow(uint row, Heap.StringHeap.String name)
                    {
                        _row = row;
                        _name = name;
                    }
                    internal ModuleRefTableRow(uint row)
                    {
                        _row = row;
                    }
                    internal void Load(Heap.StringHeap stringHeap, System.IO.BinaryReader reader)
                    {
                        uint nameIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.StringHeap stringHeap, Span<byte> data, ref uint offset)
                    {
                        uint nameIndex = 0; if (stringHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                    }
#endif
                    internal void Serialize(BinaryWriter binaryWriter)
                    {
                        binaryWriter.Write(_name.Index);
                    }
                }
                public ModuleRefTableRow Add(Heap.StringHeap.String name)
                {
                    lock (this)
                    {
                        ModuleRefTableRow row = new ModuleRefTableRow((uint)(_rows.Count + 1), name);
                        _rows.Add(row);
                        return row;
                    }
                }
                internal void Save(BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Serialize(binaryWriter);
                    }
                }
                public ModuleRefTable()
                {
                }
                internal ModuleRefTable(uint rows)
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new ModuleRefTableRow((uint)(n + 1)));
                    }
                }
                internal void Load(Heap.StringHeap stringHeap, System.IO.BinaryReader reader)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Load(stringHeap, reader);
                    }
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.StringHeap stringHeap, Span<byte> data, ref uint offset)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Load(stringHeap, data, ref offset);
                    }
                }
#endif
            }
        }
    }
}
