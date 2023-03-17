using AutoHotkey.Interop;
using System.Runtime.InteropServices;

internal class Program
{
    private static void Main(string[] args)
    {
        void AHKCallback(string s)
        {
        Console.WriteLine(s);
        }

        AHKDelegate ahkDelegate = AHKCallback;

        void writeCrapInConsoleOnPressingA()
        {
            IntPtr ptr = Marshal.GetFunctionPointerForDelegate(ahkDelegate);
            var ahk = AutoHotkeyEngine.Instance;
    //         ahk.ExecRaw(@"a::
    //   DllCall(" + ptr + @", ""Str"", ""whatever you wanna pass to AHKCallback"")
    //   return");
            ahk.ExecRaw("a::f");
            ahk.ExecRaw("a::Send D");
            Console.WriteLine(2);
        }

        // string s = 
        //     "F1::\n" 
        //     + "MouseGetPos, xpos, ypos\n"
        //     + "return";
        // string s2 = 
        //     "F2::\n"
        //     + "MsgBox, %xpos%\n"
        //     + "return";

        // string varX = "yes";
        // int varY = 0;
        // Console.WriteLine($"MouseGetPos ${varX}, {varY}");

        // writeCrapInConsoleOnPressingA();
        // AutoHotkeyEngine.Instance.ExecRaw(s);
        // AutoHotkeyEngine.Instance.ExecRaw(s2);

        void AHKRun(string Instruction) => AutoHotkeyEngine.Instance.ExecRaw(Instruction);

        string AHKGetPos(string x, string y) => $"MouseGetPos {x}, {y}";
        string AHKMouseMov(string x, string y, int speed = 1) => $"MouseMove {x}, {y}, {speed}";

        // AHKRun($"F1::{AHKGetPos("x1","y1")}");
        // AHKRun($"F2::{AHKGetPos("x2","y2")}");
        // AHKRun($"F3::{AHKGetPos("x3","y3")}");
        // AHKRun($"1::\n{AHKMouseMov("x1", "y1")}\nSend {{LButton}}");
        // AHKRun($"2::\n{AHKMouseMov("x2", "y2")}\nSend {{LButton}}");
        // AHKRun($"3::\n{AHKMouseMov("x3", "y3")}\nSend {{LButton}}");

        // AHKRun("q::ExitApp");

        Console.WriteLine($"{{{1}}}");
        Console.ReadLine();
    }
}

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate void AHKDelegate([MarshalAs(UnmanagedType.LPWStr)]string s);
