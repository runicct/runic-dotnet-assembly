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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Runic.Dotnet.Assembly;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public static class Signature
        {
            public static class PrimitiveType
            {
                internal class _Void : MetadataTable.ITypeDefOrRefOrSpec { }
                static _Void _voidInstance = new _Void();
                public static MetadataTable.ITypeDefOrRefOrSpec Void { get { return _voidInstance; } }
                internal class _Byte : MetadataTable.ITypeDefOrRefOrSpec { }
                static _Byte _byteInstance = new _Byte();
                public static MetadataTable.ITypeDefOrRefOrSpec Byte { get { return _byteInstance; } }
                internal class _SByte : MetadataTable.ITypeDefOrRefOrSpec { }
                static _SByte _sbyteInstance = new _SByte();
                public static MetadataTable.ITypeDefOrRefOrSpec SByte { get { return _sbyteInstance; } }
                internal class _Short : MetadataTable.ITypeDefOrRefOrSpec { }
                static _Short _shortInstance = new _Short();
                public static MetadataTable.ITypeDefOrRefOrSpec Short { get { return _shortInstance; } }
                internal class _UShort : MetadataTable.ITypeDefOrRefOrSpec { }
                static _UShort _ushortInstance = new _UShort();
                public static MetadataTable.ITypeDefOrRefOrSpec UShort { get { return _ushortInstance; } }
                internal class _Int : MetadataTable.ITypeDefOrRefOrSpec { }
                static _Int _intInstance = new _Int();
                public static MetadataTable.ITypeDefOrRefOrSpec Int { get { return _intInstance; } }
                internal class _UInt : MetadataTable.ITypeDefOrRefOrSpec { }
                static _UInt _uintInstance = new _UInt();
                public static MetadataTable.ITypeDefOrRefOrSpec UInt { get { return _uintInstance; } }
                internal class _Long : MetadataTable.ITypeDefOrRefOrSpec { }
                static _Long _longInstance = new _Long();
                public static MetadataTable.ITypeDefOrRefOrSpec Long { get { return _longInstance; } }
                internal class _ULong : MetadataTable.ITypeDefOrRefOrSpec { }
                static _ULong _ulongInstance = new _ULong();
                public static MetadataTable.ITypeDefOrRefOrSpec ULong { get { return _ulongInstance; } }
                internal class _Double : MetadataTable.ITypeDefOrRefOrSpec { }
                static _Double _doubleInstance = new _Double();
                public static MetadataTable.ITypeDefOrRefOrSpec Double { get { return _doubleInstance; } }
                internal class _Float : MetadataTable.ITypeDefOrRefOrSpec { }
                static _Float _floatInstance = new _Float();
                public static MetadataTable.ITypeDefOrRefOrSpec Float { get { return _floatInstance; } }

                internal class _Char : MetadataTable.ITypeDefOrRefOrSpec { }
                static _Char _charInstance = new _Char();
                public static MetadataTable.ITypeDefOrRefOrSpec Char { get { return _charInstance; } }
                internal class _Bool : MetadataTable.ITypeDefOrRefOrSpec { }
                static _Bool _boolInstance = new _Bool();
                public static MetadataTable.ITypeDefOrRefOrSpec Bool { get { return _boolInstance; } }
                internal class _String : MetadataTable.ITypeDefOrRefOrSpec { }
                static _String _stringInstance = new _String();
                public static MetadataTable.ITypeDefOrRefOrSpec String { get { return _stringInstance; } }

                internal class _Nint : MetadataTable.ITypeDefOrRefOrSpec { }
                static _Nint _nintInstance = new _Nint();
                public static MetadataTable.ITypeDefOrRefOrSpec Nint { get { return _nintInstance; } }
                internal class _Nuint : MetadataTable.ITypeDefOrRefOrSpec { }
                static _Nuint _nuintInstance = new _Nuint();
                public static MetadataTable.ITypeDefOrRefOrSpec Nuint { get { return _nuintInstance; } }
                internal class _Object : MetadataTable.ITypeDefOrRefOrSpec { }
                static _Object _objectInstance = new _Object();
                internal class  _Sentinel : MetadataTable.ITypeDefOrRefOrSpec
                {
                    
                }
                public static MetadataTable.ITypeDefOrRefOrSpec Object { get { return _objectInstance; } }
                public class Pointer : MetadataTable.ITypeDefOrRefOrSpec
                {
                    MetadataTable.ITypeDefOrRefOrSpec _target;
                    internal MetadataTable.ITypeDefOrRefOrSpec Target { get { return _target; } }
                    internal Pointer(MetadataTable.ITypeDefOrRefOrSpec target)
                    {
                        _target = target;
                    }
                    public static Pointer PointerOf(MetadataTable.ITypeDefOrRefOrSpec target)
                    {
                        return new Pointer(target);
                    }
                }
                public class Ref : MetadataTable.ITypeDefOrRefOrSpec
                {
                    MetadataTable.ITypeDefOrRefOrSpec _target;
                    internal MetadataTable.ITypeDefOrRefOrSpec Target { get { return _target; } }
                    internal Ref(MetadataTable.ITypeDefOrRefOrSpec target)
                    {
                        _target = target;
                    }
                    public static Ref ByRef(MetadataTable.ITypeDefOrRefOrSpec target)
                    {
                        return new Ref(target);
                    }
                }
                public class ValueType : MetadataTable.ITypeDefOrRefOrSpec
                {
                    MetadataTable.ITypeDefOrRefOrSpec _target;
                    internal MetadataTable.ITypeDefOrRefOrSpec Target { get { return _target; } }
                    internal ValueType(MetadataTable.ITypeDefOrRefOrSpec target)
                    {
                        _target = target;
                    }
                    public static ValueType MarkAs(MetadataTable.ITypeDefOrRefOrSpec target)
                    {
                        switch (target)
                        {
                            case MetadataTable.TypeDefTable.TypeDefTableRow _:
                            case MetadataTable.TypeRefTable.TypeRefTableRow _:
                                return new ValueType(target);
                            case ValueType valueType: return new ValueType(valueType.Target);
                            default: throw new ArgumentException("Only TypeDef and TypeRef can be marked as ValueType.");
                        }
                    }
                }
                public class GenericTypeInType : MetadataTable.ITypeDefOrRefOrSpec
                {
                    uint _index;
                    public uint Index { get { return _index; } }
                    public GenericTypeInType(uint index)
                    {
                        _index = index;
                    }
                }
                public class GenericTypeInMethod : MetadataTable.ITypeDefOrRefOrSpec
                {
                    uint _index;
                    public uint Index { get { return _index; } }
                    public GenericTypeInMethod(uint index)
                    {
                        _index = index;
                    }
                }
                public class Array : MetadataTable.ITypeDefOrRefOrSpec
                {
                    MetadataTable.ITypeDefOrRefOrSpec _elementType;
                    public MetadataTable.ITypeDefOrRefOrSpec ElementType { get { return _elementType; } }
                    uint _rank;
                    public uint Rank { get { return _rank; } }
                    uint[] _sizes;
                    public uint[] Sizes { get { return _sizes; } }
                    uint[] _lowerBounds;
                    public uint[] LowerBounds { get { return _lowerBounds; } }
                    public Array(MetadataTable.ITypeDefOrRefOrSpec elementType, uint rank, uint[] sizes, uint[] lowerBounds)
                    {
                        _elementType = elementType;
                        _rank = rank;
                        _sizes = sizes;
                        _lowerBounds = lowerBounds;
                    }
                }
            }
            internal static void EncodeCompressedInteger(uint value, List<byte> signature)
            {
                if (value <= 0x7F)
                {
                    signature.Add((byte)(value & 0x7F));
                }
                else if (value <= 0x3FFF)
                {
                    signature.Add((byte)(((value >> 8) & 0x3F) | 0x80));
                    signature.Add((byte)(value & 0xFF));
                }
                else
                {
                    signature.Add((byte)(((value >> 24) & 0x3F) | 0xC0));
                    signature.Add((byte)((value >> 16) & 0xFF));
                    signature.Add((byte)((value >> 8) & 0xFF));
                    signature.Add((byte)(value & 0xFF));
                }
            }

            internal static uint ReadCompressedInteger(byte[] data, ref uint offset)
            {
                byte firstByte = data[offset]; offset++;
                if ((firstByte & 0x80) == 0) { return (uint)firstByte; }
                byte secondByte = data[offset]; offset++;
                if ((firstByte & 0x40) == 0) { return (uint)(((uint)firstByte << 8) | (uint)secondByte) & 0x3FFF; }
                byte thirdByte = data[offset]; offset++;
                byte forthByte = data[offset]; offset++;

                return (uint)(((uint)firstByte << 24) | ((uint)secondByte << 16) | ((uint)thirdByte << 8) | ((uint)forthByte));
            }

            static internal void EncodeType(MetadataTable.ITypeDefOrRefOrSpec type, List<byte> signature)
            {
                switch (type)
                {
                    case PrimitiveType._Void _: signature.Add(0x01); break;
                    case PrimitiveType._Bool _: signature.Add(0x02); break;
                    case PrimitiveType._Char _: signature.Add(0x03); break;
                    case PrimitiveType._SByte _: signature.Add(0x04); break;
                    case PrimitiveType._Short _: signature.Add(0x06); break;
                    case PrimitiveType._Int _: signature.Add(0x08); break;
                    case PrimitiveType._Long _: signature.Add(0x0A); break;
                    case PrimitiveType._Byte _: signature.Add(0x05); break;
                    case PrimitiveType._UShort _: signature.Add(0x07); break;
                    case PrimitiveType._UInt _: signature.Add(0x09); break;
                    case PrimitiveType._ULong _: signature.Add(0x0B); break;
                    case PrimitiveType._Float _: signature.Add(0x0C); break;
                    case PrimitiveType._Double _: signature.Add(0x0D); break;
                    case PrimitiveType._String _: signature.Add(0x0E); break;
                    case PrimitiveType._Nint _: signature.Add(0x18); break;
                    case PrimitiveType._Nuint _: signature.Add(0x19); break;
                    case PrimitiveType._Object _: signature.Add(0x1C); break;
                    case PrimitiveType.Pointer pointer:
                        {
                            signature.Add(0x0F);
                            EncodeType(pointer.Target, signature);
                            break;
                        }
                    case PrimitiveType.Ref @ref:
                        {
                            signature.Add(0x10);
                            EncodeType(@ref.Target, signature);
                            break;
                        }
                    case PrimitiveType.GenericTypeInType genericTypeInType:
                        {
                            signature.Add(0x15);
                            EncodeCompressedInteger(genericTypeInType.Index, signature);
                            break;
                        }
                    case PrimitiveType.Array array:
                        {
                            if (array.Rank == 1 && array.LowerBounds.Length == 0 && array.Sizes.Length == 0)
                            {
                                signature.Add(0x1D);
                                EncodeType(array.ElementType, signature);
                                break;
                            }
                            else
                            {
                                signature.Add(0x14);
                                EncodeType(array.ElementType, signature);
                                EncodeCompressedInteger(array.Rank, signature);
                                EncodeCompressedInteger((uint)array.Sizes.Length, signature);
                                for (int i = 0; i < array.Sizes.Length; i++)
                                {
                                    EncodeCompressedInteger(array.Sizes[i], signature);
                                }
                                EncodeCompressedInteger((uint)array.LowerBounds.Length, signature);
                                for (int i = 0; i < array.LowerBounds.Length; i++)
                                {
                                    EncodeCompressedInteger(array.LowerBounds[i], signature);
                                }
                            }
                            break;
                        }
                    case PrimitiveType.GenericTypeInMethod genericTypeInMethod:
                        {
                            signature.Add(0x1E);
                            EncodeCompressedInteger(genericTypeInMethod.Index, signature);
                            break;
                        }
                    case PrimitiveType.ValueType valueType:
                        {
                            signature.Add(0x11);
                            switch (valueType.Target)
                            {
                                case MetadataTable.TypeDefTable.TypeDefTableRow typeDefRow:
                                    {
                                        EncodeCompressedInteger((typeDefRow.Row << 2) | 0x00, signature);
                                        break;
                                    }
                                case MetadataTable.TypeRefTable.TypeRefTableRow typeRefRow:
                                    {
                                        EncodeCompressedInteger((typeRefRow.Row << 2) | 0x01, signature);
                                        break;
                                    }
                                default: throw new ArgumentException("Only TypeDef and TypeRef can be marked as ValueType.");
                            }
                            break;
                        }
                    case MetadataTable.TypeDefTable.TypeDefTableRow typeDef:
                        {
                            signature.Add(0x12);
                            EncodeCompressedInteger((typeDef.Row << 2) | 0x00, signature);
                            break;
                        }
                    case MetadataTable.TypeRefTable.TypeRefTableRow typeRef:
                        {
                            signature.Add(0x12);
                            EncodeCompressedInteger((typeRef.Row << 2) | 0x01, signature);
                            break;
                        }
                }
            }
#if NET6_0_OR_GREATER
            static internal MetadataTable.ITypeDefOrRefOrSpec DecodeType(MetadataTable.TypeDefTable? typeDefTable, MetadataTable.TypeRefTable? typeRefTable, byte[] signature, ref uint offset)
#else
            static internal MetadataTable.ITypeDefOrRefOrSpec DecodeType(MetadataTable.TypeDefTable typeDefTable, MetadataTable.TypeRefTable typeRefTable, byte[] signature, ref uint offset)
#endif
            {
                switch (signature[offset])
                {
                    case 0x01: offset += 1; return PrimitiveType.Void;
                    case 0x02: offset += 1; return PrimitiveType.Bool;
                    case 0x03: offset += 1; return PrimitiveType.Char;
                    case 0x04: offset += 1; return PrimitiveType.SByte;
                    case 0x05: offset += 1; return PrimitiveType.Byte;
                    case 0x06: offset += 1; return PrimitiveType.Short;
                    case 0x07: offset += 1; return PrimitiveType.UShort;
                    case 0x08: offset += 1; return PrimitiveType.Int;
                    case 0x09: offset += 1; return PrimitiveType.UInt;
                    case 0x0A: offset += 1; return PrimitiveType.Long;
                    case 0x0B: offset += 1; return PrimitiveType.ULong;
                    case 0x0C: offset += 1; return PrimitiveType.Float;
                    case 0x0D: offset += 1; return PrimitiveType.Double;
                    case 0x0E: offset += 1; return PrimitiveType.String;
                    case 0x0F: offset += 1; return new PrimitiveType.Pointer(DecodeType(typeDefTable, typeRefTable, signature, ref offset));
                    case 0x10: offset += 1; return new PrimitiveType.Ref(DecodeType(typeDefTable, typeRefTable, signature, ref offset));
                    case 0x15: offset += 1; return new PrimitiveType.GenericTypeInType(ReadCompressedInteger(signature, ref offset));
                    case 0x1E: offset += 1; return new PrimitiveType.GenericTypeInMethod(ReadCompressedInteger(signature, ref offset));
                    case 0x18: offset += 1; return PrimitiveType.Nint;
                    case 0x19: offset += 1; return PrimitiveType.Nuint;
                    case 0x1C: offset += 1; return PrimitiveType.Object;
                    case 0x1D: offset += 1; return new PrimitiveType.Array(DecodeType(typeDefTable, typeRefTable, signature, ref offset), 1, new uint[0], new uint[0]);
                    case 0x11:
                        {
                            uint token = ReadCompressedInteger(signature, ref offset);
                            uint decodedIndex = token >> 2;
                            if ((token & 0x03) == 0x00) return new PrimitiveType.ValueType(typeDefTable[decodedIndex]);
                            else if ((token & 0x03) == 0x01) return new PrimitiveType.ValueType(typeRefTable[decodedIndex]);
                            else throw new ArgumentException("Invalid token for ValueType.");
                        }
                    case 0x12:
                        {
                            uint token = ReadCompressedInteger(signature, ref offset);
                            uint decodedIndex = token >> 2;
                            if ((token & 0x03) == 0x00) return typeDefTable[decodedIndex];
                            else if ((token & 0x03) == 0x01) return typeRefTable[decodedIndex];
                            else throw new ArgumentException("Invalid token for TypeDefOrRef.");
                        }
                    case 0x41: offset += 1; return new PrimitiveType._Sentinel();
                    default:
                        throw new ArgumentException("Invalid type signature");
                }
            }

            public static byte[] EncodeFieldSignature(MetadataTable.ITypeDefOrRefOrSpec type)
            {
                var signature = new List<byte>();
                signature.Add(0x06);
                Assembly.Signature.EncodeType(type, signature);
                return signature.ToArray();
            }
#if NET6_0_OR_GREATER
            public static MetadataTable.ITypeDefOrRefOrSpec DecodeFieldSignature(MetadataTable.TypeDefTable? typeDefTable, MetadataTable.TypeRefTable? typeRefTable, byte[] signature, ref uint offset)
#else
            public static MetadataTable.ITypeDefOrRefOrSpec DecodeFieldSignature(MetadataTable.TypeDefTable typeDefTable, MetadataTable.TypeRefTable typeRefTable, byte[] signature, ref uint offset)
#endif
            {
                if (signature[offset] != 0x06) { throw new ArgumentException("Invalid field signature"); }
                offset++;
                return Assembly.Signature.DecodeType(typeDefTable, typeRefTable, signature, ref offset);
            }
#if NET6_0_OR_GREATER
            public static void DecodeMethodSignature(MetadataTable.TypeDefTable? typeDefTable, MetadataTable.TypeRefTable? typeRefTable, byte[] signature, ref uint offset, out MetadataTable.ITypeDefOrRefOrSpec returnType, out bool hasThis, out bool explicitThis, out bool vaargs, out uint genericParameterCount, out MetadataTable.ITypeDefOrRefOrSpec[] parameters)
#else
            public static void DecodeMethodSignature(MetadataTable.TypeDefTable typeDefTable, MetadataTable.TypeRefTable typeRefTable, byte[] signature, ref uint offset, out MetadataTable.ITypeDefOrRefOrSpec returnType, out bool hasThis, out bool explicitThis, out bool vaargs, out uint genericParameterCount, out MetadataTable.ITypeDefOrRefOrSpec[] parameters)
#endif
            {
                byte flags = signature[offset]; offset++;
                hasThis = (flags & 0x20) != 0;
                explicitThis = (flags & 0x40) != 0;
                vaargs = (flags & 0x05) != 0;
                genericParameterCount = (flags & 0x10) != 0 ? ReadCompressedInteger(signature, ref offset) : 0;
                uint parameterCount = ReadCompressedInteger(signature, ref offset);
                returnType = DecodeType(typeDefTable, typeRefTable, signature, ref offset);
                parameters = new MetadataTable.ITypeDefOrRefOrSpec[parameterCount];
                for (int n = 0; n < parameterCount; n++)
                {
                    parameters[n] = DecodeType(typeDefTable, typeRefTable, signature, ref offset);
                }
            }
            public static byte[] EncodeMethodSignature(MetadataTable.ITypeDefOrRefOrSpec returnType, bool hasThis, bool explicitThis, uint genericParameterCount, MetadataTable.ITypeDefOrRefOrSpec[] parameters)
            {
                var signature = new List<byte>();
                uint flags = 0;
                if (hasThis) { flags |= 0x20; }
                if (explicitThis) { flags |= 0x40; }
                if (genericParameterCount > 0) { flags |= 0x10; }
                signature.Add((byte)flags);
                if (genericParameterCount > 0)
                {
                    EncodeCompressedInteger(genericParameterCount, signature);
                }
                EncodeCompressedInteger((uint)parameters.Length, signature);
                EncodeType(returnType, signature);
                for (int n = 0; n < parameters.Length; n++)
                {
                    EncodeType(parameters[n], signature);
                }
                return signature.ToArray();
            }
            public static byte[] EncodeMethodSignature(MetadataTable.ITypeDefOrRefOrSpec returnType, bool hasThis, bool explicitThis, bool vaargs, MetadataTable.ITypeDefOrRefOrSpec[] parameters)
            {
                var signature = new List<byte>();
                uint flags = 0;
                if (hasThis) { flags |= 0x20; }
                if (explicitThis) { flags |= 0x40; }
                if (vaargs) { flags |= 0x05; }
                signature.Add((byte)flags);
                EncodeCompressedInteger((uint)parameters.Length, signature);
                EncodeType(returnType, signature);
                for (int n = 0; n < parameters.Length; n++)
                {
                    EncodeType(parameters[n], signature);
                }
                return signature.ToArray();
            }
#if NET6_0_OR_GREATER
            public static void DecodeStandaloneMethodSignature(MetadataTable.TypeDefTable? typeDefTable, MetadataTable.TypeRefTable? typeRefTable, byte[] signature, ref uint offset, out MetadataTable.ITypeDefOrRefOrSpec returnType, out bool hasThis, out bool explicitThis, out CallingConvention callingConvention, out MetadataTable.ITypeDefOrRefOrSpec[] parameters, out MetadataTable.ITypeDefOrRefOrSpec[] extraParameters)
#else
            public static void DecodeStandaloneMethodSignature(MetadataTable.TypeDefTable typeDefTable, MetadataTable.TypeRefTable typeRefTable, byte[] signature, ref uint offset, out MetadataTable.ITypeDefOrRefOrSpec returnType, out bool hasThis, out bool explicitThis, out CallingConvention callingConvention, out MetadataTable.ITypeDefOrRefOrSpec[] parameters, out MetadataTable.ITypeDefOrRefOrSpec[] extraParameters)
#endif
            {
                byte flags = signature[offset]; offset++;
                hasThis = (flags & 0x20) != 0;
                explicitThis = (flags & 0x40) != 0;
                callingConvention = (CallingConvention)(flags & 0x0F);
                uint parameterCount = ReadCompressedInteger(signature, ref offset);
                returnType = DecodeType(typeDefTable, typeRefTable, signature, ref offset);
                List<MetadataTable.ITypeDefOrRefOrSpec> fixedParameters = new List<MetadataTable.ITypeDefOrRefOrSpec>();
                List<MetadataTable.ITypeDefOrRefOrSpec> vaargsParameters = new List<MetadataTable.ITypeDefOrRefOrSpec>();
                {
                    int n = 0;
                    for (; n < parameterCount; n++)
                    {
                        MetadataTable.ITypeDefOrRefOrSpec type = DecodeType(typeDefTable, typeRefTable, signature, ref offset);
                        if (type is PrimitiveType._Sentinel)
                        {
                            n--;
                            break;
                        }
                        fixedParameters.Add(type);
                    }
                    for (; n < parameterCount; n++)
                    {
                        vaargsParameters.Add(DecodeType(typeDefTable, typeRefTable, signature, ref offset));
                    }
                }
                parameters = fixedParameters.ToArray();
                extraParameters = vaargsParameters.ToArray();
            }
#if NET6_0_OR_GREATER
            public static byte[] EncodeStandaloneMethodSignature(MetadataTable.TypeDefTable? typeDefTable, MetadataTable.TypeRefTable? typeRefTable, MetadataTable.ITypeDefOrRefOrSpec returnType, bool hasThis, bool explicitThis, MetadataTable.ITypeDefOrRefOrSpec[] parameters, MetadataTable.ITypeDefOrRefOrSpec[] extraParameters)
#else
            public static byte[] EncodeStandaloneMethodSignature(MetadataTable.TypeDefTable typeDefTable, MetadataTable.TypeRefTable typeRefTable, MetadataTable.ITypeDefOrRefOrSpec returnType, bool hasThis, bool explicitThis, MetadataTable.ITypeDefOrRefOrSpec[] parameters, MetadataTable.ITypeDefOrRefOrSpec[] extraParameters)
#endif
            {
                List<byte> signature = new List<byte>();
                byte flags = 0;
                if (hasThis) { flags |= 0x20; }
                if (explicitThis) { flags |= 0x40; }
                flags |= (byte)(CallingConvention.VarArgs);
                signature.Add(flags);
                EncodeType(returnType, signature);
                EncodeCompressedInteger((uint)(parameters.Length + extraParameters.Length), signature);
                for (int n = 0; n < parameters.Length; n++)
                {
                    EncodeType(parameters[n], signature);
                }
                signature.Add(0x41); // Sentinel
                for (int n = 0; n < extraParameters.Length; n++)
                {
                    EncodeType(extraParameters[n], signature);
                }
                return signature.ToArray();
            }
#if NET6_0_OR_GREATER
            public static byte[] EncodeStandaloneMethodSignature(MetadataTable.TypeDefTable? typeDefTable, MetadataTable.TypeRefTable? typeRefTable, MetadataTable.ITypeDefOrRefOrSpec returnType, bool hasThis, bool explicitThis, CallingConvention callingConvention, MetadataTable.ITypeDefOrRefOrSpec[] parameters)
#else
            public static byte[] EncodeStandaloneMethodSignature(MetadataTable.TypeDefTable typeDefTable, MetadataTable.TypeRefTable typeRefTable, MetadataTable.ITypeDefOrRefOrSpec returnType, bool hasThis, bool explicitThis, CallingConvention callingConvention, MetadataTable.ITypeDefOrRefOrSpec[] parameters)
#endif
            {
                List<byte> signature = new List<byte>();
                byte flags = 0;
                if (hasThis) { flags |= 0x20; }
                if (explicitThis) { flags |= 0x40; }
                flags |= (byte)(callingConvention);
                signature.Add(flags);
                EncodeType(returnType, signature);
                EncodeCompressedInteger((uint)parameters.Length, signature);
                for (int n = 0; n < parameters.Length; n++)
                {
                    EncodeType(parameters[n], signature);
                }
                return signature.ToArray();
            }
        }
    }
}
