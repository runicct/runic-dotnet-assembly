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
            public class DeclSecurityTable : MetadataTable
            {
                List<DeclSecurityTableRow> _rows = new List<DeclSecurityTableRow>();
                public override int ID { get { return 0x0E; } }
                public override uint Columns { get { return 3; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public DeclSecurityTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public class DeclSecurityTableRow : MetadataTableRow, IHasCustomAttribute
                {
                    public override uint Length { get { return 3; } }
                    ushort _action;
                    public ushort Action { get { return _action; } }
                    IHasDeclSecurity _parent;
                    public IHasDeclSecurity Parent { get { return _parent; } }
                    Heap.BlobHeap.Blob _permissionSet;
                    public Heap.BlobHeap.Blob PermissionSet { get { return _permissionSet; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    internal DeclSecurityTableRow(uint row)
                    {
                        _row = row;
                    }
                    internal DeclSecurityTableRow(uint row, ushort action, IHasDeclSecurity hasDeclSecurity, Heap.BlobHeap.Blob permissionSet)
                    {
                        _row = row;
                        _action = action;
                        _parent = hasDeclSecurity;
                        _permissionSet = permissionSet;
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.BlobHeap blobHeap, TypeDefTable? typeDefTable, MethodDefTable? methodDefTable, AssemblyTable? assemblyTable, System.IO.BinaryReader reader)
#else
                    internal void Load(Heap.BlobHeap blobHeap, TypeDefTable typeDefTable, MethodDefTable methodDefTable, AssemblyTable assemblyTable, System.IO.BinaryReader reader)
#endif
                    {
                        _action = reader.ReadUInt16();
                        uint hasDeclSecurityToken = 0;
                        if (HasDeclSecurityLargeIndices(typeDefTable, methodDefTable, assemblyTable)) { hasDeclSecurityToken = reader.ReadUInt32(); } else { hasDeclSecurityToken = reader.ReadUInt16(); }
                        _parent = HasDeclSecurityDecode(hasDeclSecurityToken, typeDefTable, methodDefTable, assemblyTable);
                        uint blobIndex = blobHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _permissionSet = new Heap.BlobHeap.Blob(blobHeap, blobIndex);
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.BlobHeap blobHeap, TypeDefTable? typeDefTable, MethodDefTable? methodDefTable, AssemblyTable? assemblyTable, Span<byte> data, ref uint offset)
                    {
                        _action = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        uint hasDeclSecurityToken = 0;
                        if (HasDeclSecurityLargeIndices(typeDefTable, methodDefTable, assemblyTable)) { hasDeclSecurityToken = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { hasDeclSecurityToken = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _parent = HasDeclSecurityDecode(hasDeclSecurityToken, typeDefTable, methodDefTable, assemblyTable);
                        uint blobIndex = 0; if (blobHeap.LargeIndices) { blobIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { blobIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _permissionSet = new Heap.BlobHeap.Blob(blobHeap, blobIndex);
                    }
#endif

#if NET6_0_OR_GREATER
                    internal void Save(TypeDefTable? typeDefTable, MethodDefTable? methodDefTable, AssemblyTable? assemblyTable, BinaryWriter binaryWriter)
#else
                    internal void Save(TypeDefTable typeDefTable, MethodDefTable methodDefTable, AssemblyTable assemblyTable, BinaryWriter binaryWriter)
#endif
                    {
                        binaryWriter.Write(_action);
                        uint hasSecurityToken = HasDeclSecurityEncode(_parent);
                        if (HasDeclSecurityLargeIndices(typeDefTable, methodDefTable, assemblyTable)) { binaryWriter.Write((uint)hasSecurityToken); } else { binaryWriter.Write((ushort)hasSecurityToken); }
                        binaryWriter.Write(_permissionSet.Index);
                    }
                }
                public DeclSecurityTableRow Add(ushort action, IHasDeclSecurity parent, Heap.BlobHeap.Blob permissionSet)
                {
                    lock (this)
                    {
                        DeclSecurityTableRow row = new DeclSecurityTableRow((uint)(_rows.Count + 1), action, parent, permissionSet);
                        _rows.Add(row);
                        return row;
                    }
                }
#if NET6_0_OR_GREATER
                internal void Save(TypeDefTable? typeDefTable, MethodDefTable? methodDefTable, AssemblyTable? assemblyTable, BinaryWriter binaryWriter)
#else
                internal void Save(TypeDefTable typeDefTable, MethodDefTable methodDefTable, AssemblyTable assemblyTable, BinaryWriter binaryWriter)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(typeDefTable, methodDefTable, assemblyTable, binaryWriter);
                    }
                }
                internal DeclSecurityTable(uint rows)
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new DeclSecurityTableRow((uint)(n + 1)));
                    }
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.BlobHeap blobHeap, TypeDefTable? typeDefTable, MethodDefTable? methodDefTable, AssemblyTable? assemblyTable, System.IO.BinaryReader reader)
#else
                internal void Load(Heap.BlobHeap blobHeap, TypeDefTable typeDefTable, MethodDefTable methodDefTable, AssemblyTable assemblyTable, System.IO.BinaryReader reader)
#endif
                {
                    for (uint n = 0; n < _rows.Count; n++)
                    {
                        _rows[(int)n].Load(blobHeap, typeDefTable, methodDefTable, assemblyTable, reader);
                    }
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.BlobHeap blobHeap, TypeDefTable? typeDefTable, MethodDefTable? methodDefTable, AssemblyTable? assemblyTable, Span<byte> data, ref uint offset)
                {
                    for (uint n = 0; n < _rows.Count; n++)
                    {
                    _rows[(int)n].Load(blobHeap, typeDefTable, methodDefTable, assemblyTable, data, ref offset);
                    }
                }
#endif
            }
        }
    }
}