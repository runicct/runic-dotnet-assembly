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
            public static bool TypeDefOrRefOrSpecLargeIndices(TypeDefTable? typeDefTable, TypeRefTable? typeRefTable, TypeSpecTable? typeSpecTable)
#else
            public static bool TypeDefOrRefOrSpecLargeIndices(TypeDefTable typeDefTable, TypeRefTable typeRefTable, TypeSpecTable typeSpecTable)
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
        }
    }
}
