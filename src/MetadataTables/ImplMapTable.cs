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
            public class ImplMapTable : MetadataTable
            {
                List<ImplMapTableRow> _rows = new List<ImplMapTableRow>();
                public override int ID { get { return 0x1C; } }
                public override uint Columns { get { return 4; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public ImplMapTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public class ImplMapTableRow : MetadataTableRow
                {
                    ImplMapTable _parent;
                    public ImplMapTable Parent { get { return _parent; } }
                    public override uint Length { get { return 4; } }
                    ushort _flags;
                    IMemberForwarded _memberForwarded;
                    public IMemberForwarded MemberForwarded { get { return _memberForwarded; } }
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    ModuleRefTable.ModuleRefTableRow _importScope;
                    public ModuleRefTable.ModuleRefTableRow ImportScope { get { return _importScope; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    internal ImplMapTableRow(ImplMapTable parent, uint row, ushort flags, IMemberForwarded memberForwarded, Heap.StringHeap.String name, ModuleRefTable.ModuleRefTableRow importScope)
                    {
                        _parent = parent;
                        _row = row;
                        _flags = flags;
                        _name = name;
                        _importScope = importScope;
                        _memberForwarded = memberForwarded;
                    }
#if NET6_0_OR_GREATER
                    internal ImplMapTableRow(ImplMapTable parent, uint row, ModuleRefTable? moduleRefTable, FieldTable? fieldTable, MethodDefTable? methodDefTable, Heap.StringHeap stringHeap, System.IO.BinaryReader reader)
#else
                    internal ImplMapTableRow(ImplMapTable parent, uint row, ModuleRefTable moduleRefTable, FieldTable fieldTable, MethodDefTable methodDefTable, Heap.StringHeap stringHeap, System.IO.BinaryReader reader)
#endif
                    {
                        _parent = parent;
                        _row = row;
                        _flags = reader.ReadUInt16();
                        uint memberForwardedToken;
                        if (MemberForwardedLargeIndices(fieldTable, methodDefTable)) { memberForwardedToken = reader.ReadUInt32(); } else { memberForwardedToken = reader.ReadUInt16(); }
                        _memberForwarded = MemberForwardedDecode(memberForwardedToken, fieldTable, methodDefTable);
                        uint nameIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        if (moduleRefTable == null) { reader.ReadUInt16(); _importScope = null; }
                        else
                        {
                            if (moduleRefTable.LargeIndices) { _importScope = moduleRefTable[reader.ReadUInt32()]; } else { _importScope = moduleRefTable[reader.ReadUInt16()]; }
                        }
                    }
#if NET6_0_OR_GREATER
                    internal ImplMapTableRow(ImplMapTable parent, uint row, ModuleRefTable? moduleRefTable, FieldTable? fieldTable, MethodDefTable? methodDefTable, Heap.StringHeap stringHeap, Span<byte> data, ref uint offset)
                    {
                        _parent = parent;
                        _row = row;
                        _flags = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        uint memberForwardedToken;
                        if (MemberForwardedLargeIndices(fieldTable, methodDefTable)) { memberForwardedToken = BitConverterLE.ToUInt32(data, offset); offset += 2; } else { memberForwardedToken = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _memberForwarded = MemberForwardedDecode(memberForwardedToken, fieldTable, methodDefTable);
                        uint nameIndex = 0; if (stringHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        if (moduleRefTable == null) { offset += 2; _importScope = null; }
                        else
                        {
                            if (moduleRefTable.LargeIndices) { _importScope = moduleRefTable[BitConverterLE.ToUInt32(data, offset)]; offset += 4; } else { _importScope = moduleRefTable[BitConverterLE.ToUInt16(data, offset)]; offset += 2; }
                        }
                    }
#endif

#if NET6_0_OR_GREATER
                    internal void Save(FieldTable? fieldTable, MethodDefTable? methodDefTable, BinaryWriter binaryWriter)
#else
                    internal void Save(FieldTable fieldTable, MethodDefTable methodDefTable, BinaryWriter binaryWriter)
#endif
                    {
                        binaryWriter.Write(_flags);
                        uint memberForwardedToken = MemberForwardedEncode(_memberForwarded);
                        if (MemberForwardedLargeIndices(fieldTable, methodDefTable)) { binaryWriter.Write((uint)memberForwardedToken); } else { binaryWriter.Write((ushort)memberForwardedToken); }
                        if (_name.Heap.LargeIndices) { binaryWriter.Write(_name.Index); } else { binaryWriter.Write((ushort)_name.Index); }
                        if (_importScope.Parent.LargeIndices) { binaryWriter.Write(_importScope.Row); } else { binaryWriter.Write((ushort)_importScope.Row); }
                    }
                }
                public ImplMapTableRow Add(ushort flags, IMemberForwarded memberForwarded, Heap.StringHeap.String name, ModuleRefTable.ModuleRefTableRow importScope)
                {
                    lock (this)
                    {
                        ImplMapTableRow row = new ImplMapTableRow(this, (uint)(_rows.Count + 1), flags, memberForwarded, name, importScope);
                        _rows.Add(row);
                        return row;
                    }
                }
#if NET6_0_OR_GREATER
                internal void Save(FieldTable? fieldTable, MethodDefTable? methodDefTable, BinaryWriter binaryWriter)
#else
                internal void Save(FieldTable fieldTable, MethodDefTable methodDefTable,BinaryWriter binaryWriter)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(fieldTable, methodDefTable, binaryWriter);
                    }
                }
                internal ImplMapTable()
                {
                }
#if NET6_0_OR_GREATER
                internal ImplMapTable(uint rows, ModuleRefTable? moduleRefTable, FieldTable? fieldTable, MethodDefTable? methodDefTable, Heap.StringHeap stringHeap, System.IO.BinaryReader reader)
#else
                internal ImplMapTable(uint rows, ModuleRefTable moduleRefTable, FieldTable fieldTable, MethodDefTable methodDefTable, Heap.StringHeap stringHeap, System.IO.BinaryReader reader)
#endif
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new ImplMapTableRow(this, (uint)(n + 1), moduleRefTable, fieldTable, methodDefTable, stringHeap, reader));
                    }
                }
#if NET6_0_OR_GREATER
                internal ImplMapTable(uint rows, ModuleRefTable? moduleRefTable, FieldTable? fieldTable, MethodDefTable? methodDefTable, Heap.StringHeap stringHeap, Span<byte> data, ref uint offset)
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new ImplMapTableRow(this, (uint)(n + 1), moduleRefTable, fieldTable, methodDefTable, stringHeap, data, ref offset));
                    }
                }
#endif
            }
        }
    }
}
