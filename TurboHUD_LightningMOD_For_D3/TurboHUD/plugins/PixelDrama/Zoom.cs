namespace Turbo.Plugins.PixelDrama
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class Zoom
    {
        [DllImport("kernel32.dll")] static extern IntPtr OpenProcess(uint access, bool inherit, int pid);
        [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr h);
        [DllImport("kernel32.dll")] static extern bool ReadProcessMemory(IntPtr h, IntPtr addr, byte[] buf, int size, out IntPtr read);
        [DllImport("kernel32.dll")] static extern bool WriteProcessMemory(IntPtr h, IntPtr addr, byte[] buf, int size, out IntPtr written);
        [DllImport("kernel32.dll")] static extern bool VirtualProtectEx(IntPtr h, IntPtr addr, UIntPtr size, uint newProt, out uint oldProt);
        [DllImport("kernel32.dll")] static extern IntPtr VirtualAllocEx(IntPtr h, IntPtr addr, UIntPtr size, uint allocType, uint protect);
        [DllImport("kernel32.dll")] static extern bool VirtualFreeEx(IntPtr h, IntPtr addr, UIntPtr size, uint freeType);
        [DllImport("kernel32.dll")] static extern bool FlushInstructionCache(IntPtr hProcess, IntPtr lpBaseAddress, UIntPtr dwSize);

        const uint PROCESS_VM_OPERATION = 0x0008;
        const uint PROCESS_VM_READ = 0x0010;
        const uint PROCESS_VM_WRITE = 0x0020;
        const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
        const uint PROCESS_ALL_NEEDED = PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_LIMITED_INFORMATION;

        const uint MEM_RESERVE = 0x2000;
        const uint MEM_COMMIT = 0x1000;
        const uint MEM_RELEASE = 0x8000;
        const uint PAGE_EXECUTE_READWRITE = 0x40;

        private readonly string _processName;
        private readonly long _patchRva;
        private Process _proc;
        private IntPtr _hProc = IntPtr.Zero;

        private IntPtr _patchAddr;
        private readonly byte[] _orig = new byte[] { 0xF3, 0x0F, 0x10, 0x67, 0x18 };
        private bool _enabled;

        private IntPtr _caveBase = IntPtr.Zero;
        private IntPtr _codeAddr = IntPtr.Zero;
        private IntPtr _dataAddr = IntPtr.Zero;

        public Zoom(string processName, long patchRva)
        {
            _processName = processName;
            _patchRva = patchRva;
        }

        public bool Attach()
        {
            var procs = Process.GetProcessesByName(_processName);
            if (procs.Length == 0)
                return false;
            _proc = procs[0];
            _hProc = OpenProcess(PROCESS_ALL_NEEDED, false, _proc.Id);
            if (_hProc == IntPtr.Zero)
                return false;

            _patchAddr = _proc.MainModule.BaseAddress + (int)_patchRva;
            return true;
        }

        public bool Enable(float zoom)
        {
            if (_enabled)
                return true;

            if (!VerifyOriginal())
            {
                return false;
            }

            const int caveSize = 2048;
            _caveBase = AllocateNear(_patchAddr, caveSize);
            if (_caveBase == IntPtr.Zero)
            {
                return false;
            }

            _codeAddr = _caveBase;

            byte[] code = BuildNewmemWithPlaceholders();

            _dataAddr = _codeAddr + code.Length;

            byte[] finalCode = FixPlaceholders(code, _codeAddr, _dataAddr, _patchAddr + 5);

            if (!Write(_codeAddr, finalCode))
            {
                return false;
            }

            if (!Write(_dataAddr, BitConverter.GetBytes(zoom)))
            {
                return false;
            }

            byte[] jmp = BuildJmpRel32(_patchAddr, _codeAddr);
            if (!PatchBytes(_patchAddr, jmp))
            {
                return false;
            }

            FlushInstructionCache(_hProc, IntPtr.Zero, UIntPtr.Zero);

            Console.WriteLine($"[ZoomHack] patched: patch=0x{_patchAddr.ToInt64():X}, cave=0x{_caveBase.ToInt64():X}, code=0x{_codeAddr.ToInt64():X}, data=0x{_dataAddr.ToInt64():X}");
            _enabled = true;
            return true;
        }

        public bool UpdateZoom(float zoom)
        {
            if (!_enabled || _dataAddr == IntPtr.Zero)
                return false;
            return Write(_dataAddr, BitConverter.GetBytes(zoom));
        }

        public void Disable()
        {
            if (!_enabled)
                return;

            PatchBytes(_patchAddr, _orig);
            FlushInstructionCache(_hProc, IntPtr.Zero, UIntPtr.Zero);

            if (_caveBase != IntPtr.Zero)
            {
                VirtualFreeEx(_hProc, _caveBase, UIntPtr.Zero, MEM_RELEASE);
                _caveBase = IntPtr.Zero;
                _codeAddr = IntPtr.Zero;
                _dataAddr = IntPtr.Zero;
            }

            _enabled = false;
            if (_hProc != IntPtr.Zero)
            { CloseHandle(_hProc); _hProc = IntPtr.Zero; }
        }

        private bool VerifyOriginal()
        {
            var buf = new byte[_orig.Length];
            if (!ReadProcessMemory(_hProc, _patchAddr, buf, buf.Length, out _))
                return false;
            for (int i = 0; i < _orig.Length; i++)
                if (buf[i] != _orig[i])
                    return false;
            return true;
        }

        private IntPtr AllocateNear(IntPtr target, int size)
        {
            const long MaxDistance = 0x70000000;
            const int Step = 0x10000;
            long t = target.ToInt64();

            var p = VirtualAllocEx(_hProc, IntPtr.Subtract(target, 0), (UIntPtr)size, MEM_RESERVE | MEM_COMMIT, PAGE_EXECUTE_READWRITE);
            if (p != IntPtr.Zero && Math.Abs(p.ToInt64() - t) <= MaxDistance)
                return p;
            if (p != IntPtr.Zero)
                VirtualFreeEx(_hProc, p, UIntPtr.Zero, MEM_RELEASE);

            for (long delta = Step; delta < MaxDistance; delta += Step)
            {
                var down = new IntPtr(t - delta);
                p = VirtualAllocEx(_hProc, down, (UIntPtr)size, MEM_RESERVE | MEM_COMMIT, PAGE_EXECUTE_READWRITE);
                if (p != IntPtr.Zero && Math.Abs(p.ToInt64() - t) <= MaxDistance)
                    return p;
                if (p != IntPtr.Zero)
                    VirtualFreeEx(_hProc, p, UIntPtr.Zero, MEM_RELEASE);

                var up = new IntPtr(t + delta);
                p = VirtualAllocEx(_hProc, up, (UIntPtr)size, MEM_RESERVE | MEM_COMMIT, PAGE_EXECUTE_READWRITE);
                if (p != IntPtr.Zero && Math.Abs(p.ToInt64() - t) <= MaxDistance)
                    return p;
                if (p != IntPtr.Zero)
                    VirtualFreeEx(_hProc, p, UIntPtr.Zero, MEM_RELEASE);
            }

            return IntPtr.Zero;
        }

        private byte[] BuildNewmemWithPlaceholders()
        {
            var list = new System.Collections.Generic.List<byte>();

            list.Add(0x8B);
            list.Add(0x05);
            list.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 });

            list.AddRange(new byte[] { 0x89, 0x47, 0x18 });

            list.AddRange(new byte[] { 0xF3, 0x0F, 0x10, 0x67, 0x18 });

            list.Add(0xE9);
            list.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 });

            return list.ToArray();
        }

        private byte[] FixPlaceholders(byte[] codeWithPlaceholders, IntPtr codeBase, IntPtr dataAddr, IntPtr returnAddr)
        {
            var bytes = (byte[])codeWithPlaceholders.Clone();

            int dispOffsetInCode = 2;
            int disp = CalcDisp32(codeBase, dispOffsetInCode, 4, dataAddr);
            Array.Copy(BitConverter.GetBytes(disp), 0, bytes, dispOffsetInCode, 4);

            int jmpIndex = FindJmpIndex(bytes);
            if (jmpIndex < 0)
                throw new InvalidOperationException("jmp placeholder not found in template.");
            int rel = CalcRel32(codeBase, jmpIndex, 5, returnAddr);
            Array.Copy(BitConverter.GetBytes(rel), 0, bytes, jmpIndex + 1, 4);

            return bytes;
        }

        private int FindJmpIndex(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length - 1; i++)
                if (bytes[i] == 0xE9)
                    return i;
            return -1;
        }

        private int CalcRel32(IntPtr codeBase, int instrOffset, int instrLen, IntPtr target)
        {
            long ipAfter = codeBase.ToInt64() + instrOffset + instrLen;
            long rel = target.ToInt64() - ipAfter;
            return checked((int)rel);
        }

        private int CalcDisp32(IntPtr codeBase, int dispFieldOffset, int dispSize, IntPtr target)
        {
            long ripAfter = codeBase.ToInt64() + dispFieldOffset + dispSize;
            long disp = target.ToInt64() - ripAfter;
            return checked((int)disp);
        }

        private byte[] BuildJmpRel32(IntPtr fromAddr, IntPtr toAddr)
        {
            long ipAfter = fromAddr.ToInt64() + 5;
            long rel64 = toAddr.ToInt64() - ipAfter;
            if (rel64 < int.MinValue || rel64 > int.MaxValue)
                throw new InvalidOperationException("Trampoline too far for E9 rel32.");
            var buf = new byte[5];
            buf[0] = 0xE9;
            Array.Copy(BitConverter.GetBytes((int)rel64), 0, buf, 1, 4);
            return buf;
        }

        private bool PatchBytes(IntPtr addr, byte[] bytes)
        {
            if (!VirtualProtectEx(_hProc, addr, (UIntPtr)bytes.Length, PAGE_EXECUTE_READWRITE, out uint old))
                return false;

            bool ok = Write(addr, bytes);

            VirtualProtectEx(_hProc, addr, (UIntPtr)bytes.Length, old, out _);

            return ok;
        }

        private bool Write(IntPtr addr, byte[] data)
            => WriteProcessMemory(_hProc, addr, data, data.Length, out _);
    }
}
