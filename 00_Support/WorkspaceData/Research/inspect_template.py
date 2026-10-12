import sys
import UnityPy

template_path = r'01_Draft\AGF-V3-BOTH-BiomePaints-v0.0.1\Resources\Atlas.template.unity3d'
env = UnityPy.load(template_path)

print("Objects in template:")
for obj in env.objects:
    data = obj.read()
    name = getattr(data, 'name', getattr(data, 'm_Name', ''))
    print(f"  {obj.type.name:<15} {name}")
