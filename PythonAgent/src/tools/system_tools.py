"""
system_tools.py —— 系统工具集

提供与操作系统交互的工具方法：
  - run_command: 执行 shell 命令（支持超时控制）
  - open_file:   用系统默认程序打开文件/应用
  - get_os_info: 获取操作系统信息
  - 环境变量管理（get/set/list）
  - 目录操作（获取当前目录、切换目录、创建/删除目录）
  - sleep: 阻塞等待

安全提示：run_command 使用 shell=True，存在命令注入风险，
未来需要增加白名单或沙箱机制。
"""
import subprocess
import os
import platform
from typing import Optional

class SystemTools:
    """
    系统工具集

    所有方法均为静态方法，不维护内部状态。
    提供 shell 命令执行、文件打开、OS 信息查询、环境变量管理、目录操作等能力。
    """

    @staticmethod
    def run_command(command: str, timeout: int = 30) -> dict:
        """
        执行 shell 命令

        Args:
            command: 要执行的命令字符串
            timeout: 超时时间（秒），默认 30 秒

        Returns:
            结果字典，包含 success / stdout / stderr / return_code
        """
        try:
            result = subprocess.run(
                command,
                shell=True,
                capture_output=True,
                text=True,
                timeout=timeout,
                encoding="utf-8",
                errors="replace"
            )
            return {
                "success": result.returncode == 0,
                "stdout": result.stdout,
                "stderr": result.stderr,
                "return_code": result.returncode
            }
        except subprocess.TimeoutExpired:
            return {
                "success": False,
                "stdout": "",
                "stderr": f"Command timed out after {timeout} seconds",
                "return_code": -1
            }
        except Exception as e:
            return {
                "success": False,
                "stdout": "",
                "stderr": str(e),
                "return_code": -2
            }

    @staticmethod
    def open_file(filepath: str) -> bool:
        """
        用系统默认程序打开文件或应用

        自动适配 Windows（os.startfile）、macOS（open）、Linux（xdg-open）。

        Args:
            filepath: 文件路径或应用名称

        Returns:
            True 表示成功打开，False 表示打开失败
        """
        try:
            if platform.system() == "Windows":
                os.startfile(filepath)
            elif platform.system() == "Darwin":
                subprocess.run(["open", filepath])
            else:
                subprocess.run(["xdg-open", filepath])
            return True
        except Exception:
            return False

    @staticmethod
    def get_os_info() -> dict:
        """获取操作系统信息（系统类型、版本、架构、处理器）"""
        return {
            "system": platform.system(),
            "release": platform.release(),
            "version": platform.version(),
            "machine": platform.machine(),
            "processor": platform.processor()
        }

    @staticmethod
    def get_env_var(name: str) -> Optional[str]:
        """获取环境变量的值"""
        return os.environ.get(name)

    @staticmethod
    def set_env_var(name: str, value: str):
        """设置环境变量（仅在当前进程中生效）"""
        os.environ[name] = value

    @staticmethod
    def list_env_vars() -> dict:
        """获取所有环境变量"""
        return dict(os.environ)

    @staticmethod
    def sleep(seconds: int):
        """阻塞等待指定秒数"""
        import time
        time.sleep(seconds)

    @staticmethod
    def get_current_dir() -> str:
        """获取当前工作目录"""
        return os.getcwd()

    @staticmethod
    def change_dir(path: str) -> bool:
        """切换当前工作目录"""
        try:
            os.chdir(path)
            return True
        except Exception:
            return False

    @staticmethod
    def create_dir(path: str) -> bool:
        """创建目录（支持多级创建）"""
        try:
            os.makedirs(path, exist_ok=True)
            return True
        except Exception:
            return False

    @staticmethod
    def delete_dir(path: str) -> bool:
        """递归删除目录及其所有内容"""
        try:
            import shutil
            shutil.rmtree(path)
            return True
        except Exception:
            return False