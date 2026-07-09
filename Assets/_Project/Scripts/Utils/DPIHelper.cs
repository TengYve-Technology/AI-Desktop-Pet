using UnityEngine;
using System;
using System.Runtime.InteropServices;

public static class DPIHelper
{
    #region Windows API 声明

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

    #endregion

    #region 常量定义

    private const int LOGPIXELSX = 88;
    private const int LOGPIXELSY = 90;
    private const int MDT_EFFECTIVE_DPI = 0;
    private const int MDT_ANGULAR_DPI = 1;
    private const int MDT_RAW_DPI = 2;
    private const int MDT_DEFAULT = MDT_EFFECTIVE_DPI;
    private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

    private const float STANDARD_DPI = 96f;

    #endregion

    #region 结构体定义

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    #endregion

    #region 私有字段

    private static float _dpiScale = 1f;
    private static float _dpiX = 96f;
    private static float _dpiY = 96f;
    private static bool _isInitialized = false;

    #endregion

    #region 公共属性

    public static float DPIScale
    {
        get
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            return _dpiScale;
        }
    }

    public static float DpiX
    {
        get
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            return _dpiX;
        }
    }

    public static float DpiY
    {
        get
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            return _dpiY;
        }
    }

    #endregion

    #region 初始化

    private static void Initialize()
    {
        try
        {
            // 获取主显示器 DPI
            IntPtr hdc = GetDC(IntPtr.Zero);
            if (hdc != IntPtr.Zero)
            {
                _dpiX = GetDeviceCaps(hdc, LOGPIXELSX);
                _dpiY = GetDeviceCaps(hdc, LOGPIXELSY);
                ReleaseDC(IntPtr.Zero, hdc);
            }

            _dpiScale = _dpiX / STANDARD_DPI;
            _isInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DPIHelper] Failed to get DPI: {e.Message}");
            _dpiScale = 1f;
            _dpiX = 96f;
            _dpiY = 96f;
            _isInitialized = true;
        }
    }

    #endregion

    #region 公共方法

    public static float Scale(float value)
    {
        return value * DPIScale;
    }

    public static int ScaleInt(int value)
    {
        return Mathf.RoundToInt(value * DPIScale);
    }

    public static Vector2 Scale(Vector2 value)
    {
        return value * DPIScale;
    }

    public static Vector3 Scale(Vector3 value)
    {
        return value * DPIScale;
    }

    public static Rect Scale(Rect rect)
    {
        return new Rect(
            rect.x * DPIScale,
            rect.y * DPIScale,
            rect.width * DPIScale,
            rect.height * DPIScale
        );
    }

    public static float Unscale(float value)
    {
        return value / DPIScale;
    }

    public static int UnscaleInt(int value)
    {
        return Mathf.RoundToInt(value / DPIScale);
    }

    public static float GetDPIScaleForWindow(IntPtr hwnd)
    {
#if UNITY_STANDALONE_WIN
        try
        {
            POINT point = new POINT();
            IntPtr monitor = MonitorFromPoint(point, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                uint dpiX, dpiY;
                int result = GetDpiForMonitor(monitor, MDT_DEFAULT, out dpiX, out dpiY);

                if (result == 0)
                {
                    return dpiX / STANDARD_DPI;
                }
            }
        }
        catch
        {
            // 降级到主屏幕 DPI
        }
#endif

        return DPIScale;
    }

    public static float ConvertPixelsToPoints(float pixels)
    {
        return pixels / DPIScale;
    }

    public static float ConvertPointsToPixels(float points)
    {
        return points * DPIScale;
    }

    #endregion

    #region 屏幕适配

    public static int GetScaledScreenWidth()
    {
        return ScaleInt(Screen.width);
    }

    public static int GetScaledScreenHeight()
    {
        return ScaleInt(Screen.height);
    }

    public static int GetUnscaledScreenWidth()
    {
        return UnscaleInt(Screen.width);
    }

    public static int GetUnscaledScreenHeight()
    {
        return UnscaleInt(Screen.height);
    }

    #endregion
}