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
        public abstract partial class MetadataTable
        {
            public class TypeRefTable : MetadataTable
            {
                List<TypeRefTableRow> _rows = new List<TypeRefTableRow>();
                public TypeRefTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public class TypeRefTableRow : MetadataTableRow
                {
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    Heap.StringHeap.String _namespace;
                    public Heap.StringHeap.String Namespace { get { return _namespace; } }
                    ushort _resolutionScopeToken;
                    public override uint Length { get { return 3; } }
                    uint _row;
                    public override uint Row { get { return _row; } }

                    public TypeRefTableRow(uint row, ushort resolutionScopeToken, Heap.StringHeap.String name, Heap.StringHeap.String @namespace)
                    {
                        _row = row;
                        _resolutionScopeToken = resolutionScopeToken;
                        _name = name;
                        _namespace = @namespace;
                    }
                    public TypeRefTableRow(uint row, Heap.StringHeap stringHeap, System.IO.BinaryReader reader)
                    {
                        _row = row;
                        _resolutionScopeToken = reader.ReadUInt16();
                        uint nameIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        uint namespaceIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        _namespace = new Heap.StringHeap.String(stringHeap, namespaceIndex);
                    }
#if NET6_0_OR_GREATER
                    public TypeRefTableRow(uint row, Heap.StringHeap stringHeap, Span<byte> data, ref uint offset)
                    {
                        _row = row;
                        _resolutionScopeToken = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        uint nameIndex = 0; if (stringHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        uint namespaceIndex = 0; if (stringHeap.LargeIndices) { namespaceIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { namespaceIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        _namespace = new Heap.StringHeap.String(stringHeap, namespaceIndex);
                    }
#endif
                    internal void Save(BinaryWriter binaryWriter)
                    {
                        binaryWriter.Write(_resolutionScopeToken);
                        binaryWriter.Write(_name.Index);
                        binaryWriter.Write(_namespace.Index);
                    }
                }
                internal override void Save(BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                public override int ID { get { return 0x01; } }
                public override uint Columns { get { return 3; } }
                public override uint Rows { get { lock (this) { return (uint)_rows.Count; } } }
                public override bool Sorted { get { return false; } }
                public uint Add(ushort resolutionScopeToken, Heap.StringHeap.String name, Heap.StringHeap.String @namespace)
                {
                    lock (this)
                    {
                        TypeRefTableRow row = new TypeRefTableRow((uint)(_rows.Count + 1), resolutionScopeToken, name, @namespace);
                        _rows.Add(row);
                        return (uint)_rows.Count;
                    }
                }
                internal TypeRefTable()
                {
                }
                internal TypeRefTable(uint rows, Heap.StringHeap stringHeap, System.IO.BinaryReader reader)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new TypeRefTableRow((uint)(n + 1), stringHeap, reader));
                    }
                }
#if NET6_0_OR_GREATER
                internal TypeRefTable(uint rows, Heap.StringHeap stringHeap, Span<byte> data, ref uint offset)
                {
                    for (int n = 0; n < rows; n++)
                    {

                        _rows.Add(new TypeRefTableRow((uint)(n + 1), stringHeap, data, ref offset));
                    }
                }
#endif
            }
        }
    }
}