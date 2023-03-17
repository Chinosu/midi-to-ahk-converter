using AutoHotkey.Interop;

namespace MidiToAHK;

internal static class AHK 
{
    internal static void Run(string Instruction) => AutoHotkeyEngine.Instance.ExecRaw(Instruction);
    internal static string GetPos(string x, string y) => $"MouseGetPos {x}, {y}";
    internal static string MouseMov(string x, string y, int speed = 1) => $"MouseMove {x}, {y}, {speed}";
    internal static string Click = "Send {LButton}";
    internal static string Wait(int ms = 10) => $"Sleep {ms}";
}