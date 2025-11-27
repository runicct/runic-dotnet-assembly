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
            public class CustomAttributeTable : MetadataTable
            {
                List<CustomAttributeTableRow> _rows = new List<CustomAttributeTableRow>();
                public override int ID { get { return 0x0C; } }
                public override uint Columns { get { return 3; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public CustomAttributeTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public class CustomAttributeTableRow : MetadataTableRow
                {
                    public override uint Length { get { return 3; } }
                    ushort _constructorToken;
                    public ushort ConstructorToken { get { return _constructorToken; } }
                    ushort _hasCustomAttributeToken;
                    public ushort HasCustomAttributeToken { get { return _hasCustomAttributeToken; } }
                    Heap.BlobHeap.Blob _value;
                    public Heap.BlobHeap.Blob Value { get { return _value; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    public CustomAttributeTableRow(uint row, ushort hasCustomAttributeToken, ushort constructorToken, Heap.BlobHeap.Blob value)
                    {
                        _row = row;
                        _hasCustomAttributeToken = hasCustomAttributeToken;
                        _constructorToken = constructorToken;
                        _value = value;
                    }
                    public CustomAttributeTableRow(uint row, Heap.BlobHeap blobHeap, System.IO.BinaryReader reader)
                    {
                        _row = row;
                        _hasCustomAttributeToken = reader.ReadUInt16();
                        _constructorToken = reader.ReadUInt16();
                        uint valueIndex = blobHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _value = new Heap.BlobHeap.Blob(blobHeap, valueIndex);
                    }
#if NET6_0_OR_GREATER
                    public CustomAttributeTableRow(uint row, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                    {
                        _row = row;
                        _hasCustomAttributeToken = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        _constructorToken = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        uint valueIndex = 0; if (blobHeap.LargeIndices) { valueIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { valueIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                            _value = new Heap.BlobHeap.Blob(blobHeap, valueIndex);
                    }
#endif
                    internal void Save(BinaryWriter binaryWriter)
                    {
                        binaryWriter.Write(_hasCustomAttributeToken);
                        binaryWriter.Write(_constructorToken);
                        binaryWriter.Write(_value.Index);
                    }
                }
                public CustomAttributeTableRow Add(ushort hasCustomAttributeToken, ushort constructorToken, Heap.BlobHeap.Blob value)
                {
                    lock (this)
                    {
                        CustomAttributeTableRow row = new CustomAttributeTableRow((uint)(_rows.Count + 1), hasCustomAttributeToken, constructorToken, value);
                        _rows.Add(row);
                        return row;
                    }
                }
                internal override void Save(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                public CustomAttributeTable()
                {
                }
                internal CustomAttributeTable(uint rows, Heap.BlobHeap blobHeap, System.IO.BinaryReader reader)
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new CustomAttributeTableRow(n + 1, blobHeap, reader));
                    }
                }
#if NET6_0_OR_GREATER

                internal CustomAttributeTable(uint rows, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new CustomAttributeTableRow(n + 1, blobHeap, data, ref offset));
                    }
                }
#endif
            }
        }
    }
}
