#!/usr/bin/env python3
"""
Script Management MCP Server
Provides tools for indexing, analyzing, and safely modifying Python scripts.
Uses git history and function-level awareness to make targeted edits.
"""
import json
import sys
import os
import re
import ast
import subprocess
import hashlib
from typing import Dict, List, Optional, Any
from datetime import datetime

WORKSPACE_ROOT = r"c:\GitHub\7D2D-Mods"

class ScriptAnalyzer:
    """Analyzes Python scripts to extract function maps, dependencies, and git history."""
    
    def __init__(self):
        self.function_cache: Dict[str, List[Dict]] = {}
    
    def extract_functions(self, filepath: str) -> List[Dict]:
        """Extract all function definitions and their details from a Python file."""
        if filepath in self.function_cache:
            return self.function_cache[filepath]
        
        try:
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()
        except Exception as e:
            return [{"error": f"Cannot read file: {str(e)}"}]
        
        functions = []
        try:
            tree = ast.parse(content)
            for node in ast.walk(tree):
                if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef)):
                    func_info = {
                        "name": node.name,
                        "lineno": node.lineno,
                        "end_lineno": node.end_lineno,
                        "docstring": ast.get_docstring(node) or "",
                        "args": self._get_args(node),
                        "decorators": self._get_decorators(node),
                        "calls": self._get_calls(node),
                    }
                    functions.append(func_info)
        except SyntaxError as e:
            return [{"error": f"Syntax error parsing file: {str(e)}"}]
        
        self.function_cache[filepath] = functions
        return functions
    
    def _get_args(self, node) -> List[Dict]:
        """Extract function arguments."""
        args = []
        for arg in node.args.args:
            args.append({
                "name": arg.arg,
                "annotation": ast.dump(arg.annotation) if arg.annotation else None
            })
        return args
    
    def _get_decorators(self, node) -> List[str]:
        """Extract decorator names."""
        return [ast.dump(d) for d in node.decorator_list]
    
    def _get_calls(self, node) -> List[str]:
        """Extract function calls made within the function."""
        calls = []
        for child in ast.walk(node):
            if isinstance(child, ast.Call):
                if isinstance(child.func, ast.Name):
                    calls.append(child.func.id)
                elif isinstance(child.func, ast.Attribute):
                    calls.append(f"{ast.dump(child.func.value)}.{child.func.attr}")
        return calls
    
    def get_function_map(self, filepath: str) -> Dict:
        """Get complete function map for a file with git history context."""
        functions = self.extract_functions(filepath)
        git_info = self._get_git_history(filepath)
        
        return {
            "file": filepath,
            "total_functions": len([f for f in functions if "error" not in f]),
            "functions": functions,
            "git_info": git_info
        }
    
    def _get_git_history(self, filepath: str) -> Dict:
        """Get git history for a file."""
        rel_path = os.path.relpath(filepath, WORKSPACE_ROOT)
        try:
            result = subprocess.run(
                ["git", "log", "--oneline", "-20", "--", rel_path],
                capture_output=True, text=True, cwd=WORKSPACE_ROOT, timeout=10
            )
            commits = [line.strip() for line in result.stdout.split('\n') if line.strip()]
            
            # Get last modified date
            result2 = subprocess.run(
                ["git", "log", "-1", "--format=%ai", "--", rel_path],
                capture_output=True, text=True, cwd=WORKSPACE_ROOT, timeout=10
            )
            last_modified = result2.stdout.strip()
            
            return {
                "commit_count": len(commits),
                "recent_commits": commits[:10],
                "last_modified": last_modified
            }
        except Exception as e:
            return {"error": str(e)}
    
    def find_dependencies(self, filepath: str) -> Dict:
        """Find import dependencies of a script."""
        try:
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()
        except Exception:
            return {"error": "Cannot read file"}
        
        tree = ast.parse(content)
        imports = []
        for node in ast.walk(tree):
            if isinstance(node, ast.Import):
                for alias in node.names:
                    imports.append({"type": "import", "module": alias.name, "as": alias.asname})
            elif isinstance(node, ast.ImportFrom):
                for alias in node.names:
                    imports.append({
                        "type": "from", "module": node.module or "",
                        "name": alias.name, "as": alias.asname
                    })
        
        return {"file": filepath, "imports": imports}


class ScriptManagerServer:
    """MCP Server for script management."""
    
    def __init__(self):
        self.analyzer = ScriptAnalyzer()
    
    def process_request(self, request: Dict) -> Dict:
        """Process an incoming MCP request."""
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
                    "name": "analyze_script",
                    "description": "Analyze a Python script to get its function map (all functions, args, docs, and git history)",
                    "inputSchema": {
                        "type": "object",
                        "properties": {
                            "filepath": {
                                "type": "string",
                                "description": "Path to the Python script (relative to workspace root)"
                            }
                        },
                        "required": ["filepath"]
                    }
                },
                {
                    "name": "check_dependencies",
                    "description": "Find all import dependencies of a script",
                    "inputSchema": {
                        "type": "object",
                        "properties": {
                            "filepath": {
                                "type": "string",
                                "description": "Path to the Python script (relative to workspace root)"
                            }
                        },
                        "required": ["filepath"]
                    }
                },
                {
                    "name": "list_scripts",
                    "description": "List all scripts in the workspace matching a pattern",
                    "inputSchema": {
                        "type": "object",
                        "properties": {
                            "pattern": {
                                "type": "string",
                                "description": "Glob pattern (e.g., 'SCRIPT-*.py')",
                                "default": "SCRIPT-*.py"
                            }
                        },
                        "required": []
                    }
                },
                {
                    "name": "safe_modify",
                    "description": "Preview a modification before applying it - checks git diff to ensure only intended lines change",
                    "inputSchema": {
                        "type": "object",
                        "properties": {
                            "filepath": {
                                "type": "string",
                                "description": "Path to the script (relative to workspace root)"
                            },
                            "search": {
                                "type": "string",
                                "description": "The exact code block to replace"
                            },
                            "replace": {
                                "type": "string",
                                "description": "The new code block"
                            }
                        },
                        "required": ["filepath", "search", "replace"]
                    }
                },
                {
                    "name": "get_git_diff",
                    "description": "Get the current git diff for a file to see what's changed",
                    "inputSchema": {
                        "type": "object",
                        "properties": {
                            "filepath": {
                                "type": "string",
                                "description": "Path to check (relative to workspace). If empty, shows all changes."
                            }
                        },
                        "required": []
                    }
                },
                {
                    "name": "find_function",
                    "description": "Find where a specific function is defined across all scripts",
                    "inputSchema": {
                        "type": "object",
                        "properties": {
                            "function_name": {
                                "type": "string",
                                "description": "Function name to search for"
                            }
                        },
                        "required": ["function_name"]
                    }
                }
            ]
        }
    
    def _handle_tool_call(self, params: Dict) -> Dict:
        tool_name = params.get("name", "")
        arguments = params.get("arguments", {})
        
        if tool_name == "analyze_script":
            return self._analyze_script(arguments)
        elif tool_name == "check_dependencies":
            return self._check_dependencies(arguments)
        elif tool_name == "list_scripts":
            return self._list_scripts(arguments)
        elif tool_name == "safe_modify":
            return self._safe_modify(arguments)
        elif tool_name == "get_git_diff":
            return self._get_git_diff(arguments)
        elif tool_name == "find_function":
            return self._find_function(arguments)
        else:
            return self._error("tool_not_found", f"Unknown tool: {tool_name}")
    
    def _analyze_script(self, args: Dict) -> Dict:
        filepath = args.get("filepath", "")
        full_path = os.path.join(WORKSPACE_ROOT, filepath)
        
        if not os.path.exists(full_path):
            return self._error("file_not_found", f"File not found: {filepath}")
        
        result = self.analyzer.get_function_map(full_path)
        return {
            "content": [{"type": "text", "text": json.dumps(result, indent=2)}]
        }
    
    def _check_dependencies(self, args: Dict) -> Dict:
        filepath = args.get("filepath", "")
        full_path = os.path.join(WORKSPACE_ROOT, filepath)
        
        if not os.path.exists(full_path):
            return self._error("file_not_found", f"File not found: {filepath}")
        
        result = self.analyzer.find_dependencies(full_path)
        return {
            "content": [{"type": "text", "text": json.dumps(result, indent=2)}]
        }
    
    def _list_scripts(self, args: Dict) -> Dict:
        import glob
        pattern = args.get("pattern", "SCRIPT-*.py")
        full_pattern = os.path.join(WORKSPACE_ROOT, pattern)
        scripts = glob.glob(full_pattern)
        
        # Also search in subdirectories
        scripts_recursive = []
        for root, dirs, files in os.walk(WORKSPACE_ROOT):
            for f in files:
                if f.startswith(pattern.replace("*", "").replace(".py", "")) and f.endswith(".py"):
                    rel_path = os.path.relpath(os.path.join(root, f), WORKSPACE_ROOT)
                    scripts_recursive.append(rel_path)
        
        results = []
        for s in scripts_recursive:
            full = os.path.join(WORKSPACE_ROOT, s)
            size = os.path.getsize(full)
            results.append({"file": s, "size_kb": round(size / 1024, 1)})
        
        return {
            "content": [{"type": "text", "text": json.dumps(results, indent=2)}]
        }
    
    def _safe_modify(self, args: Dict) -> Dict:
        """Preview a modification by checking current state and showing what would change."""
        filepath = args.get("filepath", "")
        search = args.get("search", "")
        replace = args.get("replace", "")
        full_path = os.path.join(WORKSPACE_ROOT, filepath)
        
        if not os.path.exists(full_path):
            return self._error("file_not_found", f"File not found: {filepath}")
        
        try:
            with open(full_path, 'r', encoding='utf-8') as f:
                content = f.read()
        except Exception as e:
            return self._error("read_error", str(e))
        
        if search not in content:
            return self._error("search_not_found", "The search text was not found in the file. Please ensure exact match.")
        
        # Count occurrences
        count = content.count(search)
        
        # Check git changes first
        git_diff = self._get_git_diff_raw(filepath)
        
        # Show what would change
        new_content = content.replace(search, replace, 1)
        
        return {
            "content": [
                {
                    "type": "text",
                    "text": json.dumps({
                        "file": filepath,
                        "search_found": True,
                        "occurrences_in_file": count,
                        "current_git_status": git_diff if git_diff else "No uncommitted changes",
                        "modification_preview": {
                            "before": search,
                            "after": replace
                        },
                        "length_before": len(content),
                        "length_after": len(new_content),
                        "line_delta": new_content.count('\n') - content.count('\n'),
                        "recommendation": "Proceed with applying this change?" if count == 1 else f"Warning: '{search}' appears {count} times. Be more specific."
                    }, indent=2)
                }
            ]
        }
    
    def _get_git_diff_raw(self, filepath: Optional[str] = None) -> str:
        path_arg = ["--", os.path.relpath(os.path.join(WORKSPACE_ROOT, filepath), WORKSPACE_ROOT)] if filepath else []
        try:
            result = subprocess.run(
                ["git", "diff"] + path_arg,
                capture_output=True, text=True, cwd=WORKSPACE_ROOT, timeout=10
            )
            return result.stdout
        except Exception:
            return ""
    
    def _get_git_diff(self, args: Dict) -> Dict:
        filepath = args.get("filepath", "")
        diff_text = self._get_git_diff_raw(filepath if filepath else None)
        
        return {
            "content": [{"type": "text", "text": diff_text if diff_text else "No uncommitted changes found."}]
        }
    
    def _find_function(self, args: Dict) -> Dict:
        func_name = args.get("function_name", "")
        results = []
        
        for root, dirs, files in os.walk(WORKSPACE_ROOT):
            # Skip hidden dirs and venv
            dirs[:] = [d for d in dirs if not d.startswith('.') and d != '__pycache__' and d != '.venv']
            for f in files:
                if f.endswith('.py'):
                    full = os.path.join(root, f)
                    rel = os.path.relpath(full, WORKSPACE_ROOT)
                    try:
                        with open(full, 'r', encoding='utf-8') as fh:
                            content = fh.read()
                        tree = ast.parse(content)
                        for node in ast.walk(tree):
                            if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef)) and node.name == func_name:
                                doc = ast.get_docstring(node) or ""
                                results.append({
                                    "file": rel,
                                    "line": node.lineno,
                                    "docstring": doc[:200]  # First 200 chars
                                })
                    except:
                        pass
        
        if not results:
            return {
                "content": [{"type": "text", "text": json.dumps({"found": False, "message": f"Function '{func_name}' not found in any Python file."}, indent=2)}]
            }
        
        return {
            "content": [{"type": "text", "text": json.dumps({"found": True, "matches": results}, indent=2)}]
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
    """Run the MCP server over stdio."""
    server = ScriptManagerServer()
    
    # Read requests line by line from stdin
    for line in sys.stdin:
        line = line.strip()
        if not line:
            continue
        
        try:
            request = json.loads(line)
            response = server.process_request(request)
            # Write response to stdout
            sys.stdout.write(json.dumps(response) + '\n')
            sys.stdout.flush()
        except json.JSONDecodeError as e:
            error_response = {
                "content": [{"type": "text", "text": json.dumps({"error": "parse_error", "message": str(e)})}],
                "isError": True
            }
            sys.stdout.write(json.dumps(error_response) + '\n')
            sys.stdout.flush()
        except Exception as e:
            error_response = {
                "content": [{"type": "text", "text": json.dumps({"error": "internal_error", "message": str(e)})}],
                "isError": True
            }
            sys.stdout.write(json.dumps(error_response) + '\n')
            sys.stdout.flush()


if __name__ == "__main__":
    main()