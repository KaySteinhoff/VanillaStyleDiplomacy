using System;
using System.Linq.Expressions;
using System.Reflection;
using VanillaStyleDiplomacy.Logging;

namespace VanillaStyleDiplomacy.Helpers
{
    /// <summary>
    /// Code pulled from https://tryfinally.dev/detours-redirecting-csharp-methods-at-runtime
    /// Credits to Mateusz
    /// </summary>
    public static class DetourUtility
    {
        // this is based on an interesting technique from the RimWorld ComunityCoreLibrary project, originally credited to RawCode:
        // https://github.com/RimWorldCCLTeam/CommunityCoreLibrary/blob/master/DLL_Project/Classes/Static/Detours.cs
        // licensed under The Unlicense:
        // https://github.com/RimWorldCCLTeam/CommunityCoreLibrary/blob/master/LICENSE
        private static unsafe void TryDetourFromTo64(MethodInfo src, MethodInfo dst)
        {
            // 64-bit systems use 64-bit absolute address and jumps
            // 12 byte destructive

            // Get function pointers
            long srcBase = src.MethodHandle.GetFunctionPointer().ToInt64();
            long dstBase = dst.MethodHandle.GetFunctionPointer().ToInt64();

            // Native source address
            byte* pointerRawSource = (byte*)srcBase;

            // Pointer to insert jump address into native code
            long* pointerRawAddress = (long*)(pointerRawSource + 0x02);

            // Insert 64-bit absolute jump into native code (address in rax)
            // mov rax, immediate64
            // jmp [rax]
            *(pointerRawSource + 0x00) = 0x48;
            *(pointerRawSource + 0x01) = 0xB8;
            *pointerRawAddress = dstBase; // ( pointerRawSource + 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 )
            *(pointerRawSource + 0x0A) = 0xFF;
            *(pointerRawSource + 0x0B) = 0xE0;
        }

        private static unsafe void TryDetourFromTo32(MethodInfo src, MethodInfo dst)
        {
            // 32-bit systems use 32-bit relative offset and jump
            // 5 byte destructive

            // Get function pointers
            int srcBase = src.MethodHandle.GetFunctionPointer().ToInt32();
            int dstBase = dst.MethodHandle.GetFunctionPointer().ToInt32();

            // Native source address
            byte* pointerRawSource = (byte*)srcBase;

            // Pointer to insert jump address into native code
            int* pointerRawAddress = (int*)(pointerRawSource + 1);

            // Jump offset (less instruction size)
            int offset = dstBase - srcBase - 5;

            // Insert 32-bit relative jump into native code
            *pointerRawSource = 0xE9;
            *pointerRawAddress = offset;
        }

        public static unsafe bool TryDetourFromTo(MethodInfo src, MethodInfo dst)
        {
            bool result = true;
            try
            {
                if (IntPtr.Size == sizeof(Int64))
                    TryDetourFromTo64(src, dst);
                else
                    TryDetourFromTo32(src, dst);
            }
            catch (Exception ex)
            {
                LoggingManager.Instance.LogMessage($"Unable to detour: {src?.Name ?? "null src"} -> {dst?.Name ?? "null dst"}\n{ex}");
                LoggingManager.Instance.LogException(ex);
                result = false;
            }

            return result;
        }
    }
}