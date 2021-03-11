using GalacticARM.Decoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Memory
{
    public static unsafe class MemoryManager
    {
        public const int PageBit = 12;
        public const ulong PageSize = 1UL << PageBit;
        public const ulong PageMask = PageSize - 1;

        public static MemoryBlock PageTable     { get; set; }

        static MemoryManager()
        {
            PageTable = new MemoryBlock(((1UL << 39) / PageSize) << 3);
        }

        public static void MapMemory(ulong VirtualAddress, void* PhysicalAddress, ulong Size)
        {
            ulong Bottom = VirtualAddress & ~PageMask;
            ulong Top = Bottom + ((Size & ~PageMask) + PageMask);

            ulong Offset = (ulong)PhysicalAddress;

            for (; Bottom < Top; Bottom += PageSize)
            {
                ulong Index = Bottom >> PageBit;

                ((ulong*)PageTable.Buffer)[Index] = Offset;

                Offset += PageSize;
            }
        }

        public static void* RequestPhysicalAddress(ulong VirtualAddress)
        {
            ulong Index = VirtualAddress >> PageBit;
            ulong Offset = (VirtualAddress & PageMask);

            if (((ulong*)PageTable.Buffer)[Index] == ulong.MaxValue)
            {
                throw new Exception($"Memory at {VirtualAddress:x16} is unmapped");
            }

            return (void*)(((ulong*)PageTable.Buffer)[Index] + Offset);
        }

        public static T ReadObject<T>(ulong VirtualAddress) where T: unmanaged
        {
            return *(T*)RequestPhysicalAddress(VirtualAddress); 
        }

        public static void WriteObject<T>(ulong VirtualAddress, T Data) where T : unmanaged
        {
            *(T*)RequestPhysicalAddress(VirtualAddress) = Data;
        }

        public static string GetOpHex(ulong Address) => $"{OpCodeTable.ReverseBytes(ReadObject<uint>(Address)):x8}";
    }
}
