using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;
using WpfApp1;

namespace newtest;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        var handle = PInvoke.GetModuleHandle((string?)null);
        var hhook = PInvoke.SetWindowsHookEx(
            WINDOWS_HOOK_ID.WH_MOUSE_LL,
            (code, param, lParam) =>
            {
                if (code < 0)
                {
                    return PInvoke.CallNextHookEx(null, code, param, lParam);
                }
                if (param == PInvoke.WM_XBUTTONDOWN || param == PInvoke.WM_XBUTTONUP)
                {
                    var msll = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                    unsafe
                    {
                        // switch on high-order word
                        switch (((ushort*)&msll.mouseData)[1])
                        {
                            case PInvoke.XBUTTON1:
                                Console.WriteLine("XB1");
                                var input = new INPUT { type = INPUT_TYPE.INPUT_MOUSE };
                                input.Anonymous.mi.dx = 0;
                                input.Anonymous.mi.dy = 0;
                                if (PInvoke.GetCursorPos(out var point))
                                {
                                    input.Anonymous.mi.dx = point.X;
                                    input.Anonymous.mi.dy = point.Y;
                                }
                                input.Anonymous.mi.dwFlags =
                                    MOUSE_EVENT_FLAGS.MOUSEEVENTF_ABSOLUTE
                                    | MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTDOWN
                                    | MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTUP;
                                input.Anonymous.mi.mouseData = 0;
                                input.Anonymous.mi.dwExtraInfo = 0;
                                input.Anonymous.mi.time = 0;
                                PInvoke.SendInput(1, &input, sizeof(INPUT));
                                return (Windows.Win32.Foundation.LRESULT)1;
                            case PInvoke.XBUTTON2:
                                Console.WriteLine("XB2");
                                return (Windows.Win32.Foundation.LRESULT)1;
                            default:
                                throw new Exception();
                        }
                    }
                }
                return PInvoke.CallNextHookEx(null, code, param, lParam);
            },
            handle,
            0
        );
        var app = new App();
        app.Run();
    }
}
