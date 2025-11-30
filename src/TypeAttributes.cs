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

namespace Runic.Dotnet
{
    public partial class Assembly
    {
        public enum TypeAttributes : uint
        {
            NotPublic = 0x00000000,
            Public = 0x00000001,
            NestedPublic = 0x00000002,
            NestedPrivate = 0x00000003,
            NestedFamily = 0x00000004,
            NestedAssembly = 0x00000005,
            NestedFamilyAndAssembly = 0x00000006,
            NestedFamilyOrAssembly = 0x00000007,
            AutoLayout = 0x00000000,
            SequentialLayout = 0x00000008,
            ExplicitLayout = 0x00000010,
            Class = 0x00000000,
            Interface = 0x00000020,
            Abstract = 0x00000080,
            Sealed = 0x00000100,
            SpecialName = 0x00000400,
            Import = 0x00001000,
            Serializable = 0x00002000,
            AnsiClass = 0x00000000,
            UnicodeClass = 0x00010000,
            AutoClass = 0x00020000,
            CustomFormatClass = 0x00030000,
            BeforeFieldInit = 0x00100000,
            RTSpecialName = 0x00000800,
            HasSecurity = 0x00040000,
            IsTypeForwarder = 0x00200000,
        }
    }
}
