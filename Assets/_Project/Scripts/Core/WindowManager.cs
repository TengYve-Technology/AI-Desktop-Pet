// Assets/_Project/Scripts/Core/WindowManager.cs

using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    #region Windows API 声明

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

    [DllImport("user32.dll")]
    private static extern bool SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

    [DllImport("user32.dll")]
    private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    [DllImport("user32.dll")]
    private static extern int DeleteObject(IntPtr hObject);

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    [DllImport("user32.dll")]
    private static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(ref POINT lpPoint);

    [DllImport("shcore.dll")]
    private static extern int SetProcessDPIAwareness(int value);

    [DllImport("user32.dll")]
    private static extern bool SetProcessDPIAware();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    #endregion

    #region 常量定义

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x80000;
    private const int WS_EX_TRANSPARENT = 0x20;
    private const int WS_EX_TOOLWINDOW = 0x80;
    private const int WS_EX_TOPMOST = 0x8;

    private const uint SWP_NOMOVE = 0x2;
    private const uint SWP_NOSIZE = 0x1;
    private const uint SWP_SHOWWINDOW = 0x40;
    private const uint SWP_FRAMECHANGED = 0x20;

    private const uint LWA_COLORKEY = 0x1;
    private const uint LWA_ALPHA = 0x2;

    private const int SW_SHOW = 5;

    #endregion

    #region 结构体定义

    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS
    {
        public int leftWidth;
        public int rightWidth;
        public int topHeight;
        public int bottomHeight;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    #endregion

    #region 私有字段

    private IntPtr _windowHandle = IntPtr.Zero;
    private bool _isInitialized = false;
    private bool _isTopmost = true;
    private bool _isClickThroughEnabled = false;
    private float _windowOpacity = 1f;
    private IntPtr _windowRegion = IntPtr.Zero;

    private static WindowManager _instance;

    #endregion

    #region 公共属性

    public static WindowManager Instance => _instance;
    public bool IsTopmost => _isTopmost;
    public bool IsClickThroughEnabled => _isClickThroughEnabled;
    public IntPtr WindowHandle => _windowHandle;

    #endregion

    #region 事件

    public event Action OnWindowInitialized;
    public event Action<bool> OnTopmostChanged;
    public event Action<bool> OnClickThroughChanged;
    public event Action<float> OnOpacityChanged;

    #endregion

    #region Unity 生命周期

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        SetDPIAware();
        Invoke(nameof(InitializeWindow), 0.2f);
    }

    private void OnDestroy()
    {
        if (_windowRegion != IntPtr.Zero)
        {
            DeleteObject(_windowRegion);
            _windowRegion = IntPtr.Zero;
        }
    }

    #endregion

    #region 初始化

    private void InitializeWindow()
    {
        _windowHandle = GetActiveWindow();

        if (_windowHandle == IntPtr.Zero)
        {
            Debug.LogError("[WindowManager] Failed to get window handle");
            return;
        }

        RemoveWindowBorder();
        MakeWindowTransparent();
        ExtendFrameToClientArea();
        SetWindowTopmost(true);
        SetClickThrough(false);

        _isInitialized = true;
        Debug.Log("[WindowManager] Window initialized successfully");

        OnWindowInitialized?.Invoke();
    }

    private void SetDPIAware()
    {
        try
        {
            SetProcessDPIAwareness(1);
        }
        catch
        {
            try
            {
                SetProcessDPIAware();
            }
            catch
            {
                Debug.LogWarning("[WindowManager] DPI awareness not supported");
            }
        }
    }

    private void RemoveWindowBorder()
    {
        int style = GetWindowLong(_windowHandle, GWL_EXSTYLE);
        style |= WS_EX_LAYERED;
        style |= WS_EX_TOOLWINDOW;
        style &= ~WS_EX_TOPMOST;
        SetWindowLong(_windowHandle, GWL_EXSTYLE, style);
    }

    private void MakeWindowTransparent()
    {
        SetLayeredWindowAttributes(_windowHandle, 0x00000000, 0, LWA_COLORKEY);
    }

    private void ExtendFrameToClientArea()
    {
        MARGINS margins = new MARGINS
        {
            leftWidth = -1,
            rightWidth = -1,
            topHeight = -1,
            bottomHeight = -1
        };

        int result = DwmExtendFrameIntoClientArea(_windowHandle, ref margins);
        if (result != 0)
        {
            Debug.LogWarning($"[WindowManager] DwmExtendFrameIntoClientArea result: {result}");
        }
    }

    #endregion

    #region 窗口样式

    public void SetWindowTopmost(bool topmost)
    {
        if (!_isInitialized || _windowHandle == IntPtr.Zero) return;

        IntPtr hWndInsertAfter = topmost ? new IntPtr(-1) : new IntPtr(-2);
        bool result = SetWindowPos(_windowHandle, hWndInsertAfter, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

        if (result)
        {
            _isTopmost = topmost;
            OnTopmostChanged?.Invoke(topmost);
            Debug.Log($"[WindowManager] Topmost set to: {topmost}");
        }
        else
        {
            Debug.LogError($"[WindowManager] Failed to set topmost: {topmost}");
        }
    }

    public void SetClickThrough(bool enabled)
    {
        if (!_isInitialized || _windowHandle == IntPtr.Zero) return;

        int style = GetWindowLong(_windowHandle, GWL_EXSTYLE);

        if (enabled)
        {
            style |= WS_EX_TRANSPARENT;
        }
        else
        {
            style &= ~WS_EX_TRANSPARENT;
        }

        int result = SetWindowLong(_windowHandle, GWL_EXSTYLE, style);
        if (result != 0)
        {
            _isClickThroughEnabled = enabled;
            OnClickThroughChanged?.Invoke(enabled);
            Debug.Log($"[WindowManager] Click-through set to: {enabled}");
        }
        else
        {
            Debug.LogError("[WindowManager] Failed to set click-through");
        }
    }

    public void SetWindowOpacity(float opacity)
    {
        if (!_isInitialized || _windowHandle == IntPtr.Zero) return;

        opacity = Mathf.Clamp01(opacity);
        byte alpha = (byte)(opacity * 255);

        bool result = SetLayeredWindowAttributes(_windowHandle, 0, alpha, LWA_ALPHA);
        if (result)
        {
            _windowOpacity = opacity;
            OnOpacityChanged?.Invoke(opacity);
            Debug.Log($"[WindowManager] Opacity set to: {opacity}");
        }
        else
        {
            Debug.LogError("[WindowManager] Failed to set opacity");
        }
    }

    public void SetWindowShape(int width, int height, int cornerRadius)
    {
        if (!_isInitialized || _windowHandle == IntPtr.Zero) return;

        if (_windowRegion != IntPtr.Zero)
        {
            DeleteObject(_windowRegion);
            _windowRegion = IntPtr.Zero;
        }

        if (cornerRadius > 0)
        {
            _windowRegion = CreateRoundRectRgn(0, 0, width, height, cornerRadius, cornerRadius);
            SetWindowRgn(_windowHandle, _windowRegion, true);
        }
        else
        {
            SetWindowRgn(_windowHandle, IntPtr.Zero, true);
        }

        Debug.Log($"[WindowManager] Window shape set: {width}x{height}, radius={cornerRadius}");
    }

    public void EnableClickThroughTemporary(float duration)
    {
        SetClickThrough(true);
        Invoke(nameof(DisableClickThrough), duration);
    }

    private void DisableClickThrough()
    {
        SetClickThrough(false);
    }

    #endregion

    #region 窗口位置与大小

    public Rect GetWindowRect()
    {
        if (!_isInitialized || _windowHandle == IntPtr.Zero)
        {
            return new Rect(0, 0, Screen.width, Screen.height);
        }

        RECT rect = new RECT();
        bool result = GetWindowRect(_windowHandle, ref rect);

        if (result)
        {
            return new Rect(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }
        else
        {
            Debug.LogWarning("[WindowManager] Failed to get window rect");
            return new Rect(0, 0, Screen.width, Screen.height);
        }
    }

    public void SetWindowPosition(int x, int y)
    {
        if (!_isInitialized || _windowHandle == IntPtr.Zero) return;

        bool result = SetWindowPos(_windowHandle, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_SHOWWINDOW);
        if (!result)
        {
            Debug.LogError("[WindowManager] Failed to set window position");
        }
    }

    public void SetWindowSize(int width, int height)
    {
        if (!_isInitialized || _windowHandle == IntPtr.Zero) return;

        bool result = SetWindowPos(_windowHandle, IntPtr.Zero, 0, 0, width, height, SWP_NOMOVE | SWP_SHOWWINDOW);
        if (!result)
        {
            Debug.LogError("[WindowManager] Failed to set window size");
        }
        else
        {
            Screen.SetResolution(width, height, false);
        }
    }

    public bool IsMouseInWindow()
    {
        if (!_isInitialized || _windowHandle == IntPtr.Zero) return false;

        POINT point = new POINT();
        GetCursorPos(ref point);

        Rect windowRect = GetWindowRect();
        return windowRect.Contains(new Vector2(point.X, point.Y));
    }

    #endregion

    #region 窗口状态

    public void MinimizeWindow()
    {
        if (!_isInitialized || _windowHandle == IntPtr.Zero) return;
        ShowWindow(_windowHandle, 6);
    }

    public void MaximizeWindow()
    {
        if (!_isInitialized || _windowHandle == IntPtr.Zero) return;
        ShowWindow(_windowHandle, 3);
    }

    public void RestoreWindow()
    {
        if (!_isInitialized || _windowHandle == IntPtr.Zero) return;
        ShowWindow(_windowHandle, 9);
    }

    public void HideWindow()
    {
        if (!_isInitialized || _windowHandle == IntPtr.Zero) return;
        ShowWindow(_windowHandle, 0);
    }

    public void ShowWindow()
    {
        if (!_isInitialized || _windowHandle == IntPtr.Zero) return;
        ShowWindow(_windowHandle, 5);
    }

    #endregion
}