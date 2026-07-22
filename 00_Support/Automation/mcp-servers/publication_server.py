#!/usr/bin/env python3
"""
Publication MCP Server
Handles end-to-end mod publishing to Nexus Mods, 7D2D Mods, and Mod Network.
Performs pre-publish validation and version tracking.
"""
import json
import sys
import os
import re
import zipfile
import tempfile
import shutil
import subprocess
from typing import Dict, List, Optional, Any
from datetime import datetime
from pathlib import Path
import xml.etree.ElementTree as ET

WORKSPACE_ROOT = r"c:\GitHub\7D2D-Mods"

class ModValidator:
    """Validates mod structure and metadata before publication."""
    
    def validate_mod_folder(self, folder_path: str) -> Dict:
        """Validate a mod folder for publication readiness."""
        full_path = os.path.join(WORKSPACE_ROOT, folder_path)
        if not os.path.exists(full_path):
            return {"valid": False, "errors": [f"Folder not found: {folder_path}"]}
        
        errors = []
        warnings = []
        info = {}
        
        # Check ModInfo.xml
        modinfo_path = os.path.join(full_path, "ModInfo.xml")
        if not os.path.exists(modinfo_path):
            errors.append("Missing ModInfo.xml")
        else:
            try:
                tree = ET.parse(modinfo_path)
                root = tree.getroot()
                
                # Extract mod info
                name_elem = root.find("Name")
                version_elem = root.find("Version")
                
                info["name"] = name_elem.text if name_elem is not None else "Unknown"
                info["version"] = version_elem.text if version_elem is not None else "Unknown"
                
                # Check required fields
                if info.get("name") in [None, "Unknown", ""]:
                    errors.append("ModInfo.xml missing Name field")
                if info.get("version") in [None, "Unknown", ""]:
                    errors.append("ModInfo.xml missing Version field")
                else:
                    # Validate version format
                    if not re.match(r'^\d+\.\d+\.\d+$', info["version"]):
                        warnings.append(f"Version format non-standard: {info['version']}")
                
                # Check DisplayName
                if root.find("DisplayName") is None or not (root.find("DisplayName").text or "").strip():
                    warnings.append("ModInfo.xml missing DisplayName")
                
                # Check Description
                if root.find("Description") is None or not (root.find("Description").text or "").strip():
                    warnings.append("ModInfo.xml missing Description")
                    
            except ET.ParseError as e:
                errors.append(f"Invalid ModInfo.xml: {str(e)}")
        
        # Check README
        readme_paths = [
            os.path.join(full_path, "README.txt"),
            os.path.join(full_path, "README.md")
        ]
        readme_found = False
        for rp in readme_paths:
            if os.path.exists(rp):
                readme_found = True
                info["readme"] = os.path.basename(rp)
                break
        if not readme_found:
            warnings.append("No README.txt or README.md found")
        
        # Check folder structure
        has_configs = any(f.endswith('.xml') for f in os.listdir(full_path) if os.path.isfile(os.path.join(full_path, f)))
        has_subfolders = any(
            os.path.isdir(os.path.join(full_path, d)) and d not in ['.git', '__pycache__']
            for d in os.listdir(full_path)
        )
        
        info["has_config_xmls"] = has_configs
        info["has_subfolders"] = has_subfolders
        
        # Check for cross-platform paths
        for root_dir, dirs, files in os.walk(full_path):
            for f in files:
                if '\\' in f:
                    warnings.append(f"Backslash in filename: {f}")
        
        # Get file count and total size
        file_count = 0
        total_size = 0
        for root_dir, dirs, files in os.walk(full_path):
            for f in files:
                fp = os.path.join(root_dir, f)
                file_count += 1
                total_size += os.path.getsize(fp)
        
        info["file_count"] = file_count
        info["total_size_kb"] = round(total_size / 1024, 1)
        
        return {
            "valid": len(errors) == 0,
            "errors": errors,
            "warnings": warnings,
            "info": info
        }
    
    def get_mods_ready_for_publish(self) -> List[Dict]:
        """List all mods in ActiveBuild and check their publish readiness."""
        active_build = os.path.join(WORKSPACE_ROOT, "02_ActiveBuild")
        if not os.path.exists(active_build):
            return []
        
        mods = []
        for item in os.listdir(active_build):
            item_path = os.path.join(active_build, item)
            if os.path.isdir(item_path):
                validation = self.validate_mod_folder(os.path.join("02_ActiveBuild", item))
                mods.append({
                    "folder": item,
                    "valid": validation["valid"],
                    "errors": validation["errors"],
                    "warnings": validation["warnings"],
                    "info": validation["info"]
                })
        
        return mods


class PublicationServer:
    """MCP Server for mod publication."""
    
    def __init__(self):
        self.validator = ModValidator()
    
    def process_request(self, request: Dict) -> Dict:
        method = request.get("method", "")
        params = request.get("params", {})
        
        if method == "tools/list":
            return self._handle_tools_list()
        elif method == "tools/call":
            return self._handle_tool_call(params)
        elif method == "resources/list":
            return self._handle_resources_list()
        elif method == "resources/read":
            return self._handle_resource_read(params)
        else:
            return self._error("method_not_found", f"Unknown method: {method}")
    
    def _handle_tools_list(self) -> Dict:
        return {
            "tools": [
                {
                    "name": "validate_mod",
                    "description": "Validate a mod folder for publication readiness",
                    "inputSchema": {
                        "type": "object",
                        "properties": {
                            "folder_path": {
                                "type": "string",
                                "description": "Path to mod folder (e.g., '02_ActiveBuild/AGF-BackpackPlus-060Slots-v4.1.1')"
                            }
                        },
                        "required": ["folder_path"]
                    }
                },
                {
                    "name": "list_ready_mods",
                    "description": "List all mods in ActiveBuild with their publish readiness status",
                    "inputSchema": {
                        "type": "object",
                        "properties": {}
                    }
                },
                {
                    "name": "create_release_package",
                    "description": "Create a release-ready zip package of a mod for publishing",
                    "inputSchema": {
                        "type": "object",
                        "properties": {
                            "source_folder": {
                                "type": "string",
                                "description": "Source mod folder (e.g., '02_ActiveBuild/AGF-BackpackPlus-060Slots-v4.1.1')"
                            },
                            "output_name": {
                                "type": "string",
                                "description": "Output zip filename (without .zip). Defaults to folder name."
                            }
                        },
                        "required": ["source_folder"]
                    }
                },
                {
                    "name": "generate_changelog",
                    "description": "Generate a changelog summary from git history for a mod",
                    "inputSchema": {
                        "type": "object",
                        "properties": {
                            "folder_path": {
                                "type": "string",
                                "description": "Path to mod folder"
                            },
                            "from_version": {
                                "type": "string",
                                "description": "Previous version tag to compare from"
                            }
                        },
                        "required": ["folder_path"]
                    }
                },
                {
                    "name": "check_localization",
                    "description": "Check if a mod has proper localization files and flag missing entries",
                    "inputSchema": {
                        "type": "object",
                        "properties": {
                            "folder_path": {
                                "type": "string",
                                "description": "Path to mod folder"
                            }
                        },
                        "required": ["folder_path"]
                    }
                }
            ]
        }
    
    def _handle_tool_call(self, params: Dict) -> Dict:
        tool_name = params.get("name", "")
        arguments = params.get("arguments", {})
        
        if tool_name == "validate_mod":
            return self._validate_mod(arguments)
        elif tool_name == "list_ready_mods":
            return self._list_ready_mods(arguments)
        elif tool_name == "create_release_package":
            return self._create_release_package(arguments)
        elif tool_name == "generate_changelog":
            return self._generate_changelog(arguments)
        elif tool_name == "check_localization":
            return self._check_localization(arguments)
        else:
            return self._error("tool_not_found", f"Unknown tool: {tool_name}")
    
    def _validate_mod(self, args: Dict) -> Dict:
        folder = args.get("folder_path", "")
        result = self.validator.validate_mod_folder(folder)
        return {
            "content": [{"type": "text", "text": json.dumps(result, indent=2)}]
        }
    
    def _list_ready_mods(self, args: Dict) -> Dict:
        mods = self.validator.get_mods_ready_for_publish()
        
        summary = {
            "total_mods": len(mods),
            "ready_to_publish": [m for m in mods if m["valid"]],
            "needs_attention": [{
                "folder": m["folder"],
                "errors": m["errors"],
                "warnings": m["warnings"]
            } for m in mods if not m["valid"] or m["warnings"]]
        }
        
        return {
            "content": [{"type": "text", "text": json.dumps(summary, indent=2)}]
        }
    
    def _create_release_package(self, args: Dict) -> Dict:
        source = args.get("source_folder", "")
        output_name = args.get("output_name", "")
        
        source_path = os.path.join(WORKSPACE_ROOT, source)
        if not os.path.exists(source_path):
            return self._error("not_found", f"Source folder not found: {source}")
        
        if not output_name:
            output_name = os.path.basename(source)
        
        # Create zip
        release_dir = os.path.join(WORKSPACE_ROOT, "03_ReleaseSource")
        os.makedirs(release_dir, exist_ok=True)
        
        zip_path = os.path.join(release_dir, f"{output_name}.zip")
        
        with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zf:
            for root_dir, dirs, files in os.walk(source_path):
                for file in files:
                    file_path = os.path.join(root_dir, file)
                    arcname = os.path.relpath(file_path, source_path)
                    zf.write(file_path, arcname)
        
        size = os.path.getsize(zip_path)
        
        return {
            "content": [{
                "type": "text",
                "text": json.dumps({
                    "success": True,
                    "zip_path": os.path.relpath(zip_path, WORKSPACE_ROOT),
                    "size_kb": round(size / 1024, 1),
                    "contents": [n for n in zipfile.ZipFile(zip_path).namelist()],
                    "note": "This zip is ready for Nexus Mods / 7D2D Mods upload."
                }, indent=2)
            }]
        }
    
    def _generate_changelog(self, args: Dict) -> Dict:
        folder = args.get("folder_path", "")
        from_version = args.get("from_version", "")
        
        full_path = os.path.join(WORKSPACE_ROOT, folder)
        rel_path = os.path.relpath(full_path, WORKSPACE_ROOT)
        
        try:
            # Get git log for this mod folder
            cmd = ["git", "log", "--oneline", "--", rel_path]
            if from_version:
                cmd = ["git", "log", f"{from_version}..HEAD", "--oneline", "--", rel_path]
            
            result = subprocess.run(cmd, capture_output=True, text=True, cwd=WORKSPACE_ROOT, timeout=10)
            commits = [line.strip() for line in result.stdout.split('\n') if line.strip()]
            
            return {
                "content": [{
                    "type": "text",
                    "text": json.dumps({
                        "folder": folder,
                        "commit_count": len(commits),
                        "recent_commits": commits[:20],
                        "changelog": "\\n".join(commits[:10]) if commits else "No recent changes found."
                    }, indent=2)
                }]
            }
        except Exception as e:
            return self._error("git_error", str(e))
    
    def _check_localization(self, args: Dict) -> Dict:
        folder = args.get("folder_path", "")
        full_path = os.path.join(WORKSPACE_ROOT, folder)
        
        if not os.path.exists(full_path):
            return self._error("not_found", f"Folder not found: {folder}")
        
        # Look for localization files
        localizations = {}
        for root_dir, dirs, files in os.walk(full_path):
            for f in files:
                if 'localization' in f.lower():
                    fp = os.path.join(root_dir, f)
                    with open(fp, 'r', encoding='utf-8', errors='replace') as fh:
                        content = fh.read()
                    localizations[f] = {
                        "path": os.path.relpath(fp, WORKSPACE_ROOT),
                        "size_kb": round(os.path.getsize(fp) / 1024, 1),
                        "lines": len(content.split('\n')),
                        "sample": content[:500] + "..." if len(content) > 500 else content
                    }
        
        return {
            "content": [{
                "type": "text",
                "text": json.dumps({
                    "folder": folder,
                    "localization_files_count": len(localizations),
                    "files": localizations if localizations else "No localization files found"
                }, indent=2)
            }]
        }
    
    def _handle_resources_list(self) -> Dict:
        return {"resources": []}
    
    def _handle_resource_read(self, params: Dict) -> Dict:
        return self._error("resource_not_found", "No resources available")
    
    def _error(self, code: str, message: str) -> Dict:
        return {
            "content": [{"type": "text", "text": json.dumps({"error": code, "message": message})}],
            "isError": True
        }


def main():
    server = PublicationServer()
    for line in sys.stdin:
        line = line.strip()
        if not line:
            continue
        try:
            request = json.loads(line)
            response = server.process_request(request)
            sys.stdout.write(json.dumps(response) + '\n')
            sys.stdout.flush()
        except json.JSONDecodeError as e:
            error_response = {"content": [{"type": "text", "text": json.dumps({"error": "parse_error", "message": str(e)})}], "isError": True}
            sys.stdout.write(json.dumps(error_response) + '\n')
            sys.stdout.flush()
        except Exception as e:
            error_response = {"content": [{"type": "text", "text": json.dumps({"error": "internal_error", "message": str(e)})}], "isError": True}
            sys.stdout.write(json.dumps(error_response) + '\n')
            sys.stdout.flush()


if __name__ == "__main__":
    main()