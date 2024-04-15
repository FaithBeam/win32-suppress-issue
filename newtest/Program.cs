using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace newtest;

class Program
{
    static unsafe void Main(string[] args)
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
                    // switch on high-order word
                    switch (((ushort*)&msll.mouseData)[1])
                    {
                        case PInvoke.XBUTTON1:
                            Console.WriteLine("XB1");

                            // send rmb
                            var input = new INPUT { type = INPUT_TYPE.INPUT_MOUSE };
                            input.Anonymous.mi.dx = msll.pt.X;
                            input.Anonymous.mi.dy = msll.pt.Y;
                            input.Anonymous.mi.dwFlags =
                                MOUSE_EVENT_FLAGS.MOUSEEVENTF_ABSOLUTE
                                | MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTDOWN
                                | MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTUP;
                            input.Anonymous.mi.mouseData = 0;
                            input.Anonymous.mi.dwExtraInfo = 0;
                            input.Anonymous.mi.time = 0;
                            PInvoke.SendInput(1, &input, sizeof(INPUT));

                            // suppress xb1
                            return (LRESULT)(-1);
                    }
                }
                return PInvoke.CallNextHookEx(null, code, param, lParam);
            },
            handle,
            0
        );
        while (PInvoke.GetMessage(out var lpMsg, HWND.Null, 0, 0) != -1)
        {
            PInvoke.TranslateMessage(lpMsg);
            PInvoke.DispatchMessage(lpMsg);
        }
    }
}
