#pragma once

#ifdef PET_NATIVE_EXPORTS
#define PET_NATIVE_API __declspec(dllexport)
#else
#define PET_NATIVE_API __declspec(dllimport)
#endif

#include <windows.h>

extern "C" {

PET_NATIVE_API BOOL SetWindowTransparent(HWND hWnd, COLORREF keyColor, BYTE alpha);

PET_NATIVE_API BOOL RemoveWindowBorder(HWND hWnd);

PET_NATIVE_API BOOL SetWindowTopmost(HWND hWnd, BOOL topmost);

PET_NATIVE_API BOOL SetWindowClickThrough(HWND hWnd, BOOL enable);

PET_NATIVE_API BOOL SetWindowPosition(HWND hWnd, int x, int y);

PET_NATIVE_API BOOL SetWindowSize(HWND hWnd, int width, int height);

PET_NATIVE_API BOOL GetWindowRect(HWND hWnd, RECT* rect);

PET_NATIVE_API BOOL SetWindowShapeRoundRect(HWND hWnd, int width, int height, int cornerRadius);

PET_NATIVE_API BOOL EnableDPIAwareness();

PET_NATIVE_API BOOL SetWindowDraggable(HWND hWnd, BOOL enable);

PET_NATIVE_API HWND GetActiveWindowHandle();

}