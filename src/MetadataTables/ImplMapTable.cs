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
            public class ImplMapTable : MetadataTable
            {
                List<ImplMapTableRow> _rows = new List<ImplMapTableRow>();
                public override int ID { get { return 0x1C; } }
                public override uint Columns { get { return 4; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public ImplMapTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public class ImplMapTableRow : MetadataTableRow
                {
                    public override uint Length { get { return 4; } }
                    ushort _flags;
                    uint _memberForwardedToken;
                    public uint MemberForwardedToken { get { return _memberForwardedToken; } }
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    uint _importScope;
                    public uint ImportScope { get { return _importScope; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    public ImplMapTableRow(uint row, ushort flags, uint memberForwardedToken, Heap.StringHeap.String name, uint importScope)
                    {
                        _row = row;
                        _flags = flags;
                        _name = name;
                        _importScope = importScope;
                        _memberForwardedToken = memberForwardedToken;
                    }
                    public ImplMapTableRow(uint row, Heap.StringHeap stringHeap, System.IO.BinaryReader reader)
                    {
                        _row = row;
                        _flags = reader.ReadUInt16();
                        _memberForwardedToken = reader.ReadUInt16();
                        uint nameIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        _importScope = reader.ReadUInt16();
                    }
#if NET6_0_OR_GREATER
                    public ImplMapTableRow(uint row, Heap.StringHeap stringHeap, Span<byte> data, ref uint offset)
                    {
                        _row = row;
                        _flags = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        _memberForwardedToken = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        uint nameIndex = 0; if (stringHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        _importScope = BitConverterLE.ToUInt16(data, offset); offset += 2;
                    }
#endif
                    internal void Save(BinaryWriter binaryWriter)
                    {
                        binaryWriter.Write(_flags);
                        binaryWriter.Write((ushort)_memberForwardedToken);
                        binaryWriter.Write(_name.Index);
                        binaryWriter.Write((ushort)_importScope);
                    }
                }
                public uint Add(ushort flags, uint memberForwardToken, Heap.StringHeap.String name, uint importScope)
                {
                    lock (this)
                    {
                        _rows.Add(new ImplMapTableRow((uint)(_rows.Count + 1), flags, memberForwardToken, name, importScope));
                        return (uint)(_rows.Count);
                    }
                }
                internal override void Save(BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                internal ImplMapTable()
                {
                }
                internal ImplMapTable(uint rows, Heap.StringHeap stringHeap, System.IO.BinaryReader reader)
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new ImplMapTableRow((uint)(n + 1), stringHeap, reader));
                    }
                }
#if NET6_0_OR_GREATER
                internal ImplMapTable(uint rows, Heap.StringHeap stringHeap, Span<byte> data, ref uint offset)
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new ImplMapTableRow((uint)(n + 1), stringHeap, data, ref offset));
                    }
                }
#endif
            }
        }
    }
}
