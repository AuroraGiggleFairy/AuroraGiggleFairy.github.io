#!/usr/bin/env python3
"""
Vanilla Research MCP Server
Indexes and searches 7D2D vanilla game files (XML, Configs) to help understand
game mechanics for creating Harmony patches and matching vanilla patterns.
"""
import json
import sys
import os
import re
import subprocess
import xml.etree.ElementTree as ET
from typing import Dict, List, Optional, Any
from pathlib import Path

WORKSPACE_ROOT = r"c:\GitHub\7D2D-Mods"
# The game's Mods folder where vanilla configs can be found
GAME_MODS_PATH = r"c:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods"

class VanillaResearchServer:
    """MCP Server for researching vanilla 7D2D game code and configs."""
    
    def __init__(self):
        self.xml_cache: Dict[str, str] = {}
        self._index_vanilla_configs()
    
    def _index_vanilla_configs(self):
        """Walk game directories and cache found XML/Config files."""
        # Search the game's Data/Config directory
        game_path = r"c:\Program Files (x86)\Steam\steamapps\common\7 Days To Die"
        config_paths = [
            os.path.join(game_path, "7DaysToDieServer_Data", "config", "vanillaconfigs"),
            os.path.join(game_path, "Data", "Config"),
            os.path.join(game_path, "7DaysToDie_Data", "Config"),
        ]
        
        for path in config_paths:
            if os.path.exists(path):
                self._cache_xml_files(path)
        
        # Also index the game's Mods folder for reference
        if os.path.exists(GAME_MODS_PATH):
            self._cache_xml_files(GAME_MODS_PATH)
    
    def _cache_xml_files(self, directory: str):
        """Recursively cache XML files from a directory."""
        try:
            for root, dirs, files in os.walk(directory):
                for f in files:
                    if f.endswith('.xml'):
                        full_path = os.path.join(root, f)
                        rel_path = os.path.relpath(full_path, directory)
                        self.xml_cache[rel_path] = full_path
        except Exception:
            pass  # Directory might not exist
    
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
                    "name": "search_vanilla_xml",
                    "description": "Search vanilla 7D2D XML configs for keywords/patterns. Use this to find how the game implements specific mechanics.",
                    "inputSchema": {
                        "type": "object",
                        "properties": {
                            "query": {
                                "type": "string",
                                "description": "Search term to find in vanilla XML files (e.g., 'loot', 'container', 'interaction', 'buff', 'quest')"
                            },
                            "file_pattern": {
                                "type": "string",
                                "description": "Filter by filename pattern (e.g., 'blocks.xml', 'items.xml', '*.xml')",
                                "default": "*.xml"
                            },
                            "max_results": {
                                "type": "number",
                                "description": "Maximum number of results to return",
                                "default": 30
                            }
                        },
                        "required": ["query"]
                    }
                },
                {
                    "name": "list_vanilla_files",
                    "description": "List all indexed vanilla config files by category",
                    "inputSchema": {
                        "type": "object",
                        "properties": {
                            "category": {
                                "type": "string",
                                "description": "Filter by category: 'blocks', 'items', 'quests', 'buffs', 'ui', 'recipes', 'loot', 'materials', or 'all'",
                                "default": "all"
                            }
                        },
                        "required": []
                    }
                },
                {
                    "name": "read_vanilla_file",
                    "description": "Read the contents of a specific vanilla config file",
                    "inputSchema": {
                        "type": "object",
                        "properties": {
                            "filename": {
                                "type": "string",
                                "description": "Filename to read (e.g., 'blocks.xml', 'loot.xml')"
                            }
                        },
                        "required": ["filename"]
                    }
                },
                {
                    "name": "find_xml_structure",
                    "description": "Show the XML structure/template for a specific gametype (e.g., 'block', 'item', 'buff', 'quest') to understand required fields",
                    "inputSchema": {
                        "type": "object",
                        "properties": {
                            "gametype": {
                                "type": "string",
                                "description": "Game type to analyze: 'block', 'item', 'buff', 'quest', 'recipe', 'lootgroup', 'entity', 'ui'"
                            }
                        },
                        "required": ["gametype"]
                    }
                },
                {
                    "name": "compare_versions",
                    "description": "Compare snippets between game versions to track changes (use when you know two game versions)",
                    "inputSchema": {
                        "type": "object",
                        "properties": {
                            "xml_snippet_a": {
                                "type": "string",
                                "description": "First version snippet (paste from game files)"
                            },
                            "xml_snippet_b": {
                                "type": "string",
                                "description": "Second version snippet (paste from game files)"
                            }
                        },
                        "required": ["xml_snippet_a", "xml_snippet_b"]
                    }
                }
            ]
        }
    
    def _handle_tool_call(self, params: Dict) -> Dict:
        tool_name = params.get("name", "")
        arguments = params.get("arguments", {})
        
        if tool_name == "search_vanilla_xml":
            return self._search_vanilla_xml(arguments)
        elif tool_name == "list_vanilla_files":
            return self._list_vanilla_files(arguments)
        elif tool_name == "read_vanilla_file":
            return self._read_vanilla_file(arguments)
        elif tool_name == "find_xml_structure":
            return self._find_xml_structure(arguments)
        elif tool_name == "compare_versions":
            return self._compare_versions(arguments)
        else:
            return self._error("tool_not_found", f"Unknown tool: {tool_name}")
    
    def _search_vanilla_xml(self, args: Dict) -> Dict:
        query = args.get("query", "").lower()
        file_pattern = args.get("file_pattern", "*.xml")
        max_results = args.get("max_results", 30)
        
        results = []
        for rel_path, full_path in self.xml_cache.items():
            if not self._matches_pattern(rel_path, file_pattern):
                continue
            
            try:
                with open(full_path, 'r', encoding='utf-8', errors='replace') as f:
                    content = f.read()
                
                lines = content.split('\n')
                for i, line in enumerate(lines):
                    if query in line.lower():
                        # Get context (surrounding lines)
                        start = max(0, i - 2)
                        end = min(len(lines), i + 3)
                        context_lines = lines[start:end]
                        
                        results.append({
                            "file": rel_path,
                            "line": i + 1,
                            "context": '\n'.join(context_lines).strip()
                        })
                        
                        if len(results) >= max_results:
                            break
            except Exception:
                continue
            
            if len(results) >= max_results:
                break
        
        if not results:
            return {
                "content": [{"type": "text", "text": json.dumps({
                    "query": query,
                    "found": False,
                    "message": f"No results found for '{query}'. The vanilla configs may not be at the expected path.",
                    "search_paths": [
                        r"c:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config",
                        r"c:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\7DaysToDie_Data\Config"
                    ],
                    "alternative": "You can also use 'list_vanilla_files' to see what files are indexed, or manually provide snippets for comparison with 'compare_versions'."
                }, indent=2)}]
        
        
        # Group results by file
        by_file = {}
        for r in results:
            file = r["file"]
            if file not in by_file:
                by_file[file] = []
            by_file[file].append({
                "line": r["line"],
                "context": r["context"]
            })
        
        return {
            "content": [{"type": "text", "text": json.dumps({
                "query": query,
                "found": True,
                "total_matches": len(results),
                "files_with_matches": len(by_file),
                "results_by_file": dict(list(by_file.items())[:10])  # Limit to 10 files
            }, indent=2)}]
        }
    
    def _matches_pattern(self, path: str, pattern: str) -> bool:
        """Simple glob matching for filenames."""
        if pattern == "*.xml":
            return True
        filename = os.path.basename(path)
        if pattern.startswith("*"):
            return filename.endswith(pattern[1:])
        if pattern.endswith("*"):
            return filename.startswith(pattern[:-1])
        return filename == pattern
    
    def _list_vanilla_files(self, args: Dict) -> Dict:
        category = args.get("category", "all").lower()
        
        # Categorize files
        categories = {
            "blocks": [],
            "items": [],
            "recipes": [],
            "loot": [],
            "buffs": [],
            "quests": [],
            "ui": [],
            "materials": [],
            "other": []
        }
        
        for rel_path in sorted(self.xml_cache.keys()):
            filename = rel_path.lower()
            if any(x in filename for x in ['block', 'master']):
                categories["blocks"].append(rel_path)
            elif 'item' in filename:
                categories["items"].append(rel_path)
            elif 'recipe' in filename:
                categories["recipes"].append(rel_path)
            elif 'loot' in filename:
                categories["loot"].append(rel_path)
            elif 'buff' in filename:
                categories["buffs"].append(rel_path)
            elif 'quest' in filename:
                categories["quests"].append(rel_path)
            elif any(x in filename for x in ['ui', 'window', 'xui']):
                categories["ui"].append(rel_path)
            elif 'material' in filename:
                categories["materials"].append(rel_path)
            else:
                categories["other"].append(rel_path)
        
        if category != "all":
            filtered = {category: categories.get(category, [])}
        else:
            filtered = categories
        
        total_files = sum(len(v) for v in categories.values())
        
        return {
            "content": [{"type": "text", "text": json.dumps({
                "total_indexed_files": total_files,
                "categories": filtered,
                "note": "Files are from the game's Config directory if found, otherwise from the Mods folder."
            }, indent=2)}]
        }
    
    def _read_vanilla_file(self, args: Dict) -> Dict:
        filename = args.get("filename", "")
        
        # Search for this file in the cache
        for rel_path, full_path in self.xml_cache.items():
            if os.path.basename(rel_path).lower() == filename.lower():
                try:
                    with open(full_path, 'r', encoding='utf-8', errors='replace') as f:
                        content = f.read()
                    
                    lines = content.split('\n')
                    return {
                        "content": [{"type": "text", "text": json.dumps({
                            "file": rel_path,
                            "full_path": full_path,
                            "size_kb": round(os.path.getsize(full_path) / 1024, 1),
                            "line_count": len(lines),
                            "content": content[:100000]  # Limit to 100k chars
                        }, indent=2)}]
                    }
                except Exception as e:
                    return self._error("read_error", str(e))
        
        return {
            "content": [{"type": "text", "text": json.dumps({
                "found": False,
                "message": f"File '{filename}' not found in indexed vanilla configs.",
                "indexed_files": list(self.xml_cache.keys())[:30]
            }, indent=2)}]
        }
    
    def _find_xml_structure(self, args: Dict) -> Dict:
        gametype = args.get("gametype", "").lower()
        
        # Search for the first file that contains this gametype definition
        structures = {
            "block": {
                "description": "Blocks XML structure for defining placeable objects",
                "template": """<blocks>
  <block id="" name="">
    <property name="DisplayType" value=""/>
    <property name="DescriptionKey" value=""/>
    <property name="Material" value=""/>
    <property name="MaxDamage" value="500"/>
    <property name="Shape" value="ModelEntity"/>
    <property name="Mesh" value=""/>
    <property name="Texture" value=""/>
    <drop event="Destroy" name=""/>
    <property name="IsTerrainDecoration" value="True"/>
    <property name="SortOrder1" value=""/>
  </block>
</blocks>"""
            },
            "item": {
                "description": "Items XML structure for defining items, tools, weapons",
                "template": """<items>
  <item id="" name="">
    <property name="DisplayType" value=""/>
    <property name="DescriptionKey" value=""/>
    <property name="MaxDamage" value="100"/>
    <property name="Stacknumber" value="1"/>  <!-- -1 for unlimited -->
    <property name="EconomicValue" value=""/>
    <property name="FuelValue" value="0"/>
    <property name="Degradation" value="true"/>
    <property name="SoundJunk" value=""/>
    <effect_group>
      <passive_effect name="..." />
    </effect_group>
  </item>
</items>"""
            },
            "buff": {
                "description": "Buffs XML structure for status effects and buffs/debuffs",
                "template": """<buffs>
  <buff id="" name="">
    <display_value name="..." value=""/>
    <stack_type value="replace"/>  <!-- or 'ignore', 'stack' -->
    <duration value="0"/>  <!-- 0 = infinite -->
    <remove_other_buffs name=""/>
    <effect_group>
      <!-- passive effects here -->
    </effect_group>
    <actions>
      <!-- triggered actions here -->
    </actions>
  </buff>
</buffs>"""
            },
            "quest": {
                "description": "Quests XML structure for defining quests and objectives",
                "template": """<quests>
  <quest id="" name="">
    <property name="quest_type" value=""/>
    <property name="title_key" value=""/>
    <property name="subtitle" value=""/>
    <property name="description_key" value=""/>
    <objective type="" id="">
      <!-- objective details -->
    </objective>
    <reward type="">
      <!-- reward details -->
    </reward>
    <modifiers>
      <!-- quest modifiers -->
    </modifiers>
  </quest>
</quests>"""
            },
            "recipe": {
                "description": "Recipes XML structure for crafting recipes",
                "template": """<recipes>
  <recipe name="" count="1" scrapable="False">
    <ingredient name="" count=""/>
    <ingredient name="" count=""/>
  </recipe>
</recipes>"""
            },
            "lootgroup": {
                "description": "Loot XML structure for loot groups and containers",
                "template": """<lootcontainers>
  <lootcontainer id="" name="" count="0" size="4,6" loot_quality_template="" sound_open="">
    <item name=""/>
  </lootcontainer>
  <lootgroup name="" count="1">
    <item name="" count="" prob=""/>
  </lootgroup>
</lootcontainers>"""
            },
            "entity": {
                "description": "Entities XML structure for entity definitions",
                "template": """<entity_classes>
  <entity_class id="" name="">
    <property name="Mesh" value=""/>
    <property name="DropMesh" value=""/>
    <property name="HandMesh" value=""/>
    <property name="MountedWeapon" value=""/>
    <property name="AvatarController" value=""/>
    <property name="IsEnemyEntity" value="false"/>
    <property name="LootDropEntityClass" value=""/>
  </entity_class>
</entity_classes>"""
            },
            "ui": {
                "description": "XUI window structure for UI windows and panels",
                "template": """<window name="" style="">
  <rect pos="" width="" height="">
    <label depth="" pos="" width="" height="" font="" text_key="" />
    <sprite depth="" pos="" width="" height="" name="" color="" />
    <!-- controls, triggers, etc -->
  </rect>
  <trigger on_activated="" />
</window>"""
            }
        }
        
        if gametype in structures:
            s = structures[gametype]
            return {
                "content": [{"type": "text", "text": json.dumps(s, indent=2)}]
            }
        
        return {
            "content": [{"type": "text", "text": json.dumps({
                "error": f"Unknown gametype: {gametype}",
                "available_types": list(structures.keys())
            }, indent=2)}]
        }
    
    def _compare_versions(self, args: Dict) -> Dict:
        snippet_a = args.get("xml_snippet_a", "")
        snippet_b = args.get("xml_snippet_b", "")
        
        # Simple diff: find lines that differ
        lines_a = snippet_a.split('\n')
        lines_b = snippet_b.split('\n')
        
        changes = []
        max_lines = max(len(lines_a), len(lines_b))
        for i in range(max_lines):
            line_a = lines_a[i] if i < len(lines_a) else ""
            line_b = lines_b[i] if i < len(lines_b) else ""
            
            if line_a.strip() != line_b.strip():
                changes.append({
                    "line": i + 1,
                    "version_a": line_a,
                    "version_b": line_b
                })
        
        return {
            "content": [{"type": "text", "text": json.dumps({
                "line_matches": max_lines - len(changes),
                "differences": len(changes),
                "changes": changes[:30]  # Limit to 30 changes
            }, indent=2)}]
        }
    
    def _handle_resources_list(self) -> Dict:
        # Expose vanilla files as resources
        resources = []
        for rel_path in sorted(self.xml_cache.keys())[:50]:  # Limit to 50
            resources.append({
                "uri": f"vanilla://{rel_path}",
                "name": rel_path,
                "mimeType": "application/xml"
            })
        return {"resources": resources}
    
    def _handle_resource_read(self, params: Dict) -> Dict:
        uri = params.get("uri", "")
        # Extract path from uri like "vanilla://blocks.xml"
        path_part = uri.replace("vanilla://", "")
        
        for rel_path, full_path in self.xml_cache.items():
            if rel_path == path_part or os.path.basename(rel_path) == path_part:
                try:
                    with open(full_path, 'r', encoding='utf-8', errors='replace') as f:
                        content = f.read()
                    return {
                        "contents": [{
                            "uri": uri,
                            "mimeType": "application/xml",
                            "text": content[:50000]
                        }]
                    }
                except Exception as e:
                    return self._error("read_error", str(e))
        
        return self._error("not_found", f"Resource not found: {uri}")
    
    def _error(self, code: str, message: str) -> Dict:
        return {
            "content": [{"type": "text", "text": json.dumps({"error": code, "message": message})}],
            "isError": True
        }


def main():
    server = VanillaResearchServer()
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