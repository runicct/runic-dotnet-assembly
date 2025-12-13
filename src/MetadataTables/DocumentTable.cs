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

using System.IO;
using System.Collections.Generic;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public partial class MetadataTable
        {
            public class DocumentTable : MetadataTable
            {
                List<DocumentTableRow> _rows = new List<DocumentTableRow>();
                public class DocumentTableRow : MetadataTableRow
                {
                    uint _row;
                    public override uint Row { get { return _row; } }
                    Heap.BlobHeap.Blob _name;
                    public Heap.BlobHeap.Blob Name { get { return _name; } }
                    Heap.BlobHeap.Blob _hash;
                    public Heap.BlobHeap.Blob Hash { get { return _hash; } }
                    Heap.BlobHeap.GUIDHeap.GUID _hashAlgorithm;
                    public Heap.BlobHeap.GUIDHeap.GUID HashAlgorithm { get { return _hashAlgorithm; } }
                    Heap.BlobHeap.GUIDHeap.GUID _language;
                    public Heap.BlobHeap.GUIDHeap.GUID Language { get { return _language; } }
                    public override uint Length { get { return 0x04; } }
                    internal DocumentTableRow(uint row, Heap.BlobHeap.Blob name, Heap.BlobHeap.GUIDHeap.GUID hashAlgorithm, Heap.BlobHeap.Blob hash, Heap.BlobHeap.GUIDHeap.GUID language)
                    {
                        _row = row;
                        _name = name;
                        _hash = hash;
                        _hashAlgorithm = hashAlgorithm;
                        _language = language;
                    }
                    internal DocumentTableRow(uint row, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, System.IO.BinaryReader reader)
                    {
                        _row = row;
                        _name = new Heap.BlobHeap.Blob(blobHeap, blobHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16());
                        _hashAlgorithm = new Heap.BlobHeap.GUIDHeap.GUID(GUIDHeap, GUIDHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16());
                        _hash = new Heap.BlobHeap.Blob(blobHeap, blobHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16());
                        _language = new Heap.BlobHeap.GUIDHeap.GUID(GUIDHeap, GUIDHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16());
                    }
#if NET6_0_OR_GREATER
                    internal DocumentTableRow(uint row, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, Span<byte> data, ref uint offset)
                    {
                        _row = row;

                        uint nameIndex = 0; if (blobHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _name = new Heap.BlobHeap.Blob(blobHeap, nameIndex);
                        uint hashAlgorithmIndex = 0; if (blobHeap.LargeIndices) { hashAlgorithmIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { hashAlgorithmIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _hashAlgorithm = new Heap.BlobHeap.GUIDHeap.GUID(GUIDHeap, hashAlgorithmIndex);
                        uint hashIndex = 0; if (blobHeap.LargeIndices) { hashIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { hashIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _hash = new Heap.BlobHeap.Blob(blobHeap, hashIndex);
                        uint languageIndex = 0; if (blobHeap.LargeIndices) { languageIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { languageIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _language = new Heap.BlobHeap.GUIDHeap.GUID(GUIDHeap, languageIndex);
                    }
#endif
                    internal void Save(BinaryWriter binaryWriter)
                    {
                        if (_name.Heap.LargeIndices) { binaryWriter.Write((uint)_name.Index); } else { binaryWriter.Write((ushort)_name.Index); }
                        if (_hashAlgorithm.Heap.LargeIndices) { binaryWriter.Write((uint)_hashAlgorithm.Index); } else { binaryWriter.Write((ushort)_hashAlgorithm.Index); }
                        if (_hash.Heap.LargeIndices) { binaryWriter.Write((uint)_hash.Index); } else { binaryWriter.Write((ushort)_hash.Index); }
                        if (_language.Heap.LargeIndices) { binaryWriter.Write((uint)_language.Index); } else { binaryWriter.Write((ushort)_language.Index); }
                    }
                }
                public override int ID { get { return 0x30; } }
                public override uint Columns { get { return 4; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public DocumentTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public override bool Sorted { get { return false; } }
                internal void Save(BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                public DocumentTableRow Add(Heap.BlobHeap.Blob name, Heap.BlobHeap.GUIDHeap.GUID hashAlgorithm, Heap.BlobHeap.Blob hash, Heap.BlobHeap.GUIDHeap.GUID language)
                {
                    lock (this)
                    {
                        DocumentTableRow row = new DocumentTableRow((uint)(_rows.Count + 1), name, hashAlgorithm, hash, language);
                        _rows.Add(row);
                        return row;
                    }
                }
                public DocumentTable() : base()
                {
                }
                internal DocumentTable(uint rows, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, System.IO.BinaryReader reader) : base()
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new DocumentTableRow((uint)(n + 1), blobHeap, GUIDHeap, reader));
                    }
                }
#if NET6_0_OR_GREATER

                internal DocumentTable(uint rows, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, Span<byte> data, ref uint offset) : base()
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new DocumentTableRow((uint)(n + 1), blobHeap, GUIDHeap, data, ref offset));
                    }
                }
#endif
            }
        }
    }
}
