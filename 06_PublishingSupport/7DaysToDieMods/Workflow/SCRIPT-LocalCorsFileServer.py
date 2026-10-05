"""Serve Workflow/_listing_upload with CORS so the Cursor browser can File-assign png/zip.

Keep this script. Only the files copied into _listing_upload are scratch.
Default: http://127.0.0.1:8765/<filename>
"""
from http.server import ThreadingHTTPServer, SimpleHTTPRequestHandler
from pathlib import Path
import os

root = Path(__file__).resolve().parent / "_listing_upload"
root.mkdir(exist_ok=True)


class CORSHandler(SimpleHTTPRequestHandler):
    def end_headers(self):
        self.send_header("Access-Control-Allow-Origin", "*")
        self.send_header("Access-Control-Allow-Methods", "GET, OPTIONS")
        self.send_header("Access-Control-Allow-Headers", "*")
        super().end_headers()

    def do_OPTIONS(self):
        self.send_response(200)
        self.end_headers()


if __name__ == "__main__":
    os.chdir(root)
    server = ThreadingHTTPServer(("127.0.0.1", 8765), CORSHandler)
    print("serving", root, "on 8765")
    server.serve_forever()
