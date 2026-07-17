#!/usr/bin/env python3
"""
C# Code Quality MCP Server
Analyzes Harmony patch .dll C# source code for 7D2D-specific issues.
Validates null safety, Harmony patch patterns, and IL code safety.
"""
import json
import sys
import os
import re
import subprocess
from typing import Dict, List, Optional, Any
from pathlib import Path

WORKSPACE_ROOT = r"c:\GitHub\7D2D-Mods"

class CSharpAnalyzer:
    """Analyzes C# Harmony patch code for 7D2D-specific issues."""
    
    # Known 7D2D types that are commonly accessed and can be null
    DANGEROUS_PATTERNS = [
        r'EntityPlayerLocal\.(?!IsAlive|IsDead)',
        r'GameManager\.Instance\.',
        r'LocalPlayerUI\.primaryUI\.',
        r'XUi\.',
        r'World\.GetEntity\(\)',
        r'ItemStack\.',
        r'ItemValue\.',
        r'EntityAlive\.',
        r'EntityItem\.',
    ]
    
    # Harmony attributes that should be present
    HARMONY_TYPES = ['HarmonyPatch', 'HarmonyPrefix', 'HarmonyPostfix', 'HarmonyTranspiler']
    
    KNOWN_PITFALLS = [
        {
            "pattern": r'\.GetInstance\(\)',
            "warning": "GetInstance() can return null. Always null-check after calling.",
            "severity": "critical"
        },
        {
            "pattern": r'GamePrefs\.Get\(',
            "warning": "GamePrefs.Get() may throw if not initialized at game stage.",
            "severity": "warning"
        },
        {
            "pattern": r'Thread\.Sleep',
            "warning": "Thread.Sleep in Harmony patches can cause game stutter. Consider async alternatives.",
            "severity": "warning"
        },
        {
            "pattern": r'XUiC_',
            "warning": "XUiC controllers may be null if the window hasn't been opened yet.",
            "severity": "critical"
        },
        {
            "pattern": r'typeof\(.*\)\.GetMethod\(',
            "warning": "GetMethod() via reflection returns null if the method doesn't exist. Always null-check.",
            "severity": "critical"
        },
        {
            "pattern": r'new HarmonyMethod\(typeof\(',
            "warning": "HarmonyMethod constructor throws if the method doesn't match. Use nameof() for safety.",
            "severity": "warning"
        },
        {
            "pattern": r'EntityAlive\.(?!\s*==\s*null)(?!\s*!=\s*null)',
            "warning": "Entity instances can be null if the entity has been despawned. Always check before access.",
            "severity": "critical"
        }
    ]

    def analyze_file(self, filepath: str) -> Dict:
        """Analyze a C# file for common issues."""
        try:
            with open(filepath, 'r', encoding='utf-8', errors='replace') as f:
                content = f.read()
        except Exception as e:
            return {"error": f"Cannot read file: {str(e)}"}
        
        issues = []
        warnings = []
        info = []
        
        # Check for Harmony attributes
        has_harmony_attr = any(ht in content for ht in self.HARMONY_TYPES)
        
        # Check dangerous patterns
        for pattern in self.DANGEROUS_PATTERNS:
            matches = re.finditer(pattern, content, re.MULTILINE)
            for match in matches:
                # Check if the line has a null check nearby
                line_start = max(0, content.rfind('\n', 0, match.start()))
                line_end = content.find('\n', match.end())
                line = content[line_start:line_end].strip()
                
                # Simple check for null guard pattern
                has_null_guard = any(
                    guard in line for guard in ['== null', '!= null', '?.', 'NullCheck', '?.Invoke']
                )
                
                if not has_null_guard:
                    issues.append({
                        "type": "potential_null_ref",
                        "pattern": pattern.strip('\\'),  # Clean up for display
                        "line": content[:match.start()].count('\n') + 1,
                        "context": line[:200],
                        "severity": "critical" if 'GetInstance' in pattern or 'GetEntity' in pattern else "warning"
                    })
        
        # Check known pitfalls
        for pitfall in self.KNOWN_PITFALLS:
            matches = re.finditer(pitfall["pattern"], content, re.MULTILINE)
            for match in matches:
                line_start = max(0, content.rfind('\n', 0, match.start()))
                line_end = content.find('\n', match.end())
                line = content[line_start:line_end].strip()
                
                entry = {
                    "type": "pattern_match",
                    "pattern": match.group()[:100],
                    "line": content[:match.start()].count('\n') + 1,
                    "context": line[:200],
                    "warning": pitfall["warning"],
                    "severity": pitfall["severity"]
                }
                
                if pitfall["severity"] == "critical":
                    issues.append(entry)
                else:
                    warnings.append(entry)
        
        # Basic structure checks
        if not has_harmony_attr:
            info.append("No Harmony attributes found. If this is a patch file, ensure [HarmonyPatch] is used.")
        
        # Check for try-catch blocks
        if 'try {' not in content and has_harmony_attr:
            warnings.append("No try-catch blocks found in Harmony patch. Consider wrapping patch logic in try-catch to prevent game crashes.")
        
        # Check for __instance parameter (standard Harmony pattern)
        if '__instance' not in content:
            info.append("No __instance parameter found. If this is a Prefix/Postfix, ensure __instance is declared correctly.")
        
        return {
            "file": os.path.basename(filepath),
            "path": os.path.relpath(filepath, WORKSPACE_ROOT),
            "is_harmony_patch": has_harmony_attr,
            "issues_found": len(issues) > 0 or len(warnings) > 0,
            "critical_issues": len([i for i in issues if i.get("severity") == "critical"]),
            "warnings": len(warnings),
            "issues": issues + warnings,
            "info": info
        }
    
    def analyze_dll_project(self, project_path: str) -> Dict:
        """Analyze an entire .dll project folder."""
        full_path = os.path.join(WORKSPACE_ROOT, project_path)
        if not os.path.exists(full_path):
            return {"error": f"Path not found: {project_path}"}
        
        results = []
        total_issues = 0
        total_critical = 0
        
        for root, dirs, files in os.walk(full_path):
            for f in files:
                if f.endswith('.cs'):
                    fp = os.path.join(root, f)
                    analysis = self.analyze_file(fp)
                    if "issues" in analysis:
                        total_issues += len(analysis.get("issues", []))
                        total_critical += analysis.get("critical_issues", 0)
                    results.append(analysis)
        
        return {
            "project": project_path,
            "files_analyzed": len(results),
            "total_issues": total_issues,
            "total_critical": total_critical,
            "file_results": results,
            "summary": f"Found {total_issues} issues ({total_critical} critical) across {len(results)} files."
        }


class CSharpQualityServer:
    """MCP Server for C# code quality analysis."""
    
    def __init__(self):
        self.analyzer = CSharpAnalyzer()
    
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
                    "name": "analyze_cs_file",
                    "description": "Analyze a single C# Harmony patch file for common 7D2D issues (null refs, missing guards, etc.)",
                    "inputSchema": {
                        "type": "object",
                        "properties": {
                            "filepath": {
                                "type": "string",
                                "description": "Path to .cs file (relative to workspace root or absolute)"
                            }
                        },
                        "required": ["filepath"]
                    }
                },
                {
                    "name": "analyze_dll_project",
                    "description": "Analyze all .cs files in a .dll project directory for common issues",
                    "inputSchema": {
                        "type": "object",
                        "properties": {
                            "project_path": {
                                "type": "string",
                                "description": "Path to project folder (e.g., '_DLL-Projects/MyMod')"
                            }
                        },
                        "required": ["project_path"]
                    }
                },
                {
                    "name": "get_harmony_best_practices",
                    "description": "Get 7D2D-specific Harmony patch best practices and patterns",
                    "inputSchema": {
                        "type": "object",
                        "properties": {}
                    }
                },
                {
                    "name": "validate_harmony_patch",
                    "description": "Validate a Harmony patch method signature and structure",
                    "inputSchema": {
                        "type": "object",
                        "properties": {
                            "patch_code": {
                                "type": "string",
                                "description": "The C# patch method code to validate (full method body)"
                            },
                            "patch_type": {
                                "type": "string",
                                "description": "Type of patch: 'prefix', 'postfix', 'transpiler', 'manual'",
                                "default": "prefix"
                            }
                        },
                        "required": ["patch_code"]
                    }
                },
                {
                    "name": "find_dll_projects",
                    "description": "List all .dll C# projects in the workspace",
                    "inputSchema": {
                        "type": "object",
                        "properties": {}
                    }
                }
            ]
        }
    
    def _handle_tool_call(self, params: Dict) -> Dict:
        tool_name = params.get("name", "")
        arguments = params.get("arguments", {})
        
        if tool_name == "analyze_cs_file":
            return self._analyze_cs_file(arguments)
        elif tool_name == "analyze_dll_project":
            return self._analyze_dll_project(arguments)
        elif tool_name == "get_harmony_best_practices":
            return self._get_harmony_best_practices(arguments)
        elif tool_name == "validate_harmony_patch":
            return self._validate_harmony_patch(arguments)
        elif tool_name == "find_dll_projects":
            return self._find_dll_projects(arguments)
        else:
            return self._error("tool_not_found", f"Unknown tool: {tool_name}")
    
    def _analyze_cs_file(self, args: Dict) -> Dict:
        filepath = args.get("filepath", "")
        
        # Try as relative to workspace, then absolute
        full_path = os.path.join(WORKSPACE_ROOT, filepath)
        if not os.path.exists(full_path):
            full_path = filepath
        if not os.path.exists(full_path):
            return self._error("not_found", f"File not found: {filepath}")
        
        result = self.analyzer.analyze_file(full_path)
        return {
            "content": [{"type": "text", "text": json.dumps(result, indent=2)}]
        }
    
    def _analyze_dll_project(self, args: Dict) -> Dict:
        project_path = args.get("project_path", "")
        result = self.analyzer.analyze_dll_project(project_path)
        return {
            "content": [{"type": "text", "text": json.dumps(result, indent=2)}]
        }
    
    def _get_harmony_best_practices(self, args: Dict) -> Dict:
        practices = {
            "title": "7D2D Harmony Patch Best Practices",
            "categories": [
                {
                    "name": "Null Safety",
                    "practices": [
                        "Always null-check __instance before accessing its properties",
                        "Use ?. (null-conditional) operator for chained property access",
                        "Wrap patch logic in try-catch blocks to prevent game crashes from exceptions",
                        "Check GameManager.Instance, World, and XUi instances before use - they may not be initialized yet",
                        "Entity references can become invalid mid-patch - always check entity state"
                    ]
                },
                {
                    "name": "Patch Structure",
                    "practices": [
                        "Use [HarmonyPatch(typeof(ClassName))] and [HarmonyPatch(\"MethodName\")] attributes for clarity",
                        "Prefix methods should return bool (false = skip original, true = run original)",
                        "Postfix methods should be void",
                        "Use __result for accessing/modifying return values in Postfix",
                        "Use __state to pass data between Prefix and Postfix of the same patch"
                    ]
                },
                {
                    "name": "Performance",
                    "practices": [
                        "Keep patches lightweight - they run on main thread",
                        "Avoid allocations in frequently-called patches (use __state for caching)",
                        "Don't use LINQ in hot paths (update/render callbacks)",
                        "Cache HarmonyMethod instances, don't create them per-call"
                    ]
                },
                {
                    "name": "Common 7D2D Pitfalls",
                    "practices": [
                        "XUiC controllers may not exist until the relevant window is opened",
                        "ItemValue instances may be Value == null in some states",
                        "Thread.Sleep in patches WILL cause game freezes",
                        "GetMethod via reflection requires exact method signature matching",
                        "EntityAlive instances can be null after entity death/despawn even if reference is held",
                        "Game date/time changes mid-frame - don't cache world time"
                    ]
                },
                {
                    "name": "IL/Transpiler Guidelines",
                    "practices": [
                        "Always use try-finally when using IL instructions to ensure cleanup",
                        "Label offsets must match exactly - verify with dnSpy/ILSpy",
                        "Test transpiler patches on both Debug and Release builds (IL differs)",
                        "Document which game version the IL was captured from"
                    ]
                }
            ]
        }
        
        return {
            "content": [{"type": "text", "text": json.dumps(practices, indent=2)}]
        }
    
    def _validate_harmony_patch(self, args: Dict) -> Dict:
        code = args.get("patch_code", "")
        patch_type = args.get("patch_type", "prefix")
        
        issues = []
        
        # Check basic structure
        if patch_type == "prefix":
            if "bool" not in code.split('{')[0] if '{' in code else code:
                issues.append("Prefix methods should return bool (true = run original, false = skip original)")
            if "__instance" not in code:
                issues.append("Prefix methods typically use __instance parameter for the patched class instance")
        
        elif patch_type == "postfix":
            if "__result" not in code:
                issues.append("Postfix methods typically use __result to access/modify the return value")
        
        # Check common issues
        if "try" not in code and len(code) > 100:
            issues.append("Consider wrapping patch logic in try-catch to prevent game crashes")
        
        if "null" not in code:
            issues.append("No null checks found - consider checking for null references")
        
        if "GameManager" in code and "Instance" in code and "== null" not in code and "!= null" not in code:
            issues.append("GameManager.Instance accessed without null check")
        
        # Check return type
        if patch_type == "prefix":
            if "return false" in code:
                issues.append("return false will skip the original method - ensure this is intentional")
            if "return true" in code:
                issues.append("return true will run the original method normally")
        
        return {
            "content": [{"type": "text", "text": json.dumps({
                "patch_type": patch_type,
                "valid": len(issues) == 0,
                "issues": issues if issues else "No obvious issues found.",
                "note": "This is a basic structural validation. For deeper analysis, use analyze_cs_file."
            }, indent=2)}]
        }
    
    def _find_dll_projects(self, args: Dict) -> Dict:
        dll_dir = os.path.join(WORKSPACE_ROOT, "_DLL-Projects")
        if not os.path.exists(dll_dir):
            return {
                "content": [{"type": "text", "text": json.dumps({
                    "found": False,
                    "message": "_DLL-Projects directory not found. Your C# projects may be elsewhere."
                }, indent=2)}]
            }
        
        projects = []
        for item in os.listdir(dll_dir):
            item_path = os.path.join(dll_dir, item)
            if os.path.isdir(item_path):
                cs_files = []
                for root, dirs, files in os.walk(item_path):
                    for f in files:
                        if f.endswith('.cs'):
                            cs_files.append(os.path.relpath(os.path.join(root, f), item_path))
                
                # Check for .csproj
                csproj = [f for f in os.listdir(item_path) if f.endswith('.csproj')]
                
                projects.append({
                    "name": item,
                    "cs_files_count": len(cs_files),
                    "cs_files": cs_files[:20],  # First 20 files
                    "has_project_file": len(csproj) > 0,
                    "project_file": csproj[0] if csproj else None
                })
        
        return {
            "content": [{"type": "text", "text": json.dumps({
                "total_projects": len(projects),
                "projects": projects
            }, indent=2)}]
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
    server = CSharpQualityServer()
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