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

using System.Collections.Generic;
using System.IO;
using static Runic.Dotnet.Assembly.MetadataTable;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public partial class MetadataTable
        {
            public class MethodDebugInformationTable : MetadataTable
            {
                List<MethodDebugInformationTableRow> _rows = new List<MethodDebugInformationTableRow>();
                public class MethodDebugInformationTableRow : MetadataTableRow
                {
                    uint _row;
                    public override uint Row { get { return _row; } }
#if NET6_0_OR_GREATER
                    Heap.BlobHeap.Blob? _sequencePoint;
                    public Heap.BlobHeap.Blob? SequencePoint { get { return _sequencePoint; } }
                    DocumentTable.DocumentTableRow? _document;
                    public DocumentTable.DocumentTableRow? Document { get { return _document; } }
#else
                    Heap.BlobHeap.Blob _sequencePoint;
                    public Heap.BlobHeap.Blob SequencePoint { get { return _sequencePoint; } }
                    DocumentTable.DocumentTableRow _document;
                    public DocumentTable.DocumentTableRow Document { get { return _document; } }
#endif
                    public override uint Length { get { return 0x02; } }
#if NET6_0_OR_GREATER
                    internal MethodDebugInformationTableRow(uint row, DocumentTable.DocumentTableRow? document, Heap.BlobHeap.Blob? sequencePoint)
#else
                    internal MethodDebugInformationTableRow(uint row, DocumentTable.DocumentTableRow document, Heap.BlobHeap.Blob sequencePoint)
#endif
                    {
                        _row = row;
                        _document = document;
                        _sequencePoint = sequencePoint;
                    }
                    internal MethodDebugInformationTableRow(uint row, DocumentTable documentTable, Heap.BlobHeap blobHeap, System.IO.BinaryReader reader)
                    {
                        _row = row;
                        uint documentIndex = documentTable.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _document = documentIndex == 0 ? null : documentTable[documentIndex];
                        uint sequencePointIndex = blobHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _sequencePoint = sequencePointIndex == 0 ? null : new Heap.BlobHeap.Blob(blobHeap, sequencePointIndex);
                    }
#if NET6_0_OR_GREATER
                    internal MethodDebugInformationTableRow(uint row, DocumentTable documentTable, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset)
                    {
                        _row = row;
                        uint documentIndex = 0; if (documentTable.LargeIndices) { documentIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { documentIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        uint sequencePointIndex = 0; if (blobHeap.LargeIndices) { sequencePointIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { sequencePointIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _document = documentIndex == 0 ? null : documentTable[documentIndex];
                        _sequencePoint = sequencePointIndex == 0 ? null : new Heap.BlobHeap.Blob(blobHeap, sequencePointIndex);
                    }
#endif
                    internal void Save(BinaryWriter binaryWriter)
                    {
                    }
                }
                public override int ID { get { return 0x31; } }
                public override uint Columns { get { return 2; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public MethodDebugInformationTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public override bool Sorted { get { return false; } }
                internal override void Save(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
#if NET6_0_OR_GREATER
                internal MethodDebugInformationTableRow Add(DocumentTable.DocumentTableRow? document, Heap.BlobHeap.Blob? sequencePoint)
#else
                internal MethodDebugInformationTableRow Add(DocumentTable.DocumentTableRow document, Heap.BlobHeap.Blob sequencePoint)
#endif
                {
                    lock (this)
                    {
                        MethodDebugInformationTableRow row = new MethodDebugInformationTableRow((uint)(_rows.Count + 1), document, sequencePoint);
                        _rows.Add(row);
                        return row;
                    }
                }
                public MethodDebugInformationTable() : base()
                {
                }
                internal MethodDebugInformationTable(uint rows, DocumentTable documentTable, Heap.BlobHeap blobHeap, System.IO.BinaryReader reader) : base()
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new MethodDebugInformationTableRow((uint)(n + 1), documentTable, blobHeap, reader));
                    }
                }
#if NET6_0_OR_GREATER

                internal MethodDebugInformationTable(uint rows, DocumentTable documentTable, Heap.BlobHeap blobHeap, Span<byte> data, ref uint offset) : base()
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new MethodDebugInformationTableRow((uint)(n + 1), documentTable, blobHeap, data, ref offset));
                    }
                }
#endif
            }
        }
    }
}
