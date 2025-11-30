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
            internal virtual void Save(Heap.StringHeap stringHeap, Heap.BlobHeap blobHeap, Heap.GUIDHeap GUIDHeap, System.IO.BinaryWriter binaryWriter)
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
                DocumentTable? documentTable = null;
                LocalVariableTable? localVariableTable = null;
                LocalConstantTable? localConstantTable = null;
                ImportScopeTable? importScopeTable = null;
                TypeDefTable? typeDefTable = null;
                TypeRefTable? typeRefTable = null;
                TypeSpecTable? typeSpecTable = null;
                ModuleRefTable? moduleRefTable = null;
                AssemblyRefTable? assemblyRefTable = null;
                fieldTable = new FieldTable(rows[0x4]);
                methodDefTable = new MethodDefTable(rows[0x6]);
                paramTable = new ParamTable(rows[0x8]);
                if ((validTables & (1UL << 0x01)) != 0) { typeRefTable = new TypeRefTable(rows[0x01]); }
                if ((validTables & (1UL << 0x1A)) != 0) { moduleRefTable = new ModuleRefTable(rows[0x1A]); }
                if ((validTables & (1UL << 0x1B)) != 0) { typeSpecTable = new TypeSpecTable(rows[0x1B]); }
                if ((validTables & (1UL << 0x23)) != 0) { assemblyRefTable = new AssemblyRefTable(rows[0x23]); }
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
                            case 0x0A: MemberRefTable memberRefTable = new MemberRefTable(rows[n], stringHeap, blobHeap, data, ref offset); tables[table] = memberRefTable; break;
                            case 0x0C: CustomAttributeTable customAttributeTable = new CustomAttributeTable(rows[n], blobHeap, data, ref offset); tables[table] = customAttributeTable; break;
                            case 0x0E: DeclSecurityTable declSecurityTable = new DeclSecurityTable(rows[n], blobHeap, data, ref offset); tables[table] = declSecurityTable; break;
                            case 0x11: StandAloneSigTable standAloneSigTable = new StandAloneSigTable(rows[n], blobHeap, data, ref offset); tables[table] = standAloneSigTable; break;
                            case 0x1A: moduleRefTable.Load(stringHeap, data, ref offset); tables[table] = moduleRefTable; break;
                            case 0x1C: ImplMapTable implMapTable = new ImplMapTable(rows[n], stringHeap, data, ref offset); tables[table] = implMapTable; break;
                            case 0x1D: FieldRVATable fieldRVATable = new FieldRVATable(rows[n], fieldTable, data, ref offset); tables[table] = fieldRVATable; break;
                            case 0x20: AssemblyTable assemblyTable = new AssemblyTable(rows[n], stringHeap, blobHeap, data, ref offset); tables[table] = assemblyTable; break;
                            case 0x23: assemblyRefTable.Load(stringHeap, blobHeap, data, ref offset); tables[table] = assemblyRefTable; break;
                            case 0x29: NestedClassTable nestedClassTable = new NestedClassTable(rows[n], typeDefTable, data, ref offset); tables[table] = nestedClassTable; break;
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
                DocumentTable? documentTable = null;
                LocalVariableTable? localVariableTable = null;
                LocalConstantTable? localConstantTable = null;
                ImportScopeTable? importScopeTable = null;
                TypeDefTable? typeDefTable = null;
                TypeRefTable? typeRefTable = null;
                TypeSpecTable? typeSpecTable = null;
                ModuleTable? moduleTable = null;
                ModuleRefTable? moduleRefTable = null;
                AssemblyRefTable? assemblyRefTable = null;
#else
                FieldTable fieldTable = null;
                MethodDefTable methodDefTable = null;
                ParamTable paramTable = null;
                DocumentTable documentTable = null;
                LocalVariableTable localVariableTable = null;
                LocalConstantTable localConstantTable = null;
                ImportScopeTable importScopeTable = null;
                TypeDefTable typeDefTable = null;
                TypeRefTable typeRefTable = null;
                TypeSpecTable typeSpecTable = null;
                ModuleTable moduleTable = null;
                ModuleRefTable moduleRefTable = null;
                AssemblyRefTable assemblyRefTable = null;
#endif
                fieldTable = new FieldTable(rows[0x4]);
                methodDefTable = new MethodDefTable(rows[0x6]);
                paramTable = new ParamTable(rows[0x8]);
                if ((validTables & (1UL << 0x01)) != 0) { typeRefTable = new TypeRefTable(rows[0x01]); }
                if ((validTables & (1UL << 0x1A)) != 0) { moduleRefTable = new ModuleRefTable(rows[0x1A]); }
                if ((validTables & (1UL << 0x1B)) != 0) { typeSpecTable = new TypeSpecTable(rows[0x1B]); }
                if ((validTables & (1UL << 0x23)) != 0) { assemblyRefTable = new AssemblyRefTable(rows[0x23]); }
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
                            case 0x0A: MemberRefTable memberRefTable = new MemberRefTable(rows[n], stringHeap, blobHeap, reader); tables[table] = memberRefTable; break;
                            case 0x0C: CustomAttributeTable customAttributeTable = new CustomAttributeTable(rows[n], blobHeap, reader); tables[table] = customAttributeTable; break;
                            case 0x0E: DeclSecurityTable declSecurityTable = new DeclSecurityTable(rows[n], blobHeap, reader); tables[table] = declSecurityTable; break;
                            case 0x11: StandAloneSigTable standAloneSigTable = new StandAloneSigTable(rows[n], blobHeap, reader); tables[table] = standAloneSigTable; break;
                            case 0x1A: moduleRefTable.Load(stringHeap, reader); tables[table] = moduleRefTable; break;
                            case 0x1C: ImplMapTable implMapTable = new ImplMapTable(rows[n], stringHeap, reader); tables[table] = implMapTable; break;
                            case 0x1D: FieldRVATable fieldRVATable = new FieldRVATable(rows[n], fieldTable, reader); tables[table] = fieldRVATable; break;
                            case 0x20: AssemblyTable assemblyTable = new AssemblyTable(rows[n], stringHeap, blobHeap, reader); tables[table] = assemblyTable; break;
                            case 0x23: assemblyRefTable.Load(stringHeap, blobHeap, reader); tables[table] = assemblyRefTable; break;
                            case 0x29: NestedClassTable nestedClassTable = new NestedClassTable(rows[n], typeDefTable, reader); tables[table] = nestedClassTable; break;
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
                MemberRefTable? memberRefTable = null;
                CustomAttributeTable? customAttributeTable = null;
                DeclSecurityTable? declSecurityTable = null;
                StandAloneSigTable? standAloneSigTable = null;
                ModuleRefTable? moduleRefTable = null;
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
                MemberRefTable memberRefTable = null;
                CustomAttributeTable customAttributeTable = null;
                DeclSecurityTable declSecurityTable = null;
                StandAloneSigTable standAloneSigTable = null;
                ModuleRefTable moduleRefTable = null;
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
                        case MemberRefTable table: if (memberRefTable != null) { throw new System.Exception("More than one MemberRef Table was provided"); } memberRefTable = table; rows[0x0A] = table.Rows; validTables |= (1UL << 0x0A); break;
                        case CustomAttributeTable table: if (customAttributeTable != null) { throw new System.Exception("More than one CustomAttribute Table was provided"); } customAttributeTable = table; rows[0x0C] = table.Rows; validTables |= (1UL << 0x0C); break;
                        case DeclSecurityTable table: if (declSecurityTable != null) { throw new System.Exception("More than one DeclSecurity Table was provided"); } declSecurityTable = table; rows[0x0E] = table.Rows; validTables |= (1UL << 0x0E); break;
                        case StandAloneSigTable table: if (standAloneSigTable != null) { throw new System.Exception("More than one StandAloneSig Table was provided"); } standAloneSigTable = table; rows[0x11] = table.Rows; validTables |= (1UL << 0x11); break;
                        case ModuleRefTable table: if (moduleRefTable != null) { throw new System.Exception("More than one ModuleRef Table was provided"); } moduleRefTable = table; rows[0x1A] = table.Rows; validTables |= (1UL << 0x1A); break;
                        case ImplMapTable table: if (implMapTable != null) { throw new System.Exception("More than one ImplMap Table was provided"); } implMapTable = table; rows[0x1C] = table.Rows; validTables |= (1UL << 0x1C); break;
                        case TypeSpecTable table: if (typeSpecTable != null) { throw new System.Exception("More than one TypeSpec Table was provided"); } typeSpecTable = table; rows[0x1B] = table.Rows; validTables |= (1UL << 0x1B); break;
                        case FieldRVATable table: if (fieldRVATable != null) { throw new System.Exception("More than one FieldRVA Table was provided"); } fieldRVATable = table; rows[0x1D] = table.Rows; validTables |= (1UL << 0x1D); break;
                        case AssemblyTable table: if (assemblyTable != null) { throw new System.Exception("More than one Assembly Table was provided"); } assemblyTable = table; rows[0x20] = table.Rows; validTables |= (1UL << 0x20); break;
                        case AssemblyRefTable table: if (assemblyRefTable != null) { throw new System.Exception("More than one AssemblyRef Table was provided"); } assemblyRefTable = table; rows[0x23] = table.Rows; validTables |= (1UL << 0x23); break;
                        case NestedClassTable table: if (nestedClassTable != null) { throw new System.Exception("More than one NestedClass Table was provided"); } nestedClassTable = table; rows[0x29] = table.Rows; validTables |= (1UL << 0x29); break;
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
                            case 0x0A: binaryWriter.Write(memberRefTable.Rows); break;
                            case 0x0C: binaryWriter.Write(customAttributeTable.Rows); break;
                            case 0x0E: binaryWriter.Write(declSecurityTable.Rows); break;
                            case 0x11: binaryWriter.Write(standAloneSigTable.Rows); break;
                            case 0x1A: binaryWriter.Write(moduleRefTable.Rows); break;
                            case 0x1B: binaryWriter.Write(typeSpecTable.Rows); break;
                            case 0x1C: binaryWriter.Write(implMapTable.Rows); break;
                            case 0x1D: binaryWriter.Write(fieldRVATable.Rows); break;
                            case 0x20: binaryWriter.Write(assemblyTable.Rows); break;
                            case 0x23: binaryWriter.Write(assemblyRefTable.Rows); break;
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
                            case 0x00: moduleTable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                            case 0x01: typeRefTable.Save(stringHeap, blobHeap, GUIDHeap, moduleTable, moduleRefTable, assemblyRefTable, binaryWriter); break;
                            case 0x02: typeDefTable.Save(stringHeap, blobHeap, GUIDHeap, fieldTable, methodDefTable, typeRefTable, typeSpecTable, binaryWriter); break;
                            case 0x04: fieldTable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                            case 0x06: methodDefTable.Save(stringHeap, blobHeap, GUIDHeap, paramTable, binaryWriter); break;
                            case 0x08: paramTable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                            case 0x0A: memberRefTable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                            case 0x0C: customAttributeTable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                            case 0x0E: declSecurityTable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                            case 0x11: standAloneSigTable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                            case 0x1B: typeSpecTable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                            case 0x1A: moduleRefTable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                            case 0x1C: implMapTable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                            case 0x1D: fieldRVATable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                            case 0x20: assemblyTable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                            case 0x23: assemblyRefTable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                            case 0x29: nestedClassTable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                            case 0x30: documentTable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                            case 0x31: methodDebugInformationTable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                            case 0x32: localScopeTable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                            case 0x33: localVariableTable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                            case 0x34: localConstantTable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                            case 0x35: importScopeTable.Save(stringHeap, blobHeap, GUIDHeap, binaryWriter); break;
                        }
                        table++;
                    }
                }
            }
        }
    }
}
