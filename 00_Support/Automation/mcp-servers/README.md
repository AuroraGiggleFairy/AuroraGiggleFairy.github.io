# MCP Servers (parked / unused)

Experimental Cursor MCP server stubs. They do **nothing** unless registered in
an external Cursor MCP config and started — they are not used by `00_dispatch.py / RUN-*.bat`,
any `RUN-*.bat`, or the mod lifecycle pipeline.

| Script | Intended role |
|---|---|
| `script_manager_server.py` | Index / analyze Python scripts |
| `publication_server.py` | Publish validation helpers (overlaps real Nexus tooling elsewhere) |
| `csharp_quality_server.py` | Harmony / C# pattern checks |
| `vanilla_research_server.py` | Search vanilla game configs |
| `knowledge/` | Research notes the servers (or you) can reference |

Safe to ignore until you want to learn MCP. This folder may move later
(e.g. under `00_Support/WorkspaceData` if treated as research/access tools).
