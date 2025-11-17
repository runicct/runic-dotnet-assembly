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
namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public partial class MetadataTable
        {
            public class LocalScopeTable : MetadataTable
            {
                List<LocalScopeTableRow> _rows = new List<LocalScopeTableRow>();
                public class LocalScopeTableRow : MetadataTableRow
                {
                    uint _row;
                    public override uint Row { get { return _row; } }
                    MethodDefTable.MethodDefTableRow _method;
                    public MethodDefTable.MethodDefTableRow Method { get { return _method; } }
                    ImportScopeTable.ImportScopeTableRow _importScope;
                    public ImportScopeTable.ImportScopeTableRow ImportScope { get { return _importScope; } }
                    LocalVariableTable.LocalVariableTableRow _variableList;
                    public LocalVariableTable.LocalVariableTableRow VariableList { get { return _variableList; } }
                    LocalConstantTable.LocalConstantTableRow _constantList;
                    public LocalConstantTable.LocalConstantTableRow ConstantList { get { return _constantList; } }
                    uint _startOffset;
                    public uint StartOffset { get { return _startOffset; } }
                    uint _length;
                    public uint ScopeLength { get { return _length; } }
                    public override uint Length { get { return 0x06; } }
                    internal LocalScopeTableRow(uint row, MethodDefTable.MethodDefTableRow method, ImportScopeTable.ImportScopeTableRow importScope, LocalVariableTable.LocalVariableTableRow variableList, LocalConstantTable.LocalConstantTableRow constantList, uint startOffset, uint length)
                    {
                        _row = row;
                        _method = method;
                        _importScope = importScope;
                        _variableList = variableList;
                        _constantList = constantList;
                        _startOffset = startOffset;
                        _length = length;
                    }
                    internal LocalScopeTableRow(uint row, MethodDefTable methodTable, ImportScopeTable importScopeTable, LocalVariableTable localVariableTable, LocalConstantTable localConstantTable, System.IO.BinaryReader reader)
                    {
                        _row = row;
                        uint methodIndex = methodTable.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _method = methodTable[methodIndex];
                        uint importScopeIndex = importScopeTable.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _importScope = importScopeTable[importScopeIndex];
                        uint variableListIndex = localVariableTable.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _variableList = localVariableTable[variableListIndex];
                        uint constantListIndex = localConstantTable.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _constantList = localConstantTable[constantListIndex];
                        _startOffset = reader.ReadUInt32();
                        _length = reader.ReadUInt32();
                    }
#if NET6_0_OR_GREATER
                    internal LocalScopeTableRow(uint row, MethodDefTable methodTable, ImportScopeTable importScopeTable, LocalVariableTable localVariableTable, LocalConstantTable localConstantTable, Span<byte> data, ref uint offset)
                    {
                        _row = row;
                        uint methodIndex = 0;
                        if (methodTable.LargeIndices) { methodIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { methodIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _method = methodTable[methodIndex];
                        uint importScopeIndex = 0;
                        if (importScopeTable.LargeIndices) { importScopeIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { importScopeIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }   
                        _importScope = importScopeTable[importScopeIndex];
                        uint variableListIndex = 0;
                        if (localVariableTable.LargeIndices) { variableListIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { variableListIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _variableList = localVariableTable[variableListIndex];
                        uint constantListIndex = 0;
                        if (localConstantTable.LargeIndices) { constantListIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { constantListIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }   
                        _constantList = localConstantTable[constantListIndex];
                        _startOffset = BitConverterLE.ToUInt32(data, offset); offset += 4;
                        _length = BitConverterLE.ToUInt32(data, offset); offset += 4;
                    }
#endif
                    internal void Save(BinaryWriter binaryWriter)
                    {
                        if (_method.Parent.LargeIndices) { binaryWriter.Write((uint)_method.Row); } else { binaryWriter.Write((ushort)_method.Row); }
                        if (_importScope.Table.LargeIndices) { binaryWriter.Write((uint)_importScope.Row); } else { binaryWriter.Write((ushort)_importScope.Row); }
                        if (_variableList.Parent.LargeIndices) { binaryWriter.Write((uint)_variableList.Row); } else { binaryWriter.Write((ushort)_variableList.Row); }
                        if (_constantList.Parent.LargeIndices) { binaryWriter.Write((uint)_constantList.Row); } else { binaryWriter.Write((ushort)_constantList.Row); }
                        binaryWriter.Write(_startOffset);
                        binaryWriter.Write(_length);
                    }
                }
                public override int ID { get { return 0x33; } }
                public override uint Columns { get { return 3; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public LocalScopeTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public override bool Sorted { get { return false; } }
                internal override void Save(BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                public LocalScopeTableRow Add(MethodDefTable.MethodDefTableRow method, ImportScopeTable.ImportScopeTableRow importScope, LocalVariableTable.LocalVariableTableRow variableList, LocalConstantTable.LocalConstantTableRow constantList, uint startOffset, uint length)
                {
                    lock (this)
                    {
                        LocalScopeTableRow row = new LocalScopeTableRow((uint)(_rows.Count + 1), method, importScope, variableList, constantList, startOffset, length);
                        _rows.Add(row);
                        return row;
                    }
                }
                public LocalScopeTable() : base()
                {
                }
                internal LocalScopeTable(uint rows, MethodDefTable methodTable, ImportScopeTable importScopeTable, LocalVariableTable localVariableTable, LocalConstantTable localConstantTable, System.IO.BinaryReader reader)
                {
                    _rows = new List<LocalScopeTableRow>();
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new LocalScopeTableRow((uint)(n +1), methodTable, importScopeTable, localVariableTable, localConstantTable, reader));
                    }
                }
#if NET6_0_OR_GREATER

                internal LocalScopeTable(uint rows, MethodDefTable methodTable, ImportScopeTable importScopeTable, LocalVariableTable localVariableTable, LocalConstantTable localConstantTable, Span<byte> data, ref uint offset)
                {
                    _rows = new List<LocalScopeTableRow>();
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new LocalScopeTableRow((uint)(n + 1), methodTable, importScopeTable, localVariableTable, localConstantTable, data, ref offset));
                    }
                }
#endif
            }
        }
    }
}
