import os
import json
from typing import Optional, List

class FileTools:
    @staticmethod
    def read_file(filepath: str) -> dict:
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
        return os.path.exists(filepath)

    @staticmethod
    def delete_file(filepath: str) -> bool:
        try:
            if os.path.exists(filepath):
                os.remove(filepath)
                return True
            return False
        except Exception:
            return False

    @staticmethod
    def rename_file(old_path: str, new_path: str) -> bool:
        try:
            os.rename(old_path, new_path)
            return True
        except Exception:
            return False

    @staticmethod
    def copy_file(source: str, destination: str) -> bool:
        try:
            import shutil
            shutil.copy2(source, destination)
            return True
        except Exception:
            return False

    @staticmethod
    def get_file_size(filepath: str) -> Optional[int]:
        try:
            return os.path.getsize(filepath)
        except Exception:
            return None

    @staticmethod
    def get_file_extension(filepath: str) -> str:
        return os.path.splitext(filepath)[1]

    @staticmethod
    def get_file_name(filepath: str) -> str:
        return os.path.basename(filepath)

    @staticmethod
    def get_parent_dir(filepath: str) -> str:
        return os.path.dirname(filepath)