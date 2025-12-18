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
            public class MemberRefTable : MetadataTable
            {
                public class MemberRefTableRow : MetadataTableRow, ICustomAttributeConstructor, IHasCustomAttribute, IMethodDefOrRef
                {
                    public override uint Length { get { return 3; } }
                    IMemberRefParent _parent;
                    public IMemberRefParent Parent { get { return _parent; } }
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    Heap.BlobHeap.Blob _signature;
                    public Heap.BlobHeap.Blob Signature { get { return _signature; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    internal MemberRefTableRow(uint row, IMemberRefParent parent, Heap.StringHeap.String name, Heap.BlobHeap.Blob signature)
                    {
                        _row = row;
                        _parent = parent;
                        _signature = signature;
                        _name = name;
                    }
#if NET6_0_OR_GREATER
                    internal MemberRefTableRow(uint row, Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, ModuleRefTable? moduleRefTable, MethodDefTable? methodDefTable, System.IO.BinaryReader reader)
#else
                    internal MemberRefTableRow(uint row, Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable, ModuleRefTable moduleRefTable, MethodDefTable methodDefTable, System.IO.BinaryReader reader)
#endif
                    {
                        _row = row;
                        uint parentTag = 0;
                        if (MemberRefParentLargeIndices(typeDefTable, typeRefTable, typeSpecTable, moduleRefTable, methodDefTable)) { parentTag = reader.ReadUInt32(); } else { parentTag = reader.ReadUInt16(); }
                        _parent = MemberRefParentDecode(parentTag, typeDefTable, typeRefTable, typeSpecTable, moduleRefTable, methodDefTable);
                        uint nameIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        uint signatureIndex = blobHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        _signature = new Heap.BlobHeap.Blob(blobHeap, signatureIndex);
                    }
#if NET6_0_OR_GREATER
                    internal MemberRefTableRow(uint row, Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, ModuleRefTable? moduleRefTable, MethodDefTable? methodDefTable, Span<byte> data, ref uint offset)
                    {
                        _row = row;
                        uint parentTag = MemberRefParentEncode(_parent);
                        if (MemberRefParentLargeIndices(typeDefTable, typeRefTable, typeSpecTable, moduleRefTable, methodDefTable)) { parentTag = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { parentTag = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _parent = MemberRefParentDecode(parentTag, typeDefTable, typeRefTable, typeSpecTable, moduleRefTable, methodDefTable);
                        uint nameIndex = 0; if (stringHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        uint signatureIndex = 0; if (blobHeap.LargeIndices) { signatureIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { signatureIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        _signature = new Heap.BlobHeap.Blob(blobHeap, signatureIndex);
                    }
#endif

#if NET6_0_OR_GREATER
                    internal void Save(TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, ModuleRefTable? moduleRefTable, MethodDefTable? methodDefTable, BinaryWriter binaryWriter)
#else
                    internal void Save(TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable, ModuleRefTable moduleRefTable, MethodDefTable methodDefTable, BinaryWriter binaryWriter)
#endif
                    {
                        uint parentTag = MemberRefParentEncode(_parent);
                        if (MemberRefParentLargeIndices(typeDefTable, typeRefTable, typeSpecTable, moduleRefTable, methodDefTable)) { binaryWriter.Write(parentTag); } else { binaryWriter.Write((ushort)parentTag); }
                        if (_name.Heap.LargeIndices) { binaryWriter.Write(_name.Index); } else { binaryWriter.Write((ushort)_name.Index); }
                        if (_signature.Heap.LargeIndices) { binaryWriter.Write(_signature.Index); } else { binaryWriter.Write((ushort)_signature.Index); }
                    }
                }
                public override int ID { get { return 0x0A; } }
                public override uint Columns { get { return 3; } }
                public override uint Rows { get { lock (this) { return (uint)_rows.Count; } } }
                public override bool Sorted { get { return false; } }
#if NET6_0_OR_GREATER
                internal void Save(TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, ModuleRefTable? moduleRefTable, MethodDefTable? methodDefTable, BinaryWriter binaryWriter)
#else
                internal void Save(TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable, ModuleRefTable moduleRefTable, MethodDefTable methodDefTable, BinaryWriter binaryWriter)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(typeDefTable, typeRefTable, typeSpecTable, moduleRefTable, methodDefTable, binaryWriter);
                    }
                }
                List<MemberRefTableRow> _rows = new List<MemberRefTableRow>();
                public MemberRefTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public MemberRefTableRow Add(IMemberRefParent parent, Heap.StringHeap.String name, Heap.BlobHeap.Blob signature)
                {
                    lock (this)
                    {
                        MemberRefTableRow row = new MemberRefTableRow((uint)(_rows.Count + 1), parent, name, signature);
                        _rows.Add(row);
                        return row;
                    }
                }
                public MemberRefTable()
                {
                }
#if NET6_0_OR_GREATER
                internal MemberRefTable(uint rows, Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, ModuleRefTable? moduleRefTable, MethodDefTable? methodDefTable, System.IO.BinaryReader reader)
#else
                internal MemberRefTable(uint rows, Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable, ModuleRefTable moduleRefTable, MethodDefTable methodDefTable, System.IO.BinaryReader reader)
#endif
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new MemberRefTableRow(n + 1, stringHeap, blobHeap, typeDefTable, typeRefTable, typeSpecTable, moduleRefTable, methodDefTable, reader));
                    }
                }
#if NET6_0_OR_GREATER
                internal MemberRefTable(uint rows, Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, ModuleRefTable? moduleRefTable, MethodDefTable? methodDefTable, Span<byte> data, ref uint offset)
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new MemberRefTableRow(n + 1, stringHeap, blobHeap, typeDefTable, typeRefTable, typeSpecTable, moduleRefTable, methodDefTable, data, ref offset));
                    }
                }
#endif
            }
        }
    }
}
