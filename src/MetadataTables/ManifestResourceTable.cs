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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public partial class MetadataTable
        {
            public class ManifestResourceTable : MetadataTable
            {
                List<ManifestResourceTableRow> _rows = new List<ManifestResourceTableRow>();
                public override int ID { get { return 0x28; } }
                public override uint Columns { get { return 4; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public ManifestResourceTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public ManifestResourceTableRow Add(uint offset, ManifestResourceAttributes flags, Heap.StringHeap.String name, IImplementation implementation)
                {
                    lock (this)
                    {
                        ManifestResourceTableRow row = new ManifestResourceTableRow(this, (uint)(_rows.Count + 1), offset, flags, name, implementation);
                        _rows.Add(row);
                        return row;
                    }
                }
                public class ManifestResourceTableRow : MetadataTableRow, IHasCustomAttribute
                {
                    ManifestResourceTable _parent;
                    internal ManifestResourceTable Parent { get { return _parent; } }
                    public override uint Length { get { return 4; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    uint _offset;
                    public uint Offset { get { return _offset; } }
                    ManifestResourceAttributes _flags;
                    public ManifestResourceAttributes Flags { get { return _flags; } }
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    IImplementation _implementation;
                    public IImplementation Implementation { get { return _implementation; } }
                    internal ManifestResourceTableRow(ManifestResourceTable parent, uint row, uint offset, ManifestResourceAttributes flags, Heap.StringHeap.String name, IImplementation implementation)
                    {
                        _parent = parent;
                        _row = row;
                        _offset = offset;
                        _flags = flags;
                        _name = name;
                        _implementation = implementation;
                    }
                    internal ManifestResourceTableRow(ManifestResourceTable parent, uint row)
                    {
                        _parent = parent;
                        _row = row;
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.StringHeap stringHeap, FileTable? fileTable, AssemblyRefTable? assemblyRefTable, ExportedTypeTable? exportedTypeTable, BinaryReader reader)
#else
                    internal void Load(Heap.StringHeap stringHeap, FileTable fileTable, AssemblyRefTable assemblyRefTable, ExportedTypeTable exportedTypeTable,BinaryReader reader)
#endif
                    {
                        _offset = reader.ReadUInt32();
                        _flags = (ManifestResourceAttributes)reader.ReadUInt32();
                        uint nameIndex;
                        if (stringHeap.LargeIndices) { nameIndex = reader.ReadUInt32(); } else { nameIndex = reader.ReadUInt16(); }
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        uint implementationIndex = 0;
                        if (ImplementationLargeIndices(fileTable, assemblyRefTable, exportedTypeTable)) { implementationIndex = reader.ReadUInt32(); } else { implementationIndex = reader.ReadUInt16(); }
                        _implementation = ImplementationDecode(implementationIndex, fileTable, assemblyRefTable, exportedTypeTable);
                    }
#if NET6_0_OR_GREATER

                    internal void Load(Heap.StringHeap stringHeap, FileTable? fileTable, AssemblyRefTable? assemblyRefTable, ExportedTypeTable? exportedTypeTable, Span<byte> data, ref uint offset)
                    {
                        _offset = BitConverterLE.ToUInt32(data, offset); offset += 4;
                        _flags = (ManifestResourceAttributes)BitConverterLE.ToUInt32(data, offset); offset += 4;
                        uint nameIndex;
                        if (stringHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        uint implementationIndex = 0;
                        if (ImplementationLargeIndices(fileTable, assemblyRefTable, exportedTypeTable)) { implementationIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { implementationIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _implementation = ImplementationDecode(implementationIndex, fileTable, assemblyRefTable, exportedTypeTable);
                    }
#endif

#if NET6_0_OR_GREATER
                    internal void Save(FileTable? fileTable, AssemblyRefTable? assemblyRefTable, ExportedTypeTable? exportedTypeTable, BinaryWriter binaryWriter)
#else
                    internal void Save(FileTable fileTable, AssemblyRefTable assemblyRefTable, ExportedTypeTable exportedTypeTable, BinaryWriter binaryWriter)
#endif
                    {
                        binaryWriter.Write(_offset);
                        binaryWriter.Write((uint)_flags);
                        if (_name.Heap.LargeIndices) { binaryWriter.Write((uint)_name.Index); } else { binaryWriter.Write((ushort)_name.Index); }
                        uint implementationIndex = ImplementationEncode(_implementation);
                        if (ImplementationLargeIndices(fileTable, assemblyRefTable, exportedTypeTable)) { binaryWriter.Write((uint)implementationIndex); } else { binaryWriter.Write((ushort)implementationIndex); }
                    }
                }
#if NET6_0_OR_GREATER
                internal void Save(FileTable? fileTable, AssemblyRefTable? assemblyRefTable, ExportedTypeTable? exportedTypeTable, BinaryWriter binaryWriter)
#else
                internal void Save(FileTable fileTable, AssemblyRefTable assemblyRefTable, ExportedTypeTable exportedTypeTable, BinaryWriter binaryWriter)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(fileTable, assemblyRefTable, exportedTypeTable, binaryWriter);
                    }
                }
                public ManifestResourceTable()
                {
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.StringHeap stringHeap, FileTable? fileTable, AssemblyRefTable? assemblyRefTable, ExportedTypeTable? exportedTypeTable, BinaryReader reader)
#else
                internal void Load(Heap.StringHeap stringHeap, FileTable fileTable, AssemblyRefTable assemblyRefTable, ExportedTypeTable exportedTypeTable, BinaryReader reader)
#endif
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(stringHeap, fileTable, assemblyRefTable, exportedTypeTable, reader); }
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.StringHeap stringHeap, FileTable? fileTable, AssemblyRefTable? assemblyRefTable, ExportedTypeTable? exportedTypeTable, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(stringHeap, fileTable, assemblyRefTable, exportedTypeTable, data, ref offset); }
                }
#endif
                internal ManifestResourceTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new ManifestResourceTableRow(this, (uint)(_rows.Count + 1)));
                    }
                }
            }
        }
    }
}
