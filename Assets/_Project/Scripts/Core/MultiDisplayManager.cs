// Assets/_Project/Scripts/Core/MultiDisplayManager.cs

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MultiDisplayManager : MonoBehaviour
{
    #region Windows API 声明

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    #endregion

    #region 常量定义

    private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

    #endregion

    #region 结构体定义

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    #endregion

    #region 委托定义

    private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    #endregion

    #region 私有字段

    private List<Rect> _displayBounds = new List<Rect>();
    private Rect _totalBounds = new Rect();
    private int _currentDisplayIndex = 0;

    private static MultiDisplayManager _instance;

    #endregion

    #region 公共属性

    public static MultiDisplayManager Instance => _instance;
    public List<Rect> DisplayBounds => _displayBounds;
    public Rect TotalBounds => _totalBounds;
    public int DisplayCount => _displayBounds.Count;
    public int CurrentDisplayIndex => _currentDisplayIndex;

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
        RefreshDisplayInfo();
    }

    #endregion

    #region 公共方法

    public void RefreshDisplayInfo()
    {
        _displayBounds.Clear();

#if UNITY_STANDALONE_WIN
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, new MonitorEnumDelegate(EnumMonitorCallback), IntPtr.Zero);
#else
        _displayBounds.Add(new Rect(0, 0, Screen.width, Screen.height));
#endif

        if (_displayBounds.Count == 0)
        {
            _displayBounds.Add(new Rect(0, 0, Screen.width, Screen.height));
        }

        CalculateTotalBounds();
        UpdateCurrentDisplay();
    }

    private bool EnumMonitorCallback(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
    {
        MONITORINFO monitorInfo = new MONITORINFO();
        monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));

        if (GetMonitorInfo(hMonitor, ref monitorInfo))
        {
            Rect display = new Rect(
                monitorInfo.rcMonitor.Left,
                monitorInfo.rcMonitor.Top,
                monitorInfo.rcMonitor.Right - monitorInfo.rcMonitor.Left,
                monitorInfo.rcMonitor.Bottom - monitorInfo.rcMonitor.Top
            );
            _displayBounds.Add(display);
        }

        return true;
    }

    private void CalculateTotalBounds()
    {
        if (_displayBounds.Count == 0) return;

        float minX = _displayBounds[0].x;
        float minY = _displayBounds[0].y;
        float maxX = _displayBounds[0].x + _displayBounds[0].width;
        float maxY = _displayBounds[0].y + _displayBounds[0].height;

        foreach (Rect rect in _displayBounds)
        {
            minX = Mathf.Min(minX, rect.x);
            minY = Mathf.Min(minY, rect.y);
            maxX = Mathf.Max(maxX, rect.x + rect.width);
            maxY = Mathf.Max(maxY, rect.y + rect.height);
        }

        _totalBounds = new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    private void UpdateCurrentDisplay()
    {
        if (WindowManager.Instance == null) return;

        IntPtr hwnd = WindowManager.Instance.WindowHandle;
        if (hwnd == IntPtr.Zero) return;

        IntPtr hMonitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

        if (hMonitor == IntPtr.Zero) return;

        MONITORINFO monitorInfo = new MONITORINFO();
        monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));

        if (GetMonitorInfo(hMonitor, ref monitorInfo))
        {
            Rect currentDisplay = new Rect(
                monitorInfo.rcMonitor.Left,
                monitorInfo.rcMonitor.Top,
                monitorInfo.rcMonitor.Right - monitorInfo.rcMonitor.Left,
                monitorInfo.rcMonitor.Bottom - monitorInfo.rcMonitor.Top
            );

            for (int i = 0; i < _displayBounds.Count; i++)
            {
                if (AreRectsEqual(_displayBounds[i], currentDisplay))
                {
                    _currentDisplayIndex = i;
                    break;
                }
            }
        }
    }

    private bool AreRectsEqual(Rect a, Rect b)
    {
        return Mathf.Approximately(a.x, b.x) &&
               Mathf.Approximately(a.y, b.y) &&
               Mathf.Approximately(a.width, b.width) &&
               Mathf.Approximately(a.height, b.height);
    }

    public Rect GetDisplayBounds(int index)
    {
        if (index >= 0 && index < _displayBounds.Count)
        {
            return _displayBounds[index];
        }
        return _displayBounds[0];
    }

    public bool IsWindowWithinBounds(Rect windowRect, out Vector2 correction)
    {
        correction = Vector2.zero;

        if (!_totalBounds.Overlaps(windowRect))
        {
            correction = new Vector2(
                Mathf.Clamp(windowRect.x, _totalBounds.x, _totalBounds.x + _totalBounds.width - windowRect.width),
                Mathf.Clamp(windowRect.y, _totalBounds.y, _totalBounds.y + _totalBounds.height - windowRect.height)
            );
            return false;
        }

        return true;
    }

    public void ConstrainWindowToBounds()
    {
        if (WindowManager.Instance == null) return;

        Rect windowRect = WindowManager.Instance.GetWindowRect();
        Vector2 correction;

        if (!IsWindowWithinBounds(windowRect, out correction))
        {
            WindowManager.Instance.SetWindowPosition(Mathf.RoundToInt(correction.x), Mathf.RoundToInt(correction.y));
        }
    }

    #endregion
}