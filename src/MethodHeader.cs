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
using System.IO;
using System.Text;

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public static class MethodHeader
        {
#if NET6_0_OR_GREATER
            public static byte[] Encode(int bytecodeLength, int maxStackSize, MetadataTable.StandAloneSigTable.StandAloneSigTableRow? localSignature, bool moreSections, bool initLocals)
#else
            public static byte[] Encode(int bytecodeLength, int maxStackSize, MetadataTable.StandAloneSigTable.StandAloneSigTableRow localSignature, bool moreSections, bool initLocals)
#endif
            {
                return Encode(bytecodeLength, maxStackSize, localSignature != null ? 0x11000000 | localSignature.Row : 0, moreSections, initLocals);
            }
            public static byte[] Encode(int bytecodeLength, int maxStackSize, uint localSignatureToken, bool moreSections, bool initLocals)
            {
                byte[] header;
                bool canUseTinyHeader = true;
                if (bytecodeLength > 63) { canUseTinyHeader = false; }
                if (maxStackSize > 8) { canUseTinyHeader = false; }
                if (localSignatureToken > 0) { canUseTinyHeader = false; }
                if (moreSections) { canUseTinyHeader = false; }
                if (initLocals) { canUseTinyHeader = false; }

                if (canUseTinyHeader)
                {
                    header = new byte[1];
                    header[0] = (byte)(0x2 | (bytecodeLength << 2)); // Tiny format
                }
                else
                {
                    header = new byte[12];
                    header[0] = (byte)(0x3 | (moreSections ? 0x8 : 0x0) |(initLocals ? 0x10 : 0x0)); // Fat format
                    header[1] = 0x3 << 4; // Fat format header size
                    header[2] = (byte)(maxStackSize & 0xFF);
                    header[3] = (byte)((maxStackSize >> 8) & 0xFF);
                    header[4] = (byte)(bytecodeLength & 0xFF);
                    header[5] = (byte)((bytecodeLength >> 8) & 0xFF);
                    header[6] = (byte)((bytecodeLength >> 16) & 0xFF);
                    header[7] = (byte)((bytecodeLength >> 24) & 0xFF);
                    header[8] = (byte)(localSignatureToken & 0xFF);
                    header[9] = (byte)((localSignatureToken >> 8) & 0xFF);
                    header[10] = (byte)((localSignatureToken >> 16) & 0xFF);
                    header[11] = (byte)((localSignatureToken >> 24) & 0xFF);
                }

                return header;
            }
#if NET6_0_OR_GREATER
            public static void Save(int bytecodeLength, int maxStackSize, MetadataTable.StandAloneSigTable.StandAloneSigTableRow? localSignature, bool moreSections, bool initLocals, System.IO.BinaryWriter writer)
#else
            public static void Save(int bytecodeLength, int maxStackSize, MetadataTable.StandAloneSigTable.StandAloneSigTableRow localSignature, bool moreSections, bool initLocals, System.IO.BinaryWriter writer)
#endif
            {
                Save(bytecodeLength, maxStackSize, localSignature != null ? 0x11000000 | localSignature.Row : 0, moreSections, initLocals, writer);
            }
            public static void Save(int bytecodeLength, int maxStackSize, uint localSignatureToken, bool moreSections, bool initLocals, System.IO.BinaryWriter writer)
            {
                bool canUseTinyHeader = true;
                if (bytecodeLength > 63) { canUseTinyHeader = false; }
                if (maxStackSize > 8) { canUseTinyHeader = false; }
                if (localSignatureToken > 0) { canUseTinyHeader = false; }
                if (moreSections) { canUseTinyHeader = false; }
                if (initLocals) { canUseTinyHeader = false; }

                if (canUseTinyHeader)
                {
                    writer.Write((byte)(0x2 | (bytecodeLength << 2))); // Tiny format
                }
                else
                {
                    writer.Write((byte)(0x3 | (moreSections ? 0x8 : 0x0) | (initLocals ? 0x10 : 0x0))); // Fat format
                    writer.Write((byte)(0x3 << 4)); // Fat format header size
                    writer.Write((byte)(maxStackSize & 0xFF));
                    writer.Write((byte)((maxStackSize >> 8) & 0xFF));
                    writer.Write((byte)(bytecodeLength & 0xFF));
                    writer.Write((byte)((bytecodeLength >> 8) & 0xFF));
                    writer.Write((byte)((bytecodeLength >> 16) & 0xFF));
                    writer.Write((byte)((bytecodeLength >> 24) & 0xFF));
                    writer.Write((byte)(localSignatureToken & 0xFF));
                    writer.Write((byte)((localSignatureToken >> 8) & 0xFF));
                    writer.Write((byte)((localSignatureToken >> 16) & 0xFF));
                    writer.Write((byte)((localSignatureToken >> 24) & 0xFF));
                }
            }
            public static void Decode(byte[] header, out int bytecodeLength, out int maxStackSize, out uint localSignatureToken, out bool moreSections, out bool initLocals)
            {
                moreSections = false;
                initLocals = false;
                if ((header[0] & 0x3) == 0x2)
                {
                    // Tiny format
                    bytecodeLength = header[0] >> 2;
                    maxStackSize = 8; // Default max stack size for tiny format
                    localSignatureToken = 0; // No local signature in tiny format
                }
                else if ((header[0] & 0x3) == 0x3)
                {
                    // Fat format
                    moreSections = (header[0] & 0x8) != 0;
                    initLocals = (header[0] & 0x10) != 0;
                    maxStackSize = header[2] | (header[3] << 8);
                    bytecodeLength = header[4] | (header[5] << 8) | (header[6] << 16) | (header[7] << 24);
                    localSignatureToken = (uint)(header[8] | (header[9] << 8) | (header[10] << 16) | (header[11] << 24));
                }
                else
                {
                    throw new InvalidDataException("Invalid method header format.");
                }
            }
            public static void Load(System.IO.BinaryReader reader, out int bytecodeLength, out int maxStackSize, out uint localSignatureToken, out bool moreSections, out bool initLocals)
            {
                moreSections = false;
                initLocals = false;
                byte data = reader.ReadByte();
                if ((data & 0x3) == 0x2)
                {
                    // Tiny format
                    bytecodeLength = data >> 2;
                    maxStackSize = 8; // Default max stack size for tiny format
                    localSignatureToken = 0; // No local signature in tiny format
                }
                else if ((data & 0x3) == 0x3)
                {
                    // Fat format
                    moreSections = (data & 0x8) != 0;
                    initLocals = (data & 0x10) != 0;
                    maxStackSize = reader.ReadUInt16();
                    bytecodeLength = reader.ReadInt32();
                    localSignatureToken = (uint)(reader.ReadUInt32());
                }
                else
                {
                    throw new InvalidDataException("Invalid method header format.");
                }
            }
        }
    }
}