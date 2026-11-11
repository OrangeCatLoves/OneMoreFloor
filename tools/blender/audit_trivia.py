"""Validate the saved art contract and write reproducible lighting/scale reports."""
import bpy,bmesh,json,math
import numpy as np
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parents[2];source=root/'art-source/trivia'
bpy.ops.wm.open_mainfile(filepath=str(source/'Trivia_Round01.blend'))
scene=bpy.context.scene;deps=bpy.context.evaluated_depsgraph_get()
problems=[];meshes=[o for o in scene.objects if o.type=='MESH' and not o.name.startswith('COL_')]
counts={};groups={}
for o in meshes:
    count=sum(len(p.vertices)-2 for p in o.evaluated_get(deps).data.polygons);counts[o.name]=count
    category='Seats' if 'Seat' in o.name else 'Flowers' if 'FlowerUrn' in o.name else 'Curtains' if any(x in o.name for x in ['Curtain','Drape','Fringe']) else 'Podiums' if 'Podium' in o.name else 'Architecture and detail'
    groups[category]=groups.get(category,0)+count
for o in scene.objects:
    if o.type=='MESH' and (any(abs(v-1)>.00001 for v in o.scale) or any(abs(v)>.00001 for v in o.rotation_euler)):
        problems.append(o.name+': transforms not applied')
    if o.name.startswith('COL_'):
        bm=bmesh.new();bm.from_mesh(o.data)
        if not all(e.is_manifold for e in bm.edges):problems.append(o.name+': collision mesh is not closed')
        if bm.calc_volume(signed=True)<=0:problems.append(o.name+': collision normals point inward')
        bm.free()
for name in ['MK_Host','MK_Exit']+[f'MK_Spawn_{i:02}' for i in range(1,5)]+[f'MK_Objective_{i:02}' for i in range(1,5)]:
    if name not in scene.objects:problems.append('Missing '+name)
screens=[o for o in meshes if o.name in ('SM_Trivia_QuestionScreen','SM_Trivia_PlayerScreen') or o.name.startswith('SM_Trivia_PodiumScreen_')]
if len(screens)!=6:problems.append('Expected six live screens')
for o in screens:
    uv=o.data.uv_layers.active
    if not uv or any(not(-.0001<=v.uv.x<=1.0001 and -.0001<=v.uv.y<=1.0001) for v in uv.data):problems.append(o.name+': invalid screen UV')
if len([o for o in meshes if '_Button_' in o.name])!=16:problems.append('Expected 16 independent buttons')
if len([o for o in meshes if o.name.startswith('SM_Trivia_MarqueeChase_')])!=3:problems.append('Expected three marquee chase sets')
for i in range(1,13):
    pan=scene.objects.get(f'MK_Trivia_Spotlight_{i:02}_Pan');tilt=scene.objects.get(f'MK_Trivia_Spotlight_{i:02}_Tilt');head=scene.objects.get(f'SM_Trivia_Spotlight_{i:02}_Head')
    if not pan or not tilt or not head or tilt.parent!=pan or head.parent!=tilt:problems.append(f'Spotlight {i}: invalid pan/tilt hierarchy')
frustum={}
for i in range(1,4):
    camera=scene.objects[f'CAM_Trivia_Reference_{i:02}']
    projection=camera.calc_matrix_camera(deps,x=scene.render.resolution_x,y=scene.render.resolution_y)
    total=0
    for o in meshes:
        m=projection@camera.matrix_world.inverted()@o.matrix_world
        points=[m@Vector((*corner,1)) for corner in o.bound_box]
        outside=any(all(sign*p[axis]>p.w for p in points) for axis in range(3) for sign in [-1,1])
        if not outside:total+=counts[o.name]
    frustum[f'view_{i}']=total
sizes=json.loads((root/'docs/sizes.json').read_text())
scale={'player_height_m':sizes['player']['height'],'player_diameter_m':2*sizes['player']['radius'],
 'lift_clear_opening_m':[sizes['elevator']['door']['width'],sizes['elevator']['door']['height']],
 'head_clearance_m':sizes['elevator']['door']['height']-sizes['player']['height'],
 'podium_clear_gap_m':3.2-1.46*.85,'stage_riser_m':sizes['stairs']['stepHeight'],
 'stage_tread_m':sizes['stairs']['stepDepth'],'steps_width_m':sizes['stairs']['width'],
 'cab_size_m':sizes['elevator']['interior'],
 'note':'Geometric checks only. The separate Unity smoke test walks the lift-to-stage route; camera comfort and crowd movement require human playtesting.'}
report={'problems':problems,'triangles_total':sum(counts.values()),'triangles_by_category':groups,
 'camera_frustum_triangles_upper_bound_including_occluded_geometry':frustum,
 'render_objects':len(meshes),'unique_meshes':len({o.data.name for o in meshes}),
 'material_count':len([m for m in bpy.data.materials if m.name.startswith('M_Trivia_')]),'scale':scale}
# A successful bake call can still write an empty cubemap when the imported
# scenery lacks ReflectionProbeStatic. Check actual HDR pixels when available.
probes={}
for path in (root/'Assets/_OneMoreFloor/Art/Floors/Trivia/Review').glob('*_Reflections.exr'):
    pixels=np.asarray(bpy.data.images.load(str(path)).pixels,dtype=np.float32).reshape(-1,4)[:,:3]
    span=float(np.ptp(pixels,axis=0).max())
    probes[path.name]={'rgb_min':pixels.min(axis=0).tolist(),'rgb_max':pixels.max(axis=0).tolist(),'maximum_channel_range':span,'nonempty':span>.01,'finite':bool(np.isfinite(pixels).all()),'peak_below_review_limit':bool(pixels.max()<500)}
    if span<=.01:problems.append(path.name+': reflection bake contains only a flat background')
    if not np.isfinite(pixels).all() or pixels.max()>=500:problems.append(path.name+': reflection bake contains nonfinite or implausibly bright HDR values')
report['local_review_reflection_probes']=probes
(source/'blender-validation.json').write_text(json.dumps(report,indent=2))
lights=json.loads((source/'lighting.json').read_text())
lines=['# Trivia lighting sheet — round 2','','Source: `art-source/trivia/Trivia_Round01.blend`, EEVEE. Every actual Blender light has an exported marker. `lighting.json` carries exact positions and directions in both coordinate systems.','','## Mood and engine transfer','','Keep the stage warm and bright, the hall darker, and purple restricted to the stage fill and selected spots. Preserve the colour of the red velvet rather than raising the exposure until it washes out. Use soft highlight bloom sparingly. The draft Unity screenshot is an import check with approximate realtime lights, not a match to the final lighting.','','Blender watts are not Unity intensity values. Relative intensity below uses 1000 Blender W as 1.0; tune Unity after baking indirect light. Area lights should be baked. Bake wall lanterns and podium spill where possible; keep only selected stage spots realtime. The three marquee meshes have separate emissive sets for chasing; do not add a realtime light to every bulb. Emissive strips, columns and bulb materials share `M_Trivia_Bulb`.','','Use the supplied direction vector for a Unity `LookRotation`; do not assume that an imported FBX empty\'s local forward axis is the light direction. Add reflection probes covering the stage and hall, then use URP bloom and a restrained tone curve. Cloth uses low-F0 URP specular maps; wood uses Complex Lit clearcoat. Blender velvet sheen still has no exact URP counterpart. No light beams or confetti are included.','','## Lights','','| Marker | Type | Hex | Relative intensity | Cone | Unity position (m) | Purpose |','|---|---|---|---:|---:|---|---|']
for l in lights:
    pos=', '.join(f'{v:.2f}' for v in l['position_unity'])
    target=l.get('target_blender')
    aim=f'; target Unity ({-target[0]:.2f}, {target[2]:.2f}, {-target[1]:.2f})' if target else ''
    lines.append(f"| `{l['name']}` | {l['type']} | {l['color']} | {l['relative_intensity']:.3f} | {l['cone_degrees'] or '—'} | {pos} | {l['purpose']}{aim} |")
lines += ['', '## Review setup and limitations', '',
 'Final reference renders use EEVEE with 192 samples at 3344 x 1882, resolved to 1672 x 941, AgX Medium High Contrast, exposure +0.6 and restrained Fog Glow. Shadow pool: 1024 MB; resolution scale: 0.75. Only stage spots plus the main stage/audience area keys cast shadows; other sources provide fill. Exact per-light flags are in `lighting.json`. The refinement reduces broad fill, narrows stage spots to 44 degrees and reserves purple as a secondary colour.', '',
 f"The current Unity art review uses {len(lights)} light markers: {sum(l['type']=='Area' for l in lights)} baked areas, {sum(l['type']=='Point' for l in lights)} baked points and {sum(l['type']=='Spot' for l in lights)} realtime stage spots. Each area also supplies a restrained realtime highlight spot. Four sources cast realtime shadows. Current lightmap/probe counts are in `art-source/trivia/baked-lighting-validation.json`. It uses 4x MSAA plus High SMAA without changing shared graphics assets. The twelve physical directional fixtures now export with verified lens normals, pivots and light positions; diffuse sconces have no directional lens.", '',
 'The round-04 engine comparisons capture the actual review scene at the three source cameras. Separately baked cubemaps are serialized as Custom probe textures, with the stage map also assigned as the indoor default reflection. Reopening the scene is validated: a direct bakedTexture assignment previously vanished and exposed the default outdoor reflection. Probe captures disable reflection feedback; EXRs import as linear, uncompressed HDR and are audited for finite, bounded pixels. Current review intensity divisors are 35 for points, 18 followed by a 0.12 baked-area multiplier (plus 0.65 for each derived realtime highlight spot), and 24 for stage spots; these are art-directed review values, not a unit conversion.', '',
 'The measured FBX position conversion is Blender `(x, y, z)` to Unity `(-x, z, -y)`. Unity validation compares every imported light marker with this data. The EEVEE and Unity shading models differ, so the lighting match remains work for the integrated scene.']
(root/'art-source/renders/trivia/lighting_sheet.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
print(json.dumps(report,indent=2))
if problems:raise RuntimeError('Art contract failed: '+'; '.join(problems))
