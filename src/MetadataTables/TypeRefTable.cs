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
        public abstract partial class MetadataTable
        {
            public class TypeRefTable : MetadataTable
            {
                List<TypeRefTableRow> _rows = new List<TypeRefTableRow>();
                public TypeRefTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public class TypeRefTableRow : MetadataTableRow, ITypeDefOrRefOrSpec, IResolutionScope
                {
                    TypeRefTable _parent;
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    Heap.StringHeap.String _namespace;
                    public Heap.StringHeap.String Namespace { get { return _namespace; } }
                    IResolutionScope _resolutionScope;
                    public override uint Length { get { return 3; } }
                    uint _row;
                    public override uint Row { get { return _row; } }

                    internal TypeRefTableRow(TypeRefTable parent, uint row, IResolutionScope resolutionScope, Heap.StringHeap.String name, Heap.StringHeap.String @namespace)
                    {
                        _parent = parent;
                        _row = row;
                        _resolutionScope = resolutionScope;
                        _name = name;
                        _namespace = @namespace;
                    }
                    internal TypeRefTableRow(TypeRefTable parent, uint row)
                    {
                        _parent = parent;
                        _row = row;
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.StringHeap stringHeap, ModuleTable? moduleTable, ModuleRefTable? moduleRefTable, AssemblyRefTable? assemblyRefTable, System.IO.BinaryReader reader)
#else
                    internal void Load(Heap.StringHeap stringHeap, ModuleTable moduleTable, ModuleRefTable moduleRefTable, AssemblyRefTable assemblyRefTable, System.IO.BinaryReader reader)
#endif
                    {
                        uint resolutionScopeToken = 0;
                        if (ResolutionScopeLargeIndices(moduleTable, moduleRefTable, assemblyRefTable, _parent)) { resolutionScopeToken = reader.ReadUInt32(); } else { resolutionScopeToken = reader.ReadUInt16(); }
                        _resolutionScope = ResolutionScopeDecode(resolutionScopeToken, moduleTable, moduleRefTable, assemblyRefTable, _parent);
                        uint nameIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        uint namespaceIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        _namespace = new Heap.StringHeap.String(stringHeap, namespaceIndex);
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.StringHeap stringHeap, ModuleTable? moduleTable, ModuleRefTable? moduleRefTable, AssemblyRefTable? assemblyRefTable, Span<byte> data, ref uint offset)
                    {
                        uint resolutionScopeToken = 0;
                        if (ResolutionScopeLargeIndices(moduleTable, moduleRefTable, assemblyRefTable, _parent)) { resolutionScopeToken = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { resolutionScopeToken = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _resolutionScope = ResolutionScopeDecode(resolutionScopeToken, moduleTable, moduleRefTable, assemblyRefTable, _parent);
                        uint nameIndex = 0; if (stringHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        uint namespaceIndex = 0; if (stringHeap.LargeIndices) { namespaceIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { namespaceIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        _namespace = new Heap.StringHeap.String(stringHeap, namespaceIndex);
                    }
#endif
#if NET6_0_OR_GREATER
                    internal void Save(ModuleTable? moduleTable, ModuleRefTable? moduleRefTable, AssemblyRefTable? assemblyRefTable, BinaryWriter binaryWriter)
#else
                    internal void Save(ModuleTable moduleTable, ModuleRefTable moduleRefTable, AssemblyRefTable assemblyRefTable, BinaryWriter binaryWriter)
#endif
                    {
                        uint resolutionScopeToken = ResolutionScopeEncode(_resolutionScope);
                        if (ResolutionScopeLargeIndices(moduleTable, moduleRefTable, assemblyRefTable, _parent)) { binaryWriter.Write(resolutionScopeToken); } else { binaryWriter.Write((ushort)resolutionScopeToken); }
                        if (_name.Heap.LargeIndices) { binaryWriter.Write(_name.Index); } else { binaryWriter.Write((ushort)_name.Index); }
                        if (_namespace.Heap.LargeIndices) { binaryWriter.Write(_namespace.Index); } else { binaryWriter.Write((ushort)_namespace.Index); }
                    }
                }
                internal override void Save(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, BinaryWriter binaryWriter)
                {
                    Save(stringHeap, blobHeap, GUIDHeap, null, null, null, binaryWriter);
                }
#if NET6_0_OR_GREATER
                internal void Save(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, ModuleTable? moduleTable, ModuleRefTable? moduleRefTable, AssemblyRefTable? assemblyRefTable, BinaryWriter binaryWriter)
#else
                internal void Save(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, ModuleTable moduleTable, ModuleRefTable moduleRefTable, AssemblyRefTable assemblyRefTable, BinaryWriter binaryWriter)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(moduleTable, moduleRefTable, assemblyRefTable, binaryWriter);
                    }
                }
                public override int ID { get { return 0x01; } }
                public override uint Columns { get { return 3; } }
                public override uint Rows { get { lock (this) { return (uint)_rows.Count; } } }
                public override bool Sorted { get { return false; } }
                public TypeRefTableRow Add(IResolutionScope resolutionScope, Heap.StringHeap.String name, Heap.StringHeap.String @namespace)
                {
                    lock (this)
                    {
                        TypeRefTableRow row = new TypeRefTableRow(this, (uint)(_rows.Count + 1), resolutionScope, name, @namespace);
                        _rows.Add(row);
                        return row;
                    }
                }
                public TypeRefTable()
                {
                }
                internal TypeRefTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new TypeRefTableRow(this, (uint)(n + 1)));
                    }
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.StringHeap stringHeap, ModuleTable? moduleTable, ModuleRefTable? moduleRefTable, AssemblyRefTable? assemblyRefTable, System.IO.BinaryReader reader)
#else
                internal void Load(Heap.StringHeap stringHeap, ModuleTable moduleTable, ModuleRefTable moduleRefTable, AssemblyRefTable assemblyRefTable, System.IO.BinaryReader reader)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Load(stringHeap, moduleTable, moduleRefTable, assemblyRefTable, reader);
                    }
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.StringHeap stringHeap, ModuleTable? moduleTable, ModuleRefTable? moduleRefTable, AssemblyRefTable? assemblyRefTable, Span<byte> data, ref uint offset)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Load(stringHeap, moduleTable, moduleRefTable, assemblyRefTable, data, ref offset);
                    }
                }
#endif
            }
        }
    }
}