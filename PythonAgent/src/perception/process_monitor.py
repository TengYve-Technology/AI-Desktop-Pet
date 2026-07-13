"""
process_monitor.py —— 进程监控与系统信息采集

基于 psutil 库，提供：
  - 运行进程列表与排序（按 CPU 占用降序）
  - 进程搜索 / 查询 / 终止
  - 系统信息采集（CPU / 内存 / 磁盘 / 启动时间）
  - 网络流量统计
  - 电池状态查询

供 PetAgent 的 system_info / process_list 工具和系统提示词使用。
"""
import psutil
import os
from datetime import datetime

class ProcessMonitor:
    """
    进程监控器

    封装 psutil 库，提供进程管理和系统信息采集能力。
    所有方法均为无状态方法，不维护内部状态。
    """

    def get_running_processes(self) -> list:
        """
        获取所有运行中的进程列表

        Returns:
            按CPU占用降序排列的进程字典列表，每个包含 pid / name / cpu_percent / memory_percent / status
        """
        processes = []
        for proc in psutil.process_iter(["pid", "name", "cpu_percent", "memory_percent", "status"]):
            try:
                processes.append({
                    "pid": proc.info["pid"],
                    "name": proc.info["name"],
                    "cpu_percent": proc.info["cpu_percent"],
                    "memory_percent": proc.info["memory_percent"],
                    "status": proc.info["status"]
                })
            except (psutil.NoSuchProcess, psutil.AccessDenied):
                continue
        
        return sorted(processes, key=lambda x: x["cpu_percent"], reverse=True)

    def get_top_processes(self, count: int = 10) -> list:
        """
        获取 CPU 占用最高的 N 个进程

        Args:
            count: 返回的进程数量

        Returns:
            前 N 个高 CPU 占用进程的列表
        """
        processes = self.get_running_processes()
        return processes[:count]

    def get_process_by_name(self, name: str) -> list:
        """按名称模糊搜索进程"""
        name_lower = name.lower()
        return [p for p in self.get_running_processes() if name_lower in p["name"].lower()]

    def is_process_running(self, name: str) -> bool:
        """判断指定名称的进程是否正在运行"""
        return len(self.get_process_by_name(name)) > 0

    def get_process_count(self) -> int:
        """获取当前运行中的进程总数"""
        return len(self.get_running_processes())

    def get_system_info(self) -> dict:
        """
        获取系统综合信息

        Returns:
            包含 CPU / 内存 / 磁盘 / 启动时间 / CPU 核心数等信息的字典
        """
        cpu_percent = psutil.cpu_percent()
        memory = psutil.virtual_memory()
        disk = psutil.disk_usage("/")
        boot_time = datetime.fromtimestamp(psutil.boot_time())
        
        return {
            "cpu_percent": cpu_percent,
            "memory_percent": memory.percent,
            "memory_total_gb": round(memory.total / (1024 ** 3), 2),
            "memory_available_gb": round(memory.available / (1024 ** 3), 2),
            "disk_percent": disk.percent,
            "disk_total_gb": round(disk.total / (1024 ** 3), 2),
            "disk_available_gb": round(disk.free / (1024 ** 3), 2),
            "boot_time": boot_time.strftime("%Y-%m-%d %H:%M:%S"),
            "cpu_count": psutil.cpu_count(),
            "cpu_count_logical": psutil.cpu_count(logical=True)
        }

    def kill_process(self, pid: int) -> bool:
        """
        终止指定 PID 的进程

        Args:
            pid: 进程 ID

        Returns:
            True 表示成功终止，False 表示进程不存在或无权限
        """
        try:
            proc = psutil.Process(pid)
            proc.kill()
            return True
        except (psutil.NoSuchProcess, psutil.AccessDenied):
            return False

    def get_process_info(self, pid: int) -> dict:
        """
        获取指定进程的详细信息

        Args:
            pid: 进程 ID

        Returns:
            进程详细信息字典（名称、路径、命令行、CPU/内存占用等），失败返回空字典
        """
        try:
            proc = psutil.Process(pid)
            return {
                "pid": pid,
                "name": proc.name(),
                "exe": proc.exe(),
                "cmdline": " ".join(proc.cmdline()),
                "cpu_percent": proc.cpu_percent(),
                "memory_percent": proc.memory_percent(),
                "memory_info": proc.memory_info()._asdict(),
                "status": proc.status(),
                "create_time": datetime.fromtimestamp(proc.create_time()).strftime("%Y-%m-%d %H:%M:%S"),
                "username": proc.username()
            }
        except (psutil.NoSuchProcess, psutil.AccessDenied):
            return {}

    def get_network_info(self) -> dict:
        """获取网络流量统计（收发字节数、包数、错误数、丢包数）"""
        net_io = psutil.net_io_counters()
        return {
            "bytes_sent": net_io.bytes_sent,
            "bytes_recv": net_io.bytes_recv,
            "packets_sent": net_io.packets_sent,
            "packets_recv": net_io.packets_recv,
            "errin": net_io.errin,
            "errout": net_io.errout,
            "dropin": net_io.dropin,
            "dropout": net_io.dropout
        }

    def get_battery_info(self) -> dict:
        """
        获取电池状态（笔记本适用）

        Returns:
            电池信息字典，包含电量百分比、剩余时间、是否充电中；无电池时返回 {available: False}
        """
        if not psutil.sensors_battery():
            return {"available": False}
        
        battery = psutil.sensors_battery()
        return {
            "available": True,
            "percent": battery.percent,
            "secsleft": battery.secsleft,
            "power_plugged": battery.power_plugged
        }