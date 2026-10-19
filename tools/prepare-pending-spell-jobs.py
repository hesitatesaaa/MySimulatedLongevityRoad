"""Export missing spell stage prompts without re-generating installed stages."""
import ast,json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
tree=ast.parse((ROOT/'tools/write-web-art-guide.py').read_text(encoding='utf-8-sig'))
design=next(ast.literal_eval(node.value) for node in tree.body if isinstance(node,ast.Assign) and any(isinstance(t,ast.Name) and t.id=='spell_design' for t in node.targets))
manifest=json.loads((ROOT/'resources/beast-spell-effects.manifest.json').read_text(encoding='utf-8-sig'))
jobs=[]
for entry in manifest['assets']:
 if (ROOT/entry['path']).exists():continue
 name,*stages=design[entry['spell']];stage=stages[{'cast':0,'travel':1,'hit':2}[entry['phase']]]
 rows=(entry['frames']+3)//4;ratio={2:'2:1',4:'1:1',6:'2:3'}[rows]
 phase_motion={'cast':'Chronological sequence of small scattered sparks or sigil fragments gathering and condensing into a completed compact form.',
               'travel':'A coherent looping motion or a gradually unfolding array. For horizontal projectiles, point RIGHT in every frame, tail LEFT, fixed projectile core and scale. For ground arrays, fixed slight top-down perspective and stable ground-center pivot.',
               'hit':'One continuous impact sequence: contact flash, expansion, peak, fading and sparse final particles. Each frame is a distinct successive instant. No repeated impacts and no repeated full-intensity peak.'}[entry['phase']]
 prompt=f"Create ONE independent production spell-animation atlas for Chinese cultivation pixel game: {entry['spell']} {name}, ONLY the {entry['phase']} stage. Visual design: {stage}. {phase_motion} Exactly {entry['frames']} frames, FOUR equal columns and {rows} equal rows, aspect ratio {ratio}, chronological left-to-right then top-to-bottom. Equal square cells, exact fixed center pivot. ALL content including faint glow must fit inside the central 70% of EACH cell, leaving 15% truly empty transparent margin on each side; no clipping, no cross-cell glow or stray pixels. The complete grid must fill the canvas evenly with no extra outer title area. Crisp shaded pixel-art silhouette, restrained tiny highlights and clean palette, Chinese xianxia spell vocabulary. Each spell has its own specific geometry; no recoloring a generic explosion. No rocky chunks except earth-themed effects. No people, landscapes, readable text, numbering, grid lines, borders, watermark, or painted checkerboard. TRUE transparent alpha PNG. Target installation cells 192x192, sheet {entry['width']}x{entry['height']}; native larger output is fine if the exact grid and aspect are maintained."
 jobs.append(dict(id=entry['id'],kind='spell',references=[],prompt=prompt))
target=ROOT/'output/beast-overhaul/remaining-spell-jobs.json';target.write_text(json.dumps(jobs,ensure_ascii=True,indent=2),encoding='utf-8')
print(json.dumps(jobs,ensure_ascii=True))
