import psutil
import os
from datetime import datetime

class ProcessMonitor:
    def get_running_processes(self) -> list:
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
        processes = self.get_running_processes()
        return processes[:count]

    def get_process_by_name(self, name: str) -> list:
        name_lower = name.lower()
        return [p for p in self.get_running_processes() if name_lower in p["name"].lower()]

    def is_process_running(self, name: str) -> bool:
        return len(self.get_process_by_name(name)) > 0

    def get_process_count(self) -> int:
        return len(self.get_running_processes())

    def get_system_info(self) -> dict:
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
        try:
            proc = psutil.Process(pid)
            proc.kill()
            return True
        except (psutil.NoSuchProcess, psutil.AccessDenied):
            return False

    def get_process_info(self, pid: int) -> dict:
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
        if not psutil.sensors_battery():
            return {"available": False}
        
        battery = psutil.sensors_battery()
        return {
            "available": True,
            "percent": battery.percent,
            "secsleft": battery.secsleft,
            "power_plugged": battery.power_plugged
        }