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

using System.IO;
using static Runic.Dotnet.Assembly.MetadataTable;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public abstract partial class MetadataTable
        {
            public abstract int ID { get; }
            public abstract bool Sorted { get; }
            internal MetadataTable()
            {
            }
            public abstract class MetadataTableRow
            {
                public abstract uint Row { get; }
                public abstract uint Length { get; }
            }
            public abstract uint Columns { get; }
            public abstract uint Rows { get; }
            public virtual int TableIndexSize { get { return Rows < 65536 ? 2 : 4; } }
            public virtual bool LargeIndices { get { return Rows >= 65536; } }
#if NET6_0_OR_GREATER
            public static MetadataTable[] Load(Span<byte> data, uint offset, MetadataRoot root, out Heap.StringHeap stringHeap, out Heap.BlobHeap blobHeap, out Heap.GUIDHeap GUIDHeap)
            {
                Runic.Dotnet.Assembly.MetadataRoot.Stream stringStream = null;
                Runic.Dotnet.Assembly.MetadataRoot.Stream blobStream = null;
                Runic.Dotnet.Assembly.MetadataRoot.Stream GUIDStream = null;
                for (int n = 0; n < root.Streams.Length; n++)
                {
                    switch (root.Streams[n].Name)
                    {
                        case "#Strings":
                            stringStream = root.Streams[n];
                            break;
                        case "#Blob":
                            blobStream = root.Streams[n];
                            break;
                        case "#GUID":
                            GUIDStream = root.Streams[n];
                            break;
                    }
                }
                return Load(data, offset, stringStream, blobStream, GUIDStream, out stringHeap, out blobHeap, out GUIDHeap);
            }
#endif

            public static MetadataTable[] Load(BinaryReader reader, MetadataRoot root, out Heap.StringHeap stringHeap, out Heap.BlobHeap blobHeap, out Heap.GUIDHeap GUIDHeap)
            {
                Runic.Dotnet.Assembly.MetadataRoot.Stream stringStream = null;
                Runic.Dotnet.Assembly.MetadataRoot.Stream blobStream = null;
                Runic.Dotnet.Assembly.MetadataRoot.Stream GUIDStream = null;
                for (int n = 0; n < root.Streams.Length; n++)
                {
                    switch (root.Streams[n].Name)
                    {
                        case "#Strings":
                            stringStream = root.Streams[n];
                            break;
                        case "#Blob":
                            blobStream = root.Streams[n];
                            break;
                        case "#GUID":
                            GUIDStream = root.Streams[n];
                            break;
                    }
                }
                return Load(reader, stringStream, blobStream, GUIDStream, out stringHeap, out blobHeap, out GUIDHeap);
            }
#if NET6_0_OR_GREATER
            public static MetadataTable[] Load(Span<byte> data, uint offset, MetadataRoot.Stream strings, MetadataRoot.Stream blob, MetadataRoot.Stream GUID, out Heap.StringHeap stringHeap, out Heap.BlobHeap blobHeap, out Heap.GUIDHeap GUIDHeap)
            {
                offset += 4; // Reserved
                byte tableSchemataMajorVersion = data[(int)offset]; offset++;
                byte tableSchemataMinorVersion = data[(int)offset]; offset++;
                byte heapSize = data[(int)offset]; offset++;
                bool stringLargeIndices = (heapSize & 0x01) != 0;
                bool GUIDLargeIndices = (heapSize & 0x02) != 0;
                bool blobLargeIndices = (heapSize & 0x04) != 0;

                stringHeap = new Heap.StringHeap(stringLargeIndices, strings.RelativeVirtualAddress, strings.Size);
                blobHeap = new Heap.BlobHeap(blobLargeIndices, blob.RelativeVirtualAddress, blob.Size);
                GUIDHeap = new Heap.GUIDHeap(GUIDLargeIndices, GUID.RelativeVirtualAddress, GUID.Size);

                offset++; // Reserved
                ulong validTables = BitConverterLE.ToUInt64(data, offset); offset += 8;
                ulong sortedTables = BitConverterLE.ToUInt64(data, offset); offset += 8;
                uint[] rows = new uint[64];
                uint tableCount = 0;
                for (int n = 0; n < 64; n++)
                {
                    if ((validTables & (1UL << n)) != 0)
                    {
                        rows[n] = BitConverterLE.ToUInt32(data, offset); offset += 4;
                        tableCount++;
                    }
                }

                MetadataTable[] tables = new MetadataTable[tableCount];
                FieldTable fieldTable = null;
                ModuleTable moduleTable = null;
                MethodDefTable? methodDefTable = null;
                ParamTable? paramTable = null;
                InterfaceImplTable? interfaceImplTable = null;
                DocumentTable? documentTable = null;
                ConstantTable? constantTable = null;
                LocalVariableTable? localVariableTable = null;
                LocalConstantTable? localConstantTable = null;
                ImportScopeTable? importScopeTable = null;
                TypeDefTable? typeDefTable = null;
                TypeRefTable? typeRefTable = null;
                TypeSpecTable? typeSpecTable = null;
                ModuleRefTable? moduleRefTable = null;
                DeclSecurityTable? declSecurityTable = null;
                ClassLayoutTable? classLayoutTable = null;
                FieldMarshalTable? fieldMarshalTable = null;
                FieldLayoutTable? fieldLayoutTable = null;
                EventTable? eventTable = null;
                FileTable? fileTable = null;
                ExportedTypeTable? exportedTypeTable = null;
                ManifestResourceTable? manifestResourceTable = null;
                GenericParamTable? genericParamTable = null;
                GenericParamConstraintTable? genericParamConstraintTable = null;
                MethodSpecTable? methodSpecTable = null;
                MethodSemanticsTable? methodSemanticsTable = null;
                PropertyTable? propertyTable = null;
                PropertyMapTable? propertyMapTable = null;
                MethodImplTable? methodImplTable = null;
                MemberRefTable? memberRefTable = null;
                AssemblyRefTable? assemblyRefTable = null;
                AssemblyTable? assemblyTable = null;
                fieldTable = new FieldTable(rows[0x4]);
                methodDefTable = new MethodDefTable(rows[0x6]);
                paramTable = new ParamTable(rows[0x8]);
                if ((validTables & (1UL << 0x01)) != 0) { typeRefTable = new TypeRefTable(rows[0x01]); }
                if ((validTables & (1UL << 0x09)) != 0) { interfaceImplTable = new InterfaceImplTable(rows[0x09]); }
                if ((validTables & (1UL << 0x0B)) != 0) { constantTable = new ConstantTable(rows[0x0B]); }
                if ((validTables & (1UL << 0x0D)) != 0) { fieldMarshalTable = new FieldMarshalTable(rows[0x0D]); }
                if ((validTables & (1UL << 0x0E)) != 0) { declSecurityTable = new DeclSecurityTable(rows[0x0E]); }
                if ((validTables & (1UL << 0x0F)) != 0) { classLayoutTable = new ClassLayoutTable(rows[0x0F]); }
                if ((validTables & (1UL << 0x10)) != 0) { fieldLayoutTable = new FieldLayoutTable(rows[0x10]); }
                if ((validTables & (1UL << 0x14)) != 0) { eventTable = new EventTable(rows[0x14]); }
                if ((validTables & (1UL << 0x15)) != 0) { propertyMapTable = new PropertyMapTable(rows[0x15]); }
                if ((validTables & (1UL << 0x17)) != 0) { propertyTable = new PropertyTable(rows[0x17]); }
                if ((validTables & (1UL << 0x18)) != 0) { methodSemanticsTable = new MethodSemanticsTable(rows[0x18]); }
                if ((validTables & (1UL << 0x19)) != 0) { methodImplTable = new MethodImplTable(rows[0x19]); }
                if ((validTables & (1UL << 0x1A)) != 0) { moduleRefTable = new ModuleRefTable(rows[0x1A]); }
                if ((validTables & (1UL << 0x1B)) != 0) { typeSpecTable = new TypeSpecTable(rows[0x1B]); }
                if ((validTables & (1UL << 0x20)) != 0) { assemblyTable = new AssemblyTable(rows[0x20]); }
                if ((validTables & (1UL << 0x23)) != 0) { assemblyRefTable = new AssemblyRefTable(rows[0x23]); }
                if ((validTables & (1UL << 0x26)) != 0) { fileTable = new FileTable(rows[0x26]); }
                if ((validTables & (1UL << 0x27)) != 0) { exportedTypeTable = new ExportedTypeTable(rows[0x27]); }
                if ((validTables & (1UL << 0x28)) != 0) { manifestResourceTable = new ManifestResourceTable(rows[0x28]); }
                if ((validTables & (1UL << 0x2A)) != 0) { genericParamTable = new GenericParamTable(rows[0x2A]); }
                if ((validTables & (1UL << 0x2B)) != 0) { methodSpecTable = new MethodSpecTable(rows[0x2B]); }
                if ((validTables & (1UL << 0x2C)) != 0) { genericParamConstraintTable = new GenericParamConstraintTable(rows[0x2C]); }
                if ((validTables & (1UL << 0x33)) != 0) { localVariableTable = new LocalVariableTable(rows[0x33]); }
                if ((validTables & (1UL << 0x34)) != 0) { localConstantTable = new LocalConstantTable(rows[0x34]); }
                if ((validTables & (1UL << 0x35)) != 0) { importScopeTable = new ImportScopeTable(rows[0x35]); }

                for (int n = 0, table = 0; n < 64; n++)
                {
                    if ((validTables & (1UL << n)) != 0)
                    {
                        switch (n)
                        {
                            case 0x00: moduleTable = new ModuleTable(rows[n], stringHeap, GUIDHeap, data, ref offset); tables[table] = moduleTable; break;
                            case 0x01: typeRefTable.Load(stringHeap, moduleTable, moduleRefTable, assemblyRefTable, data, ref offset); tables[table] = typeRefTable; break;
                            case 0x02: typeDefTable = new TypeDefTable(rows[n], stringHeap, fieldTable, methodDefTable, typeRefTable, typeSpecTable, data, ref offset); tables[table] = typeDefTable; break;
                            case 0x04: fieldTable.Load(stringHeap, blobHeap, data, ref offset); tables[table] = fieldTable; break;
                            case 0x06: methodDefTable.Load(stringHeap, blobHeap, paramTable, data, ref offset); tables[table] = methodDefTable; break;
                            case 0x08: paramTable.Load(stringHeap, data, ref offset); tables[table] = paramTable; break;
                            case 0x09: interfaceImplTable.Load(typeDefTable, typeRefTable, typeSpecTable, data, ref offset); tables[table] = interfaceImplTable; break;
                            case 0x0A: memberRefTable = new MemberRefTable(rows[n], stringHeap, blobHeap, typeDefTable, typeRefTable, typeSpecTable, moduleRefTable, methodDefTable, data, ref offset); tables[table] = memberRefTable; break;
                            case 0x0B: constantTable.Load(blobHeap, fieldTable, paramTable, propertyTable, data, ref offset); tables[table] = constantTable; break;
                            case 0x0C: CustomAttributeTable customAttributeTable = new CustomAttributeTable(rows[n], blobHeap, methodDefTable, fieldTable, typeRefTable, typeDefTable, paramTable, interfaceImplTable, memberRefTable, moduleTable, declSecurityTable, eventTable, propertyTable, typeSpecTable, assemblyTable, assemblyRefTable, fileTable, exportedTypeTable, manifestResourceTable, genericParamTable, genericParamConstraintTable, methodSpecTable, data, ref offset); tables[table] = customAttributeTable; break;
                            case 0x0D: fieldMarshalTable.Load(blobHeap, fieldTable, paramTable, data, ref offset); tables[table] = fieldMarshalTable; break;
                            case 0x0E: declSecurityTable.Load(blobHeap,typeDefTable, methodDefTable, assemblyTable, data, ref offset); tables[table] = declSecurityTable; break;
                            case 0x0F: classLayoutTable.Load(typeDefTable, data, ref offset); tables[table] = classLayoutTable; break;
                            case 0x10: fieldLayoutTable.Load(fieldTable, data, ref offset); tables[table] = fieldLayoutTable; break;
                            case 0x11: StandAloneSigTable standAloneSigTable = new StandAloneSigTable(rows[n], blobHeap, data, ref offset); tables[table] = standAloneSigTable; break;
                            case 0x14: eventTable.Load(stringHeap, typeDefTable, typeRefTable, typeSpecTable, data, ref offset); tables[table] = eventTable; break;
                            case 0x15: propertyMapTable.Load(typeDefTable, propertyTable, data, ref offset); tables[table] = propertyMapTable; break;
                            case 0x17: propertyTable.Load(stringHeap, blobHeap, data, ref offset); tables[table] = propertyTable; break;
                            case 0x18: methodSemanticsTable.Load(methodDefTable, eventTable, propertyTable, data, ref offset); tables[table] = methodSemanticsTable; break;
                            case 0x19: methodImplTable.Load(typeDefTable, methodDefTable, memberRefTable, data, ref offset); tables[table] = methodImplTable; break;
                            case 0x1A: moduleRefTable.Load(stringHeap, data, ref offset); tables[table] = moduleRefTable; break;
                            case 0x1B: typeSpecTable.Load(blobHeap, data, ref offset); tables[table] = typeSpecTable; break;
                            case 0x1C: ImplMapTable implMapTable = new ImplMapTable(rows[n], stringHeap, data, ref offset); tables[table] = implMapTable; break;
                            case 0x1D: FieldRVATable fieldRVATable = new FieldRVATable(rows[n], fieldTable, data, ref offset); tables[table] = fieldRVATable; break;
                            case 0x20: assemblyTable.Load(stringHeap, blobHeap, data, ref offset); tables[table] = assemblyTable; break;
                            case 0x23: assemblyRefTable.Load(stringHeap, blobHeap, data, ref offset); tables[table] = assemblyRefTable; break;
                            case 0x26: fileTable.Load(stringHeap, blobHeap, data, ref offset); tables[table] = fileTable; break;
                            case 0x27: exportedTypeTable.Load(stringHeap, typeDefTable, fileTable, assemblyRefTable, data, ref offset); tables[table] = exportedTypeTable; break;
                            case 0x28: manifestResourceTable.Load(stringHeap, fileTable, assemblyRefTable, exportedTypeTable, data, ref offset); tables[table] = manifestResourceTable; break;
                            case 0x29: NestedClassTable nestedClassTable = new NestedClassTable(rows[n], typeDefTable, data, ref offset); tables[table] = nestedClassTable; break;
                            case 0x2A: genericParamTable.Load(stringHeap, typeDefTable, methodDefTable, data, ref offset); tables[table] = genericParamTable; break;
                            case 0x2B: methodSpecTable.Load(blobHeap, methodDefTable, memberRefTable, data, ref offset); tables[table] = methodSpecTable; break;
                            case 0x2C: genericParamConstraintTable.Load(genericParamTable, typeDefTable, typeRefTable, typeSpecTable, data, ref offset); tables[table] = genericParamConstraintTable; break;
                            case 0x30: documentTable = new DocumentTable(rows[n], blobHeap, GUIDHeap, data, ref offset); tables[table] = documentTable; break;
                            case 0x31: MethodDebugInformationTable methodDebugInformationTable = new MethodDebugInformationTable(rows[n], documentTable, blobHeap, data, ref offset); tables[table] = methodDebugInformationTable; break;
                            case 0x32: LocalScopeTable localScopeTable = new LocalScopeTable(rows[n], methodDefTable, importScopeTable, localVariableTable, localConstantTable, data, ref offset); tables[table] = localScopeTable; break;
                            case 0x33: localVariableTable.Load(stringHeap, data, ref offset); tables[table] = localVariableTable; break;
                            case 0x34: localConstantTable.Load(stringHeap, blobHeap, data, ref offset); tables[table] = localConstantTable; break;
                            case 0x35: importScopeTable.Load(blobHeap, data, ref offset); tables[table] = importScopeTable; break;
                        }
                        table++;
                    }
                }
                return tables;
            }
#endif
            public static MetadataTable[] Load(BinaryReader reader, MetadataRoot.Stream strings, MetadataRoot.Stream blob, MetadataRoot.Stream GUID, out Heap.StringHeap stringHeap, out Heap.BlobHeap blobHeap, out Heap.GUIDHeap GUIDHeap)
            {
                reader.ReadUInt32(); // Reserved
                byte tableSchemataMajorVersion = reader.ReadByte();
                byte tableSchemataMinorVersion = reader.ReadByte();
                byte heapSize = reader.ReadByte();
                bool stringLargeIndices = (heapSize & 0x01) != 0;
                bool GUIDLargeIndices = (heapSize & 0x02) != 0;
                bool blobLargeIndices = (heapSize & 0x04) != 0;

                stringHeap = new Heap.StringHeap(stringLargeIndices, strings.RelativeVirtualAddress, strings.Size);
                blobHeap = new Heap.BlobHeap(blobLargeIndices, blob.RelativeVirtualAddress, blob.Size);
                GUIDHeap = new Heap.GUIDHeap(GUIDLargeIndices, GUID.RelativeVirtualAddress, GUID.Size);

                reader.ReadByte(); // Reserved

                ulong validTables = reader.ReadUInt64();
                ulong sortedTables = reader.ReadUInt64();
                uint[] rows = new uint[64];
                uint tableCount = 0;
                for (int n = 0; n < 64; n++)
                {
                    if ((validTables & (1UL << n)) != 0)
                    {
                        rows[n] = reader.ReadUInt32();
                        tableCount++;
                    }
                }

                MetadataTable[] tables = new MetadataTable[tableCount];
#if NET6_0_OR_GREATER
                FieldTable? fieldTable = null;
                MethodDefTable? methodDefTable = null;
                ParamTable? paramTable = null;
                InterfaceImplTable? interfaceImplTable = null;
                DocumentTable? documentTable = null;
                ConstantTable? constantTable = null;
                LocalVariableTable? localVariableTable = null;
                LocalConstantTable? localConstantTable = null;
                ImportScopeTable? importScopeTable = null;
                TypeDefTable? typeDefTable = null;
                TypeRefTable? typeRefTable = null;
                TypeSpecTable? typeSpecTable = null;
                ModuleTable? moduleTable = null;
                FieldMarshalTable? fieldMarshalTable = null;
                ClassLayoutTable? classLayoutTable = null;
                FieldLayoutTable? fieldLayoutTable = null;
                FileTable? fileTable = null;
                ExportedTypeTable? exportedTypeTable = null;
                ManifestResourceTable? manifestResourceTable = null;
                GenericParamTable? genericParamTable = null;
                GenericParamConstraintTable? genericParamConstraintTable = null;
                MethodSpecTable? methodSpecTable = null;
                EventTable? eventTable = null;
                EventMapTable? eventMapTable = null;
                PropertyMapTable? propertyMapTable = null;
                PropertyTable? propertyTable = null;
                MethodImplTable? methodImplTable = null;
                MethodSemanticsTable? methodSemanticsTable = null;
                ModuleRefTable? moduleRefTable = null;
                AssemblyTable? assemblyTable = null;
                AssemblyRefTable? assemblyRefTable = null;
                MemberRefTable? memberRefTable = null;
                DeclSecurityTable? declSecurityTable = null;
#else
                FieldTable fieldTable = null;
                MethodDefTable methodDefTable = null;
                ParamTable paramTable = null;
                InterfaceImplTable interfaceImplTable = null;
                DocumentTable documentTable = null;
                ConstantTable constantTable = null;
                LocalVariableTable localVariableTable = null;
                LocalConstantTable localConstantTable = null;
                ImportScopeTable importScopeTable = null;
                TypeDefTable typeDefTable = null;
                TypeRefTable typeRefTable = null;
                TypeSpecTable typeSpecTable = null;
                ModuleTable moduleTable = null;
                FieldMarshalTable fieldMarshalTable = null;
                ClassLayoutTable classLayoutTable = null;
                FieldLayoutTable fieldLayoutTable = null;
                ModuleRefTable moduleRefTable = null;
                FileTable fileTable = null;
                ExportedTypeTable exportedTypeTable = null;
                ManifestResourceTable manifestResourceTable = null;
                GenericParamTable genericParamTable = null;
                GenericParamConstraintTable genericParamConstraintTable = null;
                MethodSpecTable methodSpecTable = null;
                EventTable eventTable = null;
                EventMapTable eventMapTable = null;
                PropertyMapTable propertyMapTable = null;
                PropertyTable propertyTable = null;
                MethodImplTable methodImplTable = null;
                MethodSemanticsTable methodSemanticsTable = null;
                AssemblyTable assemblyTable = null;
                AssemblyRefTable assemblyRefTable = null;
                MemberRefTable memberRefTable = null;
                DeclSecurityTable declSecurityTable = null;
#endif
                fieldTable = new FieldTable(rows[0x4]);
                methodDefTable = new MethodDefTable(rows[0x6]);
                paramTable = new ParamTable(rows[0x8]);
                if ((validTables & (1UL << 0x01)) != 0) { typeRefTable = new TypeRefTable(rows[0x01]); }
                if ((validTables & (1UL << 0x09)) != 0) { interfaceImplTable = new InterfaceImplTable(rows[0x09]); }
                if ((validTables & (1UL << 0x0B)) != 0) { constantTable = new ConstantTable(rows[0x0B]); }
                if ((validTables & (1UL << 0x0D)) != 0) { fieldMarshalTable = new FieldMarshalTable(rows[0x0D]); }
                if ((validTables & (1UL << 0x0E)) != 0) { declSecurityTable = new DeclSecurityTable(rows[0x0E]); }
                if ((validTables & (1UL << 0x0F)) != 0) { classLayoutTable = new ClassLayoutTable(rows[0x0F]); }
                if ((validTables & (1UL << 0x10)) != 0) { fieldLayoutTable = new FieldLayoutTable(rows[0x10]); }
                if ((validTables & (1UL << 0x12)) != 0) { eventMapTable = new EventMapTable(rows[0x12]); }
                if ((validTables & (1UL << 0x14)) != 0) { eventTable = new EventTable(rows[0x14]); }
                if ((validTables & (1UL << 0x15)) != 0) { propertyMapTable = new PropertyMapTable(rows[0x15]); }
                if ((validTables & (1UL << 0x17)) != 0) { propertyTable = new PropertyTable(rows[0x17]); }
                if ((validTables & (1UL << 0x18)) != 0) { methodSemanticsTable = new MethodSemanticsTable(rows[0x18]); }
                if ((validTables & (1UL << 0x19)) != 0) { methodImplTable = new MethodImplTable(rows[0x19]); }
                if ((validTables & (1UL << 0x1A)) != 0) { moduleRefTable = new ModuleRefTable(rows[0x1A]); }
                if ((validTables & (1UL << 0x1B)) != 0) { typeSpecTable = new TypeSpecTable(rows[0x1B]); }
                if ((validTables & (1UL << 0x20)) != 0) { assemblyTable = new AssemblyTable(rows[0x20]); }
                if ((validTables & (1UL << 0x23)) != 0) { assemblyRefTable = new AssemblyRefTable(rows[0x23]); }
                if ((validTables & (1UL << 0x26)) != 0) { fileTable = new FileTable(rows[0x26]); }
                if ((validTables & (1UL << 0x27)) != 0) { exportedTypeTable = new ExportedTypeTable(rows[0x27]); }
                if ((validTables & (1UL << 0x28)) != 0) { manifestResourceTable = new ManifestResourceTable(rows[0x28]); }
                if ((validTables & (1UL << 0x2A)) != 0) { genericParamTable = new GenericParamTable(rows[0x2A]); }
                if ((validTables & (1UL << 0x2B)) != 0) { methodSpecTable = new MethodSpecTable(rows[0x2B]); }
                if ((validTables & (1UL << 0x2C)) != 0) { genericParamConstraintTable = new GenericParamConstraintTable(rows[0x2C]); }
                if ((validTables & (1UL << 0x33)) != 0) { localVariableTable = new LocalVariableTable(rows[0x33]); }
                if ((validTables & (1UL << 0x35)) != 0) { importScopeTable = new ImportScopeTable(rows[0x35]); }

                for (int n = 0, table = 0; n < 64; n++)
                {
                    if ((validTables & (1UL << n)) != 0)
                    {
                        switch (n)
                        {
                            case 0x00: moduleTable = new ModuleTable(rows[n], stringHeap, GUIDHeap, reader); tables[table] = moduleTable; break;
                            case 0x01: typeRefTable.Load(stringHeap, moduleTable, moduleRefTable, assemblyRefTable, reader); tables[table] = typeRefTable; break;
                            case 0x02: typeDefTable = new TypeDefTable(rows[n], stringHeap, fieldTable, methodDefTable, typeRefTable, typeSpecTable, reader); tables[table] = typeDefTable; break;
                            case 0x04: fieldTable.Load(stringHeap, blobHeap, reader); tables[table] = fieldTable; break;
                            case 0x06: methodDefTable.Load(stringHeap, blobHeap, paramTable, reader); tables[table] = methodDefTable; break;
                            case 0x08: paramTable.Load(stringHeap, reader); tables[table] = paramTable; break;
                            case 0x09: interfaceImplTable.Load(typeDefTable, typeRefTable, typeSpecTable, reader); tables[table] = interfaceImplTable; break;
                            case 0x0A: memberRefTable = new MemberRefTable(rows[n], stringHeap, blobHeap, typeDefTable, typeRefTable, typeSpecTable, moduleRefTable, methodDefTable, reader); tables[table] = memberRefTable; break;
                            case 0x0B: constantTable.Load(blobHeap, fieldTable, paramTable, propertyTable, reader); tables[table] = constantTable; break;
                            case 0x0C: CustomAttributeTable customAttributeTable = new CustomAttributeTable(rows[n], blobHeap, methodDefTable, fieldTable, typeRefTable, typeDefTable, paramTable, interfaceImplTable, memberRefTable, moduleTable, declSecurityTable, eventTable, propertyTable, typeSpecTable, assemblyTable, assemblyRefTable, fileTable, exportedTypeTable, manifestResourceTable, genericParamTable, genericParamConstraintTable, methodSpecTable, reader); tables[table] = customAttributeTable; break;
                            case 0x0D: fieldMarshalTable.Load(blobHeap, fieldTable, paramTable, reader); tables[table] = fieldMarshalTable; break;
                            case 0x0E: declSecurityTable.Load(blobHeap, typeDefTable, methodDefTable, assemblyTable, reader); tables[table] = declSecurityTable; break;
                            case 0x0F: classLayoutTable.Load(typeDefTable, reader); tables[table] = classLayoutTable; break;
                            case 0x10: fieldLayoutTable.Load(fieldTable, reader); tables[table] = fieldLayoutTable; break;
                            case 0x11: StandAloneSigTable standAloneSigTable = new StandAloneSigTable(rows[n], blobHeap, reader); tables[table] = standAloneSigTable; break;
                            case 0x12: eventMapTable.Load(typeDefTable, eventTable, reader); tables[table] = eventMapTable; break;
                            case 0x14: eventTable.Load(stringHeap, typeDefTable, typeRefTable, typeSpecTable, reader); tables[table] = eventTable; break;
                            case 0x15: propertyMapTable.Load(typeDefTable, propertyTable, reader); tables[table] = propertyMapTable; break;
                            case 0x17: propertyTable.Load(stringHeap, blobHeap, reader); tables[table] = propertyTable; break;
                            case 0x18: methodSemanticsTable.Load(methodDefTable, eventTable, propertyTable, reader); tables[table] = methodSemanticsTable; break;
                            case 0x19: methodImplTable.Load(typeDefTable, methodDefTable, memberRefTable, reader); tables[table] = methodImplTable; break;
                            case 0x1A: moduleRefTable.Load(stringHeap, reader); tables[table] = moduleRefTable; break;
                            case 0x1B: typeSpecTable.Load(blobHeap, reader); tables[table] = typeSpecTable; break;
                            case 0x1C: ImplMapTable implMapTable = new ImplMapTable(rows[n], stringHeap, reader); tables[table] = implMapTable; break;
                            case 0x1D: FieldRVATable fieldRVATable = new FieldRVATable(rows[n], fieldTable, reader); tables[table] = fieldRVATable; break;
                            case 0x20: assemblyTable.Load(stringHeap, blobHeap, reader); tables[table] = assemblyTable; break;
                            case 0x23: assemblyRefTable.Load(stringHeap, blobHeap, reader); tables[table] = assemblyRefTable; break;
                            case 0x26: fileTable.Load(stringHeap, blobHeap, reader); tables[table] = fileTable; break;
                            case 0x27: exportedTypeTable.Load(stringHeap, typeDefTable, fileTable, assemblyRefTable, reader); tables[table] = exportedTypeTable; break;
                            case 0x28: manifestResourceTable.Load(stringHeap, fileTable, assemblyRefTable, exportedTypeTable, reader); tables[table] = manifestResourceTable; break;
                            case 0x29: NestedClassTable nestedClassTable = new NestedClassTable(rows[n], typeDefTable, reader); tables[table] = nestedClassTable; break;
                            case 0x2A: genericParamTable.Load(stringHeap, typeDefTable, methodDefTable, reader); tables[table] = genericParamTable; break;
                            case 0x2B: methodSpecTable.Load(blobHeap, methodDefTable, memberRefTable, reader); tables[table] = methodSpecTable; break;
                            case 0x2C: genericParamConstraintTable.Load(genericParamTable, typeDefTable, typeRefTable, typeSpecTable, reader); tables[table] = genericParamConstraintTable; break;
                            case 0x30: documentTable = new DocumentTable(rows[n], blobHeap, GUIDHeap, reader); tables[table] = documentTable; break;
                            case 0x31: MethodDebugInformationTable methodDebugInformationTable = new MethodDebugInformationTable(rows[n], documentTable, blobHeap, reader); tables[table] = methodDebugInformationTable; break;
                            case 0x32: LocalScopeTable localScopeTable = new LocalScopeTable(rows[n], methodDefTable, importScopeTable, localVariableTable, localConstantTable, reader); tables[table] = localScopeTable; break;
                            case 0x33: localVariableTable.Load(stringHeap, reader); tables[table] = localVariableTable; break;
                            case 0x34: localConstantTable.Load(stringHeap, blobHeap, reader); tables[table] = localConstantTable; break;
                            case 0x35: importScopeTable.Load(blobHeap, reader); tables[table] = importScopeTable; break;
                        }
                        table++;
                    }
                }
                return tables;
            }

            public static void Save(MetadataTable[] metadataTables, Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, BinaryWriter binaryWriter)
            {
#if NET6_0_OR_GREATER
                ModuleTable? moduleTable = null;
                TypeRefTable? typeRefTable = null;
                TypeDefTable? typeDefTable = null;
                TypeSpecTable? typeSpecTable = null;
                FieldTable? fieldTable = null;
                MethodDefTable? methodDefTable = null;
                ParamTable? paramTable = null;
                InterfaceImplTable? interfaceImplTable = null;
                MemberRefTable? memberRefTable = null;
                ConstantTable? constantTable = null;
                CustomAttributeTable? customAttributeTable = null;
                DeclSecurityTable? declSecurityTable = null;
                StandAloneSigTable? standAloneSigTable = null;
                ModuleRefTable? moduleRefTable = null;
                FieldMarshalTable? fieldMarshalTable = null;
                ClassLayoutTable? classLayoutTable = null;
                FieldLayoutTable? fieldLayoutTable = null;
                EventMapTable? eventMapTable = null;
                FileTable? fileTable = null;
                ExportedTypeTable? exportedTypeTable = null;
                ManifestResourceTable? manifestResourceTable = null;
                GenericParamTable? genericParamTable = null;
                GenericParamConstraintTable? genericParamConstraintTable = null;
                MethodSpecTable? methodSpecTable = null;
                EventTable? eventTable = null;
                PropertyMapTable? propertyMapTable = null;
                PropertyTable? propertyTable = null;
                MethodImplTable? methodImplTable = null;
                MethodSemanticsTable? methodSemanticsTable = null;
                ImplMapTable? implMapTable = null;
                FieldRVATable? fieldRVATable = null;
                AssemblyTable? assemblyTable = null;
                AssemblyRefTable? assemblyRefTable = null;
                NestedClassTable? nestedClassTable = null;
                DocumentTable? documentTable = null;
                MethodDebugInformationTable? methodDebugInformationTable = null;
                LocalScopeTable? localScopeTable = null;
                LocalVariableTable? localVariableTable = null;
                LocalConstantTable? localConstantTable = null;
                ImportScopeTable? importScopeTable = null;
#else
                ModuleTable moduleTable = null;
                TypeRefTable typeRefTable = null;
                TypeDefTable typeDefTable = null;
                TypeSpecTable typeSpecTable = null;
                FieldTable fieldTable = null;
                MethodDefTable methodDefTable = null;
                ParamTable paramTable = null;
                InterfaceImplTable interfaceImplTable = null;
                MemberRefTable memberRefTable = null;
                ConstantTable constantTable = null;
                CustomAttributeTable customAttributeTable = null;
                DeclSecurityTable declSecurityTable = null;
                StandAloneSigTable standAloneSigTable = null;
                ModuleRefTable moduleRefTable = null;
                FieldMarshalTable fieldMarshalTable = null;
                ClassLayoutTable classLayoutTable = null;
                FieldLayoutTable fieldLayoutTable = null;
                FileTable fileTable = null;
                ExportedTypeTable exportedTypeTable = null;
                ManifestResourceTable manifestResourceTable = null;
                GenericParamTable genericParamTable = null;
                GenericParamConstraintTable genericParamConstraintTable = null;
                MethodSpecTable methodSpecTable = null;
                EventMapTable eventMapTable = null;
                EventTable eventTable = null;
                PropertyMapTable propertyMapTable = null;
                PropertyTable propertyTable = null;
                MethodImplTable methodImplTable = null;
                MethodSemanticsTable methodSemanticsTable = null;
                ImplMapTable implMapTable = null;
                FieldRVATable fieldRVATable = null;
                AssemblyTable assemblyTable = null;
                AssemblyRefTable assemblyRefTable = null;
                NestedClassTable nestedClassTable = null;
                DocumentTable documentTable = null;
                MethodDebugInformationTable methodDebugInformationTable = null;
                LocalScopeTable localScopeTable = null;
                LocalVariableTable localVariableTable = null;
                LocalConstantTable localConstantTable = null;
                ImportScopeTable importScopeTable = null;
#endif
                uint[] rows = new uint[64];
                ulong validTables = 0;

                for (int n = 0; n < metadataTables.Length; n++)
                {
                    switch (metadataTables[n])
                    {
                        case ModuleTable table: if (moduleTable != null) { throw new System.Exception("More than one Module Table was provided"); } moduleTable = table; rows[0x00] = table.Rows; validTables |= (1UL << 0x00); break;
                        case TypeRefTable table: if (typeRefTable != null) { throw new System.Exception("More than one TypeRef Table was provided"); } typeRefTable = table; rows[0x01] = table.Rows; validTables |= (1UL << 0x01); break;
                        case TypeDefTable table: if (typeDefTable != null) { throw new System.Exception("More than one TypeDef Table was provided"); } typeDefTable = table; rows[0x02] = table.Rows; validTables |= (1UL << 0x02); break;
                        case FieldTable table: if (fieldTable != null) { throw new System.Exception("More than one Field Table was provided"); } fieldTable = table; rows[0x04] = table.Rows; validTables |= (1UL << 0x04); break;
                        case MethodDefTable table: if (methodDefTable != null) { throw new System.Exception("More than one MethodDef Table was provided"); } methodDefTable = table; rows[0x06] = table.Rows; validTables |= (1UL << 0x06); break;
                        case ParamTable table: if (paramTable != null) { throw new System.Exception("More than one Param Table was provided"); } paramTable = table; rows[0x08] = table.Rows; validTables |= (1UL << 0x08); break;
                        case InterfaceImplTable table: if (interfaceImplTable != null) { throw new System.Exception("More than one InterfaceImpl Table was provided"); } interfaceImplTable = table; rows[0x09] = table.Rows; validTables |= (1UL << 0x09); break;
                        case MemberRefTable table: if (memberRefTable != null) { throw new System.Exception("More than one MemberRef Table was provided"); } memberRefTable = table; rows[0x0A] = table.Rows; validTables |= (1UL << 0x0A); break;
                        case ConstantTable table: if (constantTable != null) { throw new System.Exception("More than one Constant Table was provided"); } constantTable = table; rows[0x0B] = table.Rows; validTables |= (1UL << 0x0B); break;
                        case CustomAttributeTable table: if (customAttributeTable != null) { throw new System.Exception("More than one CustomAttribute Table was provided"); } customAttributeTable = table; rows[0x0C] = table.Rows; validTables |= (1UL << 0x0C); break;
                        case FieldMarshalTable table: if (fieldMarshalTable != null) { throw new System.Exception("More than one FieldMarshal Table was provided"); } fieldMarshalTable = table; rows[0x0D] = table.Rows; validTables |= (1UL << 0x0D); break;
                        case DeclSecurityTable table: if (declSecurityTable != null) { throw new System.Exception("More than one DeclSecurity Table was provided"); } declSecurityTable = table; rows[0x0E] = table.Rows; validTables |= (1UL << 0x0E); break;
                        case ClassLayoutTable table: if (classLayoutTable != null) { throw new System.Exception("More than one ClassLayout Table was provided"); } classLayoutTable = table; rows[0x0F] = table.Rows; validTables |= (1UL << 0x0F); break;
                        case FieldLayoutTable table: if (fieldLayoutTable != null) { throw new System.Exception("More than one FieldLayout Table was provided"); } fieldLayoutTable = table; rows[0x10] = table.Rows; validTables |= (1UL << 0x10); break;
                        case StandAloneSigTable table: if (standAloneSigTable != null) { throw new System.Exception("More than one StandAloneSig Table was provided"); } standAloneSigTable = table; rows[0x11] = table.Rows; validTables |= (1UL << 0x11); break;
                        case EventMapTable table: if (eventMapTable != null) { throw new System.Exception("More than one eventMap Table was provided"); } eventMapTable = table; rows[0x12] = table.Rows; validTables |= (1UL << 0x12); break;
                        case EventTable table: if (eventTable != null) { throw new System.Exception("More than one eventTable Table was provided"); } eventTable = table; rows[0x14] = table.Rows; validTables |= (1UL << 0x14); break;
                        case PropertyMapTable table: if (propertyMapTable != null) { throw new System.Exception("More than one PropertyMap Table was provided"); } propertyMapTable = table; rows[0x15] = table.Rows; validTables |= (1UL << 0x15); break;
                        case PropertyTable table: if (propertyTable != null) { throw new System.Exception("More than one Property Table was provided"); } propertyTable = table; rows[0x17] = table.Rows; validTables |= (1UL << 0x17); break;
                        case MethodSemanticsTable table: if (methodSemanticsTable != null) { throw new System.Exception("More than one MethodSemantics Table was provided"); } methodSemanticsTable = table; rows[0x18] = table.Rows; validTables |= (1UL << 0x18); break;
                        case MethodImplTable table: if (methodImplTable != null) { throw new System.Exception("More than one MethodImpl Table was provided"); } methodImplTable = table; rows[0x19] = table.Rows; validTables |= (1UL << 0x19); break;
                        case ModuleRefTable table: if (moduleRefTable != null) { throw new System.Exception("More than one ModuleRef Table was provided"); } moduleRefTable = table; rows[0x1A] = table.Rows; validTables |= (1UL << 0x1A); break;
                        case ImplMapTable table: if (implMapTable != null) { throw new System.Exception("More than one ImplMap Table was provided"); } implMapTable = table; rows[0x1C] = table.Rows; validTables |= (1UL << 0x1C); break;
                        case TypeSpecTable table: if (typeSpecTable != null) { throw new System.Exception("More than one TypeSpec Table was provided"); } typeSpecTable = table; rows[0x1B] = table.Rows; validTables |= (1UL << 0x1B); break;
                        case FieldRVATable table: if (fieldRVATable != null) { throw new System.Exception("More than one FieldRVA Table was provided"); } fieldRVATable = table; rows[0x1D] = table.Rows; validTables |= (1UL << 0x1D); break;
                        case AssemblyTable table: if (assemblyTable != null) { throw new System.Exception("More than one Assembly Table was provided"); } assemblyTable = table; rows[0x20] = table.Rows; validTables |= (1UL << 0x20); break;
                        case AssemblyRefTable table: if (assemblyRefTable != null) { throw new System.Exception("More than one AssemblyRef Table was provided"); } assemblyRefTable = table; rows[0x23] = table.Rows; validTables |= (1UL << 0x23); break;
                        case FileTable table: if (fileTable != null) { throw new System.Exception("More than one File Table was provided"); } fileTable = table; rows[0x26] = table.Rows; validTables |= (1UL << 0x26); break;
                        case ExportedTypeTable table: if (exportedTypeTable != null) { throw new System.Exception("More than one ExportedType Table was provided"); } exportedTypeTable = table; rows[0x27] = table.Rows; validTables |= (1UL << 0x27); break;
                        case ManifestResourceTable table: if (manifestResourceTable != null) { throw new System.Exception("More than one ManifestResource Table was provided"); } manifestResourceTable = table; rows[0x28] = table.Rows; validTables |= (1UL << 0x28); break;
                        case NestedClassTable table: if (nestedClassTable != null) { throw new System.Exception("More than one NestedClass Table was provided"); } nestedClassTable = table; rows[0x29] = table.Rows; validTables |= (1UL << 0x29); break;
                        case GenericParamTable table: if (genericParamTable != null) { throw new System.Exception("More than one GenericParam Table was provided"); } genericParamTable = table; rows[0x2A] = table.Rows; validTables |= (1UL << 0x2A); break;
                        case MethodSpecTable table: if (methodSpecTable != null) { throw new System.Exception("More than one MethodSpec Table was provided"); } methodSpecTable = table; rows[0x2B] = table.Rows; validTables |= (1UL << 0x2B); break;
                        case GenericParamConstraintTable table: if (genericParamConstraintTable != null) { throw new System.Exception("More than one GenericParamConstraint Table was provided"); } genericParamConstraintTable = table; rows[0x2C] = table.Rows; validTables |= (1UL << 0x2C); break;
                        case DocumentTable table: if (documentTable != null) { throw new System.Exception("More than one Document Table was provided"); } documentTable = table; rows[0x30] = table.Rows; validTables |= (1UL << 0x30); break;
                        case MethodDebugInformationTable table: if (methodDebugInformationTable != null) { throw new System.Exception("More than one MethodDebugInformation Table was provided"); } methodDebugInformationTable = table; rows[0x31] = table.Rows; validTables |= (1UL << 0x31); break;
                        case LocalScopeTable table: if (localScopeTable != null) { throw new System.Exception("More than one LocalScope Table was provided"); } localScopeTable = table; rows[0x32] = table.Rows; validTables |= (1UL << 0x32); break;
                        case LocalVariableTable table: if (localVariableTable != null) { throw new System.Exception("More than one LocalVariable Table was provided"); } localVariableTable = table; rows[0x33] = table.Rows; validTables |= (1UL << 0x33); break;
                        case LocalConstantTable table: if (localConstantTable != null) { throw new System.Exception("More than one LocalConstant Table was provided"); } localConstantTable = table; rows[0x34] = table.Rows; validTables |= (1UL << 0x34); break;
                        case ImportScopeTable table: if (importScopeTable != null) { throw new System.Exception("More than one ImportScope Table was provided"); } importScopeTable = table; rows[0x35] = table.Rows; validTables |= (1UL << 0x35); break;
                    }
                }

                binaryWriter.Write((int)0); // Reserved
                binaryWriter.Write((byte)2);
                binaryWriter.Write((byte)0);
                byte heapSize = (byte)((stringHeap.LargeIndices ? 0x01 : 0x00) | (GUIDHeap.LargeIndices ? 0x02 : 0x00) | (blobHeap.LargeIndices ? 0x04 : 0x00));
                binaryWriter.Write(heapSize);

                binaryWriter.Write((byte)0); // Reserved

                binaryWriter.Write(validTables);
                binaryWriter.Write((ulong)0x000016003301FA00); // Sorted Tables

                for (int n = 0, table = 0; n < 64; n++)
                {
                    if ((validTables & (1UL << n)) != 0)
                    {
                        switch (n)
                        {
                            case 0x00: binaryWriter.Write(moduleTable.Rows); break;
                            case 0x01: binaryWriter.Write(typeRefTable.Rows); break;
                            case 0x02: binaryWriter.Write(typeDefTable.Rows); break;
                            case 0x04: binaryWriter.Write(fieldTable.Rows); break;
                            case 0x06: binaryWriter.Write(methodDefTable.Rows); break;
                            case 0x08: binaryWriter.Write(paramTable.Rows); break;
                            case 0x09: binaryWriter.Write(interfaceImplTable.Rows); break;
                            case 0x0A: binaryWriter.Write(memberRefTable.Rows); break;
                            case 0x0B: binaryWriter.Write(constantTable.Rows); break;
                            case 0x0C: binaryWriter.Write(customAttributeTable.Rows); break;
                            case 0x0D: binaryWriter.Write(fieldMarshalTable.Rows); break;
                            case 0x0E: binaryWriter.Write(declSecurityTable.Rows); break;
                            case 0x0F: binaryWriter.Write(classLayoutTable.Rows); break;
                            case 0x10: binaryWriter.Write(fieldLayoutTable.Rows); break;
                            case 0x11: binaryWriter.Write(standAloneSigTable.Rows); break;
                            case 0x12: binaryWriter.Write(eventMapTable.Rows); break;
                            case 0x14: binaryWriter.Write(eventTable.Rows); break;
                            case 0x15: binaryWriter.Write(propertyMapTable.Rows); break;
                            case 0x17: binaryWriter.Write(propertyTable.Rows); break;
                            case 0x18: binaryWriter.Write(methodSemanticsTable.Rows); break;
                            case 0x19: binaryWriter.Write(methodImplTable.Rows); break;
                            case 0x1A: binaryWriter.Write(moduleRefTable.Rows); break;
                            case 0x1B: binaryWriter.Write(typeSpecTable.Rows); break;
                            case 0x1C: binaryWriter.Write(implMapTable.Rows); break;
                            case 0x1D: binaryWriter.Write(fieldRVATable.Rows); break;
                            case 0x20: binaryWriter.Write(assemblyTable.Rows); break;
                            case 0x23: binaryWriter.Write(assemblyRefTable.Rows); break;
                            case 0x26: binaryWriter.Write(fileTable.Rows); break;
                            case 0x27: binaryWriter.Write(exportedTypeTable.Rows); break;
                            case 0x28: binaryWriter.Write(manifestResourceTable.Rows); break;
                            case 0x2A: binaryWriter.Write(genericParamTable.Rows); break;
                            case 0x2B: binaryWriter.Write(methodSpecTable.Rows); break;
                            case 0x2C: binaryWriter.Write(genericParamConstraintTable.Rows); break;
                            case 0x29: binaryWriter.Write(nestedClassTable.Rows); break;
                            case 0x30: binaryWriter.Write(documentTable.Rows); break;
                            case 0x31: binaryWriter.Write(methodDebugInformationTable.Rows); break;
                            case 0x32: binaryWriter.Write(localScopeTable.Rows); break;
                            case 0x33: binaryWriter.Write(localVariableTable.Rows); break;
                            case 0x34: binaryWriter.Write(localConstantTable.Rows); break;
                            case 0x35: binaryWriter.Write(importScopeTable.Rows); break;
                        }
                        table++;
                    }
                }

                for (int n = 0, table = 0; n < 64; n++)
                {
                    if ((validTables & (1UL << n)) != 0)
                    {
                        switch (n)
                        {   
                            case 0x00: moduleTable.Save(GUIDHeap, binaryWriter); break;
                            case 0x01: typeRefTable.Save(moduleTable, moduleRefTable, assemblyRefTable, binaryWriter); break;
                            case 0x02: typeDefTable.Save(fieldTable, methodDefTable, typeRefTable, typeSpecTable, binaryWriter); break;
                            case 0x04: fieldTable.Save(binaryWriter); break;
                            case 0x06: methodDefTable.Save(paramTable, binaryWriter); break;
                            case 0x08: paramTable.Save(binaryWriter); break;
                            case 0x09: interfaceImplTable.Save(typeDefTable, typeRefTable, typeSpecTable, binaryWriter); break;
                            case 0x0A: memberRefTable.Save(typeDefTable, typeRefTable, typeSpecTable, moduleRefTable, methodDefTable, binaryWriter); break;
                            case 0x0B: constantTable.Save(fieldTable, paramTable, propertyTable, binaryWriter); break;
                            case 0x0C: customAttributeTable.Save(methodDefTable, fieldTable, typeRefTable, typeDefTable, paramTable, interfaceImplTable, memberRefTable, moduleTable, declSecurityTable, eventTable, propertyTable, typeSpecTable, assemblyTable, assemblyRefTable, fileTable, exportedTypeTable, manifestResourceTable, genericParamTable, genericParamConstraintTable, methodSpecTable, binaryWriter); break;
                            case 0x0D: fieldMarshalTable.Save(fieldTable, paramTable, binaryWriter); break;
                            case 0x0E: declSecurityTable.Save(typeDefTable, methodDefTable, assemblyTable, binaryWriter); break;
                            case 0x0F: classLayoutTable.Save(binaryWriter); break;
                            case 0x10: fieldLayoutTable.Save(binaryWriter); break;
                            case 0x11: standAloneSigTable.Save(binaryWriter); break;
                            case 0x12: eventMapTable.Save(typeDefTable, eventTable, binaryWriter); break;
                            case 0x14: eventTable.Save(typeDefTable, typeRefTable, typeSpecTable, binaryWriter); break;
                            case 0x15: propertyMapTable.Save(typeDefTable, propertyTable, binaryWriter); break;
                            case 0x17: propertyTable.Save(binaryWriter); break;
                            case 0x18: methodSemanticsTable.Save(eventTable, propertyTable, binaryWriter); break;
                            case 0x19: methodImplTable.Save(methodDefTable, memberRefTable, binaryWriter); break;
                            case 0x1A: moduleRefTable.Save(binaryWriter); break;
                            case 0x1B: typeSpecTable.Save(binaryWriter); break;
                            case 0x1C: implMapTable.Save(binaryWriter); break;
                            case 0x1D: fieldRVATable.Save(binaryWriter); break;
                            case 0x20: assemblyTable.Save(stringHeap, blobHeap, binaryWriter); break;
                            case 0x23: assemblyRefTable.Save(binaryWriter, blobHeap); break;
                            case 0x26: fileTable.Save(binaryWriter); break;
                            case 0x27: exportedTypeTable.Save(typeDefTable, fileTable, assemblyRefTable, binaryWriter); break;
                            case 0x28: manifestResourceTable.Save(fileTable, assemblyRefTable, exportedTypeTable, binaryWriter); break;
                            case 0x29: nestedClassTable.Save(binaryWriter); break;
                            case 0x2A: genericParamTable.Save(typeDefTable, methodDefTable, binaryWriter); break;
                            case 0x2B: methodSpecTable.Save(methodDefTable, memberRefTable, binaryWriter); break;
                            case 0x2C: genericParamConstraintTable.Save(typeDefTable, typeRefTable, typeSpecTable, binaryWriter); break;
                            case 0x30: documentTable.Save(binaryWriter); break;
                            case 0x31: methodDebugInformationTable.Save(binaryWriter); break;
                            case 0x32: localScopeTable.Save(binaryWriter); break;
                            case 0x33: localVariableTable.Save(binaryWriter); break;
                            case 0x34: localConstantTable.Save(binaryWriter); break;
                            case 0x35: importScopeTable.Save(binaryWriter); break;
                        }
                        table++;
                    }
                }
            }
        }
    }
}
