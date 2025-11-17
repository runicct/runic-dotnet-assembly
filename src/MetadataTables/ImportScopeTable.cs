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
            public class ImportScopeTable : MetadataTable
            {
                public class ImportScopeTableRow : MetadataTableRow
                {
                    public override uint Length { get { return 2; } }
                    ImportScopeTable _table;
                    internal ImportScopeTable Table { get { return _table; } }
#if NET6_0_OR_GREATER
                    ImportScopeTableRow? _parent = null;
                    public ImportScopeTableRow? Parent { get { return _parent; } }
#else
                    ImportScopeTableRow _parent = null;
                    public ImportScopeTableRow Parent { get { return _parent; } }
#endif
                    Heap.BlobHeap.Blob _imports;
                    public Heap.BlobHeap.Blob Import { get { return _imports; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
#if NET6_0_OR_GREATER
                    internal ImportScopeTableRow(ImportScopeTable table, uint row, ImportScopeTableRow? parent, Heap.BlobHeap.Blob imports)
#else
                    internal ImportScopeTableRow(ImportScopeTable table, uint row, ImportScopeTableRow parent, Heap.BlobHeap.Blob imports)
#endif
                    {
                        _table = table;
                        _row = row;
                        _imports = imports;
                        _parent = parent;
                    }

                    internal ImportScopeTableRow(ImportScopeTable table, uint row)
                    {
                        _table = table;
                        _row = row;
                    }
                    internal void Load(ImportScopeTable parent, Heap.BlobHeap blobHeap, System.IO.BinaryReader reader)
                    {
                        uint parentIndex = parent.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        if (parentIndex != 0) { _parent = parent[(uint)parentIndex]; }
                        uint importsIndex = blobHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _imports = new Heap.BlobHeap.Blob(blobHeap, importsIndex);
                    }
#if NET6_0_OR_GREATER
                    internal void Load(ImportScopeTable parent, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                    {
                        uint parentIndex = 0;
                        if (parent.LargeIndices) { parentIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { parentIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        if (parentIndex != 0) { _parent = parent[(uint)parentIndex]; }
                        uint importsIndex = 0;
                        if (blobHeap.LargeIndices) { importsIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { importsIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _imports = new Heap.BlobHeap.Blob(blobHeap, importsIndex);
                    }
#endif
                    internal void Save(BinaryWriter binaryWriter)
                    {
                        if (_parent == null)
                        {
                            if (_table.LargeIndices) { binaryWriter.Write((uint)0); } else { binaryWriter.Write((ushort)0); }
                        }
                        else
                        {
                            if (_table.LargeIndices) { binaryWriter.Write((uint)_parent.Row); } else { binaryWriter.Write((ushort)_parent.Row); }
                        }
                        if (_imports.Heap.LargeIndices) { binaryWriter.Write((uint)_imports.Index); } else { binaryWriter.Write((ushort)_imports.Index); }
                    }
                }
                public override int ID { get { return 0x35; } }
                public override uint Columns { get { return 2; } }
                public override uint Rows { get { lock (this) { return (uint)_rows.Count; } } }
                public override bool Sorted { get { return false; } }
                internal override void Save(BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                List<ImportScopeTableRow> _rows = new List<ImportScopeTableRow>();
                public ImportScopeTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
#if NET6_0_OR_GREATER
                public ImportScopeTableRow Add(ImportScopeTableRow? parent, Heap.BlobHeap.Blob import)
#else
                public ImportScopeTableRow Add(ImportScopeTableRow parent, Heap.BlobHeap.Blob import)
#endif
                {
                    lock (this)
                    {
                        ImportScopeTableRow row = new ImportScopeTableRow(this, (uint)(_rows.Count + 1), parent, import);
                        _rows.Add(row);
                        return row;
                    }
                }
                public ImportScopeTable()
                {
                }
                internal ImportScopeTable(uint rows)
                {
                    _rows = new List<ImportScopeTableRow>();
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new ImportScopeTableRow(this, n + 1));
                    }
                }
                internal void Load(Heap.BlobHeap blobHeap, System.IO.BinaryReader reader)
                {
                    for (uint n = 0; n < _rows.Count; n++)
                    {
                        _rows[(int)n].Load(this, blobHeap, reader);
                    }
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                {
                    for (uint n = 0; n < _rows.Count; n++)
                    {
                        _rows[(int)n].Load(this, blobHeap, data, ref offset);
                    }
                }
#endif
            }
        }
    }
}
