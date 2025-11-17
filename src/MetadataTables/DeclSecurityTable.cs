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
            public class DeclSecurityTable : MetadataTable
            {
                List<DeclSecurityTableRow> _rows = new List<DeclSecurityTableRow>();
                public override int ID { get { return 0x0E; } }
                public override uint Columns { get { return 3; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public DeclSecurityTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public class DeclSecurityTableRow : MetadataTableRow
                {
                    DeclSecurityTable _parent;
                    public override uint Length { get { return 3; } }
                    ushort _action;
                    public ushort Action { get { return _action; } }
                    uint _hasDeclSecurityToken;
                    public uint HasDeclSecurityToken { get { return _hasDeclSecurityToken; } }
                    Heap.BlobHeap.Blob _permissionSet;
                    public Heap.BlobHeap.Blob PermissionSet { get { return _permissionSet; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    public DeclSecurityTableRow(uint row, ushort action, Heap.BlobHeap.Blob permissionSet)
                    {
                        _row = row;
                        _action = action;
                        _permissionSet = permissionSet;
                    }
                    public DeclSecurityTableRow(uint row, Heap.BlobHeap blobHeap, System.IO.BinaryReader reader)
                    {
                        _row = row;
                        _action = reader.ReadUInt16();
                        _hasDeclSecurityToken = reader.ReadUInt16();
                        uint blobIndex = blobHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _permissionSet = new Heap.BlobHeap.Blob(blobHeap, blobIndex);
                    }
#if NET6_0_OR_GREATER
                    public DeclSecurityTableRow(uint row, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                    {
                        _row = row;
                        _action = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        _hasDeclSecurityToken = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        uint blobIndex = 0; if (blobHeap.LargeIndices) { BitConverterLE.ToUInt32(data, offset); offset += 4; } else { BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _permissionSet = new Heap.BlobHeap.Blob(blobHeap, blobIndex);
                    }
#endif
                    internal void Save(BinaryWriter binaryWriter)
                    {
                        binaryWriter.Write(_action);
                        binaryWriter.Write((ushort)_hasDeclSecurityToken);
                        binaryWriter.Write(_permissionSet.Index);
                    }
                }
                public DeclSecurityTableRow Add(ushort action, Heap.BlobHeap.Blob permissionSet)
                {
                    lock (this)
                    {
                        DeclSecurityTableRow row = new DeclSecurityTableRow((uint)(_rows.Count + 1), action, permissionSet);
                        _rows.Add(row);
                        return row;
                    }
                }
                internal override void Save(BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                internal DeclSecurityTable()
                {
                }
                internal DeclSecurityTable(uint rows, Heap.BlobHeap blobHeap, System.IO.BinaryReader reader)
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new DeclSecurityTableRow((uint)(n + 1), blobHeap, reader));
                    }
                }
#if NET6_0_OR_GREATER
                internal DeclSecurityTable(uint rows, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new DeclSecurityTableRow((uint)(n + 1), blobHeap, data, ref offset));
                    }
                }
#endif
            }
        }
    }
}