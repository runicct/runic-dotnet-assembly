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
using System.Text;
using System.Threading.Tasks;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public partial class MetadataTable
        {
            public class FieldLayoutTable : MetadataTable
            {
                List<FieldLayoutTableRow> _rows = new List<FieldLayoutTableRow>();
                public override int ID { get { return 0x10; } }
                public override uint Columns { get { return 2; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public FieldLayoutTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public FieldLayoutTableRow Add(uint offset, FieldTable.FieldTableRow field)
                {
                    lock (this)
                    {
                        FieldLayoutTableRow row = new FieldLayoutTableRow((uint)(_rows.Count + 1), offset, field);
                        _rows.Add(row);
                        return row;
                    }
                }
                public class FieldLayoutTableRow : MetadataTableRow
                {

                    public override uint Length { get { return 2; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    uint _offset;
                    public uint Offset { get { return _offset; } }
                    FieldTable.FieldTableRow _field;
                    public FieldTable.FieldTableRow Field { get { return _field; } }
                    internal FieldLayoutTableRow(uint row, uint offset, FieldTable.FieldTableRow field)
                    {
                        _row = row;
                        _offset = offset;
                        _field = field;
                    }
                    internal FieldLayoutTableRow(uint row)
                    {
                        _row = row;
                    }
#if NET6_0_OR_GREATER
                    internal void Load(FieldTable? fieldTable, BinaryReader reader)
#else
                    internal void Load(FieldTable fieldTable, BinaryReader reader)
#endif
                    {
                        _offset = reader.ReadUInt32();
                        if (fieldTable != null)
                        {
                            uint fieldIndex;
                            if (fieldTable.LargeIndices) { fieldIndex = reader.ReadUInt32(); } else { fieldIndex = reader.ReadUInt16(); }
                            _field = fieldTable[(uint)fieldIndex];
                        }
                        else
                        {
                            throw new InvalidDataException("Field Table is required to load FieldLayout table.");
                        }
                    }
#if NET6_0_OR_GREATER

                    internal void Load(FieldTable? fieldTable, Span<byte> data, ref uint offset)
                    {
                        _offset = BitConverterLE.ToUInt32(data, offset); offset += 4;
                        if (fieldTable != null)
                        {
                            uint fieldIndex;
                            if (fieldTable.LargeIndices) { fieldIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { fieldIndex = BitConverterLE.ToUInt32(data, offset); offset += 2; }
                            _field = fieldTable[(uint)fieldIndex];
                        }
                        else
                        {
                            throw new InvalidDataException("Field Table is required to load FieldLayout table.");
                        }
                    }
#endif

#if NET6_0_OR_GREATER
                    internal void Save(BinaryWriter binaryWriter)
#else
                    internal void Save(BinaryWriter binaryWriter)
#endif
                    {
                        binaryWriter.Write(_offset);
                        if (_field.Parent.LargeIndices) { binaryWriter.Write((uint)_field.Row); } else { binaryWriter.Write((ushort)_field.Row); }
                    }
                }
#if NET6_0_OR_GREATER
                internal void Save(BinaryWriter binaryWriter)
#else
                internal void Save(BinaryWriter binaryWriter)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                public FieldLayoutTable()
                {
                }
#if NET6_0_OR_GREATER
                internal void Load(FieldTable? fieldTable, BinaryReader reader)
#else
                internal void Load(FieldTable fieldTable, BinaryReader reader)
#endif
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(fieldTable, reader); }
                }
#if NET6_0_OR_GREATER
                internal void Load(FieldTable? fieldTable, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(fieldTable, data, ref offset); }
                }
#endif
                internal FieldLayoutTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new FieldLayoutTableRow((uint)(_rows.Count + 1)));
                    }
                }
            }
        }
    }
}
