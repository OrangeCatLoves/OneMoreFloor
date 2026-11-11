"""Inventory every changed imported name, pose and bounds; derive clearances."""
from pathlib import Path
import json,math
root=Path(__file__).resolve().parents[2];out=root/'art-source/renders/trivia/round-05'
before={r['name']:r for r in json.loads((out/'before-inventory.json').read_text(encoding='utf8'))}
after={r['name']:r for r in json.loads((out/'after-inventory.json').read_text(encoding='utf8'))}
def different(a,b):return max(abs(x-y) for x,y in zip(a,b))>.001
def fmt(v):return '('+', '.join(f'{n:.3f}' for n in v)+')'
changes=[]
for name in sorted(before.keys()|after.keys()):
    if name not in before:changes.append({'name':name,'change':'added','position':after[name]['position']});continue
    if name not in after:changes.append({'name':name,'change':'removed','position':before[name]['position']});continue
    a,b=before[name],after[name];kinds=[]
    if different(a['position'],b['position']):kinds.append('pivot moved')
    if different(a['forward'],b['forward']) or different(a['up'],b['up']):kinds.append('axes rotated')
    if 'bounds_min' in a and 'bounds_min' in b and (different(a['bounds_min'],b['bounds_min']) or different(a['bounds_max'],b['bounds_max'])):kinds.append('geometry bounds changed')
    if a.get('triangles')!=b.get('triangles'):kinds.append('topology changed')
    if kinds:changes.append({'name':name,'change':', '.join(kinds),'before':a,'after':b})
(out/'name-changes.json').write_text(json.dumps(changes,indent=2),encoding='utf8')
lines=['# Round 2 imported object contract changes','','Names not listed retain their imported pose, bounds and triangle count to 1 mm. Mesh rotations baked into vertices are described by the numbered review; triangle count and bounds alone do not prove identical topology. Every row comes from the saved Unity review scene, including its open lift-door display pose.','','Explicit replacements: `SM_Trivia_GoldTieback` becomes `SM_Trivia_GoldTieback_L` and `_R`; centre `COL_Trivia_Step*` becomes corner `COL_Trivia_StageStep*`; the separate old balcony collision decks become one continuous static `COL_Trivia_BalconyDeck`. Original pan/tilt names stay; optical-axis children are added. Other removals/additions are listed below.','','| Name | Change | Before pivot | After pivot |','|---|---|---|---|']
for r in changes:
    a=r.get('before',{}).get('position',r['position'] if r['change']=='removed' else None)
    b=r.get('after',{}).get('position',r['position'] if r['change']=='added' else None)
    lines.append(f"| `{r['name']}` | {r['change']} | {fmt(a) if a else '-'} | {fmt(b) if b else '-'} |")
(out/'name-changes.md').write_text('\n'.join(lines)+'\n',encoding='utf8')
seat=next(r for n,r in after.items() if n.startswith('SM_Trivia_Seat_R00'))
depth=seat['bounds_max'][2]-seat['bounds_min'][2]
ground=[r for n,r in after.items() if n.startswith('SM_Trivia_Seat_R')]
outer=max(max(abs(r['bounds_min'][0]),abs(r['bounds_max'][0])) for r in ground)
rear=[r for n,r in after.items() if n.startswith('SM_Trivia_UpperSeat_')]
rear_back=max(r['bounds_max'][2] for r in rear)
upper_outer=max(max(abs(r['bounds_min'][0]),abs(r['bounds_max'][0])) for r in rear)
clearance={'ground_row_pitch_m':2,'ground_seat_depth_m':depth,'row_clearance_m':2-depth,
 'narrowest_side_route_at_gallery_stair_m':10.95-.035-outer,'central_aisle_m':3,
 'rear_cross_aisle_m':23.4-21.3,'rear_gallery_behind_seats_m':25.64-rear_back,
 'side_gallery_between_guard_and_pilaster_m':14.2-12.535,
 'upper_seats_to_stair_guard_m':10.915-upper_outer,
 'gallery_bottom_landing_depth_m':25.8-24.4,
 'third_person_clear_distance_behind_objectives_m':4,
 'note':'Dimensions are clear geometric corridors. Separate Unity motor test exercises each route; crowd comfort remains the owner playtest.'}
(out/'clearances.json').write_text(json.dumps(clearance,indent=2),encoding='utf8')
assert all(clearance[k]>=1.2 for k in ['row_clearance_m','narrowest_side_route_at_gallery_stair_m','rear_cross_aisle_m','rear_gallery_behind_seats_m','side_gallery_between_guard_and_pilaster_m','gallery_bottom_landing_depth_m','upper_seats_to_stair_guard_m'])
print(f'{len(changes)} changed/added/removed imported objects; aisle clearances pass')
