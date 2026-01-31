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
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public partial class MetadataTable
        {
            public class FieldTable : MetadataTable
            {
                List<FieldTableRow> _rows = new List<FieldTableRow>();
                public override int ID { get { return 0x04; } }
                public override uint Columns { get { return 3; } }
                public override uint Rows { get { lock (this) { return (uint)_rows.Count; } } }
                public override bool Sorted { get { return false; } }
                public FieldTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }

                public class FieldTableRow : MetadataTableRow, IHasCustomAttribute, IHasConstant, IHasFieldMarshal
                {
                    int _id;
                    FieldTable _parent;
                    public FieldTable Parent { get { return _parent; } }
                    FieldAttributes _attributes;
                    public FieldAttributes Attributes { get { return _attributes; } internal set { _attributes = value; } }
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    Heap.BlobHeap.Blob _signature;
                    public Heap.BlobHeap.Blob Signature { get { return _signature; } }
                    public override uint Length { get { return 3; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    internal FieldTableRow(FieldTable parent, uint row)
                    {
                        _row = row;
                        _parent = parent;
                    }
                    internal FieldTableRow(FieldTable parent, uint row, FieldAttributes fieldAttribute, Heap.StringHeap.String name, Heap.BlobHeap.Blob signature)
                    {
                        _row = row;
                        _attributes = fieldAttribute;
                        _name = name;
                        _signature = signature;
                        _parent = parent;
                    }
                    internal void Load(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, BinaryReader reader)
                    {
                        _attributes = (FieldAttributes)reader.ReadUInt16();
                        uint nameIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        uint blobIndex = blobHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        _signature = new Heap.BlobHeap.Blob(blobHeap, blobIndex);
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                    {
                        _attributes = (FieldAttributes)BitConverterLE.ToUInt16(data, offset); offset += 2;
                        uint nameIndex = 0; if (stringHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        uint blobIndex = 0; if (blobHeap.LargeIndices) { blobIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { blobIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        _signature = new Heap.BlobHeap.Blob(blobHeap, blobIndex);
                    }
#endif
                    internal void Save(BinaryWriter writer)
                    {
                        writer.Write((ushort)_attributes);
                        if (_name.Heap.LargeIndices) { writer.Write((uint)_name.Index); } else { writer.Write((ushort)_name.Index); }
                        if (_signature.Heap.LargeIndices) { writer.Write((uint)_signature.Index); } else { writer.Write((ushort)_signature.Index); }
                    }
                }
                internal void Save(BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                public FieldTableRow Add(FieldAttributes attributes, Heap.StringHeap.String name, Heap.BlobHeap.Blob signature)
                {
                    lock (this)
                    {
                        FieldTableRow row = new FieldTableRow(this, (uint)(_rows.Count + 1), attributes, name, signature);
                        _rows.Add(row);
                        return row;
                    }
                }
                public FieldTable()
                {
                }
                internal FieldTable(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, BinaryReader reader)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++)
                    {
                        _rows[n].Load(stringHeap, blobHeap, reader);
                    }
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++)
                    {
                        _rows[n].Load(stringHeap, blobHeap, data, ref offset);
                    }
                }
#endif
                internal void Load(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, BinaryReader reader)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++)
                    {
                        _rows[n].Load(stringHeap, blobHeap, reader);
                    }
                }
                internal FieldTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new FieldTableRow(this, (uint)(n + 1)));
                    }
                }
            }
        }
    }
}