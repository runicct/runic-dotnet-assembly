/*
 * MIT License
 * 
 * Copyright (c) 2026 Runic Compiler Toolkit Contributors
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
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public partial class MetadataTable
        {
            public class StateMachineMethodTable  : MetadataTable
            {
                List<StateMachineMethodTableRow> _rows = new List<StateMachineMethodTableRow>();
                public override int ID { get { return 0x36; } }
                public override uint Columns { get { return 2; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public StateMachineMethodTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public StateMachineMethodTableRow Add(MethodDefTable.MethodDefTableRow moveNextMethod, MethodDefTable.MethodDefTableRow kickoffMethod)
                {
                    lock (this)
                    {
                        StateMachineMethodTableRow row = new StateMachineMethodTableRow(this, (uint)(_rows.Count + 1), moveNextMethod, kickoffMethod);
                        _rows.Add(row);
                        return row;
                    }
                }
                public class StateMachineMethodTableRow : MetadataTableRow
                {
                    StateMachineMethodTable _parent;
                    uint _row;
                    public override uint Row { get { return _row; } }
                    public override uint Length { get { return 2; } }
                    MethodDefTable.MethodDefTableRow _moveNextMethod;
                    public MethodDefTable.MethodDefTableRow MoveNextMethod { get { return _moveNextMethod; } }
                    MethodDefTable.MethodDefTableRow _kickoffMethod;
                    public MethodDefTable.MethodDefTableRow KickoffMethod { get { return _kickoffMethod; } }

                    internal StateMachineMethodTableRow(StateMachineMethodTable parent, uint row, MethodDefTable.MethodDefTableRow moveNextMethod, MethodDefTable.MethodDefTableRow kickoffMethod)
                    {
                        _parent = parent;
                        _row = row;
                        _moveNextMethod = moveNextMethod;
                        _kickoffMethod = kickoffMethod;
                    }
                    internal StateMachineMethodTableRow(StateMachineMethodTable parent, uint row)
                    {
                        _parent = parent;
                        _row = row;
                    }
                    internal void Load(MethodDefTable methodDef, BinaryReader reader)
                    {
                        uint moveNextMethodIndex;
                        uint kickoffMethodIndex;
                        if (methodDef.LargeIndices)
                        {
                            moveNextMethodIndex = reader.ReadUInt32();
                            kickoffMethodIndex = reader.ReadUInt32();
                        }
                        else
                        {
                            moveNextMethodIndex = reader.ReadUInt16();
                            kickoffMethodIndex = reader.ReadUInt16();
                        }
                        if (moveNextMethodIndex == 0 || (moveNextMethodIndex - 1) >= methodDef.Rows) { _moveNextMethod = null; } else { _moveNextMethod = methodDef[moveNextMethodIndex]; }
                        if (kickoffMethodIndex == 0 || (kickoffMethodIndex - 1) >= methodDef.Rows) { _kickoffMethod = null; } else { _kickoffMethod = methodDef[kickoffMethodIndex]; }
                    }
#if NET6_0_OR_GREATER

                    internal void Load(MethodDefTable methodDef, Span<byte> data, ref uint offset)
                    {
                        uint moveNextMethodIndex;
                        uint kickoffMethodIndex;
                        if (methodDef.LargeIndices)
                        {
                            moveNextMethodIndex = BitConverterLE.ToUInt32(data, offset); offset += 4;
                            kickoffMethodIndex = BitConverterLE.ToUInt32(data, offset); offset += 4;
                        }
                        else
                        {
                            moveNextMethodIndex = BitConverterLE.ToUInt16(data, offset); offset += 2;
                            kickoffMethodIndex = BitConverterLE.ToUInt16(data, offset); offset += 2;
                        }
                        if (moveNextMethodIndex == 0 || (moveNextMethodIndex - 1) >= methodDef.Rows) { _moveNextMethod = null; } else { _moveNextMethod = methodDef[moveNextMethodIndex]; }
                        if (kickoffMethodIndex == 0 || (kickoffMethodIndex - 1) >= methodDef.Rows) { _kickoffMethod = null; } else { _kickoffMethod = methodDef[kickoffMethodIndex]; }
                    }
#endif
                    internal void Save(BinaryWriter binaryWriter)
                    {
                    }
                }
                internal void Save(BinaryWriter binaryWriter)
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(binaryWriter);
                    }
                }
                public StateMachineMethodTable()
                {
                }
                internal void Load(MethodDefTable methodDef, BinaryReader reader)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(methodDef, reader); }
                }
#if NET6_0_OR_GREATER
                internal void Load(MethodDefTable methodDef, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(methodDef, data, ref offset); }
                }
#endif
                internal StateMachineMethodTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new StateMachineMethodTableRow(this, (uint)(_rows.Count + 1)));
                    }
                }
            }
        }
    }
}
