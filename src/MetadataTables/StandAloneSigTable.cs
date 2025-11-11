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
            public class StandAloneSigTable : MetadataTable
            {
                public class StandAloneSigTableRow : MetadataTableRow
                {
                    Heap.BlobHeap.Blob _signature;
                    public Heap.BlobHeap.Blob Signature { get { return _signature; } }
                    public override uint Length { get { return 1; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    internal StandAloneSigTableRow(uint row, Heap.BlobHeap.Blob signature)
                    {
                        _row = row;
                        _signature = signature;
                    }
                    internal StandAloneSigTableRow(uint row, Heap.BlobHeap blobHeap, System.IO.BinaryReader reader)
                    {
                        _row = row;
                        uint signatureIndex = blobHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _signature = new Heap.BlobHeap.Blob(blobHeap, signatureIndex);
                    }
#if NET6_0_OR_GREATER
                    internal StandAloneSigTableRow(uint row, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                    {
                        _row = row;
                        uint signatureIndex = 0; if (blobHeap.LargeIndices) { signatureIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { signatureIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _signature = new Heap.BlobHeap.Blob(blobHeap, signatureIndex);
                    }
#endif
                    internal void Save(BinaryWriter binaryWriter)
                    {
                        binaryWriter.Write(_signature.Index);
                    }
                }
                public override int ID { get { return 0x11; } }
                public override uint Columns { get { return 1; } }
                public override uint Rows { get { lock (this) { return (uint)_rows.Count; } } }
                public override bool Sorted { get { return false; } }
                public StandAloneSigTableRow this[int row]
                {
                    get { lock (this) { return _rows[row - 1]; } }
                }
                internal override void Save(BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                List<StandAloneSigTableRow> _rows = new List<StandAloneSigTableRow>();
                public StandAloneSigTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public uint Add(Heap.BlobHeap.Blob signature)
                {
                    lock (this)
                    {
                        StandAloneSigTableRow row = new StandAloneSigTableRow((uint)(_rows.Count + 1), signature);
                        _rows.Add(row);
                        return (uint)_rows.Count;
                    }
                }
                internal StandAloneSigTable()
                {

                }
                internal StandAloneSigTable(uint rows, Heap.BlobHeap blobHeap, System.IO.BinaryReader reader)
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new StandAloneSigTableRow((uint)(n + 1), blobHeap, reader));
                    }
                }
#if NET6_0_OR_GREATER
                internal StandAloneSigTable(uint rows, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new StandAloneSigTableRow((uint)(n + 1), blobHeap, data, ref offset));
                    }
                }
#endif
            }
        }
    }
}
