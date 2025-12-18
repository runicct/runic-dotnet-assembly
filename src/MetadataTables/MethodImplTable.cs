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
            public class MethodImplTable : MetadataTable
            {
                List<MethodImplTableRow> _rows = new List<MethodImplTableRow>();
                public override int ID { get { return 0x19; } }
                public override uint Columns { get { return 3; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public MethodImplTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public MethodImplTableRow Add(TypeDefTable.TypeDefTableRow @class, IMethodDefOrRef methodBody, IMethodDefOrRef methodDeclaration)
                {
                    lock (this)
                    {
                        MethodImplTableRow row = new MethodImplTableRow((uint)(_rows.Count + 1), @class, methodBody, methodDeclaration);
                        _rows.Add(row);
                        return row;
                    }
                }
                public class MethodImplTableRow : MetadataTableRow
                {

                    public override uint Length { get { return 3; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    TypeDefTable.TypeDefTableRow _class;
                    public TypeDefTable.TypeDefTableRow Class { get { return _class; } }
                    IMethodDefOrRef _methodBody;
                    public IMethodDefOrRef MethodBody { get { return _methodBody; } }
                    IMethodDefOrRef _methodDeclaration;
                    public IMethodDefOrRef MethodDeclaration { get { return _methodDeclaration; } }
                    internal MethodImplTableRow(uint row, TypeDefTable.TypeDefTableRow @class, IMethodDefOrRef methodBody, IMethodDefOrRef methodDeclaration)
                    {
                        _row = row;
                        _class = @class;
                        _methodBody = methodBody;
                        _methodDeclaration = methodDeclaration;
                    }
                    internal MethodImplTableRow(uint row)
                    {
                        _row = row;
                    }
#if NET6_0_OR_GREATER
                    internal void Load(TypeDefTable typeDefTable, MethodDefTable? methodDefTable, MemberRefTable? memberRefTable, BinaryReader reader)
#else
                    internal void Load(TypeDefTable typeDefTable, MethodDefTable methodDefTable, MemberRefTable memberRefTable, BinaryReader reader)
#endif
                    {
                        uint classIndex;
                        if (typeDefTable.LargeIndices) { classIndex = reader.ReadUInt32(); } else { classIndex = reader.ReadUInt16(); }
                        uint methodBodyIndex;
                        uint methodDeclarationIndex;
                        if (MethodDefOrRefLargeIndices(methodDefTable, memberRefTable))
                        {
                            methodBodyIndex = reader.ReadUInt32();
                            methodDeclarationIndex = reader.ReadUInt32();
                        }
                        else
                        {
                            methodBodyIndex = reader.ReadUInt16();
                            methodDeclarationIndex = reader.ReadUInt16();
                        }
                        _methodBody = MethodDefOrRefDecode(methodBodyIndex, methodDefTable, memberRefTable);
                        _methodDeclaration = MethodDefOrRefDecode(methodDeclarationIndex, methodDefTable, memberRefTable);
                    }
#if NET6_0_OR_GREATER

                    internal void Load(TypeDefTable typeDefTable, MethodDefTable? methodDefTable, MemberRefTable? memberRefTable, Span<byte> data, ref uint offset)
                    {
                        uint classIndex;
                        if (typeDefTable.LargeIndices) { classIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { classIndex = BitConverterLE.ToUInt32(data, offset); offset += 2; }
                        uint methodBodyIndex;
                        uint methodDeclarationIndex;
                        if (MethodDefOrRefLargeIndices(methodDefTable, memberRefTable))
                        {
                            methodBodyIndex = BitConverterLE.ToUInt32(data, offset); offset += 4;
                            methodDeclarationIndex = BitConverterLE.ToUInt32(data, offset); offset += 4;
                        }
                        else
                        {
                            methodBodyIndex = BitConverterLE.ToUInt32(data, offset); offset += 2;
                            methodDeclarationIndex = BitConverterLE.ToUInt32(data, offset); offset += 2;
                        }
                        _methodBody = MethodDefOrRefDecode(methodBodyIndex, methodDefTable, memberRefTable);
                        _methodDeclaration = MethodDefOrRefDecode(methodDeclarationIndex, methodDefTable, memberRefTable);
                    }
#endif

#if NET6_0_OR_GREATER
                    internal void Save(MethodDefTable? methodDefTable, MemberRefTable? memberRefTable, BinaryWriter binaryWriter)
#else
                    internal void Save(MethodDefTable methodDefTable, MemberRefTable memberRefTable, BinaryWriter binaryWriter)
#endif
                    {
                        if (_class.Parent.LargeIndices) { binaryWriter.Write((uint)_class.Row); } else { binaryWriter.Write((ushort)_class.Row); }
                        uint methodBodyIndex = MethodDefOrRefEncode(_methodBody);
                        uint methodDeclarationIndex = MethodDefOrRefEncode(_methodDeclaration);
                        if (MethodDefOrRefLargeIndices(methodDefTable, memberRefTable))
                        {
                            binaryWriter.Write(methodBodyIndex);
                            binaryWriter.Write(methodDeclarationIndex);
                        }
                        else
                        {
                            binaryWriter.Write((ushort)methodBodyIndex);
                            binaryWriter.Write((ushort)methodDeclarationIndex);
                        }
                    }
                }
#if NET6_0_OR_GREATER
                internal void Save(MethodDefTable? methodDefTable, MemberRefTable? memberRefTable, BinaryWriter binaryWriter)
#else
                internal void Save(MethodDefTable methodDefTable, MemberRefTable memberRefTable, BinaryWriter binaryWriter)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(methodDefTable, memberRefTable, binaryWriter);
                    }
                }
                public MethodImplTable()
                {
                }
#if NET6_0_OR_GREATER
                internal void Load(TypeDefTable typeDefTable, MethodDefTable? methodDefTable, MemberRefTable? memberRefTable, BinaryReader reader)
#else
                internal void Load(TypeDefTable typeDefTable, MethodDefTable methodDefTable, MemberRefTable memberRefTable, BinaryReader reader)
#endif
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(typeDefTable, methodDefTable, memberRefTable, reader); }
                }
#if NET6_0_OR_GREATER
                internal void Load(TypeDefTable typeDefTable, MethodDefTable? methodDefTable, MemberRefTable? memberRefTable, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(typeDefTable, methodDefTable, memberRefTable, data, ref offset); }
                }
#endif
                internal MethodImplTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new MethodImplTableRow((uint)(_rows.Count + 1)));
                    }
                }
            }
        }
    }
}
