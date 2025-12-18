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

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public abstract partial class MetadataTable
        {
#if NET6_0_OR_GREATER
            internal static bool TypeDefOrRefOrSpecLargeIndices(TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable)
#else
            internal static bool TypeDefOrRefOrSpecLargeIndices(TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable)
#endif
            {
                const uint maxRows = 0x3FFF;
                if ((typeDefTable != null) && (typeDefTable.Rows >= maxRows)) { return true; }
                if ((typeRefTable != null) && (typeRefTable.Rows >= maxRows)) { return true; }
                if ((typeSpecTable != null) && (typeSpecTable.Rows >= maxRows)) { return true; }
                return false;
            }
#if NET6_0_OR_GREATER
            internal static ITypeDefOrRefOrSpec? TypeDefOrRefOrSpecDecode(uint tag, TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable)
#else
            internal static ITypeDefOrRefOrSpec TypeDefOrRefOrSpecDecode(uint tag, TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable)
#endif
            {
                uint index = tag >> 2;
                uint type = tag & 0x3;
                switch (type)
                {
                    case 0x0:
                        {
                            if (typeDefTable == null) { return null; }
                            return typeDefTable[index];
                        }
                    case 0x1:
                        {
                            if (typeRefTable == null) { return null; }
                            return typeRefTable[index];
                        }
                    case 0x2:
                        {
                            if (typeSpecTable == null) { return null; }
                            return typeSpecTable[index];
                        }
                }
                return null;
            }
#if NET6_0_OR_GREATER
            internal static uint TypeDefOrRefOrSpecEncode(ITypeDefOrRefOrSpec? tag)
#else
            internal static uint TypeDefOrRefOrSpecEncode(ITypeDefOrRefOrSpec tag)
#endif
            {
                switch (tag)
                {
                    case TypeDefTable.TypeDefTableRow typeDef: return (uint)(typeDef.Row << 2);
                    case TypeRefTable.TypeRefTableRow typeRef: return (uint)(typeRef.Row << 2) | 0x01;
                    case TypeSpecTable.TypeSpecTableRow typeSpec: return (uint)(typeSpec.Row << 2) | 0x02;
                }
                return 0;
            }
            public interface ITypeDefOrRefOrSpec
            {
            }
#if NET6_0_OR_GREATER
            public static bool ResolutionScopeLargeIndices(ModuleTable? moduleTable, ModuleRefTable? moduleRefTable, AssemblyRefTable? assemblyRefTable, TypeRefTable? typeRefTable)
#else
            public static bool ResolutionScopeLargeIndices(ModuleTable moduleTable, ModuleRefTable moduleRefTable, AssemblyRefTable assemblyRefTable, TypeRefTable typeRefTable)
#endif
            {
                const uint maxRows = 0x3FFF;
                if ((moduleTable != null) && (moduleTable.Rows >= maxRows)) { return true; }
                if ((moduleRefTable != null) && (moduleRefTable.Rows >= maxRows)) { return true; }
                if ((assemblyRefTable != null) && (assemblyRefTable.Rows >= maxRows)) { return true; }
                if ((typeRefTable != null) && (typeRefTable.Rows >= maxRows)) { return true; }
                return false;
            }
#if NET6_0_OR_GREATER
            internal static uint ResolutionScopeEncode(IResolutionScope? tag)
#else
            internal static uint ResolutionScopeEncode(IResolutionScope tag)
#endif
            {
                switch (tag)
                {
                    case ModuleTable.ModuleTableRow module: return (uint)(module.Row << 2);
                    case ModuleRefTable.ModuleRefTableRow moduleRef: return (uint)(moduleRef.Row << 2) | 0x01;
                    case AssemblyRefTable.AssemblyRefTableRow assemblyRef: return (uint)(assemblyRef.Row << 2) | 0x02;
                    case TypeRefTable.TypeRefTableRow typeRef: return (uint)(typeRef.Row << 2) | 0x03;
                }
                return 0;
            }
#if NET6_0_OR_GREATER
            internal static IResolutionScope? ResolutionScopeDecode(uint tag, ModuleTable? moduleTable, ModuleRefTable? moduleRefTable, AssemblyRefTable? assemblyRefTable, TypeRefTable? typeRefTable)
#else
            internal static IResolutionScope ResolutionScopeDecode(uint tag, ModuleTable moduleTable, ModuleRefTable moduleRefTable, AssemblyRefTable assemblyRefTable, TypeRefTable typeRefTable)
#endif
            {
                uint index = tag >> 2;
                uint type = tag & 0x3;
                switch (type)
                {
                    case 0x0:
                        {
                            if (moduleTable == null) { return null; }
                            return moduleTable[index];
                        }
                    case 0x1:
                        {
                            if (moduleRefTable == null) { return null; }
                            return moduleRefTable[index];
                        }
                    case 0x2:
                        {
                            if (assemblyRefTable == null) { return null; }
                            return assemblyRefTable[index];
                        }
                    case 0x3:
                        {
                            if (typeRefTable == null) { return null; }
                            return typeRefTable[index];
                        }
                }
                return null;
            }
            public interface IResolutionScope
            {
            }

#if NET6_0_OR_GREATER
            internal static bool MemberRefParentLargeIndices(TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, ModuleRefTable? moduleRefTable, MethodDefTable? methodDefTable)
#else
            internal static bool MemberRefParentLargeIndices(TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable, ModuleRefTable moduleRefTable, MethodDefTable methodDefTable)
#endif
            {
                const uint maxRows = 0x1FFF;
                if ((typeDefTable != null) && (typeDefTable.Rows >= maxRows)) { return true; }
                if ((typeRefTable != null) && (typeRefTable.Rows >= maxRows)) { return true; }
                if ((typeSpecTable != null) && (typeSpecTable.Rows >= maxRows)) { return true; }
                if ((moduleRefTable != null) && (moduleRefTable.Rows >= maxRows)) { return true; }
                if ((methodDefTable != null) && (methodDefTable.Rows >= maxRows)) { return true; }
                return false;
            }
#if NET6_0_OR_GREATER
            internal static IMemberRefParent? MemberRefParentDecode(uint tag, TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable, ModuleRefTable? moduleRefTable, MethodDefTable? methodDefTable)
#else
            internal static IMemberRefParent MemberRefParentDecode(uint tag, TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable, ModuleRefTable moduleRefTable, MethodDefTable methodDefTable)
#endif
            {
                uint index = tag >> 3;
                uint type = tag & 0x7;
                switch (type)
                {
                    case 0x0:
                        {
                            if (typeDefTable == null) { return null; }
                            return typeDefTable[index];
                        }
                    case 0x1:
                        {
                            if (typeRefTable == null) { return null; }
                            return typeRefTable[index];
                        }
                    case 0x2:
                        {
                            if (moduleRefTable == null) { return null; }
                            return moduleRefTable[index];
                        }
                    case 0x3:
                        {
                            if (methodDefTable == null) { return null; }
                            return methodDefTable[index];
                        }
                    case 0x4:
                        {
                            if (typeSpecTable == null) { return null; }
                            return typeSpecTable[index];
                        }
                }
                return null;
            }
#if NET6_0_OR_GREATER
            internal static uint MemberRefParentEncode(IMemberRefParent? tag)
#else
            internal static uint MemberRefParentEncode(IMemberRefParent tag)
#endif
            {
                switch (tag)
                {
                    case TypeDefTable.TypeDefTableRow typeDef: return (uint)(typeDef.Row << 3);
                    case TypeRefTable.TypeRefTableRow typeRef: return (uint)(typeRef.Row << 3) | 0x01;
                    case ModuleRefTable.ModuleRefTableRow moduleRef: return (uint)(moduleRef.Row << 3) | 0x02;
                    case MethodDefTable.MethodDefTableRow methodDef: return (uint)(methodDef.Row << 3) | 0x03;
                    case TypeSpecTable.TypeSpecTableRow typeSpec: return (uint)(typeSpec.Row << 3) | 0x04;
                }
                return 0;
            }
            public interface IMemberRefParent
            {

            }


#if NET6_0_OR_GREATER
            internal static bool CustomAttributeConstructorLargeIndices(MethodDefTable? methodDefTable, MemberRefTable? memberRefTable)
#else
            internal static bool CustomAttributeConstructorLargeIndices(MethodDefTable methodDefTable, MemberRefTable memberRefTable)
#endif
            {
                const uint maxRows = 0x1FFF;
                if ((methodDefTable != null) && (methodDefTable.Rows >= maxRows)) { return true; }
                if ((memberRefTable != null) && (memberRefTable.Rows >= maxRows)) { return true; }
                return false;
            }
#if NET6_0_OR_GREATER
            internal static ICustomAttributeConstructor? CustomAttributeConstructorDecode(uint tag, MethodDefTable? methodDefTable, MemberRefTable? memberRefTable)
#else
            internal static ICustomAttributeConstructor CustomAttributeConstructorDecode(uint tag, MethodDefTable methodDefTable, MemberRefTable memberRefTable)
#endif
            {
                uint index = tag >> 3;
                uint type = tag & 0x7;
                switch (type)
                {
                    case 0x2:
                        {
                            if (methodDefTable == null) { return null; }
                            return methodDefTable[index];
                        }
                    case 0x3:
                        {
                            if (memberRefTable == null) { return null; }
                            return memberRefTable[index];
                        }
                }
                return null;
            }
#if NET6_0_OR_GREATER
            internal static uint CustomAttributeConstructorEncode(ICustomAttributeConstructor? tag)
#else
            internal static uint CustomAttributeConstructorEncode(ICustomAttributeConstructor tag)
#endif
            {
                switch (tag)
                {
                    case MethodDefTable.MethodDefTableRow methodDef: return (uint)(methodDef.Row << 3) | 0x02;
                    case MemberRefTable.MemberRefTableRow memberRef: return (uint)(memberRef.Row << 3) | 0x03;
                }
                return 0;
            }
            public interface ICustomAttributeConstructor
            {

            }
#if NET6_0_OR_GREATER
            internal static bool HasCustomAttributeLargeIndices(MethodDefTable? methodDefTable, FieldTable? fieldTable, TypeRefTable? typeRefTable, TypeDefTable? typeDefTable, ParamTable? paramTable, InterfaceImplTable? interfaceImplTable, MemberRefTable? memberRefTable, ModuleTable? moduleTable, DeclSecurityTable? declSecurityTable, EventTable? eventTable, PropertyTable? propertyTable, TypeSpecTable? typeSpecTable, AssemblyTable? assemblyTable, AssemblyRefTable? assemblyRefTable, FileTable? fileTable, ExportedTypeTable? exportedTypeTable, ManifestResourceTable? manifestResourceTable, GenericParamTable? genericParamTable, GenericParamConstraintTable? genericParamConstraintTable, MethodSpecTable? methodSpecTable)
#else
            internal static bool HasCustomAttributeLargeIndices(MethodDefTable methodDefTable, FieldTable fieldTable, TypeRefTable typeRefTable, TypeDefTable typeDefTable, ParamTable paramTable, InterfaceImplTable interfaceImplTable,  MemberRefTable memberRefTable, ModuleTable moduleTable, DeclSecurityTable declSecurityTable, EventTable eventTable, PropertyTable propertyTable, TypeSpecTable typeSpecTable, AssemblyTable assemblyTable, AssemblyRefTable assemblyRefTable, FileTable fileTable, ExportedTypeTable exportedTypeTable, ManifestResourceTable manifestResourceTable, GenericParamTable genericParamTable, GenericParamConstraintTable genericParamConstraintTable, MethodSpecTable methodSpecTable)
#endif
            {
                const uint maxRows = 0x7FF;
                if ((methodDefTable != null) && (methodDefTable.Rows >= maxRows)) { return true; }
                if ((fieldTable != null) && (fieldTable.Rows >= maxRows)) { return true; }
                if ((typeRefTable != null) && (typeRefTable.Rows >= maxRows)) { return true; }
                if ((typeDefTable != null) && (typeDefTable.Rows >= maxRows)) { return true; }
                if ((paramTable != null) && (paramTable.Rows >= maxRows)) { return true; }
                if ((interfaceImplTable != null) && (interfaceImplTable.Rows >= maxRows)) { return true; }
                if ((memberRefTable != null) && (memberRefTable.Rows >= maxRows)) { return true; }
                if ((moduleTable != null) && (moduleTable.Rows >= maxRows)) { return true; }
                if ((declSecurityTable != null) && (declSecurityTable.Rows >= maxRows)) { return true; }
                if ((eventTable != null) && (eventTable.Rows >= maxRows)) { return true; }
                if ((propertyTable != null) && (propertyTable.Rows >= maxRows)) { return true; }
                if ((typeSpecTable != null) && (typeSpecTable.Rows >= maxRows)) { return true; }
                if ((assemblyTable != null) && (assemblyTable.Rows >= maxRows)) { return true; }
                if ((assemblyRefTable != null) && (assemblyRefTable.Rows >= maxRows)) { return true; }
                if ((fileTable != null) && (fileTable.Rows >= maxRows)) { return true; }
                if ((exportedTypeTable != null) && (exportedTypeTable.Rows >= maxRows)) { return true; }
                if ((manifestResourceTable != null) && (manifestResourceTable.Rows >= maxRows)) { return true; }
                if ((genericParamTable != null) && (genericParamTable.Rows >= maxRows)) { return true; }
                if ((genericParamConstraintTable != null) && (genericParamConstraintTable.Rows >= maxRows)) { return true; }
                if ((methodSpecTable != null) && (methodSpecTable.Rows >= maxRows)) { return true; }
                return false;
            }
#if NET6_0_OR_GREATER
            internal static IHasCustomAttribute HasCustomAttributeDecode(uint tag, MethodDefTable? methodDefTable, FieldTable? fieldTable, TypeRefTable? typeRefTable, TypeDefTable? typeDefTable, ParamTable? paramTable, InterfaceImplTable? interfaceImplTable, MemberRefTable? memberRefTable, ModuleTable? moduleTable, DeclSecurityTable? declSecurityTable, EventTable? eventTable, PropertyTable? propertyTable, TypeSpecTable? typeSpecTable, AssemblyTable? assemblyTable, AssemblyRefTable? assemblyRefTable, FileTable? fileTable, ExportedTypeTable? exportedTypeTable, ManifestResourceTable? manifestResourceTable, GenericParamTable? genericParamTable, GenericParamConstraintTable? genericParamConstraintTable, MethodSpecTable? methodSpecTable)
#else
            internal static IHasCustomAttribute HasCustomAttributeDecode(uint tag, MethodDefTable methodDefTable, FieldTable fieldTable, TypeRefTable typeRefTable, TypeDefTable typeDefTable, ParamTable paramTable, InterfaceImplTable interfaceImplTable, MemberRefTable memberRefTable, ModuleTable moduleTable, DeclSecurityTable declSecurityTable, EventTable eventTable, PropertyTable propertyTable, TypeSpecTable typeSpecTable, AssemblyTable assemblyTable, AssemblyRefTable assemblyRefTable, FileTable fileTable, ExportedTypeTable exportedTypeTable, ManifestResourceTable manifestResourceTable, GenericParamTable genericParamTable, GenericParamConstraintTable genericParamConstraintTable, MethodSpecTable methodSpecTable)
#endif
            {
                uint index = tag >> 5;
                uint type = tag & 0x1F;
                switch (type)
                {
                    case 0x0:
                        {
                            if (methodDefTable == null) { return null; }
                            return methodDefTable[index];
                        }
                    case 0x1:
                        {
                            if (fieldTable == null) { return null; }
                            return fieldTable[index];
                        }
                    case 0x2:
                        {
                            if (typeRefTable == null) { return null; }
                            return typeRefTable[index];
                        }
                    case 0x3:
                        {
                            if (typeDefTable == null) { return null; }
                            return typeDefTable[index];
                        }
                    case 0x4:
                        {
                            if (paramTable == null) { return null; }
                            return paramTable[index];
                        }
                    case 0x5:
                        {
                            if (interfaceImplTable == null) { return null; }
                            return interfaceImplTable[index];
                        }
                    case 0x6:
                        {
                            if (memberRefTable == null) { return null; }
                            return memberRefTable[index];
                        }
                    case 0x7:
                        {
                            if (moduleTable == null) { return null; }
                            return moduleTable[index];
                        }
                    case 0x8:
                        {
                            if (declSecurityTable == null) { return null; }
                            return declSecurityTable[index];
                        }
                    case 0x9:
                        {
                            if (propertyTable == null) { return null; }
                            return propertyTable[index];
                        }
                    case 0xA:
                        {
                            if (eventTable == null) { return null; }
                            return eventTable[index];
                        }
                    case 0xD:
                        {
                            if (typeSpecTable == null) { return null; }
                            return typeSpecTable[index];
                        }
                    case 0xE:
                        {
                            if (assemblyTable == null) { return null; }
                            return assemblyTable[index];
                        }
                    case 0xF:
                        {
                            if (assemblyRefTable == null) { return null; }
                            return assemblyRefTable[index];
                        }
                    case 0x10:
                        {
                            if (fileTable == null) { return null; }
                            return fileTable[index];
                        }
                    case 0x11:
                        {
                            if (exportedTypeTable == null) { return null; }
                            return exportedTypeTable[index];
                        }
                    case 0x12:
                        {
                            if (manifestResourceTable == null) { return null; }
                            return manifestResourceTable[index];
                        }
                    case 0x13:
                        {
                            if (genericParamTable == null) { return null; }
                            return genericParamTable[index];
                        }
                    case 0x14:
                        {
                            if (genericParamConstraintTable == null) { return null; }
                            return genericParamConstraintTable[index];
                        }
                    case 0x15:
                        {
                            if (methodSpecTable == null) { return null; }
                            return methodSpecTable[index];
                        }
                }
                return null;
            }
#if NET6_0_OR_GREATER
            internal static uint HasCustomAttributeEncode(IHasCustomAttribute? tag)
#else
            internal static uint HasCustomAttributeEncode(IHasCustomAttribute tag)
#endif
            {
                switch (tag)
                {
                    case MethodDefTable.MethodDefTableRow methodDef: return (uint)(methodDef.Row << 5);
                    case FieldTable.FieldTableRow fieldDef: return (uint)(fieldDef.Row << 5) | 0x01;
                    case TypeRefTable.TypeRefTableRow typeRef: return (uint)(typeRef.Row << 5) | 0x02;
                    case TypeDefTable.TypeDefTableRow typeDef: return (uint)(typeDef.Row << 5) | 0x03;
                    case ParamTable.ParamTableRow param: return (uint)(param.Row << 5) | 0x04;
                    case InterfaceImplTable.InterfaceImplTableRow interfaceImpl: return (uint)(interfaceImpl.Row << 5) | 0x05;
                    case MemberRefTable.MemberRefTableRow memberRef: return (uint)(memberRef.Row << 5) | 0x06;
                    case ModuleTable.ModuleTableRow module: return (uint)(module.Row << 5) | 0x07;
                    case DeclSecurityTable.DeclSecurityTableRow declSecurity: return (uint)(declSecurity.Row << 5) | 0x08;
                    case PropertyTable.PropertyTableRow property: return (uint)(property.Row << 5) | 0x09;
                    case EventTable.EventTableRow eventDef: return (uint)(eventDef.Row << 5) | 0x0A;
                    case StandAloneSigTable.StandAloneSigTableRow standAloneSig: return (uint)(standAloneSig.Row << 5) | 0x0B;
                    case ModuleRefTable.ModuleRefTableRow moduleRef: return (uint)(moduleRef.Row << 5) | 0x0C;
                    case TypeSpecTable.TypeSpecTableRow typeSpec: return (uint)(typeSpec.Row << 5) | 0x0D;
                    case AssemblyTable.AssemblyTableRow assembly: return (uint)(assembly.Row << 5) | 0x0E;
                    case AssemblyRefTable.AssemblyRefTableRow assemblyRef: return (uint)(assemblyRef.Row << 5) | 0x0F;
                    case FileTable.FileTableRow file: return (uint)(file.Row << 5) | 0x10;
                    case ExportedTypeTable.ExportedTypeTableRow exportedType: return (uint)(exportedType.Row << 5) | 0x11;
                    case ManifestResourceTable.ManifestResourceTableRow manifestResource: return (uint)(manifestResource.Row << 5) | 0x12;
                    case GenericParamTable.GenericParamTableRow genericParam: return (uint)(genericParam.Row << 5) | 0x13;
                    case GenericParamConstraintTable.GenericParamConstraintTableRow genericParamConstraint: return (uint)(genericParamConstraint.Row << 5) | 0x14;
                    case MethodSpecTable.MethodSpecTableRow methodSpec: return (uint)(methodSpec.Row << 5) | 0x15;
                }
                return 0;
            }
            public interface IHasCustomAttribute
            {
            }
#if NET6_0_OR_GREATER
            internal static bool HasSemanticsDecodeLargeIndices(EventTable? eventTable, PropertyTable? propertyTable)
#else
            internal static bool HasSemanticsDecodeLargeIndices(EventTable eventTable, PropertyTable propertyTable)
#endif
            {
                const uint maxRows = 0x7FFF;
                if ((eventTable != null) && (eventTable.Rows >= maxRows)) { return true; }
                if ((propertyTable != null) && (propertyTable.Rows >= maxRows)) { return true; }
                return false;
            }
#if NET6_0_OR_GREATER
            internal static IHasSemantics HasSemanticsDecode(uint tag, EventTable? eventTable, PropertyTable? propertyTable)
#else
            internal static IHasSemantics HasSemanticsDecode(uint tag, EventTable eventTable, PropertyTable propertyTable)
#endif
            {
                uint index = tag >> 1;
                uint type = tag & 0x1;
                switch (type)
                {
                    case 0x0:
                        {
                            if (eventTable == null) { return null; }
                            return eventTable[index];
                        }
                    case 0x1:
                        {
                            if (propertyTable == null) { return null; }
                            return propertyTable[index];
                        }
                }
                return null;
            }
#if NET6_0_OR_GREATER
            internal static uint HasSemanticsEncode(IHasSemantics? tag)
#else
            internal static uint HasSemanticsEncode(IHasSemantics tag)
#endif
            {
                switch (tag)
                {
                    case EventTable.EventTableRow eventDef: return (uint)(eventDef.Row << 1);
                    case PropertyTable.PropertyTableRow property: return (uint)(property.Row << 1) | 0x01;
                }
                return 0;
            }
            public interface IHasSemantics
            {
            }


#if NET6_0_OR_GREATER
            internal static bool TypeDefOrMethodDefLargeIndices(TypeDefTable? typeDefTable, MethodDefTable? methodDefTable)
#else
            internal static bool TypeDefOrMethodDefLargeIndices(TypeDefTable typeDefTable, MethodDefTable methodDefTable)
#endif
            {
                const uint maxRows = 0x7FFF;
                if ((typeDefTable != null) && (typeDefTable.Rows >= maxRows)) { return true; }
                if ((methodDefTable != null) && (methodDefTable.Rows >= maxRows)) { return true; }
                return false;
            }
#if NET6_0_OR_GREATER
            internal static ITypeDefOrMethodDef TypeDefOrMethodDefDecode(uint tag, TypeDefTable? typeDefTable, MethodDefTable? methodDefTable)
#else
            internal static ITypeDefOrMethodDef TypeDefOrMethodDefDecode(uint tag, TypeDefTable typeDefTable, MethodDefTable methodDefTable)
#endif
            {
                uint index = tag >> 1;
                uint type = tag & 0x1;
                switch (type)
                {
                    case 0x0:
                        {
                            if (typeDefTable == null) { return null; }
                            return typeDefTable[index];
                        }
                    case 0x1:
                        {
                            if (methodDefTable == null) { return null; }
                            return methodDefTable[index];
                        }
                }
                return null;
            }
#if NET6_0_OR_GREATER
            internal static uint TypeDefOrMethodDefEncode(ITypeDefOrMethodDef? tag)
#else
            internal static uint TypeDefOrMethodDefEncode(ITypeDefOrMethodDef tag)
#endif
            {
                switch (tag)
                {
                    case TypeDefTable.TypeDefTableRow typeDef: return (uint)(typeDef.Row << 1);
                    case MethodDefTable.MethodDefTableRow methodDef: return (uint)(methodDef.Row << 1) | 0x01;
                }
                return 0;
            }
            public interface ITypeDefOrMethodDef
            {
            }

#if NET6_0_OR_GREATER
            internal static bool MethodDefOrRefLargeIndices(MethodDefTable? methodDefTable, MemberRefTable? memberRefTable)
#else
                internal static bool MethodDefOrRefLargeIndices(MethodDefTable methodDefTable, MemberRefTable memberRefTable)
#endif
            {
                const uint maxRows = 0x7FFF;
                if ((methodDefTable != null) && (methodDefTable.Rows >= maxRows)) { return true; }
                if ((memberRefTable != null) && (memberRefTable.Rows >= maxRows)) { return true; }
                return false;
            }
#if NET6_0_OR_GREATER
            internal static IMethodDefOrRef MethodDefOrRefDecode(uint tag, MethodDefTable? methodDefTable, MemberRefTable? memberRefTable)
#else
            internal static IMethodDefOrRef MethodDefOrRefDecode(uint tag, MethodDefTable methodDefTable, MemberRefTable memberRefTable)
#endif
            {
                uint index = tag >> 1;
                uint type = tag & 0x1;
                switch (type)
                {
                    case 0x0:
                        {
                            if (methodDefTable == null) { return null; }
                            return methodDefTable[index];
                        }
                    case 0x1:
                        {
                            if (memberRefTable == null) { return null; }
                            return memberRefTable[index];
                        }
                }
                return null;
            }
#if NET6_0_OR_GREATER
            internal static uint MethodDefOrRefEncode(IMethodDefOrRef? tag)
#else
            internal static uint MethodDefOrRefEncode(IMethodDefOrRef tag)
#endif
            {
                switch (tag)
                {
                    case MethodDefTable.MethodDefTableRow methodDef: return (uint)(methodDef.Row << 1);
                    case MemberRefTable.MemberRefTableRow memberRef: return (uint)(memberRef.Row << 1) | 0x01;
                }
                return 0;
            }


            public interface IMethodDefOrRef
            {
            }
#if NET6_0_OR_GREATER
            internal static bool ImplementationLargeIndices(FileTable? fileTable, AssemblyRefTable? assemblyRefTable, ExportedTypeTable? exportedTypeTable)
#else
            internal static bool ImplementationLargeIndices(FileTable fileTable, AssemblyRefTable assemblyRefTable , ExportedTypeTable exportedTypeTable)
#endif
            {
                const uint maxRows = 0x3FFF;
                if ((fileTable != null) && (fileTable.Rows >= maxRows)) { return true; }
                if ((assemblyRefTable != null) && (assemblyRefTable.Rows >= maxRows)) { return true; }
                if ((exportedTypeTable != null) && (exportedTypeTable.Rows >= maxRows)) { return true; }
                return false;
            }

#if NET6_0_OR_GREATER
            internal static IImplementation? ImplementationDecode(uint tag, FileTable? fileTable, AssemblyRefTable? assemblyRefTable, ExportedTypeTable? exportedTypeTable)
#else
            internal static IImplementation ImplementationDecode(uint tag, FileTable fileTable, AssemblyRefTable assemblyRefTable, ExportedTypeTable exportedTypeTable)
#endif
            {
                uint index = tag >> 2;
                uint type = tag & 0x3;
                switch (type)
                {
                    case 0x0:
                        {
                            if (fileTable == null) { return null; }
                            return fileTable[index];
                        }
                    case 0x1:
                        {
                            if (assemblyRefTable == null) { return null; }
                            return assemblyRefTable[index];
                        }
                    case 0x2:
                        {
                            if (exportedTypeTable == null) { return null; }
                            return exportedTypeTable[index];
                        }
                }
                return null;
            }
#if NET6_0_OR_GREATER
            internal static uint ImplementationEncode(IImplementation? tag)
#else
            internal static uint ImplementationEncode(IImplementation tag)
#endif
            {
                switch (tag)
                {
                    case FileTable.FileTableRow file: return (uint)(file.Row << 2);
                    case AssemblyRefTable.AssemblyRefTableRow assemblyRef: return (uint)(assemblyRef.Row << 2) | 0x01;
                    case ExportedTypeTable.ExportedTypeTableRow exportedType: return (uint)(exportedType.Row << 2) | 0x02;
                }
                return 0;
            }


            public interface IImplementation
            {
            }
#if NET6_0_OR_GREATER
            internal static bool HasDeclSecurityLargeIndices(TypeDefTable? typeDefTable, MethodDefTable? methodDefTable, AssemblyTable? assemblyTable)
#else
            internal static bool HasDeclSecurityLargeIndices(TypeDefTable typeDefTable, MethodDefTable methodDefTable, AssemblyTable assemblyTable)
#endif
            {
                const uint maxRows = 0x3FFF;
                if ((typeDefTable != null) && (typeDefTable.Rows >= maxRows)) { return true; }
                if ((methodDefTable != null) && (methodDefTable.Rows >= maxRows)) { return true; }
                if ((assemblyTable != null) && (assemblyTable.Rows >= maxRows)) { return true; }
                return false;
            }

#if NET6_0_OR_GREATER
            internal static IHasDeclSecurity? HasDeclSecurityDecode(uint tag, TypeDefTable? typeDefTable, MethodDefTable? methodDefTable, AssemblyTable? assemblyTable)
#else
            internal static IHasDeclSecurity HasDeclSecurityDecode(uint tag, TypeDefTable typeDefTable, MethodDefTable methodDefTable, AssemblyTable assemblyTable)
#endif
            {
                uint index = tag >> 2;
                uint type = tag & 0x3;
                switch (type)
                {
                    case 0x0:
                        {
                            if (typeDefTable == null) { return null; }
                            return typeDefTable[index];
                        }
                    case 0x1:
                        {
                            if (methodDefTable == null) { return null; }
                            return methodDefTable[index];
                        }
                    case 0x2:
                        {
                            if (assemblyTable == null) { return null; }
                            return assemblyTable[index];
                        }
                }
                return null;
            }

#if NET6_0_OR_GREATER
            internal static uint HasDeclSecurityEncode(IHasDeclSecurity? tag)
#else
            internal static uint HasDeclSecurityEncode(IHasDeclSecurity tag)
#endif
            {
                switch (tag)
                {
                    case TypeDefTable.TypeDefTableRow typeDef: return (uint)(typeDef.Row << 2);
                    case MethodDefTable.MethodDefTableRow methodDef: return (uint)(methodDef.Row << 2) | 0x01;
                    case AssemblyTable.AssemblyTableRow assembly: return (uint)(assembly.Row << 2) | 0x02;
                }
                return 0;
            }


            public interface IHasDeclSecurity
            {
            }

#if NET6_0_OR_GREATER
            internal static bool HasConstantLargeIndices(FieldTable? fieldTable, ParamTable? paramTable, PropertyTable? propertyTable)
#else
            internal static bool HasConstantLargeIndices(FieldTable fieldTable, ParamTable paramTable, PropertyTable propertyTable)
#endif
            {
                const uint maxRows = 0x3FFF;
                if ((fieldTable != null) && (fieldTable.Rows >= maxRows)) { return true; }
                if ((paramTable != null) && (paramTable.Rows >= maxRows)) { return true; }
                if ((propertyTable != null) && (propertyTable.Rows >= maxRows)) { return true; }
                return false;
            }

#if NET6_0_OR_GREATER
            internal static IHasConstant? HasConstantDecode(uint tag, FieldTable? fieldTable, ParamTable? paramTable, PropertyTable? propertyTable)
#else
            internal static IHasConstant  HasConstantDecode(uint tag, FieldTable fieldTable, ParamTable paramTable, PropertyTable propertyTable)
#endif
            {
                uint index = tag >> 2;
                uint type = tag & 0x3;
                switch (type)
                {
                    case 0x0:
                        {
                            if (fieldTable == null) { return null; }
                            return fieldTable[index];
                        }
                    case 0x1:
                        {
                            if (paramTable == null) { return null; }
                            return paramTable[index];
                        }
                    case 0x2:
                        {
                            if (propertyTable == null) { return null; }
                            return propertyTable[index];
                        }
                }
                return null;
            }

#if NET6_0_OR_GREATER
            internal static uint HasConstantEncode(IHasConstant? tag)
#else
            internal static uint HasConstantEncode(IHasConstant tag)
#endif
            {
                switch (tag)
                {
                    case FieldTable.FieldTableRow field: return (uint)(field.Row << 2);
                    case ParamTable.ParamTableRow @param: return (uint)(@param.Row << 2) | 0x01;
                    case PropertyTable.PropertyTableRow property: return (uint)(property.Row << 2) | 0x02;
                }
                return 0;
            }

            public interface IHasConstant
            {
            }
#if NET6_0_OR_GREATER
            internal static bool HasFieldMarshalLargeIndices(FieldTable? fieldTable, ParamTable? paramTable)
#else
            internal static bool HasFieldMarshalLargeIndices(FieldTable fieldTable, ParamTable paramTable)
#endif
            {
                const uint maxRows = 0x7FFF;
                if ((fieldTable != null) && (fieldTable.Rows >= maxRows)) { return true; }
                if ((paramTable != null) && (paramTable.Rows >= maxRows)) { return true; }
                return false;
            }

#if NET6_0_OR_GREATER
            internal static IHasFieldMarshal? HasFieldMarshalDecode(uint tag, FieldTable? fieldTable, ParamTable? paramTable)
#else
            internal static IHasFieldMarshal  HasFieldMarshalDecode(uint tag, FieldTable fieldTable, ParamTable paramTable)
#endif
            {
                uint index = tag >> 1;
                uint type = tag & 0x1;
                switch (type)
                {
                    case 0x0:
                        {
                            if (fieldTable == null) { return null; }
                            return fieldTable[index];
                        }
                    case 0x1:
                        {
                            if (paramTable == null) { return null; }
                            return paramTable[index];
                        }
                }
                return null;
            }

#if NET6_0_OR_GREATER
            internal static uint HasFieldMarshalEncode(IHasFieldMarshal? tag)
#else
            internal static uint HasFieldMarshalEncode(IHasFieldMarshal tag)
#endif
            {
                switch (tag)
                {
                    case FieldTable.FieldTableRow field: return (uint)(field.Row << 1);
                    case ParamTable.ParamTableRow @param: return (uint)(@param.Row << 1) | 0x01;
                }
                return 0;
            }
            public interface IHasFieldMarshal
            {
            }
        }
    }
}
