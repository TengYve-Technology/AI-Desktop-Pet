"""
perception 包 —— 感知模块

让宠物"感知"用户电脑的状态，包括：
  - ScreenCapture: 屏幕截图与图像处理
  - ProcessMonitor: 进程监控与系统信息采集

计划书中的 OCR / VLM 屏幕理解功能待后续实现。
"""
from .screen_capture import ScreenCapture
from .process_monitor import ProcessMonitor

__all__ = ["ScreenCapture", "ProcessMonitor"]