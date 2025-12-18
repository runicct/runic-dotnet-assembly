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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public partial class MetadataTable
        {
            public class CustomAttributeTable : MetadataTable
            {
                List<CustomAttributeTableRow> _rows = new List<CustomAttributeTableRow>();
                public override int ID { get { return 0x0C; } }
                public override uint Columns { get { return 3; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public CustomAttributeTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public class CustomAttributeTableRow : MetadataTableRow
                {
                    public override uint Length { get { return 3; } }
                    ICustomAttributeConstructor _constructor;
                    public ICustomAttributeConstructor Constructor { get { return _constructor; } }
                    IHasCustomAttribute _parent;
                    public IHasCustomAttribute Parent { get { return _parent; } }
                    Heap.BlobHeap.Blob _value;
                    public Heap.BlobHeap.Blob Value { get { return _value; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    internal CustomAttributeTableRow(uint row, IHasCustomAttribute parent, ICustomAttributeConstructor constructor, Heap.BlobHeap.Blob value)
                    {
                        _row = row;
                        _parent = parent;
                        _constructor = constructor;
                        _value = value;
                    }
#if NET6_0_OR_GREATER
                    internal CustomAttributeTableRow(uint row, Heap.BlobHeap blobHeap, MethodDefTable? methodDefTable, FieldTable? fieldTable, TypeRefTable? typeRefTable, TypeDefTable? typeDefTable, ParamTable? paramTable, InterfaceImplTable? interfaceImplTable, MemberRefTable? memberRefTable, ModuleTable? moduleTable, DeclSecurityTable? declSecurityTable, EventTable? eventTable, PropertyTable? propertyTable, TypeSpecTable? typeSpecTable, AssemblyTable? assemblyTable, AssemblyRefTable? assemblyRefTable, FileTable? fileTable, ExportedTypeTable? exportedTypeTable, ManifestResourceTable? manifestResourceTable, GenericParamTable? genericParamTable, GenericParamConstraintTable? genericParamConstraintTable, MethodSpecTable? methodSpecTable, System.IO.BinaryReader reader)
#else
                    internal CustomAttributeTableRow(uint row, Heap.BlobHeap blobHeap, MethodDefTable methodDefTable, FieldTable fieldTable, TypeRefTable typeRefTable, TypeDefTable typeDefTable, ParamTable paramTable, InterfaceImplTable interfaceImplTable, MemberRefTable memberRefTable, ModuleTable moduleTable, DeclSecurityTable declSecurityTable, EventTable eventTable, PropertyTable propertyTable, TypeSpecTable typeSpecTable, AssemblyTable assemblyTable, AssemblyRefTable assemblyRefTable, FileTable fileTable, ExportedTypeTable exportedTypeTable, ManifestResourceTable manifestResourceTable, GenericParamTable genericParamTable, GenericParamConstraintTable genericParamConstraintTable, MethodSpecTable methodSpecTable, System.IO.BinaryReader reader)
#endif
                    {
                        _row = row;
                        uint parentToken = 0;
                        if (HasCustomAttributeLargeIndices(methodDefTable, fieldTable, typeRefTable, typeDefTable, paramTable, interfaceImplTable, memberRefTable, moduleTable, declSecurityTable, eventTable, propertyTable, typeSpecTable, assemblyTable, assemblyRefTable, fileTable, exportedTypeTable, manifestResourceTable, genericParamTable, genericParamConstraintTable, methodSpecTable)) { parentToken = reader.ReadUInt32(); } else { parentToken = reader.ReadUInt16(); }
                        _parent = HasCustomAttributeDecode(parentToken, methodDefTable, fieldTable, typeRefTable, typeDefTable, paramTable, interfaceImplTable, memberRefTable, moduleTable, declSecurityTable, eventTable, propertyTable, typeSpecTable, assemblyTable, assemblyRefTable, fileTable, exportedTypeTable, manifestResourceTable, genericParamTable, genericParamConstraintTable, methodSpecTable);
                        uint constructorToken = 0;
                        if (CustomAttributeConstructorLargeIndices(methodDefTable, memberRefTable)) { constructorToken = reader.ReadUInt32(); } else { constructorToken = reader.ReadUInt16(); }
                        _constructor = CustomAttributeConstructorDecode(constructorToken, methodDefTable, memberRefTable);
                        uint valueIndex = blobHeap.LargeIndices ? reader.ReadUInt32() : reader.ReadUInt16();
                        _value = new Heap.BlobHeap.Blob(blobHeap, valueIndex);
                    }
#if NET6_0_OR_GREATER
                    internal CustomAttributeTableRow(uint row, Heap.BlobHeap blobHeap, MethodDefTable? methodDefTable, FieldTable? fieldTable, TypeRefTable? typeRefTable, TypeDefTable? typeDefTable, ParamTable? paramTable, InterfaceImplTable? interfaceImplTable, MemberRefTable? memberRefTable, ModuleTable? moduleTable, DeclSecurityTable? declSecurityTable, EventTable? eventTable, PropertyTable? propertyTable, TypeSpecTable? typeSpecTable, AssemblyTable? assemblyTable, AssemblyRefTable? assemblyRefTable, FileTable? fileTable, ExportedTypeTable? exportedTypeTable, ManifestResourceTable? manifestResourceTable, GenericParamTable? genericParamTable, GenericParamConstraintTable? genericParamConstraintTable, MethodSpecTable? methodSpecTable, Span<byte> data, ref uint offset)
                    {
                        _row = row;
                        uint parentToken = 0;
                        if (HasCustomAttributeLargeIndices(methodDefTable, fieldTable, typeRefTable, typeDefTable, paramTable, interfaceImplTable, memberRefTable, moduleTable, declSecurityTable, eventTable, propertyTable, typeSpecTable, assemblyTable, assemblyRefTable, fileTable, exportedTypeTable, manifestResourceTable, genericParamTable, genericParamConstraintTable, methodSpecTable)) { parentToken = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { parentToken = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _parent = HasCustomAttributeDecode(parentToken, methodDefTable, fieldTable, typeRefTable, typeDefTable, paramTable, interfaceImplTable, memberRefTable, moduleTable, declSecurityTable, eventTable, propertyTable, typeSpecTable, assemblyTable, assemblyRefTable, fileTable, exportedTypeTable, manifestResourceTable, genericParamTable, genericParamConstraintTable, methodSpecTable);
                        uint constructorToken = 0;
                        if (CustomAttributeConstructorLargeIndices(methodDefTable, memberRefTable)) { constructorToken = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { constructorToken = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _constructor = CustomAttributeConstructorDecode(constructorToken, methodDefTable, memberRefTable);
                        uint valueIndex = 0; if (blobHeap.LargeIndices) { valueIndex = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { valueIndex = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _value = new Heap.BlobHeap.Blob(blobHeap, valueIndex);
                    }
#endif
#if NET6_0_OR_GREATER
                    internal void Save(MethodDefTable? methodDefTable, FieldTable? fieldTable, TypeRefTable? typeRefTable, TypeDefTable? typeDefTable, ParamTable? paramTable, InterfaceImplTable? interfaceImplTable, MemberRefTable? memberRefTable, ModuleTable? moduleTable, DeclSecurityTable? declSecurityTable, EventTable? eventTable, PropertyTable? propertyTable, TypeSpecTable? typeSpecTable, AssemblyTable? assemblyTable, AssemblyRefTable? assemblyRefTable, FileTable? fileTable, ExportedTypeTable? exportedTypeTable, ManifestResourceTable? manifestResourceTable, GenericParamTable? genericParamTable, GenericParamConstraintTable? genericParamConstraintTable, MethodSpecTable? methodSpecTable, BinaryWriter binaryWriter)
#else
                    internal void Save(MethodDefTable methodDefTable, FieldTable fieldTable, TypeRefTable typeRefTable, TypeDefTable typeDefTable, ParamTable paramTable, InterfaceImplTable interfaceImplTable, MemberRefTable memberRefTable, ModuleTable moduleTable, DeclSecurityTable declSecurityTable, EventTable eventTable, PropertyTable propertyTable, TypeSpecTable typeSpecTable, AssemblyTable assemblyTable, AssemblyRefTable assemblyRefTable, FileTable fileTable, ExportedTypeTable exportedTypeTable, ManifestResourceTable manifestResourceTable, GenericParamTable genericParamTable, GenericParamConstraintTable genericParamConstraintTable, MethodSpecTable methodSpecTable, BinaryWriter binaryWriter)
#endif
                    {
                        uint parentToken = HasCustomAttributeEncode(_parent);
                        if (HasCustomAttributeLargeIndices(methodDefTable, fieldTable, typeRefTable, typeDefTable, paramTable, interfaceImplTable, memberRefTable, moduleTable, declSecurityTable, eventTable, propertyTable, typeSpecTable, assemblyTable, assemblyRefTable, fileTable, exportedTypeTable, manifestResourceTable, genericParamTable, genericParamConstraintTable, methodSpecTable)) { binaryWriter.Write(parentToken); } else { binaryWriter.Write((ushort)parentToken); }
                        uint constructorToken = CustomAttributeConstructorEncode(_constructor);
                        if (CustomAttributeConstructorLargeIndices(methodDefTable, memberRefTable)) { binaryWriter.Write(constructorToken); } else { binaryWriter.Write((ushort)constructorToken); }
                        binaryWriter.Write(_value.Index);
                    }
                }
                public CustomAttributeTableRow Add(IHasCustomAttribute parent, ICustomAttributeConstructor constructor, Heap.BlobHeap.Blob value)
                {
                    lock (this)
                    {
                        CustomAttributeTableRow row = new CustomAttributeTableRow((uint)(_rows.Count + 1), parent, constructor, value);
                        _rows.Add(row);
                        return row;
                    }
                }
#if NET6_0_OR_GREATER
                internal void Save(MethodDefTable? methodDefTable, FieldTable? fieldTable, TypeRefTable? typeRefTable, TypeDefTable? typeDefTable, ParamTable? paramTable, InterfaceImplTable? interfaceImplTable,  MemberRefTable? memberRefTable, ModuleTable? moduleTable, DeclSecurityTable? declSecurityTable, EventTable? eventTable, PropertyTable? propertyTable, TypeSpecTable? typeSpecTable, AssemblyTable? assemblyTable, AssemblyRefTable? assemblyRefTable, FileTable? fileTable, ExportedTypeTable? exportedTypeTable, ManifestResourceTable? manifestResourceTable, GenericParamTable? genericParamTable, GenericParamConstraintTable? genericParamConstraintTable, MethodSpecTable? methodSpecTable, BinaryWriter binaryWriter)
#else
                internal void Save(MethodDefTable methodDefTable, FieldTable fieldTable, TypeRefTable typeRefTable, TypeDefTable typeDefTable, ParamTable paramTable, InterfaceImplTable interfaceImplTable, MemberRefTable memberRefTable, ModuleTable moduleTable, DeclSecurityTable declSecurityTable, EventTable eventTable, PropertyTable propertyTable, TypeSpecTable typeSpecTable, AssemblyTable assemblyTable, AssemblyRefTable assemblyRefTable, FileTable fileTable,  ExportedTypeTable exportedTypeTable, ManifestResourceTable manifestResourceTable, GenericParamTable genericParamTable, GenericParamConstraintTable genericParamConstraintTable, MethodSpecTable methodSpecTable, BinaryWriter binaryWriter)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(methodDefTable, fieldTable, typeRefTable, typeDefTable, paramTable, interfaceImplTable, memberRefTable, moduleTable, declSecurityTable, eventTable, propertyTable, typeSpecTable, assemblyTable, assemblyRefTable, fileTable, exportedTypeTable, manifestResourceTable, genericParamTable, genericParamConstraintTable, methodSpecTable, binaryWriter);
                    }
                }
                public CustomAttributeTable()
                {
                }
#if NET6_0_OR_GREATER
                internal CustomAttributeTable(uint rows, Heap.BlobHeap blobHeap, MethodDefTable? methodDefTable, FieldTable? fieldTable, TypeRefTable? typeRefTable, TypeDefTable? typeDefTable, ParamTable? paramTable, InterfaceImplTable?  interfaceImplTable, MemberRefTable? memberRefTable, ModuleTable? moduleTable, DeclSecurityTable? declSecurityTable, EventTable? eventTable, PropertyTable? propertyTable, TypeSpecTable? typeSpecTable, AssemblyTable? assemblyTable, AssemblyRefTable? assemblyRefTable, FileTable? fileTable, ExportedTypeTable? exportedTypeTable, ManifestResourceTable? manifestResourceTable, GenericParamTable? genericParamTable, GenericParamConstraintTable? genericParamConstraintTable, MethodSpecTable? methodSpecTable, System.IO.BinaryReader reader)
#else
                internal CustomAttributeTable(uint rows, Heap.BlobHeap blobHeap, MethodDefTable methodDefTable, FieldTable fieldTable, TypeRefTable typeRefTable, TypeDefTable typeDefTable, ParamTable paramTable, InterfaceImplTable interfaceImplTable, MemberRefTable memberRefTable, ModuleTable moduleTable, DeclSecurityTable declSecurityTable, EventTable eventTable, PropertyTable propertyTable, TypeSpecTable typeSpecTable, AssemblyTable assemblyTable, AssemblyRefTable assemblyRefTable, FileTable fileTable, ExportedTypeTable exportedTypeTable, ManifestResourceTable manifestResourceTable, GenericParamTable genericParamTable, GenericParamConstraintTable genericParamConstraintTable, MethodSpecTable methodSpecTable, System.IO.BinaryReader reader)
#endif
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new CustomAttributeTableRow(n + 1, blobHeap, methodDefTable, fieldTable, typeRefTable, typeDefTable, paramTable, interfaceImplTable, memberRefTable, moduleTable, declSecurityTable, eventTable, propertyTable, typeSpecTable, assemblyTable, assemblyRefTable, fileTable, exportedTypeTable, manifestResourceTable, genericParamTable, genericParamConstraintTable, methodSpecTable, reader));
                    }
                }
#if NET6_0_OR_GREATER

                internal CustomAttributeTable(uint rows, Heap.BlobHeap blobHeap, MethodDefTable? methodDefTable, FieldTable? fieldTable, TypeRefTable? typeRefTable, TypeDefTable? typeDefTable, ParamTable? paramTable, InterfaceImplTable? interfaceImplTable, MemberRefTable? memberRefTable, ModuleTable? moduleTable, DeclSecurityTable? declSecurityTable, EventTable? eventTable, PropertyTable? propertyTable, TypeSpecTable? typeSpecTable, AssemblyTable? assemblyTable, AssemblyRefTable? assemblyRefTable, FileTable? fileTable, ExportedTypeTable? exportedTypeTable, ManifestResourceTable? manifestResourceTable, GenericParamTable? genericParamTable, GenericParamConstraintTable? genericParamConstraintTable, MethodSpecTable? methodSpecTable, Span<byte> data, ref uint offset)
                {
                    for (uint n = 0; n < rows; n++)
                    {
                        _rows.Add(new CustomAttributeTableRow(n + 1, blobHeap, methodDefTable, fieldTable, typeRefTable, typeDefTable, paramTable, interfaceImplTable, memberRefTable, moduleTable, declSecurityTable, eventTable, propertyTable, typeSpecTable, assemblyTable, assemblyRefTable, fileTable, exportedTypeTable, manifestResourceTable, genericParamTable, genericParamConstraintTable, methodSpecTable, data, ref offset));
                    }
                }
#endif
            }
        }
    }
}
