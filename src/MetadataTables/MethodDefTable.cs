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
            public class MethodDefTable : MetadataTable
            {
                public override int ID { get { return 6; } }
                public override bool Sorted { get { return false; } }
                public MethodDefTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }

                public class MethodDefTableRow : MetadataTableRow
                {
                    MethodDefTable _parent;
                    public MethodDefTable Parent { get { return _parent; } }
                    MethodImplAttributes _implAttributes;
                    public MethodImplAttributes ImplAttributes { get { return _implAttributes; } }
                    public override uint Length { get { return 6; } }
                    uint _methodBodyRVA;
                    public uint MethodBodyRelativeVirtualAddress { get { return _methodBodyRVA; } set { _methodBodyRVA = value; } }
                    Heap.StringHeap.String _name;
                    public Heap.StringHeap.String Name { get { return _name; } }
                    Heap.BlobHeap.Blob _signature;
                    public Heap.BlobHeap.Blob Signature { get { return _signature; } }
                    MethodAttributes _attributes;
                    public MethodAttributes Attributes { get { return _attributes; } }
                    ParamTable.ParamTableRow _parameterList;
                    public ParamTable.ParamTableRow ParameterList { get { return _parameterList; } }
                    /// <summary>
                    /// Return the next ParamTableRow after this method's last parameter, or null if this is the last method with parameters.
                    /// </summary>
#if NET6_0_OR_GREATER
                    public ParamTable.ParamTableRow? ParameterListEnd
#else
                    public ParamTable.ParamTableRow ParameterListEnd
#endif
                    {
                        get
                        {
                            for (uint n = _row + 1; n <= _parent.Rows; n++)
                            {
                                MethodDefTableRow methodRow = _parent[n];
                                if (methodRow.ParameterList != null)
                                {
                                    return methodRow.ParameterList;
                                }
                            }
                            return null;
                        }
                    }

                    uint _row;
                    public override uint Row { get { return _row; } }
                    internal MethodDefTableRow(MethodDefTable parent, uint row, Heap.StringHeap.String name, Heap.BlobHeap.Blob signature, MethodAttributes attributes, MethodImplAttributes implAttributes, uint methodBodyRVA, ParamTable.ParamTableRow paramList)
                    {
                        _row = row;
                        _name = name;
                        _signature = signature;
                        _parent = parent;
                        _attributes = attributes;
                        _implAttributes = implAttributes;
                        _methodBodyRVA = methodBodyRVA;
                        _parameterList = paramList;
                    }
                    internal MethodDefTableRow(MethodDefTable parent, uint row)
                    {
                        _parent = parent;
                        _row = row;
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, ParamTable? paramTable, BinaryReader reader)
#else
                    internal void Load(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, ParamTable paramTable, BinaryReader reader)
#endif
                    {
                        _methodBodyRVA = reader.ReadUInt32();
                        _implAttributes = (MethodImplAttributes)reader.ReadUInt16();
                        _attributes = (MethodAttributes)reader.ReadUInt16();
                        uint nameIndex = stringHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        uint blobIndex = blobHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        if (paramTable != null)
                        {
                            _parameterList = paramTable[(paramTable.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16())];
                        }
                        else
                        {
                            _parameterList = null;
                        }
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        _signature = new Heap.BlobHeap.Blob(blobHeap, blobIndex);
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, ParamTable? paramTable, Span<byte> data, ref uint offset)
                    {
                        _methodBodyRVA = BitConverterLE.ToUInt32(data, offset); offset += 4;
                        _implAttributes = (MethodImplAttributes)BitConverterLE.ToUInt16(data, offset); offset += 2;
                        _attributes = (MethodAttributes)BitConverterLE.ToUInt16(data, offset); offset += 2;
                        uint nameIndex = 0; if (stringHeap.LargeIndices) { nameIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { nameIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        uint blobIndex = 0; if (blobHeap.LargeIndices) { blobIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { blobIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _name = new Heap.StringHeap.String(stringHeap, nameIndex);
                        _signature = new Heap.BlobHeap.Blob(blobHeap, blobIndex);
                        if (paramTable != null)
                        {
                            uint paramIndex = 0;
                            if (paramTable.LargeIndices) { paramIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { paramIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                            if (paramIndex == 0 || (paramIndex + 1) >= paramTable.Rows) { _parameterList = null; } else { _parameterList = paramTable[paramIndex]; }
                        }
                        else
                        {
                            _parameterList = null;
                        }
                    }
#endif
#if NET6_0_OR_GREATER
                    internal void Save(ParamTable? paramTable, BinaryWriter binaryWriter)
#else
                    internal void Save(ParamTable paramTable, BinaryWriter binaryWriter)
#endif
                    {
                        lock (this)
                        {
                            binaryWriter.Write(_methodBodyRVA);
                            binaryWriter.Write((ushort)_implAttributes);
                            binaryWriter.Write((ushort)_attributes);
                            if (_name.Heap.LargeIndices) { binaryWriter.Write(_name.Index); } else { binaryWriter.Write((ushort)_name.Index); }
                            if (_signature.Heap.LargeIndices) { binaryWriter.Write(_signature.Index); } else { binaryWriter.Write((ushort)_signature.Index); }
                            if (_parameterList == null)
                            {
#if NET6_0_OR_GREATER
                                ParamTable.ParamTableRow? paramListEnd = ParameterListEnd;
#else
                                ParamTable.ParamTableRow paramListEnd = ParameterListEnd;
#endif
                                if (paramListEnd != null)
                                {
                                    if (paramListEnd.Parent.LargeIndices) { binaryWriter.Write((uint)(paramListEnd.Row)); } else { binaryWriter.Write((ushort)(paramListEnd.Row)); }
                                }
                                else
                                {
                                    if (paramTable == null) { binaryWriter.Write((ushort)(1)); }
                                    else if (paramTable.LargeIndices) { binaryWriter.Write((uint)(paramTable.Rows + 1)); } else { binaryWriter.Write((ushort)(paramTable.Rows + 1)); }
                                }
                            }
                            else
                            {
                                if (_parameterList.Parent.LargeIndices) { binaryWriter.Write((uint)_parameterList.Row); } else { binaryWriter.Write((ushort)_parameterList.Row); }
                            }
                        }
                    }
                }
                List<MethodDefTableRow> _rows = new List<MethodDefTableRow>();
                public override uint Columns { get { return 6; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public MethodDefTable()
                {
                }
                internal void Load(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, ParamTable paramTable, BinaryReader reader)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++)
                    {
                        _rows[n].Load(stringHeap, blobHeap, paramTable, reader);
                    }
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, ParamTable? paramTable, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++)
                    {
                        _rows[n].Load(stringHeap, blobHeap, paramTable, data, ref offset);
                    }
                }
#endif
                internal MethodDefTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new MethodDefTableRow(this, (uint)(n + 1)));
                    }
                }
                internal override void Save(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, BinaryWriter binaryWriter)
                {
                    Save(stringHeap, blobHeap, GUIDHeap, null, binaryWriter);
                }
#if NET6_0_OR_GREATER
                internal void Save(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, ParamTable? paramTable, BinaryWriter binaryWriter)
#else
                internal void Save(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, ParamTable paramTable, BinaryWriter binaryWriter)
#endif
                {
                    lock (this)
                    {
                        for (int n = 0; n < _rows.Count; n++)
                        {
                            _rows[n].Save(paramTable, binaryWriter);
                        }
                    }
                }
                public MethodDefTableRow Add(Heap.StringHeap.String name, Heap.BlobHeap.Blob signature, MethodAttributes attributes, MethodImplAttributes implAttributes, uint methodBodyRelativeVirtualAddress, ParamTable.ParamTableRow paramList)
                {
                    lock (this)
                    {
                        MethodDefTableRow row = new MethodDefTableRow(this, (uint)(_rows.Count + 1), name, signature, attributes, implAttributes, methodBodyRelativeVirtualAddress, paramList);
                        _rows.Add(row);
                        return row;
                    }
                }
            }
        }
    }
}
