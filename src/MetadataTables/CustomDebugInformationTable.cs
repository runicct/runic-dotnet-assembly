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
            public class CustomDebugInformationTable : MetadataTable
            {
                List<CustomDebugInformationTableRow> _rows = new List<CustomDebugInformationTableRow>();
                public override int ID { get { return 0x37; } }
                public override uint Columns { get { return 3; } }
                public override uint Rows { get { return (uint)_rows.Count; } }
                public override bool Sorted { get { return false; } }
                public CustomDebugInformationTableRow this[uint index] { get { lock (this) { return _rows[(int)(index - 1)]; } } }
                public CustomDebugInformationTableRow Add(IHasCustomDebugInformation parent, Heap.GUIDHeap.GUID kind, Heap.BlobHeap.Blob value)
                {
                    lock (this)
                    {
                        CustomDebugInformationTableRow row = new CustomDebugInformationTableRow(this, (uint)(_rows.Count + 1), parent, kind, value);
                        _rows.Add(row);
                        return row;
                    }
                }
                public class CustomDebugInformationTableRow : MetadataTableRow
                {
                    CustomDebugInformationTable _parentTable;
                    IHasCustomDebugInformation _parent;
                    public IHasCustomDebugInformation Parent { get { return _parent; } }
                    Heap.GUIDHeap.GUID _kind;
                    public Heap.GUIDHeap.GUID Kind { get { return _kind; } }
                    public override uint Length { get { return 3; } }
                    Heap.BlobHeap.Blob _value;
                    public Heap.BlobHeap.Blob Value { get { return _value; } set { _value = value; } }
                    uint _row;
                    public override uint Row { get { return _row; } }
                    internal CustomDebugInformationTableRow(CustomDebugInformationTable parentTable, uint row, IHasCustomDebugInformation parent, Heap.GUIDHeap.GUID kind, Heap.BlobHeap.Blob value)
                    {
                        _parentTable = parentTable;
                        _row = row;
                        _parent = parent;
                        _kind = kind;
                        _value = value;
                    }
                    internal CustomDebugInformationTableRow(CustomDebugInformationTable parentTable, uint row)
                    {
                        _parentTable = parentTable;
                        _row = row;
                    }
#if NET6_0_OR_GREATER
                    internal void Load(Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, MethodDefTable? methodDefTable, FieldTable? fieldTable, TypeRefTable? typeRefTable, TypeDefTable? typeDefTable, ParamTable? paramTable, InterfaceImplTable? interfaceImplTable, MemberRefTable? memberRefTable, ModuleTable? moduleTable, DeclSecurityTable? declSecurityTable, PropertyTable? propertyTable, EventTable? eventTable, StandAloneSigTable? standAloneSigTable, ModuleRefTable? moduleRefTable, TypeSpecTable? typeSpecTable, AssemblyTable? assemblyTable, AssemblyRefTable? assemblyRefTable, FileTable? fileTable, ExportedTypeTable? exportedTypeTable, ManifestResourceTable? manifestResourceTable, GenericParamTable? genericParamTable, GenericParamConstraintTable? genericParamConstraintTable, MethodSpecTable? methodSpecTable, DocumentTable? documentTable, LocalScopeTable? localScopeTable, LocalVariableTable? localVariableTable, LocalConstantTable? localConstantTable, ImportScopeTable? importScopeTable, BinaryReader reader)
#else
                    internal void Load(Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, MethodDefTable methodDefTable, FieldTable fieldTable, TypeRefTable typeRefTable, TypeDefTable typeDefTable, ParamTable paramTable, InterfaceImplTable interfaceImplTable, MemberRefTable memberRefTable, ModuleTable moduleTable, DeclSecurityTable declSecurityTable, PropertyTable propertyTable, EventTable eventTable, StandAloneSigTable standAloneSigTable, ModuleRefTable moduleRefTable, TypeSpecTable typeSpecTable, AssemblyTable assemblyTable, AssemblyRefTable assemblyRefTable, FileTable fileTable, ExportedTypeTable exportedTypeTable, ManifestResourceTable manifestResourceTable, GenericParamTable genericParamTable, GenericParamConstraintTable genericParamConstraintTable, MethodSpecTable methodSpecTable, DocumentTable documentTable, LocalScopeTable localScopeTable, LocalVariableTable localVariableTable, LocalConstantTable localConstantTable, ImportScopeTable importScopeTable, BinaryReader reader)
#endif
                    {
                        bool largeParent = HasCustomDebugInformationLargeIndices(methodDefTable, fieldTable, typeRefTable, typeDefTable, paramTable, interfaceImplTable, memberRefTable, moduleTable, declSecurityTable, propertyTable, eventTable, standAloneSigTable, moduleRefTable, typeSpecTable, assemblyTable, assemblyRefTable, fileTable, exportedTypeTable, manifestResourceTable, genericParamTable, genericParamConstraintTable, methodSpecTable, documentTable, localScopeTable, localVariableTable, localConstantTable, importScopeTable);
                        uint tag = 0;
                        if (largeParent) { tag = reader.ReadUInt32(); } else { tag = reader.ReadUInt16(); }
                        _parent = HasCustomDebugInformationDecode(tag, methodDefTable, fieldTable, typeRefTable, typeDefTable, paramTable, interfaceImplTable, memberRefTable, moduleTable, declSecurityTable, propertyTable, eventTable, standAloneSigTable, moduleRefTable, typeSpecTable, assemblyTable, assemblyRefTable, fileTable, exportedTypeTable, manifestResourceTable, genericParamTable, genericParamConstraintTable, methodSpecTable, documentTable, localScopeTable, localVariableTable, localConstantTable, importScopeTable);
                        if (GUIDHeap.LargeIndices) { _kind = new Heap.GUIDHeap.GUID(GUIDHeap, reader.ReadUInt32()); } else { _kind = new Heap.GUIDHeap.GUID(GUIDHeap, reader.ReadUInt16()); }
                        if (blobHeap.LargeIndices) { _value = new Heap.BlobHeap.Blob(blobHeap, reader.ReadUInt32()); } else { _value = new Heap.BlobHeap.Blob(blobHeap, reader.ReadUInt16()); }
                    }
#if NET6_0_OR_GREATER

                    internal void Load(Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, MethodDefTable? methodDefTable, FieldTable? fieldTable, TypeRefTable? typeRefTable, TypeDefTable? typeDefTable, ParamTable? paramTable, InterfaceImplTable? interfaceImplTable, MemberRefTable? memberRefTable, ModuleTable? moduleTable, DeclSecurityTable? declSecurityTable, PropertyTable? propertyTable, EventTable? eventTable, StandAloneSigTable? standAloneSigTable, ModuleRefTable? moduleRefTable, TypeSpecTable? typeSpecTable, AssemblyTable? assemblyTable, AssemblyRefTable? assemblyRefTable, FileTable? fileTable, ExportedTypeTable? exportedTypeTable, ManifestResourceTable? manifestResourceTable, GenericParamTable? genericParamTable, GenericParamConstraintTable? genericParamConstraintTable, MethodSpecTable? methodSpecTable, DocumentTable? documentTable, LocalScopeTable? localScopeTable, LocalVariableTable? localVariableTable, LocalConstantTable? localConstantTable, ImportScopeTable? importScopeTable, Span<byte> data, ref uint offset)
                    {
                        bool largeParent = HasCustomDebugInformationLargeIndices(methodDefTable, fieldTable, typeRefTable, typeDefTable, paramTable, interfaceImplTable, memberRefTable, moduleTable, declSecurityTable, propertyTable, eventTable, standAloneSigTable, moduleRefTable, typeSpecTable, assemblyTable, assemblyRefTable, fileTable, exportedTypeTable, manifestResourceTable, genericParamTable, genericParamConstraintTable, methodSpecTable, documentTable, localScopeTable, localVariableTable, localConstantTable, importScopeTable);
                        uint tag = 0;
                        if (largeParent) { tag = BitConverterLE.ToUInt32(data, offset); offset += 4; } else { tag = BitConverterLE.ToUInt16(data, offset); offset += 2; }
                        _parent = HasCustomDebugInformationDecode(tag, methodDefTable, fieldTable, typeRefTable, typeDefTable, paramTable, interfaceImplTable, memberRefTable, moduleTable, declSecurityTable, propertyTable, eventTable, standAloneSigTable, moduleRefTable, typeSpecTable, assemblyTable, assemblyRefTable, fileTable, exportedTypeTable, manifestResourceTable, genericParamTable, genericParamConstraintTable, methodSpecTable, documentTable, localScopeTable, localVariableTable, localConstantTable, importScopeTable);
                        if (GUIDHeap.LargeIndices) { _kind = new Heap.GUIDHeap.GUID(GUIDHeap, BitConverterLE.ToUInt32(data, offset)); offset += 4; } else { _kind = new Heap.GUIDHeap.GUID(GUIDHeap, BitConverterLE.ToUInt16(data, offset)); offset += 2; }
                        if (blobHeap.LargeIndices) { _value = new Heap.BlobHeap.Blob(blobHeap, BitConverterLE.ToUInt32(data, offset)); offset += 4; } else { _value = new Heap.BlobHeap.Blob(blobHeap, BitConverterLE.ToUInt16(data, offset)); offset += 2; }
                    }
#endif

#if NET6_0_OR_GREATER
                    internal void Save(MethodDefTable? methodDefTable, FieldTable? fieldTable, TypeRefTable? typeRefTable, TypeDefTable? typeDefTable, ParamTable? paramTable, InterfaceImplTable? interfaceImplTable, MemberRefTable? memberRefTable, ModuleTable? moduleTable, DeclSecurityTable? declSecurityTable, PropertyTable? propertyTable, EventTable? eventTable, StandAloneSigTable? standAloneSigTable, ModuleRefTable? moduleRefTable, TypeSpecTable? typeSpecTable, AssemblyTable? assemblyTable, AssemblyRefTable? assemblyRefTable, FileTable? fileTable, ExportedTypeTable? exportedTypeTable, ManifestResourceTable? manifestResourceTable, GenericParamTable? genericParamTable, GenericParamConstraintTable? genericParamConstraintTable, MethodSpecTable? methodSpecTable, DocumentTable? documentTable, LocalScopeTable? localScopeTable, LocalVariableTable? localVariableTable, LocalConstantTable? localConstantTable, ImportScopeTable? importScopeTable, BinaryWriter binaryWriter)
#else
                    internal void Save(MethodDefTable methodDefTable, FieldTable fieldTable, TypeRefTable typeRefTable, TypeDefTable typeDefTable, ParamTable paramTable, InterfaceImplTable interfaceImplTable, MemberRefTable memberRefTable, ModuleTable moduleTable, DeclSecurityTable declSecurityTable, PropertyTable propertyTable, EventTable eventTable, StandAloneSigTable standAloneSigTable, ModuleRefTable moduleRefTable, TypeSpecTable typeSpecTable, AssemblyTable assemblyTable, AssemblyRefTable assemblyRefTable, FileTable fileTable, ExportedTypeTable exportedTypeTable, ManifestResourceTable manifestResourceTable, GenericParamTable genericParamTable, GenericParamConstraintTable genericParamConstraintTable, MethodSpecTable methodSpecTable, DocumentTable documentTable, LocalScopeTable localScopeTable, LocalVariableTable localVariableTable, LocalConstantTable localConstantTable, ImportScopeTable importScopeTable, BinaryWriter binaryWriter)
#endif
                    {
                        bool largeParent = HasCustomDebugInformationLargeIndices(methodDefTable, fieldTable, typeRefTable, typeDefTable, paramTable, interfaceImplTable, memberRefTable, moduleTable, declSecurityTable, propertyTable, eventTable, standAloneSigTable, moduleRefTable, typeSpecTable, assemblyTable, assemblyRefTable, fileTable, exportedTypeTable, manifestResourceTable, genericParamTable, genericParamConstraintTable, methodSpecTable, documentTable, localScopeTable, localVariableTable, localConstantTable, importScopeTable);
                        uint tag = HasCustomDebugInformationEncode(_parent);
                        if (largeParent) { binaryWriter.Write(tag); } else { binaryWriter.Write((ushort)tag); }
                        if (_kind.Heap.LargeIndices) { binaryWriter.Write(_kind.Index); } else { binaryWriter.Write((ushort)_kind.Index); }
                        if (_value.Heap.LargeIndices) { binaryWriter.Write(_value.Index); } else { binaryWriter.Write((ushort)_value.Index); }
                    }
                }
#if NET6_0_OR_GREATER
                internal void Save(MethodDefTable? methodDefTable, FieldTable? fieldTable, TypeRefTable? typeRefTable, TypeDefTable? typeDefTable, ParamTable? paramTable, InterfaceImplTable? interfaceImplTable, MemberRefTable? memberRefTable, ModuleTable? moduleTable, DeclSecurityTable? declSecurityTable, PropertyTable? propertyTable, EventTable? eventTable, StandAloneSigTable? standAloneSigTable, ModuleRefTable? moduleRefTable, TypeSpecTable? typeSpecTable, AssemblyTable? assemblyTable, AssemblyRefTable? assemblyRefTable, FileTable? fileTable, ExportedTypeTable? exportedTypeTable, ManifestResourceTable? manifestResourceTable, GenericParamTable? genericParamTable, GenericParamConstraintTable? genericParamConstraintTable, MethodSpecTable? methodSpecTable, DocumentTable? documentTable, LocalScopeTable? localScopeTable, LocalVariableTable? localVariableTable, LocalConstantTable? localConstantTable, ImportScopeTable? importScopeTable, BinaryWriter binaryWriter)
#else
                internal void Save(MethodDefTable methodDefTable, FieldTable fieldTable, TypeRefTable typeRefTable, TypeDefTable typeDefTable, ParamTable paramTable, InterfaceImplTable interfaceImplTable, MemberRefTable memberRefTable, ModuleTable moduleTable, DeclSecurityTable declSecurityTable, PropertyTable propertyTable, EventTable eventTable, StandAloneSigTable standAloneSigTable, ModuleRefTable moduleRefTable, TypeSpecTable typeSpecTable, AssemblyTable assemblyTable, AssemblyRefTable assemblyRefTable, FileTable fileTable, ExportedTypeTable exportedTypeTable, ManifestResourceTable manifestResourceTable, GenericParamTable genericParamTable, GenericParamConstraintTable genericParamConstraintTable, MethodSpecTable methodSpecTable, DocumentTable documentTable, LocalScopeTable localScopeTable, LocalVariableTable localVariableTable, LocalConstantTable localConstantTable, ImportScopeTable importScopeTable, BinaryWriter binaryWriter)
#endif
                {
                    for (int n = 0; n < _rows.Count; n++)
                    {
                        _rows[n].Save(methodDefTable, fieldTable, typeRefTable, typeDefTable, paramTable, interfaceImplTable, memberRefTable, moduleTable, declSecurityTable, propertyTable, eventTable, standAloneSigTable, moduleRefTable, typeSpecTable, assemblyTable, assemblyRefTable, fileTable, exportedTypeTable, manifestResourceTable, genericParamTable, genericParamConstraintTable, methodSpecTable, documentTable, localScopeTable, localVariableTable, localConstantTable, importScopeTable, binaryWriter);
                    }
                }
                public CustomDebugInformationTable()
                {
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, MethodDefTable? methodDefTable, FieldTable? fieldTable, TypeRefTable? typeRefTable, TypeDefTable? typeDefTable, ParamTable? paramTable, InterfaceImplTable? interfaceImplTable, MemberRefTable? memberRefTable, ModuleTable? moduleTable, DeclSecurityTable? declSecurityTable, PropertyTable? propertyTable, EventTable? eventTable, StandAloneSigTable? standAloneSigTable, ModuleRefTable? moduleRefTable, TypeSpecTable? typeSpecTable, AssemblyTable? assemblyTable, AssemblyRefTable? assemblyRefTable, FileTable? fileTable, ExportedTypeTable? exportedTypeTable, ManifestResourceTable? manifestResourceTable, GenericParamTable? genericParamTable, GenericParamConstraintTable? genericParamConstraintTable, MethodSpecTable? methodSpecTable, DocumentTable? documentTable, LocalScopeTable? localScopeTable, LocalVariableTable? localVariableTable, LocalConstantTable? localConstantTable, ImportScopeTable? importScopeTable, BinaryReader reader)
#else
                internal void Load(Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, MethodDefTable methodDefTable, FieldTable fieldTable, TypeRefTable typeRefTable, TypeDefTable typeDefTable, ParamTable paramTable, InterfaceImplTable interfaceImplTable, MemberRefTable memberRefTable, ModuleTable moduleTable, DeclSecurityTable declSecurityTable, PropertyTable propertyTable, EventTable eventTable, StandAloneSigTable standAloneSigTable, ModuleRefTable moduleRefTable, TypeSpecTable typeSpecTable, AssemblyTable assemblyTable, AssemblyRefTable assemblyRefTable, FileTable fileTable, ExportedTypeTable exportedTypeTable, ManifestResourceTable manifestResourceTable, GenericParamTable genericParamTable, GenericParamConstraintTable genericParamConstraintTable, MethodSpecTable methodSpecTable, DocumentTable documentTable, LocalScopeTable localScopeTable, LocalVariableTable localVariableTable, LocalConstantTable localConstantTable, ImportScopeTable importScopeTable, BinaryReader reader)
#endif
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(blobHeap, GUIDHeap, methodDefTable, fieldTable, typeRefTable, typeDefTable, paramTable, interfaceImplTable, memberRefTable, moduleTable, declSecurityTable, propertyTable, eventTable, standAloneSigTable, moduleRefTable, typeSpecTable, assemblyTable, assemblyRefTable, fileTable, exportedTypeTable, manifestResourceTable, genericParamTable, genericParamConstraintTable, methodSpecTable, documentTable, localScopeTable, localVariableTable, localConstantTable, importScopeTable, reader); }
                }
#if NET6_0_OR_GREATER
                internal void Load(Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, MethodDefTable? methodDefTable, FieldTable? fieldTable, TypeRefTable? typeRefTable, TypeDefTable? typeDefTable, ParamTable? paramTable, InterfaceImplTable? interfaceImplTable, MemberRefTable? memberRefTable, ModuleTable? moduleTable, DeclSecurityTable? declSecurityTable, PropertyTable? propertyTable, EventTable? eventTable, StandAloneSigTable? standAloneSigTable, ModuleRefTable? moduleRefTable, TypeSpecTable? typeSpecTable, AssemblyTable? assemblyTable, AssemblyRefTable? assemblyRefTable, FileTable? fileTable, ExportedTypeTable? exportedTypeTable, ManifestResourceTable? manifestResourceTable, GenericParamTable? genericParamTable, GenericParamConstraintTable? genericParamConstraintTable, MethodSpecTable? methodSpecTable, DocumentTable? documentTable, LocalScopeTable? localScopeTable, LocalVariableTable? localVariableTable, LocalConstantTable? localConstantTable, ImportScopeTable? importScopeTable, Span<byte> data, ref uint offset)
                {
                    int rows = _rows.Count;
                    for (int n = 0; n < rows; n++) { _rows[n].Load(blobHeap, GUIDHeap, methodDefTable, fieldTable, typeRefTable, typeDefTable, paramTable, interfaceImplTable, memberRefTable, moduleTable, declSecurityTable, propertyTable, eventTable, standAloneSigTable, moduleRefTable, typeSpecTable, assemblyTable, assemblyRefTable, fileTable, exportedTypeTable, manifestResourceTable, genericParamTable, genericParamConstraintTable, methodSpecTable, documentTable, localScopeTable, localVariableTable, localConstantTable, importScopeTable, data, ref offset); }
                }
#endif
                internal CustomDebugInformationTable(uint rows)
                {
                    for (int n = 0; n < rows; n++)
                    {
                        _rows.Add(new CustomDebugInformationTableRow(this, (uint)(_rows.Count + 1)));
                    }
                }
            }
        }
    }
}
