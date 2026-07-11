#include "window_utils.h"
#include <dwmapi.h>
#include <shcore.h>

#pragma comment(lib, "user32.lib")
#pragma comment(lib, "dwmapi.lib")
#pragma comment(lib, "shcore.lib")

PET_NATIVE_API BOOL SetWindowTransparent(HWND hWnd, COLORREF keyColor, BYTE alpha)
{
    if (hWnd == NULL) return FALSE;

    LONG exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
    exStyle |= WS_EX_LAYERED;
    SetWindowLong(hWnd, GWL_EXSTYLE, exStyle);

    if (alpha > 0)
    {
        return SetLayeredWindowAttributes(hWnd, keyColor, alpha, LWA_COLORKEY | LWA_ALPHA);
    }
    else
    {
        return SetLayeredWindowAttributes(hWnd, keyColor, 0, LWA_COLORKEY);
    }
}

PET_NATIVE_API BOOL RemoveWindowBorder(HWND hWnd)
{
    if (hWnd == NULL) return FALSE;

    LONG style = GetWindowLong(hWnd, GWL_STYLE);
    style &= ~(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU);
    SetWindowLong(hWnd, GWL_STYLE, style);

    LONG exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
    exStyle |= WS_EX_LAYERED | WS_EX_TOOLWINDOW;
    exStyle &= ~WS_EX_WINDOWEDGE;
    SetWindowLong(hWnd, GWL_EXSTYLE, exStyle);

    MARGINS margins = { -1, -1, -1, -1 };
    DwmExtendFrameIntoClientArea(hWnd, &margins);

    SetWindowPos(hWnd, NULL, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_FRAMECHANGED | SWP_SHOWWINDOW);

    return TRUE;
}

PET_NATIVE_API BOOL SetWindowTopmost(HWND hWnd, BOOL topmost)
{
    if (hWnd == NULL) return FALSE;

    HWND hWndInsertAfter = topmost ? HWND_TOPMOST : HWND_NOTOPMOST;
    return SetWindowPos(hWnd, hWndInsertAfter, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
}

PET_NATIVE_API BOOL SetWindowClickThrough(HWND hWnd, BOOL enable)
{
    if (hWnd == NULL) return FALSE;

    LONG exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);

    if (enable)
    {
        exStyle |= WS_EX_TRANSPARENT;
    }
    else
    {
        exStyle &= ~WS_EX_TRANSPARENT;
    }

    return SetWindowLong(hWnd, GWL_EXSTYLE, exStyle) != 0;
}

PET_NATIVE_API BOOL SetWindowPosition(HWND hWnd, int x, int y)
{
    if (hWnd == NULL) return FALSE;
    return SetWindowPos(hWnd, NULL, x, y, 0, 0, SWP_NOSIZE | SWP_SHOWWINDOW);
}

PET_NATIVE_API BOOL SetWindowSize(HWND hWnd, int width, int height)
{
    if (hWnd == NULL) return FALSE;
    return SetWindowPos(hWnd, NULL, 0, 0, width, height, SWP_NOMOVE | SWP_SHOWWINDOW);
}

PET_NATIVE_API BOOL GetWindowRect(HWND hWnd, RECT* rect)
{
    if (hWnd == NULL || rect == NULL) return FALSE;
    return ::GetWindowRect(hWnd, rect);
}

PET_NATIVE_API BOOL SetWindowShapeRoundRect(HWND hWnd, int width, int height, int cornerRadius)
{
    if (hWnd == NULL) return FALSE;

    HRGN hRgn = CreateRoundRectRgn(0, 0, width, height, cornerRadius, cornerRadius);
    if (hRgn == NULL) return FALSE;

    BOOL result = SetWindowRgn(hWnd, hRgn, TRUE);
    DeleteObject(hRgn);

    return result;
}

PET_NATIVE_API BOOL EnableDPIAwareness()
{
    return SetProcessDPIAware();
}

PET_NATIVE_API BOOL SetWindowDraggable(HWND hWnd, BOOL enable)
{
    if (hWnd == NULL) return FALSE;

    LONG style = GetWindowLong(hWnd, GWL_STYLE);

    if (enable)
    {
        style |= WS_CAPTION;
    }
    else
    {
        style &= ~WS_CAPTION;
    }

    return SetWindowLong(hWnd, GWL_STYLE, style) != 0;
}

PET_NATIVE_API HWND GetActiveWindowHandle()
{
    return GetActiveWindow();
}