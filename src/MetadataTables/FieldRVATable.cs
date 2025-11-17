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
            public class FieldRVATable : MetadataTable
            {
                List<FieldRVATableRow> _rows = new List<FieldRVATableRow>();
                public override int ID { get { return 0x1D; } }
                public override uint Columns { get { return 2; } }
                public override uint Rows { get { lock (this) { return (uint)_rows.Count; } } }
                public override bool Sorted { get { return false; } }
                public FieldRVATableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public class FieldRVATableRow : MetadataTableRow
                {
                    uint _rva;
                    public uint RVA { get { return _rva; } }
                    FieldTable.FieldTableRow _field;
                    public FieldTable.FieldTableRow Field { get { return _field; } }
                    public override uint Length { get { return 2; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    public FieldRVATableRow(uint row, uint RVA, FieldTable.FieldTableRow Field)
                    {
                        _row = row;
                        _rva = RVA;
                        _field = Field;
                    }
                    public FieldRVATableRow(uint row, FieldTable fieldTable, BinaryReader reader)
                    {
                        _row = row;
                        _rva = reader.ReadUInt32();
                        uint fieldIndex = fieldTable.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _field = fieldTable[fieldIndex];
                    }
#if NET6_0_OR_GREATER
                    public FieldRVATableRow(uint row, FieldTable fieldTable, Span<byte> data, ref uint offset)
                    {
                        _row = row;
                        _rva = BitConverterLE.ToUInt32(data, offset); offset += 4;
                        uint fieldIndex = 0; if (fieldTable.LargeIndices) { fieldIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { fieldIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _field = fieldTable[fieldIndex];
                    }
#endif
                    internal void Save(BinaryWriter writer)
                    {
                        writer.Write(_rva);
                        if (_field.Parent.LargeIndices) { writer.Write((uint)_field.Row); } else { writer.Write((ushort)_field.Row); }
                    }
                }
                internal override void Save(BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                public FieldRVATableRow Add(uint RVA, FieldTable.FieldTableRow Field)
                {
                    lock (this)
                    {
                        FieldRVATableRow row = new FieldRVATableRow((uint)(_rows.Count + 1), RVA, Field);
                        _rows.Add(row);
                        return row;
                    }
                }
                public FieldRVATable()
                {
                }
                internal FieldRVATable(uint rows, FieldTable fieldTable, BinaryReader reader)
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new FieldRVATableRow((uint)(n + 1), fieldTable, reader));
                    }
                }
#if NET6_0_OR_GREATER
                internal FieldRVATable(uint rows, FieldTable fieldTable, Span<byte> data, ref uint offset)
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new FieldRVATableRow((uint)(n + 1), fieldTable, data, ref offset));
                    }
                }
#endif
            }
        }
    }
}
