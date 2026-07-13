"""
file_tools.py —— 文件工具集

提供文件操作的工具方法：
  - 文本文件读写（read_file / write_file，支持追加模式）
  - JSON 文件读写（read_json / write_json）
  - 目录列举（list_files，返回文件/文件夹信息）
  - 文件管理（exists / delete / rename / copy）
  - 文件信息查询（size / extension / name / parent_dir）

所有方法均使用 UTF-8 编码，返回统一格式的结果字典。
"""
import os
import json
from typing import Optional, List

class FileTools:
    """
    文件工具集

    所有方法均为静态方法，不维护内部状态。
    提供文本/JSON 文件读写、目录列举、文件管理操作等能力。
    """

    @staticmethod
    def read_file(filepath: str) -> dict:
        """
        读取文本文件内容

        Args:
            filepath: 文件路径

        Returns:
            结果字典，包含 success / content / error
        """
        try:
            with open(filepath, "r", encoding="utf-8") as f:
                content = f.read()
            return {
                "success": True,
                "content": content,
                "error": ""
            }
        except Exception as e:
            return {
                "success": False,
                "content": "",
                "error": str(e)
            }

    @staticmethod
    def write_file(filepath: str, content: str, append: bool = False) -> dict:
        """
        写入文本文件

        Args:
            filepath: 文件路径
            content:  写入内容
            append:   是否追加模式（True 为追加，False 为覆盖）

        Returns:
            结果字典，包含 success / error
        """
        try:
            mode = "a" if append else "w"
            with open(filepath, mode, encoding="utf-8") as f:
                f.write(content)
            return {
                "success": True,
                "error": ""
            }
        except Exception as e:
            return {
                "success": False,
                "error": str(e)
            }

    @staticmethod
    def read_json(filepath: str) -> dict:
        """
        读取 JSON 文件

        Args:
            filepath: JSON 文件路径

        Returns:
            结果字典，包含 success / data / error
        """
        try:
            with open(filepath, "r", encoding="utf-8") as f:
                data = json.load(f)
            return {
                "success": True,
                "data": data,
                "error": ""
            }
        except Exception as e:
            return {
                "success": False,
                "data": None,
                "error": str(e)
            }

    @staticmethod
    def write_json(filepath: str, data: dict, indent: int = 2) -> dict:
        """
        写入 JSON 文件

        Args:
            filepath: JSON 文件路径
            data:     要写入的字典数据
            indent:   JSON 缩进空格数

        Returns:
            结果字典，包含 success / error
        """
        try:
            with open(filepath, "w", encoding="utf-8") as f:
                json.dump(data, f, ensure_ascii=False, indent=indent)
            return {
                "success": True,
                "error": ""
            }
        except Exception as e:
            return {
                "success": False,
                "error": str(e)
            }

    @staticmethod
    def list_files(path: str = ".") -> list:
        """
        列举目录下的文件和子目录

        Args:
            path: 目录路径，默认为当前目录

        Returns:
            文件/目录信息列表，每项包含 name / path / type / size / mtime
        """
        try:
            files = []
            for entry in os.listdir(path):
                full_path = os.path.join(path, entry)
                if os.path.isfile(full_path):
                    files.append({
                        "name": entry,
                        "path": full_path,
                        "type": "file",
                        "size": os.path.getsize(full_path),
                        "mtime": os.path.getmtime(full_path)
                    })
                elif os.path.isdir(full_path):
                    files.append({
                        "name": entry,
                        "path": full_path,
                        "type": "directory",
                        "size": 0,
                        "mtime": os.path.getmtime(full_path)
                    })
            return files
        except Exception:
            return []

    @staticmethod
    def file_exists(filepath: str) -> bool:
        """判断文件或目录是否存在"""
        return os.path.exists(filepath)

    @staticmethod
    def delete_file(filepath: str) -> bool:
        """删除文件"""
        try:
            if os.path.exists(filepath):
                os.remove(filepath)
                return True
            return False
        except Exception:
            return False

    @staticmethod
    def rename_file(old_path: str, new_path: str) -> bool:
        """重命名/移动文件"""
        try:
            os.rename(old_path, new_path)
            return True
        except Exception:
            return False

    @staticmethod
    def copy_file(source: str, destination: str) -> bool:
        """复制文件（保留元数据）"""
        try:
            import shutil
            shutil.copy2(source, destination)
            return True
        except Exception:
            return False

    @staticmethod
    def get_file_size(filepath: str) -> Optional[int]:
        """获取文件大小（字节）"""
        try:
            return os.path.getsize(filepath)
        except Exception:
            return None

    @staticmethod
    def get_file_extension(filepath: str) -> str:
        """获取文件扩展名（含点号，如 '.py'）"""
        return os.path.splitext(filepath)[1]

    @staticmethod
    def get_file_name(filepath: str) -> str:
        """获取文件名（含扩展名）"""
        return os.path.basename(filepath)

    @staticmethod
    def get_parent_dir(filepath: str) -> str:
        """获取文件所在的父目录路径"""
        return os.path.dirname(filepath)