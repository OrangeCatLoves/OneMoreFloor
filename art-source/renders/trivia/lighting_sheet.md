# Trivia lighting sheet — round 01

Source: `art-source/trivia/Trivia_Round01.blend`, EEVEE. Every actual Blender light has an exported marker. `lighting.json` carries exact positions and directions in both coordinate systems.

## Mood and engine transfer

Keep the stage warm and bright, the hall darker, and purple restricted to the stage fill and selected spots. Preserve the colour of the red velvet rather than raising the exposure until it washes out. Use soft highlight bloom sparingly. The draft Unity screenshot is an import check with approximate realtime lights, not a match to the final lighting.

Blender watts are not Unity intensity values. Relative intensity below uses 1000 Blender W as 1.0; tune Unity after baking indirect light. Area lights should be baked. Bake wall lanterns and podium spill where possible; keep only selected stage spots realtime. The three marquee meshes have separate emissive sets for chasing; do not add a realtime light to every bulb. Emissive strips, columns and bulb materials share `M_Trivia_Bulb`.

Use the supplied direction vector for a Unity `LookRotation`; do not assume that an imported FBX empty's local forward axis is the light direction. Add reflection probes covering the stage and hall, then use URP bloom and a restrained tone curve. Cloth uses low-F0 URP specular maps; wood uses Complex Lit clearcoat. Blender velvet sheen still has no exact URP counterpart. No light beams or confetti are included.

## Lights

| Marker | Type | Hex | Relative intensity | Cone | Unity position (m) | Purpose |
|---|---|---|---:|---:|---|---|
| `MK_Light_Point_01` | Point | #FFCF83 | 0.002 | — | 4.80, 0.85, 3.18 | Subtle podium practical spill on polished wood; bake |
| `MK_Light_Point_02` | Point | #FFCF83 | 0.002 | — | 1.60, 0.85, 3.18 | Subtle podium practical spill on polished wood; bake |
| `MK_Light_Point_03` | Point | #FFCF83 | 0.002 | — | -1.60, 0.85, 3.18 | Subtle podium practical spill on polished wood; bake |
| `MK_Light_Point_04` | Point | #FFCF83 | 0.002 | — | -4.80, 0.85, 3.18 | Subtle podium practical spill on polished wood; bake |
| `MK_Light_Area_05` | Area | #FFE1AD | 0.180 | — | -0.00, 5.60, -3.95 | Aggregate marquee bounce; bulbs use three emissive chase sets |
| `MK_Light_Spot_06` | Spot | #FFE0AD | 0.650 | 44 | 8.00, 7.84, -1.00 | Warm stage pool |
| `MK_Light_Spot_07` | Spot | #FFE0AD | 0.650 | 44 | 5.70, 7.84, -1.00 | Warm stage pool |
| `MK_Light_Spot_08` | Spot | #B78DFF | 0.650 | 44 | 3.40, 7.84, -1.00 | Purple stage wash |
| `MK_Light_Spot_09` | Spot | #FFE0AD | 0.650 | 44 | 1.10, 7.84, -1.00 | Warm stage pool |
| `MK_Light_Spot_10` | Spot | #FFE0AD | 0.650 | 44 | -1.10, 7.84, -1.00 | Warm stage pool |
| `MK_Light_Spot_11` | Spot | #B78DFF | 0.650 | 44 | -3.40, 7.84, -1.00 | Purple stage wash |
| `MK_Light_Spot_12` | Spot | #FFE0AD | 0.650 | 44 | -5.70, 7.84, -1.00 | Warm stage pool |
| `MK_Light_Spot_13` | Spot | #FFE0AD | 0.650 | 44 | -8.00, 7.84, -1.00 | Warm stage pool |
| `MK_Light_Spot_14` | Spot | #B78DFF | 0.260 | 44 | 9.00, 1.10, 2.00 | Purple stage wash |
| `MK_Light_Spot_15` | Spot | #FFE0AD | 0.260 | 44 | -9.00, 1.10, 2.00 | Warm stage pool |
| `MK_Light_Spot_16` | Spot | #FFE0AD | 0.260 | 44 | 7.00, 1.10, -3.00 | Warm stage pool |
| `MK_Light_Spot_17` | Spot | #B78DFF | 0.260 | 44 | -7.00, 1.10, -3.00 | Purple stage wash |
| `MK_Light_Point_18` | Point | #FFB85A | 0.065 | — | 13.75, 6.02, -1.91 | Amber wall lantern; bake indirect light |
| `MK_Light_Point_19` | Point | #FFB85A | 0.065 | — | 11.05, 6.02, -1.91 | Amber wall lantern; bake indirect light |
| `MK_Light_Point_20` | Point | #FFB85A | 0.065 | — | -11.05, 6.02, -1.91 | Amber wall lantern; bake indirect light |
| `MK_Light_Point_21` | Point | #FFB85A | 0.065 | — | -13.75, 6.02, -1.91 | Amber wall lantern; bake indirect light |
| `MK_Light_Point_22` | Point | #FFB85A | 0.065 | — | 13.21, 6.02, 7.75 | Amber wall lantern; bake indirect light |
| `MK_Light_Point_23` | Point | #FFB85A | 0.065 | — | 13.21, 6.02, 5.05 | Amber wall lantern; bake indirect light |
| `MK_Light_Point_24` | Point | #FFB85A | 0.065 | — | -13.21, 6.02, 5.05 | Amber wall lantern; bake indirect light |
| `MK_Light_Point_25` | Point | #FFB85A | 0.065 | — | -13.21, 6.02, 7.75 | Amber wall lantern; bake indirect light |
| `MK_Light_Point_26` | Point | #FFB85A | 0.065 | — | 11.00, 7.42, 24.80 | Amber wall lantern; bake indirect light |
| `MK_Light_Point_27` | Point | #FFB85A | 0.065 | — | 7.00, 7.42, 24.80 | Amber wall lantern; bake indirect light |
| `MK_Light_Point_28` | Point | #FFB85A | 0.065 | — | 3.00, 7.42, 24.80 | Amber wall lantern; bake indirect light |
| `MK_Light_Point_29` | Point | #FFB85A | 0.065 | — | -1.00, 7.42, 24.80 | Amber wall lantern; bake indirect light |
| `MK_Light_Point_30` | Point | #FFB85A | 0.065 | — | -5.00, 7.42, 24.80 | Amber wall lantern; bake indirect light |
| `MK_Light_Point_31` | Point | #FFB85A | 0.065 | — | -9.00, 7.42, 24.80 | Amber wall lantern; bake indirect light |
| `MK_Light_Point_32` | Point | #FFB85A | 0.065 | — | -13.00, 7.42, 24.80 | Amber wall lantern; bake indirect light |
| `MK_Light_Point_33` | Point | #FFB85A | 0.065 | — | 12.35, 2.62, 4.10 | Amber wall lantern; bake indirect light |
| `MK_Light_Point_34` | Point | #FFB85A | 0.065 | — | 12.35, 2.62, -0.10 | Amber wall lantern; bake indirect light |
| `MK_Light_Area_35` | Area | #FFE0B0 | 1.800 | — | -0.00, 7.50, 3.00 | Broad warm stage key, bake indirect |
| `MK_Light_Area_36` | Area | #FFD6AF | 0.650 | — | 7.00, 5.00, 6.00 | Warm camera-left fill |
| `MK_Light_Area_37` | Area | #AD83FF | 0.450 | — | -6.00, 6.80, -2.00 | Purple rim and wood reflections |
| `MK_Light_Area_38` | Area | #FFD6AD | 1.050 | — | -0.00, 8.00, 15.00 | Audience fill, preserve stage contrast |
| `MK_Light_Area_39` | Area | #DFA778 | 0.450 | — | -0.00, 8.00, 23.00 | Upper balcony soft fill |
| `MK_Light_Area_40` | Area | #FFC78A | 1.600 | — | 10.00, 7.50, 8.00 | Lift and side balcony amber key |
| `MK_Light_Area_41` | Area | #FFDDAA | 0.500 | — | 9.00, 4.00, 5.00 | Brushed gold lift door reflection key |
| `MK_Light_Area_42` | Area | #FFC78A | 0.950 | — | -7.00, 7.00, 18.00 | Rear balcony and banner key |
| `MK_Light_Area_43` | Area | #FFC78A | 0.950 | — | 7.00, 7.00, 18.00 | Rear balcony balancing fill |

## Review setup and limitations

Final reference renders use EEVEE with 192 samples at 3344 x 1882, resolved to 1672 x 941, AgX Medium High Contrast, exposure +0.6 and restrained Fog Glow. Shadow pool: 1024 MB; resolution scale: 0.75. Only stage spots plus the main stage/audience area keys cast shadows; other sources provide fill. Exact per-light flags are in `lighting.json`. The refinement reduces broad fill, narrows stage spots to 44 degrees and reserves purple as a secondary colour.

The diagnostic Unity import screenshot uses an extra unshadowed directional fill outside the room, with no probes. Do not copy that fill into production. The separate playable Art/Floors/Trivia/Review scene uses the 43 marker lights, four shadow sources, warm ambient fill, ACES and two baked 128-pixel reflection probes. Round 04 adds baked indirect illumination: 10 rectangular area sources and 21 point sources are baked, with 12 stage spots plus 10 derived area-highlight spots left realtime. Four lights cast realtime shadows. It stores UV2 copies for 319 lightmapped renderers, and 113 probes supply moving objects. Its light intensities are a review approximation, not a physical conversion of Blender watts. It uses a temporary pipeline clone with 4x MSAA plus High SMAA; shared graphics assets remain unchanged.

The round-04 engine comparisons capture the actual review scene at the three source cameras. Separately baked cubemaps are serialized as Custom probe textures, with the stage map also assigned as the indoor default reflection. Reopening the scene is validated: a direct bakedTexture assignment previously vanished and exposed the default outdoor reflection. Probe captures disable reflection feedback; EXRs import as linear, uncompressed HDR and are audited for finite, bounded pixels. Current review intensity divisors are 35 for points, 18 followed by a 0.12 baked-area multiplier (plus 0.65 for each derived realtime highlight spot), and 24 for stage spots; these are art-directed review values, not a unit conversion.

The measured FBX position conversion is Blender `(x, y, z)` to Unity `(-x, z, -y)`. Unity validation compares every imported light marker with this data. The EEVEE and Unity shading models differ, so the lighting match remains work for the integrated scene.
