import subprocess
import os
import platform
from typing import Optional

class SystemTools:
    @staticmethod
    def run_command(command: str, timeout: int = 30) -> dict:
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
        return {
            "system": platform.system(),
            "release": platform.release(),
            "version": platform.version(),
            "machine": platform.machine(),
            "processor": platform.processor()
        }

    @staticmethod
    def get_env_var(name: str) -> Optional[str]:
        return os.environ.get(name)

    @staticmethod
    def set_env_var(name: str, value: str):
        os.environ[name] = value

    @staticmethod
    def list_env_vars() -> dict:
        return dict(os.environ)

    @staticmethod
    def sleep(seconds: int):
        import time
        time.sleep(seconds)

    @staticmethod
    def get_current_dir() -> str:
        return os.getcwd()

    @staticmethod
    def change_dir(path: str) -> bool:
        try:
            os.chdir(path)
            return True
        except Exception:
            return False

    @staticmethod
    def create_dir(path: str) -> bool:
        try:
            os.makedirs(path, exist_ok=True)
            return True
        except Exception:
            return False

    @staticmethod
    def delete_dir(path: str) -> bool:
        try:
            import shutil
            shutil.rmtree(path)
            return True
        except Exception:
            return False