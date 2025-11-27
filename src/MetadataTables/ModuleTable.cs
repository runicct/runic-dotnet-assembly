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
            public class ModuleTable : MetadataTable
            {
                public class ModuleTableRow : MetadataTableRow
                {
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    Heap.GUIDHeap.GUID _mvid;
                    public Heap.GUIDHeap.GUID Mvid { get { return _mvid; } }
                    public override uint Length { get { return 5; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    ushort _generation;
                    public ushort Generation { get { return _generation; } }
                    internal ModuleTableRow(ModuleTable parent, uint row, Heap.StringHeap.String name, Heap.GUIDHeap.GUID mvid)
                    {
                        _row = row;
                        _name = name;
                        _mvid = mvid;
                    }
                    internal ModuleTableRow(ModuleTable parent, uint row, Heap.StringHeap stringHeap, Heap.GUIDHeap guidHeap, BinaryReader reader)
                    {
                        short generation = reader.ReadInt16();
                        uint nameIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        uint mvidIndex = guidHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        uint encIdIndex = guidHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        uint encBaseIdIndex = guidHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _row = row;
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        _mvid = new Heap.GUIDHeap.GUID(guidHeap, mvidIndex);
                    }
#if NET6_0_OR_GREATER
                    internal ModuleTableRow(ModuleTable parent, uint row, Heap.StringHeap stringHeap, Heap.GUIDHeap guidHeap, Span<byte> data, ref uint offset)
                    {
                        _generation = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        uint nameIndex = 0; if (stringHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        uint mvidIndex = 0;  if (guidHeap.LargeIndices) { mvidIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { mvidIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        uint encIdIndex = 0; if (guidHeap.LargeIndices) { encIdIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { encIdIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        uint encBaseIdIndex = 0; if (guidHeap.LargeIndices) { encBaseIdIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { encBaseIdIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _row = row;
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        _mvid = new Heap.GUIDHeap.GUID(guidHeap, mvidIndex);
                    }
#endif
                    internal void Save(Heap.GUIDHeap guidHeap, BinaryWriter binaryWriter)
                    {
                        binaryWriter.Write(_generation);
                        if (_name.Heap.LargeIndices) { binaryWriter.Write(_name.Index); } else { binaryWriter.Write((short)_name.Index); }
                        if (_mvid.Heap.LargeIndices) { binaryWriter.Write(_mvid.Index); } else { binaryWriter.Write((short)_mvid.Index); }
                        if (guidHeap.LargeIndices) { binaryWriter.Write((int)0x0); } else { binaryWriter.Write((short)0x0); }
                        if (guidHeap.LargeIndices) { binaryWriter.Write((int)0x0); } else { binaryWriter.Write((short)0x0); }
                    }
                }
                public override int ID { get { return 0; } }
                public override uint Columns { get { return 5; } }
                public override uint Rows { get { lock (this) { return (uint)_rows.Count; } } }
                public override bool Sorted { get { return false; } }
                internal override void Save(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(GUIDHeap, binaryWriter);
                    }
                }
                List<ModuleTableRow> _rows = new List<ModuleTableRow>();
                public ModuleTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public ModuleTableRow Add(Heap.StringHeap.String name, Heap.GUIDHeap.GUID mvid)
                {
                    lock (this)
                    {
                        ModuleTableRow row = new ModuleTableRow(this, (uint)(_rows.Count + 1), name, mvid);
                        _rows.Add(row);
                        return row;
                    }
                }
                public ModuleTable()
                {
                }
                internal ModuleTable(uint rows, Heap.StringHeap stringHeap, Heap.GUIDHeap guidHeap, BinaryReader reader)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new ModuleTableRow(this, (uint)(n + 1), stringHeap, guidHeap, reader));
                    }
                }
#if NET6_0_OR_GREATER
                internal ModuleTable(uint rows, Heap.StringHeap stringHeap, Heap.GUIDHeap guidHeap, Span<byte> data, ref uint offset)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new ModuleTableRow(this, (uint)(n + 1), stringHeap, guidHeap, data, ref offset));
                    }
                }
#endif
            }
        }
    }
}
