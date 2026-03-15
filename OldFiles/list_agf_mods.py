import os

# Paths (edit if needed)
PUBLISH_READY_DIR = os.path.abspath('.')
IN_PROGRESS_DIR = os.path.join(PUBLISH_READY_DIR, '_In-Progress')
GAME_MODS_DIR = r'C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods'

# Only list mods starting with these prefixes
MOD_PREFIXES = ('AGF-', 'zzzAGF-')

def list_mods(directory, label, output_lines):
    output_lines.append(f'\n{label}:')
    if not os.path.isdir(directory):
        output_lines.append('  (Folder not found)')
        return
    mods = [d for d in os.listdir(directory)
            if os.path.isdir(os.path.join(directory, d))
            and d.startswith(MOD_PREFIXES)]
    if mods:
        for mod in sorted(mods):
            output_lines.append(f'  {mod}')
    else:
        output_lines.append('  (No matching mods found)')

def main():
    output_lines = []
    list_mods(PUBLISH_READY_DIR, 'Publish-Ready Mods', output_lines)
    list_mods(IN_PROGRESS_DIR, 'In-Progress Mods', output_lines)
    list_mods(GAME_MODS_DIR, 'Game Mods (AGF only)', output_lines)
    output = '\n'.join(output_lines)
    print(output)
    # Write to file with prefix
    with open('LIST-AGF-Mods.txt', 'w', encoding='utf-8') as f:
        f.write(output)
    print('\nMod list also saved to LIST-AGF-Mods.txt')

if __name__ == '__main__':
    main()
