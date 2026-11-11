# Round 2 imported object contract changes

Names not listed retain their imported pose, bounds and triangle count to 1 mm. Mesh rotations baked into vertices are described by the numbered review; triangle count and bounds alone do not prove identical topology. Every row comes from the saved Unity review scene, including its open lift-door display pose.

Explicit replacements: `SM_Trivia_GoldTieback` becomes `SM_Trivia_GoldTieback_L` and `_R`; centre `COL_Trivia_Step*` becomes corner `COL_Trivia_StageStep*`; the separate old balcony collision decks become one continuous static `COL_Trivia_BalconyDeck`. Original pan/tilt names stay; optical-axis children are added. Other removals/additions are listed below.

| Name | Change | Before pivot | After pivot |
|---|---|---|---|
| `COL_Trivia_Aisle` | removed | (0.000, -0.010, 8.300) | - |
| `COL_Trivia_Aisle.001` | removed | (0.000, 0.090, 9.730) | - |
| `COL_Trivia_Aisle.002` | removed | (0.000, 0.190, 11.160) | - |
| `COL_Trivia_Aisle.003` | removed | (0.000, 0.290, 12.590) | - |
| `COL_Trivia_Aisle.004` | removed | (0.000, 0.390, 14.020) | - |
| `COL_Trivia_Aisle.005` | removed | (0.000, 0.490, 15.450) | - |
| `COL_Trivia_Aisle.006` | removed | (0.000, 0.590, 16.880) | - |
| `COL_Trivia_Aisle.007` | removed | (0.000, 0.690, 18.310) | - |
| `COL_Trivia_Aisle.008` | removed | (0.000, 0.790, 19.740) | - |
| `COL_Trivia_BackBalconyDeck` | removed | (0.000, 4.720, 24.000) | - |
| `COL_Trivia_BalconyDeck` | pivot moved, geometry bounds changed, topology changed | (13.500, 4.720, 5.000) | (0.000, 0.000, 0.000) |
| `COL_Trivia_BalconyDeck.001` | removed | (-13.500, 4.720, 5.000) | - |
| `COL_Trivia_CameraStation` | added | - | (-12.800, 1.200, 12.300) |
| `COL_Trivia_CameraStation.001` | added | - | (12.800, 1.200, 12.300) |
| `COL_Trivia_CameraStationFoot` | added | - | (-12.800, 0.460, 12.300) |
| `COL_Trivia_CameraStationFoot.001` | added | - | (12.800, 0.460, 12.300) |
| `COL_Trivia_ControlDesk` | added | - | (0.000, 2.100, 23.800) |
| `COL_Trivia_ControlRack` | added | - | (-1.700, 1.620, 23.800) |
| `COL_Trivia_ControlRack.001` | added | - | (1.700, 1.620, 23.800) |
| `COL_Trivia_FrontRowGuard` | added | - | (-5.700, 0.500, 7.500) |
| `COL_Trivia_FrontRowGuard.001` | added | - | (5.700, 0.500, 7.500) |
| `COL_Trivia_GalleryCurvedGuard` | added | - | (12.487, 5.350, 10.409) |
| `COL_Trivia_GalleryEndGuard` | added | - | (-13.450, 5.350, -1.000) |
| `COL_Trivia_GalleryFrontGuard` | added | - | (-12.500, 5.350, 4.500) |
| `COL_Trivia_GalleryStep` | added | - | (-12.450, 1.300, 24.250) |
| `COL_Trivia_GalleryVoidGuard` | added | - | (-10.950, 5.350, 21.700) |
| `COL_Trivia_HallFloor` | pivot moved, geometry bounds changed | (0.000, -0.120, 10.000) | (-0.750, -0.120, 10.000) |
| `COL_Trivia_HallFloor.001` | added | - | (13.750, -0.120, -3.250) |
| `COL_Trivia_HallFloor.002` | added | - | (13.750, -0.120, 15.250) |
| `COL_Trivia_LiftThreshold` | added | - | (13.000, 0.012, 2.000) |
| `COL_Trivia_PlayerTV` | added | - | (0.000, 1.345, 5.320) |
| `COL_Trivia_PlayerTVFoot` | added | - | (0.000, 0.650, 5.320) |
| `COL_Trivia_PlayerTVStand` | added | - | (0.000, 0.973, 5.420) |
| `COL_Trivia_Plinth.002` | pivot moved, geometry bounds changed | (12.500, 0.570, 4.500) | (12.400, 0.570, 6.600) |
| `COL_Trivia_Plinth.003` | pivot moved, geometry bounds changed | (12.500, 0.570, -0.500) | (12.400, 0.570, -1.700) |
| `COL_Trivia_RearCrossAisle` | added | - | (0.000, 0.600, 23.550) |
| `COL_Trivia_SeatTier` | added | - | (0.000, 0.100, 10.300) |
| `COL_Trivia_SeatTier.001` | added | - | (0.000, 0.200, 12.300) |
| `COL_Trivia_SeatTier.002` | added | - | (0.000, 0.300, 14.300) |
| `COL_Trivia_SeatTier.003` | added | - | (0.000, 0.400, 16.300) |
| `COL_Trivia_SeatTier.004` | added | - | (0.000, 0.500, 18.300) |
| `COL_Trivia_SeatTier.005` | added | - | (0.000, 0.600, 20.300) |
| `COL_Trivia_SeatingTier` | pivot moved, geometry bounds changed | (7.000, 0.550, 8.300) | (-5.590, 0.630, 8.300) |
| `COL_Trivia_SeatingTier.001` | pivot moved, geometry bounds changed | (-7.000, 0.550, 8.300) | (5.590, 0.630, 8.300) |
| `COL_Trivia_SeatingTier.002` | pivot moved, geometry bounds changed | (7.000, 0.750, 9.730) | (-5.590, 0.830, 10.300) |
| `COL_Trivia_SeatingTier.003` | pivot moved, geometry bounds changed | (-7.000, 0.750, 9.730) | (5.590, 0.830, 10.300) |
| `COL_Trivia_SeatingTier.004` | pivot moved, geometry bounds changed | (7.000, 0.950, 11.160) | (-5.590, 1.030, 12.300) |
| `COL_Trivia_SeatingTier.005` | pivot moved, geometry bounds changed | (-7.000, 0.950, 11.160) | (5.590, 1.030, 12.300) |
| `COL_Trivia_SeatingTier.006` | pivot moved, geometry bounds changed | (7.000, 1.150, 12.590) | (-5.590, 1.230, 14.300) |
| `COL_Trivia_SeatingTier.007` | pivot moved, geometry bounds changed | (-7.000, 1.150, 12.590) | (5.590, 1.230, 14.300) |
| `COL_Trivia_SeatingTier.008` | pivot moved, geometry bounds changed | (7.000, 1.350, 14.020) | (-5.590, 1.430, 16.300) |
| `COL_Trivia_SeatingTier.009` | pivot moved, geometry bounds changed | (-7.000, 1.350, 14.020) | (5.590, 1.430, 16.300) |
| `COL_Trivia_SeatingTier.010` | pivot moved, geometry bounds changed | (7.000, 1.550, 15.450) | (-5.590, 1.630, 18.300) |
| `COL_Trivia_SeatingTier.011` | pivot moved, geometry bounds changed | (-7.000, 1.550, 15.450) | (5.590, 1.630, 18.300) |
| `COL_Trivia_SeatingTier.012` | pivot moved, geometry bounds changed | (7.000, 1.750, 16.880) | (-5.590, 1.830, 20.300) |
| `COL_Trivia_SeatingTier.013` | pivot moved, geometry bounds changed | (-7.000, 1.750, 16.880) | (5.590, 1.830, 20.300) |
| `COL_Trivia_SeatingTier.014` | removed | (7.000, 1.950, 18.310) | - |
| `COL_Trivia_SeatingTier.015` | removed | (-7.000, 1.950, 18.310) | - |
| `COL_Trivia_SeatingTier.016` | removed | (7.000, 2.150, 19.740) | - |
| `COL_Trivia_SeatingTier.017` | removed | (-7.000, 2.150, 19.740) | - |
| `COL_Trivia_SideTheatreBox` | removed | (13.900, 3.850, 6.400) | - |
| `COL_Trivia_SideTheatreBox.001` | removed | (-13.900, 3.850, 6.400) | - |
| `COL_Trivia_Spotlight_09_Base` | added | - | (9.000, 1.010, 2.000) |
| `COL_Trivia_Spotlight_10_Base` | added | - | (-9.000, 1.010, 2.000) |
| `COL_Trivia_Spotlight_11_Base` | added | - | (7.000, 1.010, -3.000) |
| `COL_Trivia_Spotlight_12_Base` | added | - | (-7.000, 1.010, -3.000) |
| `COL_Trivia_StageLanding` | added | - | (0.000, 0.000, 0.000) |
| `COL_Trivia_StageStep` | added | - | (8.900, 0.100, 5.350) |
| `COL_Trivia_StageStep.001` | added | - | (8.900, 0.200, 5.050) |
| `COL_Trivia_StageStep.002` | added | - | (8.900, 0.300, 4.750) |
| `COL_Trivia_Step` | removed | (0.000, 0.100, 6.350) | - |
| `COL_Trivia_Step.001` | removed | (0.000, 0.200, 6.050) | - |
| `COL_Trivia_Step.002` | removed | (0.000, 0.300, 5.750) | - |
| `COL_Trivia_UpperSeating` | added | - | (-5.240, 5.430, 23.100) |
| `COL_Trivia_UpperSeating.001` | added | - | (5.240, 5.430, 23.100) |
| `MK_Host` | pivot moved, axes rotated | (0.000, 0.600, -0.500) | (-6.400, 0.600, 3.100) |
| `MK_Light_Area_50` | added | - | (0.000, 4.350, 23.800) |
| `MK_Light_Point_01` | pivot moved, axes rotated | (4.800, 0.850, 3.180) | (4.800, 0.850, 1.820) |
| `MK_Light_Point_02` | pivot moved, axes rotated | (1.600, 0.850, 3.180) | (1.600, 0.850, 1.820) |
| `MK_Light_Point_03` | pivot moved, axes rotated | (-1.600, 0.850, 3.180) | (-1.600, 0.850, 1.820) |
| `MK_Light_Point_04` | pivot moved, axes rotated | (-4.800, 0.850, 3.180) | (-4.800, 0.850, 1.820) |
| `MK_Light_Point_22` | pivot moved | (13.210, 6.020, 7.750) | (13.310, 7.420, 7.750) |
| `MK_Light_Point_23` | pivot moved | (13.210, 6.020, 5.050) | (13.310, 7.420, 5.050) |
| `MK_Light_Point_24` | pivot moved | (-13.210, 6.020, 5.050) | (-13.310, 7.420, 5.050) |
| `MK_Light_Point_25` | pivot moved | (-13.210, 6.020, 7.750) | (-13.310, 7.420, 7.750) |
| `MK_Light_Point_44` | added | - | (-13.980, 3.400, 10.000) |
| `MK_Light_Point_45` | added | - | (-13.980, 3.400, 15.000) |
| `MK_Light_Point_46` | added | - | (-13.980, 3.400, 20.000) |
| `MK_Light_Point_47` | added | - | (13.980, 3.400, 10.000) |
| `MK_Light_Point_48` | added | - | (13.980, 3.400, 15.000) |
| `MK_Light_Point_49` | added | - | (13.980, 3.400, 20.000) |
| `MK_Light_Spot_06` | pivot moved, axes rotated | (8.000, 7.840, -1.000) | (7.889, 7.630, -0.915) |
| `MK_Light_Spot_07` | pivot moved, axes rotated | (5.700, 7.840, -1.000) | (5.617, 7.620, -0.911) |
| `MK_Light_Spot_08` | pivot moved, axes rotated | (3.400, 7.840, -1.000) | (3.349, 7.611, -0.907) |
| `MK_Light_Spot_09` | pivot moved, axes rotated | (1.100, 7.840, -1.000) | (1.083, 7.607, -0.905) |
| `MK_Light_Spot_10` | pivot moved, axes rotated | (-1.100, 7.840, -1.000) | (-1.083, 7.607, -0.905) |
| `MK_Light_Spot_11` | pivot moved, axes rotated | (-3.400, 7.840, -1.000) | (-3.349, 7.611, -0.907) |
| `MK_Light_Spot_12` | pivot moved, axes rotated | (-5.700, 7.840, -1.000) | (-5.617, 7.620, -0.911) |
| `MK_Light_Spot_13` | pivot moved, axes rotated | (-8.000, 7.840, -1.000) | (-7.889, 7.630, -0.915) |
| `MK_Light_Spot_14` | pivot moved, axes rotated | (9.000, 1.100, 2.000) | (8.961, 1.288, 1.837) |
| `MK_Light_Spot_15` | pivot moved, axes rotated | (-9.000, 1.100, 2.000) | (-8.961, 1.288, 1.837) |
| `MK_Light_Spot_16` | pivot moved, axes rotated | (7.000, 1.100, -3.000) | (6.904, 1.319, -3.078) |
| `MK_Light_Spot_17` | pivot moved, axes rotated | (-7.000, 1.100, -3.000) | (-6.904, 1.319, -3.078) |
| `MK_Objective_01` | pivot moved | (4.800, 0.600, 3.550) | (4.800, 0.600, 1.450) |
| `MK_Objective_02` | pivot moved | (1.600, 0.600, 3.550) | (1.600, 0.600, 1.450) |
| `MK_Objective_03` | pivot moved | (-1.600, 0.600, 3.550) | (-1.600, 0.600, 1.450) |
| `MK_Objective_04` | pivot moved | (-4.800, 0.600, 3.550) | (-4.800, 0.600, 1.450) |
| `MK_Trivia_Spotlight_01_OpticalAxis` | added | - | (7.889, 7.630, -0.915) |
| `MK_Trivia_Spotlight_01_Pan` | axes rotated | (8.000, 8.200, -1.000) | (8.000, 8.200, -1.000) |
| `MK_Trivia_Spotlight_01_Tilt` | axes rotated | (8.000, 7.840, -1.000) | (8.000, 7.840, -1.000) |
| `MK_Trivia_Spotlight_02_OpticalAxis` | added | - | (5.617, 7.620, -0.911) |
| `MK_Trivia_Spotlight_02_Pan` | axes rotated | (5.700, 8.200, -1.000) | (5.700, 8.200, -1.000) |
| `MK_Trivia_Spotlight_02_Tilt` | axes rotated | (5.700, 7.840, -1.000) | (5.700, 7.840, -1.000) |
| `MK_Trivia_Spotlight_03_OpticalAxis` | added | - | (3.349, 7.611, -0.907) |
| `MK_Trivia_Spotlight_03_Pan` | axes rotated | (3.400, 8.200, -1.000) | (3.400, 8.200, -1.000) |
| `MK_Trivia_Spotlight_03_Tilt` | axes rotated | (3.400, 7.840, -1.000) | (3.400, 7.840, -1.000) |
| `MK_Trivia_Spotlight_04_OpticalAxis` | added | - | (1.083, 7.607, -0.905) |
| `MK_Trivia_Spotlight_04_Pan` | axes rotated | (1.100, 8.200, -1.000) | (1.100, 8.200, -1.000) |
| `MK_Trivia_Spotlight_04_Tilt` | axes rotated | (1.100, 7.840, -1.000) | (1.100, 7.840, -1.000) |
| `MK_Trivia_Spotlight_05_OpticalAxis` | added | - | (-1.083, 7.607, -0.905) |
| `MK_Trivia_Spotlight_05_Pan` | axes rotated | (-1.100, 8.200, -1.000) | (-1.100, 8.200, -1.000) |
| `MK_Trivia_Spotlight_05_Tilt` | axes rotated | (-1.100, 7.840, -1.000) | (-1.100, 7.840, -1.000) |
| `MK_Trivia_Spotlight_06_OpticalAxis` | added | - | (-3.349, 7.611, -0.907) |
| `MK_Trivia_Spotlight_06_Pan` | axes rotated | (-3.400, 8.200, -1.000) | (-3.400, 8.200, -1.000) |
| `MK_Trivia_Spotlight_06_Tilt` | axes rotated | (-3.400, 7.840, -1.000) | (-3.400, 7.840, -1.000) |
| `MK_Trivia_Spotlight_07_OpticalAxis` | added | - | (-5.617, 7.620, -0.911) |
| `MK_Trivia_Spotlight_07_Pan` | axes rotated | (-5.700, 8.200, -1.000) | (-5.700, 8.200, -1.000) |
| `MK_Trivia_Spotlight_07_Tilt` | axes rotated | (-5.700, 7.840, -1.000) | (-5.700, 7.840, -1.000) |
| `MK_Trivia_Spotlight_08_OpticalAxis` | added | - | (-7.889, 7.630, -0.915) |
| `MK_Trivia_Spotlight_08_Pan` | axes rotated | (-8.000, 8.200, -1.000) | (-8.000, 8.200, -1.000) |
| `MK_Trivia_Spotlight_08_Tilt` | axes rotated | (-8.000, 7.840, -1.000) | (-8.000, 7.840, -1.000) |
| `MK_Trivia_Spotlight_09_OpticalAxis` | added | - | (8.961, 1.288, 1.837) |
| `MK_Trivia_Spotlight_09_Pan` | axes rotated | (9.000, 0.740, 2.000) | (9.000, 0.740, 2.000) |
| `MK_Trivia_Spotlight_09_Tilt` | axes rotated | (9.000, 1.100, 2.000) | (9.000, 1.100, 2.000) |
| `MK_Trivia_Spotlight_10_OpticalAxis` | added | - | (-8.961, 1.288, 1.837) |
| `MK_Trivia_Spotlight_10_Pan` | axes rotated | (-9.000, 0.740, 2.000) | (-9.000, 0.740, 2.000) |
| `MK_Trivia_Spotlight_10_Tilt` | axes rotated | (-9.000, 1.100, 2.000) | (-9.000, 1.100, 2.000) |
| `MK_Trivia_Spotlight_11_OpticalAxis` | added | - | (6.904, 1.319, -3.078) |
| `MK_Trivia_Spotlight_11_Pan` | axes rotated | (7.000, 0.740, -3.000) | (7.000, 0.740, -3.000) |
| `MK_Trivia_Spotlight_11_Tilt` | axes rotated | (7.000, 1.100, -3.000) | (7.000, 1.100, -3.000) |
| `MK_Trivia_Spotlight_12_OpticalAxis` | added | - | (-6.904, 1.319, -3.078) |
| `MK_Trivia_Spotlight_12_Pan` | axes rotated | (-7.000, 0.740, -3.000) | (-7.000, 0.740, -3.000) |
| `MK_Trivia_Spotlight_12_Tilt` | axes rotated | (-7.000, 1.100, -3.000) | (-7.000, 1.100, -3.000) |
| `SM_Trivia_AcanthusMidrib` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_AcanthusRelief` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_AisleBorder` | geometry bounds changed, topology changed | (1.390, 0.029, 8.300) | (1.390, 0.029, 8.300) |
| `SM_Trivia_AisleCarpet` | geometry bounds changed, topology changed | (0.000, 0.012, 8.300) | (0.000, 0.012, 8.300) |
| `SM_Trivia_AisleRiser` | removed | (0.000, 0.000, 0.000) | - |
| `SM_Trivia_AisleSconceBack` | added | - | (-14.330, 3.400, 10.000) |
| `SM_Trivia_AisleSconceDiffuser` | added | - | (-14.200, 3.400, 10.000) |
| `SM_Trivia_AisleStar` | removed | (0.000, 0.000, 0.000) | - |
| `SM_Trivia_AisleTerrace` | removed | (0.000, 0.100, 9.730) | - |
| `SM_Trivia_ApronFlute` | topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_ArchRosette` | topology changed | (9.714, 8.906, -1.770) | (9.714, 8.906, -1.770) |
| `SM_Trivia_BackBalconyDeck` | removed | (0.000, 4.720, 24.000) | - |
| `SM_Trivia_BalconyBulb` | geometry bounds changed, topology changed | (12.438, 4.980, 9.970) | (12.438, 4.980, 9.970) |
| `SM_Trivia_BalconyDeck` | geometry bounds changed, topology changed | (13.500, 4.720, 5.000) | (13.500, 4.720, 5.000) |
| `SM_Trivia_BalconyFascia` | geometry bounds changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_BalconyMoulding` | geometry bounds changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_Baluster` | geometry bounds changed, topology changed | (13.520, 4.750, -2.220) | (13.520, 4.750, -2.220) |
| `SM_Trivia_BalusterCollar` | geometry bounds changed, topology changed | (13.520, 4.880, -2.220) | (13.520, 4.880, -2.220) |
| `SM_Trivia_BannerLaurel` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_BoxArch` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_BoxArchBead` | geometry bounds changed, topology changed | (11.210, 6.150, -2.360) | (11.210, 6.150, -2.360) |
| `SM_Trivia_BoxCornice` | geometry bounds changed, topology changed | (12.400, 1.000, -2.800) | (12.400, 1.000, -2.800) |
| `SM_Trivia_BoxCurtain` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_BoxCurtain_Fringe` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_BoxCurtain_FringeThread` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_BoxCurtain_HemBraid` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_BoxFascia` | geometry bounds changed, topology changed | (12.400, 4.300, -2.600) | (12.400, 4.300, -2.600) |
| `SM_Trivia_BoxKeystone` | geometry bounds changed, topology changed | (12.400, 7.270, -2.370) | (12.400, 7.270, -2.370) |
| `SM_Trivia_BoxLayeredCornice` | geometry bounds changed, topology changed | (12.400, 1.150, -2.650) | (12.400, 1.150, -2.650) |
| `SM_Trivia_BoxPilaster` | geometry bounds changed, topology changed | (13.580, 4.200, -2.800) | (13.580, 4.200, -2.800) |
| `SM_Trivia_BoxPilasterInset` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_BoxPilasterTrim` | geometry bounds changed, topology changed | (13.680, 4.200, -2.440) | (13.680, 4.200, -2.440) |
| `SM_Trivia_BoxRecess` | geometry bounds changed, topology changed | (12.400, 5.700, -3.020) | (12.400, 5.700, -3.020) |
| `SM_Trivia_BoxSwag` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_BoxSwagBraid` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_BoxUpperDrape` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_BoxUpperDrape_Fringe` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_BoxUpperDrape_FringeThread` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_BoxUpperDrape_HemBraid` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_BroadcastCamera` | added | - | (-12.800, 2.020, 12.300) |
| `SM_Trivia_BroadcastLens` | added | - | (-12.800, 2.020, 11.900) |
| `SM_Trivia_CameraPedestal` | added | - | (-12.800, 1.150, 12.300) |
| `SM_Trivia_CameraStationFoot` | added | - | (-12.800, 0.460, 12.300) |
| `SM_Trivia_CarvedRosette` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_ControlCableDuct` | added | - | (0.000, 1.970, 24.150) |
| `SM_Trivia_ControlDesk` | added | - | (0.000, 2.100, 23.800) |
| `SM_Trivia_ControlDisplay` | added | - | (-1.200, 2.560, 24.000) |
| `SM_Trivia_ControlFader` | added | - | (-0.850, 2.190, 23.552) |
| `SM_Trivia_ControlFaderTrack` | added | - | (-0.850, 2.179, 23.590) |
| `SM_Trivia_ControlMonitor` | added | - | (-1.200, 2.560, 23.950) |
| `SM_Trivia_ControlMonitorStand` | added | - | (-1.200, 2.300, 23.950) |
| `SM_Trivia_ControlRack` | added | - | (-1.700, 1.620, 23.800) |
| `SM_Trivia_CrestStar` | geometry bounds changed | (0.000, 0.000, 2.000) | (0.000, 0.000, 2.000) |
| `SM_Trivia_FlowerUrn` | geometry bounds changed, topology changed | (8.802, 0.600, 0.096) | (8.802, 0.600, 0.095) |
| `SM_Trivia_FramePearl` | removed | (4.650, 3.460, -4.090) | - |
| `SM_Trivia_FrontCrossBorder` | added | - | (4.450, 0.017, 5.720) |
| `SM_Trivia_FrontCrossCarpet` | added | - | (4.450, 0.008, 6.500) |
| `SM_Trivia_GalleryLanternMount` | added | - | (-14.280, 7.140, 5.050) |
| `SM_Trivia_GalleryStairPost` | added | - | (0.000, 0.000, 0.000) |
| `SM_Trivia_GalleryStairRail` | added | - | (0.000, 0.000, 0.000) |
| `SM_Trivia_GalleryStep` | added | - | (-12.450, 1.300, 24.250) |
| `SM_Trivia_GalleryStepNosing` | added | - | (0.000, 0.000, 0.000) |
| `SM_Trivia_GiltScroll` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_GoldTieback` | removed | (0.000, 0.000, 2.000) | - |
| `SM_Trivia_GoldTieback_L` | added | - | (0.000, 0.000, 0.000) |
| `SM_Trivia_GoldTieback_R` | added | - | (0.000, 0.000, 0.000) |
| `SM_Trivia_HallFloor` | topology changed | (0.000, -0.120, 10.000) | (0.000, -0.120, 10.000) |
| `SM_Trivia_Handrail` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_HandrailBottom` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_HostCompass` | pivot moved, geometry bounds changed | (0.000, 0.000, 0.000) | (-6.400, 0.000, 3.600) |
| `SM_Trivia_HostCompassRing` | pivot moved, geometry bounds changed | (0.000, 0.000, 0.000) | (-6.400, 0.000, 3.600) |
| `SM_Trivia_InnerQuestionFrame` | removed | (0.000, 0.000, 0.000) | - |
| `SM_Trivia_LampMount` | added | - | (0.000, 0.000, 0.000) |
| `SM_Trivia_LiftThreshold` | added | - | (13.000, 0.012, 2.000) |
| `SM_Trivia_MarqueeChase_01` | topology changed | (-6.150, 6.875, -4.120) | (-6.150, 6.875, -4.120) |
| `SM_Trivia_MarqueeChase_02` | topology changed | (-6.066, 7.194, -4.120) | (-6.066, 7.194, -4.120) |
| `SM_Trivia_MarqueeChase_03` | topology changed | (-5.837, 7.430, -4.120) | (-5.837, 7.430, -4.120) |
| `SM_Trivia_PlayerScreen` | added | - | (0.000, 1.345, 5.320) |
| `SM_Trivia_PlayerTVBezel` | added | - | (0.000, 1.345, 5.320) |
| `SM_Trivia_PlayerTVCable` | added | - | (0.000, 0.000, 0.000) |
| `SM_Trivia_PlayerTVCableGland` | added | - | (0.100, 0.609, 5.400) |
| `SM_Trivia_PlayerTVFoot` | added | - | (0.000, 0.650, 5.320) |
| `SM_Trivia_PlayerTVHousing` | added | - | (0.000, 1.345, 5.320) |
| `SM_Trivia_PlayerTVStand` | added | - | (0.000, 0.973, 5.420) |
| `SM_Trivia_PlayerTVTiltJoint` | added | - | (0.000, 1.285, 5.395) |
| `SM_Trivia_PodiumScreen_01` | pivot moved, geometry bounds changed | (0.720, 0.000, 0.000) | (9.300, 0.000, 3.560) |
| `SM_Trivia_PodiumScreen_02` | pivot moved, geometry bounds changed | (0.240, 0.000, 0.000) | (3.513, 0.000, 4.548) |
| `SM_Trivia_PodiumScreen_03` | pivot moved, geometry bounds changed | (-0.240, 0.000, 0.000) | (-3.513, 0.000, 4.548) |
| `SM_Trivia_PodiumScreen_04` | pivot moved, geometry bounds changed | (-0.720, 0.000, 0.000) | (-9.300, 0.000, 3.560) |
| `SM_Trivia_Podium_01` | pivot moved, geometry bounds changed, topology changed | (4.800, 0.600, 2.511) | (4.809, 0.600, 2.490) |
| `SM_Trivia_Podium_01_Button_Circle` | pivot moved, geometry bounds changed | (4.979, 1.825, 2.674) | (4.622, 1.785, 2.349) |
| `SM_Trivia_Podium_01_Button_Cross` | pivot moved, geometry bounds changed | (4.979, 1.729, 2.965) | (4.621, 1.720, 2.049) |
| `SM_Trivia_Podium_01_Button_Square` | pivot moved, geometry bounds changed | (4.622, 1.825, 2.670) | (4.979, 1.785, 2.352) |
| `SM_Trivia_Podium_01_Button_Triangle` | pivot moved, geometry bounds changed | (4.622, 1.729, 2.967) | (4.979, 1.720, 2.046) |
| `SM_Trivia_Podium_02` | pivot moved, geometry bounds changed, topology changed | (1.600, 0.600, 2.511) | (1.609, 0.600, 2.490) |
| `SM_Trivia_Podium_02_Button_Circle` | pivot moved, geometry bounds changed | (1.778, 1.825, 2.674) | (1.422, 1.785, 2.349) |
| `SM_Trivia_Podium_02_Button_Cross` | pivot moved, geometry bounds changed | (1.778, 1.729, 2.965) | (1.422, 1.720, 2.049) |
| `SM_Trivia_Podium_02_Button_Square` | pivot moved, geometry bounds changed | (1.421, 1.825, 2.670) | (1.779, 1.785, 2.352) |
| `SM_Trivia_Podium_02_Button_Triangle` | pivot moved, geometry bounds changed | (1.421, 1.729, 2.967) | (1.779, 1.720, 2.046) |
| `SM_Trivia_Podium_03` | pivot moved, geometry bounds changed, topology changed | (-1.600, 0.600, 2.511) | (-1.609, 0.600, 2.490) |
| `SM_Trivia_Podium_03_Button_Circle` | pivot moved, geometry bounds changed | (-1.421, 1.825, 2.674) | (-1.779, 1.785, 2.349) |
| `SM_Trivia_Podium_03_Button_Cross` | pivot moved, geometry bounds changed | (-1.421, 1.729, 2.965) | (-1.779, 1.720, 2.049) |
| `SM_Trivia_Podium_03_Button_Square` | pivot moved, geometry bounds changed | (-1.778, 1.825, 2.670) | (-1.422, 1.785, 2.352) |
| `SM_Trivia_Podium_03_Button_Triangle` | pivot moved, geometry bounds changed | (-1.778, 1.729, 2.967) | (-1.422, 1.720, 2.046) |
| `SM_Trivia_Podium_04` | pivot moved, geometry bounds changed, topology changed | (-4.800, 0.600, 2.511) | (-4.809, 0.600, 2.490) |
| `SM_Trivia_Podium_04_Button_Circle` | pivot moved, geometry bounds changed | (-4.622, 1.825, 2.674) | (-4.979, 1.785, 2.349) |
| `SM_Trivia_Podium_04_Button_Cross` | pivot moved, geometry bounds changed | (-4.622, 1.729, 2.965) | (-4.979, 1.720, 2.049) |
| `SM_Trivia_Podium_04_Button_Square` | pivot moved, geometry bounds changed | (-4.979, 1.825, 2.670) | (-4.622, 1.785, 2.352) |
| `SM_Trivia_Podium_04_Button_Triangle` | pivot moved, geometry bounds changed | (-4.979, 1.729, 2.967) | (-4.622, 1.720, 2.046) |
| `SM_Trivia_QuestionScreen` | geometry bounds changed, topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_QuestionSurround` | removed | (0.000, 0.000, 0.000) | - |
| `SM_Trivia_RearCrossAisle` | added | - | (0.000, 0.600, 23.550) |
| `SM_Trivia_RosetteBoss` | geometry bounds changed, topology changed | (5.580, 3.970, -4.057) | (5.580, 3.970, -4.057) |
| `SM_Trivia_ScreenCornerCarving` | removed | (0.000, 0.000, 0.000) | - |
| `SM_Trivia_ScreenCornerCarving.001` | removed | (0.000, 0.000, 0.000) | - |
| `SM_Trivia_ScreenCornerCarving.002` | removed | (0.000, 0.000, 0.000) | - |
| `SM_Trivia_ScreenCornerCarving.003` | removed | (0.000, 0.000, 0.000) | - |
| `SM_Trivia_ScreenCornerCarving.004` | removed | (0.000, 0.000, 0.000) | - |
| `SM_Trivia_ScreenCornerCarving.005` | removed | (0.000, 0.000, 0.000) | - |
| `SM_Trivia_ScreenCornerCarving.006` | removed | (0.000, 0.000, 0.000) | - |
| `SM_Trivia_ScreenCornerCarving.007` | removed | (0.000, 0.000, 0.000) | - |
| `SM_Trivia_ScreenCornerCarving.008` | removed | (0.000, 0.000, 0.000) | - |
| `SM_Trivia_ScreenCornerCarving.009` | removed | (0.000, 0.000, 0.000) | - |
| `SM_Trivia_ScreenCornerCarving.010` | removed | (0.000, 0.000, 0.000) | - |
| `SM_Trivia_ScreenCornerCarving.011` | removed | (0.000, 0.000, 0.000) | - |
| `SM_Trivia_ScreenLaurel` | removed | (0.000, 0.000, 0.000) | - |
| `SM_Trivia_ScreenLaurel.001` | removed | (0.000, 0.000, 0.000) | - |
| `SM_Trivia_ScrollLeaf` | geometry bounds changed, topology changed | (5.220, 3.920, -4.090) | (5.220, 3.920, -4.090) |
| `SM_Trivia_SeatTier` | geometry bounds changed, topology changed | (7.000, -0.010, 8.300) | (7.000, -0.010, 8.300) |
| `SM_Trivia_Seat_R00_-1_00` | pivot moved, geometry bounds changed, topology changed | (2.150, 0.000, 8.453) | (2.150, 0.000, 8.517) |
| `SM_Trivia_Seat_R00_-1_01` | pivot moved, geometry bounds changed, topology changed | (3.010, 0.000, 8.453) | (3.010, 0.000, 8.517) |
| `SM_Trivia_Seat_R00_-1_02` | pivot moved, geometry bounds changed, topology changed | (3.870, 0.000, 8.453) | (3.870, 0.000, 8.517) |
| `SM_Trivia_Seat_R00_-1_03` | pivot moved, geometry bounds changed, topology changed | (4.730, 0.000, 8.453) | (4.730, 0.000, 8.517) |
| `SM_Trivia_Seat_R00_-1_04` | pivot moved, geometry bounds changed, topology changed | (5.590, 0.000, 8.453) | (5.590, 0.000, 8.517) |
| `SM_Trivia_Seat_R00_-1_05` | pivot moved, geometry bounds changed, topology changed | (6.450, 0.000, 8.453) | (6.450, 0.000, 8.517) |
| `SM_Trivia_Seat_R00_-1_06` | pivot moved, geometry bounds changed, topology changed | (7.310, 0.000, 8.453) | (7.310, 0.000, 8.517) |
| `SM_Trivia_Seat_R00_-1_07` | pivot moved, geometry bounds changed, topology changed | (8.170, 0.000, 8.453) | (8.170, 0.000, 8.517) |
| `SM_Trivia_Seat_R00_-1_08` | pivot moved, geometry bounds changed, topology changed | (9.030, 0.000, 8.453) | (9.030, 0.000, 8.517) |
| `SM_Trivia_Seat_R00_-1_09` | removed | (9.890, 0.000, 8.453) | - |
| `SM_Trivia_Seat_R00_-1_10` | removed | (10.750, 0.000, 8.453) | - |
| `SM_Trivia_Seat_R00_1_00` | pivot moved, geometry bounds changed, topology changed | (-2.150, 0.000, 8.453) | (-2.150, 0.000, 8.517) |
| `SM_Trivia_Seat_R00_1_01` | pivot moved, geometry bounds changed, topology changed | (-3.010, 0.000, 8.453) | (-3.010, 0.000, 8.517) |
| `SM_Trivia_Seat_R00_1_02` | pivot moved, geometry bounds changed, topology changed | (-3.870, 0.000, 8.453) | (-3.870, 0.000, 8.517) |
| `SM_Trivia_Seat_R00_1_03` | pivot moved, geometry bounds changed, topology changed | (-4.730, 0.000, 8.453) | (-4.730, 0.000, 8.517) |
| `SM_Trivia_Seat_R00_1_04` | pivot moved, geometry bounds changed, topology changed | (-5.590, 0.000, 8.453) | (-5.590, 0.000, 8.517) |
| `SM_Trivia_Seat_R00_1_05` | pivot moved, geometry bounds changed, topology changed | (-6.450, 0.000, 8.453) | (-6.450, 0.000, 8.517) |
| `SM_Trivia_Seat_R00_1_06` | pivot moved, geometry bounds changed, topology changed | (-7.310, 0.000, 8.453) | (-7.310, 0.000, 8.517) |
| `SM_Trivia_Seat_R00_1_07` | pivot moved, geometry bounds changed, topology changed | (-8.170, 0.000, 8.453) | (-8.170, 0.000, 8.517) |
| `SM_Trivia_Seat_R00_1_08` | pivot moved, geometry bounds changed, topology changed | (-9.030, 0.000, 8.453) | (-9.030, 0.000, 8.517) |
| `SM_Trivia_Seat_R00_1_09` | removed | (-9.890, 0.000, 8.453) | - |
| `SM_Trivia_Seat_R00_1_10` | removed | (-10.750, 0.000, 8.453) | - |
| `SM_Trivia_Seat_R01_-1_00` | pivot moved, geometry bounds changed, topology changed | (2.150, 0.200, 9.883) | (2.150, 0.200, 10.517) |
| `SM_Trivia_Seat_R01_-1_01` | pivot moved, geometry bounds changed, topology changed | (3.010, 0.200, 9.883) | (3.010, 0.200, 10.517) |
| `SM_Trivia_Seat_R01_-1_02` | pivot moved, geometry bounds changed, topology changed | (3.870, 0.200, 9.883) | (3.870, 0.200, 10.517) |
| `SM_Trivia_Seat_R01_-1_03` | pivot moved, geometry bounds changed, topology changed | (4.730, 0.200, 9.883) | (4.730, 0.200, 10.517) |
| `SM_Trivia_Seat_R01_-1_04` | pivot moved, geometry bounds changed, topology changed | (5.590, 0.200, 9.883) | (5.590, 0.200, 10.517) |
| `SM_Trivia_Seat_R01_-1_05` | pivot moved, geometry bounds changed, topology changed | (6.450, 0.200, 9.883) | (6.450, 0.200, 10.517) |
| `SM_Trivia_Seat_R01_-1_06` | pivot moved, geometry bounds changed, topology changed | (7.310, 0.200, 9.883) | (7.310, 0.200, 10.517) |
| `SM_Trivia_Seat_R01_-1_07` | pivot moved, geometry bounds changed, topology changed | (8.170, 0.200, 9.883) | (8.170, 0.200, 10.517) |
| `SM_Trivia_Seat_R01_-1_08` | pivot moved, geometry bounds changed, topology changed | (9.030, 0.200, 9.883) | (9.030, 0.200, 10.517) |
| `SM_Trivia_Seat_R01_-1_09` | removed | (9.890, 0.200, 9.883) | - |
| `SM_Trivia_Seat_R01_-1_10` | removed | (10.750, 0.200, 9.883) | - |
| `SM_Trivia_Seat_R01_1_00` | pivot moved, geometry bounds changed, topology changed | (-2.150, 0.200, 9.883) | (-2.150, 0.200, 10.517) |
| `SM_Trivia_Seat_R01_1_01` | pivot moved, geometry bounds changed, topology changed | (-3.010, 0.200, 9.883) | (-3.010, 0.200, 10.517) |
| `SM_Trivia_Seat_R01_1_02` | pivot moved, geometry bounds changed, topology changed | (-3.870, 0.200, 9.883) | (-3.870, 0.200, 10.517) |
| `SM_Trivia_Seat_R01_1_03` | pivot moved, geometry bounds changed, topology changed | (-4.730, 0.200, 9.883) | (-4.730, 0.200, 10.517) |
| `SM_Trivia_Seat_R01_1_04` | pivot moved, geometry bounds changed, topology changed | (-5.590, 0.200, 9.883) | (-5.590, 0.200, 10.517) |
| `SM_Trivia_Seat_R01_1_05` | pivot moved, geometry bounds changed, topology changed | (-6.450, 0.200, 9.883) | (-6.450, 0.200, 10.517) |
| `SM_Trivia_Seat_R01_1_06` | pivot moved, geometry bounds changed, topology changed | (-7.310, 0.200, 9.883) | (-7.310, 0.200, 10.517) |
| `SM_Trivia_Seat_R01_1_07` | pivot moved, geometry bounds changed, topology changed | (-8.170, 0.200, 9.883) | (-8.170, 0.200, 10.517) |
| `SM_Trivia_Seat_R01_1_08` | pivot moved, geometry bounds changed, topology changed | (-9.030, 0.200, 9.883) | (-9.030, 0.200, 10.517) |
| `SM_Trivia_Seat_R01_1_09` | removed | (-9.890, 0.200, 9.883) | - |
| `SM_Trivia_Seat_R01_1_10` | removed | (-10.750, 0.200, 9.883) | - |
| `SM_Trivia_Seat_R02_-1_00` | pivot moved, geometry bounds changed, topology changed | (2.150, 0.400, 11.313) | (2.150, 0.400, 12.517) |
| `SM_Trivia_Seat_R02_-1_01` | pivot moved, geometry bounds changed, topology changed | (3.010, 0.400, 11.313) | (3.010, 0.400, 12.517) |
| `SM_Trivia_Seat_R02_-1_02` | pivot moved, geometry bounds changed, topology changed | (3.870, 0.400, 11.313) | (3.870, 0.400, 12.517) |
| `SM_Trivia_Seat_R02_-1_03` | pivot moved, geometry bounds changed, topology changed | (4.730, 0.400, 11.313) | (4.730, 0.400, 12.517) |
| `SM_Trivia_Seat_R02_-1_04` | pivot moved, geometry bounds changed, topology changed | (5.590, 0.400, 11.313) | (5.590, 0.400, 12.517) |
| `SM_Trivia_Seat_R02_-1_05` | pivot moved, geometry bounds changed, topology changed | (6.450, 0.400, 11.313) | (6.450, 0.400, 12.517) |
| `SM_Trivia_Seat_R02_-1_06` | pivot moved, geometry bounds changed, topology changed | (7.310, 0.400, 11.313) | (7.310, 0.400, 12.517) |
| `SM_Trivia_Seat_R02_-1_07` | pivot moved, geometry bounds changed, topology changed | (8.170, 0.400, 11.313) | (8.170, 0.400, 12.517) |
| `SM_Trivia_Seat_R02_-1_08` | pivot moved, geometry bounds changed, topology changed | (9.030, 0.400, 11.313) | (9.030, 0.400, 12.517) |
| `SM_Trivia_Seat_R02_-1_09` | removed | (9.890, 0.400, 11.313) | - |
| `SM_Trivia_Seat_R02_-1_10` | removed | (10.750, 0.400, 11.313) | - |
| `SM_Trivia_Seat_R02_1_00` | pivot moved, geometry bounds changed, topology changed | (-2.150, 0.400, 11.313) | (-2.150, 0.400, 12.517) |
| `SM_Trivia_Seat_R02_1_01` | pivot moved, geometry bounds changed, topology changed | (-3.010, 0.400, 11.313) | (-3.010, 0.400, 12.517) |
| `SM_Trivia_Seat_R02_1_02` | pivot moved, geometry bounds changed, topology changed | (-3.870, 0.400, 11.313) | (-3.870, 0.400, 12.517) |
| `SM_Trivia_Seat_R02_1_03` | pivot moved, geometry bounds changed, topology changed | (-4.730, 0.400, 11.313) | (-4.730, 0.400, 12.517) |
| `SM_Trivia_Seat_R02_1_04` | pivot moved, geometry bounds changed, topology changed | (-5.590, 0.400, 11.313) | (-5.590, 0.400, 12.517) |
| `SM_Trivia_Seat_R02_1_05` | pivot moved, geometry bounds changed, topology changed | (-6.450, 0.400, 11.313) | (-6.450, 0.400, 12.517) |
| `SM_Trivia_Seat_R02_1_06` | pivot moved, geometry bounds changed, topology changed | (-7.310, 0.400, 11.313) | (-7.310, 0.400, 12.517) |
| `SM_Trivia_Seat_R02_1_07` | pivot moved, geometry bounds changed, topology changed | (-8.170, 0.400, 11.313) | (-8.170, 0.400, 12.517) |
| `SM_Trivia_Seat_R02_1_08` | pivot moved, geometry bounds changed, topology changed | (-9.030, 0.400, 11.313) | (-9.030, 0.400, 12.517) |
| `SM_Trivia_Seat_R02_1_09` | removed | (-9.890, 0.400, 11.313) | - |
| `SM_Trivia_Seat_R02_1_10` | removed | (-10.750, 0.400, 11.313) | - |
| `SM_Trivia_Seat_R03_-1_00` | pivot moved, geometry bounds changed, topology changed | (2.150, 0.600, 12.743) | (2.150, 0.600, 14.517) |
| `SM_Trivia_Seat_R03_-1_01` | pivot moved, geometry bounds changed, topology changed | (3.010, 0.600, 12.743) | (3.010, 0.600, 14.517) |
| `SM_Trivia_Seat_R03_-1_02` | pivot moved, geometry bounds changed, topology changed | (3.870, 0.600, 12.743) | (3.870, 0.600, 14.517) |
| `SM_Trivia_Seat_R03_-1_03` | pivot moved, geometry bounds changed, topology changed | (4.730, 0.600, 12.743) | (4.730, 0.600, 14.517) |
| `SM_Trivia_Seat_R03_-1_04` | pivot moved, geometry bounds changed, topology changed | (5.590, 0.600, 12.743) | (5.590, 0.600, 14.517) |
| `SM_Trivia_Seat_R03_-1_05` | pivot moved, geometry bounds changed, topology changed | (6.450, 0.600, 12.743) | (6.450, 0.600, 14.517) |
| `SM_Trivia_Seat_R03_-1_06` | pivot moved, geometry bounds changed, topology changed | (7.310, 0.600, 12.743) | (7.310, 0.600, 14.517) |
| `SM_Trivia_Seat_R03_-1_07` | pivot moved, geometry bounds changed, topology changed | (8.170, 0.600, 12.743) | (8.170, 0.600, 14.517) |
| `SM_Trivia_Seat_R03_-1_08` | pivot moved, geometry bounds changed, topology changed | (9.030, 0.600, 12.743) | (9.030, 0.600, 14.517) |
| `SM_Trivia_Seat_R03_-1_09` | removed | (9.890, 0.600, 12.743) | - |
| `SM_Trivia_Seat_R03_-1_10` | removed | (10.750, 0.600, 12.743) | - |
| `SM_Trivia_Seat_R03_1_00` | pivot moved, geometry bounds changed, topology changed | (-2.150, 0.600, 12.743) | (-2.150, 0.600, 14.517) |
| `SM_Trivia_Seat_R03_1_01` | pivot moved, geometry bounds changed, topology changed | (-3.010, 0.600, 12.743) | (-3.010, 0.600, 14.517) |
| `SM_Trivia_Seat_R03_1_02` | pivot moved, geometry bounds changed, topology changed | (-3.870, 0.600, 12.743) | (-3.870, 0.600, 14.517) |
| `SM_Trivia_Seat_R03_1_03` | pivot moved, geometry bounds changed, topology changed | (-4.730, 0.600, 12.743) | (-4.730, 0.600, 14.517) |
| `SM_Trivia_Seat_R03_1_04` | pivot moved, geometry bounds changed, topology changed | (-5.590, 0.600, 12.743) | (-5.590, 0.600, 14.517) |
| `SM_Trivia_Seat_R03_1_05` | pivot moved, geometry bounds changed, topology changed | (-6.450, 0.600, 12.743) | (-6.450, 0.600, 14.517) |
| `SM_Trivia_Seat_R03_1_06` | pivot moved, geometry bounds changed, topology changed | (-7.310, 0.600, 12.743) | (-7.310, 0.600, 14.517) |
| `SM_Trivia_Seat_R03_1_07` | pivot moved, geometry bounds changed, topology changed | (-8.170, 0.600, 12.743) | (-8.170, 0.600, 14.517) |
| `SM_Trivia_Seat_R03_1_08` | pivot moved, geometry bounds changed, topology changed | (-9.030, 0.600, 12.743) | (-9.030, 0.600, 14.517) |
| `SM_Trivia_Seat_R03_1_09` | removed | (-9.890, 0.600, 12.743) | - |
| `SM_Trivia_Seat_R03_1_10` | removed | (-10.750, 0.600, 12.743) | - |
| `SM_Trivia_Seat_R04_-1_00` | pivot moved, geometry bounds changed, topology changed | (2.150, 0.800, 14.237) | (2.150, 0.800, 16.517) |
| `SM_Trivia_Seat_R04_-1_01` | pivot moved, geometry bounds changed, topology changed | (3.010, 0.800, 14.237) | (3.010, 0.800, 16.517) |
| `SM_Trivia_Seat_R04_-1_02` | pivot moved, geometry bounds changed, topology changed | (3.870, 0.800, 14.237) | (3.870, 0.800, 16.517) |
| `SM_Trivia_Seat_R04_-1_03` | pivot moved, geometry bounds changed, topology changed | (4.730, 0.800, 14.237) | (4.730, 0.800, 16.517) |
| `SM_Trivia_Seat_R04_-1_04` | pivot moved, geometry bounds changed, topology changed | (5.590, 0.800, 14.237) | (5.590, 0.800, 16.517) |
| `SM_Trivia_Seat_R04_-1_05` | pivot moved, geometry bounds changed, topology changed | (6.450, 0.800, 14.237) | (6.450, 0.800, 16.517) |
| `SM_Trivia_Seat_R04_-1_06` | pivot moved, geometry bounds changed, topology changed | (7.310, 0.800, 14.237) | (7.310, 0.800, 16.517) |
| `SM_Trivia_Seat_R04_-1_07` | pivot moved, geometry bounds changed, topology changed | (8.170, 0.800, 14.237) | (8.170, 0.800, 16.517) |
| `SM_Trivia_Seat_R04_-1_08` | pivot moved, geometry bounds changed, topology changed | (9.030, 0.800, 14.237) | (9.030, 0.800, 16.517) |
| `SM_Trivia_Seat_R04_-1_09` | removed | (9.890, 0.800, 14.237) | - |
| `SM_Trivia_Seat_R04_-1_10` | removed | (10.750, 0.800, 14.237) | - |
| `SM_Trivia_Seat_R04_1_00` | pivot moved, geometry bounds changed, topology changed | (-2.150, 0.800, 14.237) | (-2.150, 0.800, 16.517) |
| `SM_Trivia_Seat_R04_1_01` | pivot moved, geometry bounds changed, topology changed | (-3.010, 0.800, 14.237) | (-3.010, 0.800, 16.517) |
| `SM_Trivia_Seat_R04_1_02` | pivot moved, geometry bounds changed, topology changed | (-3.870, 0.800, 14.237) | (-3.870, 0.800, 16.517) |
| `SM_Trivia_Seat_R04_1_03` | pivot moved, geometry bounds changed, topology changed | (-4.730, 0.800, 14.237) | (-4.730, 0.800, 16.517) |
| `SM_Trivia_Seat_R04_1_04` | pivot moved, geometry bounds changed, topology changed | (-5.590, 0.800, 14.237) | (-5.590, 0.800, 16.517) |
| `SM_Trivia_Seat_R04_1_05` | pivot moved, geometry bounds changed, topology changed | (-6.450, 0.800, 14.237) | (-6.450, 0.800, 16.517) |
| `SM_Trivia_Seat_R04_1_06` | pivot moved, geometry bounds changed, topology changed | (-7.310, 0.800, 14.237) | (-7.310, 0.800, 16.517) |
| `SM_Trivia_Seat_R04_1_07` | pivot moved, geometry bounds changed, topology changed | (-8.170, 0.800, 14.237) | (-8.170, 0.800, 16.517) |
| `SM_Trivia_Seat_R04_1_08` | pivot moved, geometry bounds changed, topology changed | (-9.030, 0.800, 14.237) | (-9.030, 0.800, 16.517) |
| `SM_Trivia_Seat_R04_1_09` | removed | (-9.890, 0.800, 14.237) | - |
| `SM_Trivia_Seat_R04_1_10` | removed | (-10.750, 0.800, 14.237) | - |
| `SM_Trivia_Seat_R05_-1_00` | pivot moved, geometry bounds changed, topology changed | (2.150, 1.000, 15.667) | (2.150, 1.000, 18.517) |
| `SM_Trivia_Seat_R05_-1_01` | pivot moved, geometry bounds changed, topology changed | (3.010, 1.000, 15.667) | (3.010, 1.000, 18.517) |
| `SM_Trivia_Seat_R05_-1_02` | pivot moved, geometry bounds changed, topology changed | (3.870, 1.000, 15.667) | (3.870, 1.000, 18.517) |
| `SM_Trivia_Seat_R05_-1_03` | pivot moved, geometry bounds changed, topology changed | (4.730, 1.000, 15.667) | (4.730, 1.000, 18.517) |
| `SM_Trivia_Seat_R05_-1_04` | pivot moved, geometry bounds changed, topology changed | (5.590, 1.000, 15.667) | (5.590, 1.000, 18.517) |
| `SM_Trivia_Seat_R05_-1_05` | pivot moved, geometry bounds changed, topology changed | (6.450, 1.000, 15.667) | (6.450, 1.000, 18.517) |
| `SM_Trivia_Seat_R05_-1_06` | pivot moved, geometry bounds changed, topology changed | (7.310, 1.000, 15.667) | (7.310, 1.000, 18.517) |
| `SM_Trivia_Seat_R05_-1_07` | pivot moved, geometry bounds changed, topology changed | (8.170, 1.000, 15.667) | (8.170, 1.000, 18.517) |
| `SM_Trivia_Seat_R05_-1_08` | pivot moved, geometry bounds changed, topology changed | (9.030, 1.000, 15.667) | (9.030, 1.000, 18.517) |
| `SM_Trivia_Seat_R05_-1_09` | removed | (9.890, 1.000, 15.667) | - |
| `SM_Trivia_Seat_R05_-1_10` | removed | (10.750, 1.000, 15.667) | - |
| `SM_Trivia_Seat_R05_1_00` | pivot moved, geometry bounds changed, topology changed | (-2.150, 1.000, 15.667) | (-2.150, 1.000, 18.517) |
| `SM_Trivia_Seat_R05_1_01` | pivot moved, geometry bounds changed, topology changed | (-3.010, 1.000, 15.667) | (-3.010, 1.000, 18.517) |
| `SM_Trivia_Seat_R05_1_02` | pivot moved, geometry bounds changed, topology changed | (-3.870, 1.000, 15.667) | (-3.870, 1.000, 18.517) |
| `SM_Trivia_Seat_R05_1_03` | pivot moved, geometry bounds changed, topology changed | (-4.730, 1.000, 15.667) | (-4.730, 1.000, 18.517) |
| `SM_Trivia_Seat_R05_1_04` | pivot moved, geometry bounds changed, topology changed | (-5.590, 1.000, 15.667) | (-5.590, 1.000, 18.517) |
| `SM_Trivia_Seat_R05_1_05` | pivot moved, geometry bounds changed, topology changed | (-6.450, 1.000, 15.667) | (-6.450, 1.000, 18.517) |
| `SM_Trivia_Seat_R05_1_06` | pivot moved, geometry bounds changed, topology changed | (-7.310, 1.000, 15.667) | (-7.310, 1.000, 18.517) |
| `SM_Trivia_Seat_R05_1_07` | pivot moved, geometry bounds changed, topology changed | (-8.170, 1.000, 15.667) | (-8.170, 1.000, 18.517) |
| `SM_Trivia_Seat_R05_1_08` | pivot moved, geometry bounds changed, topology changed | (-9.030, 1.000, 15.667) | (-9.030, 1.000, 18.517) |
| `SM_Trivia_Seat_R05_1_09` | removed | (-9.890, 1.000, 15.667) | - |
| `SM_Trivia_Seat_R05_1_10` | removed | (-10.750, 1.000, 15.667) | - |
| `SM_Trivia_Seat_R06_-1_00` | pivot moved, geometry bounds changed, topology changed | (2.150, 1.200, 17.097) | (2.150, 1.200, 20.517) |
| `SM_Trivia_Seat_R06_-1_01` | pivot moved, geometry bounds changed, topology changed | (3.010, 1.200, 17.097) | (3.010, 1.200, 20.517) |
| `SM_Trivia_Seat_R06_-1_02` | pivot moved, geometry bounds changed, topology changed | (3.870, 1.200, 17.097) | (3.870, 1.200, 20.517) |
| `SM_Trivia_Seat_R06_-1_03` | pivot moved, geometry bounds changed, topology changed | (4.730, 1.200, 17.097) | (4.730, 1.200, 20.517) |
| `SM_Trivia_Seat_R06_-1_04` | pivot moved, geometry bounds changed, topology changed | (5.590, 1.200, 17.097) | (5.590, 1.200, 20.517) |
| `SM_Trivia_Seat_R06_-1_05` | pivot moved, geometry bounds changed, topology changed | (6.450, 1.200, 17.097) | (6.450, 1.200, 20.517) |
| `SM_Trivia_Seat_R06_-1_06` | pivot moved, geometry bounds changed, topology changed | (7.310, 1.200, 17.097) | (7.310, 1.200, 20.517) |
| `SM_Trivia_Seat_R06_-1_07` | pivot moved, geometry bounds changed, topology changed | (8.170, 1.200, 17.097) | (8.170, 1.200, 20.517) |
| `SM_Trivia_Seat_R06_-1_08` | pivot moved, geometry bounds changed, topology changed | (9.030, 1.200, 17.097) | (9.030, 1.200, 20.517) |
| `SM_Trivia_Seat_R06_-1_09` | removed | (9.890, 1.200, 17.097) | - |
| `SM_Trivia_Seat_R06_-1_10` | removed | (10.750, 1.200, 17.097) | - |
| `SM_Trivia_Seat_R06_1_00` | pivot moved, geometry bounds changed, topology changed | (-2.150, 1.200, 17.097) | (-2.150, 1.200, 20.517) |
| `SM_Trivia_Seat_R06_1_01` | pivot moved, geometry bounds changed, topology changed | (-3.010, 1.200, 17.097) | (-3.010, 1.200, 20.517) |
| `SM_Trivia_Seat_R06_1_02` | pivot moved, geometry bounds changed, topology changed | (-3.870, 1.200, 17.097) | (-3.870, 1.200, 20.517) |
| `SM_Trivia_Seat_R06_1_03` | pivot moved, geometry bounds changed, topology changed | (-4.730, 1.200, 17.097) | (-4.730, 1.200, 20.517) |
| `SM_Trivia_Seat_R06_1_04` | pivot moved, geometry bounds changed, topology changed | (-5.590, 1.200, 17.097) | (-5.590, 1.200, 20.517) |
| `SM_Trivia_Seat_R06_1_05` | pivot moved, geometry bounds changed, topology changed | (-6.450, 1.200, 17.097) | (-6.450, 1.200, 20.517) |
| `SM_Trivia_Seat_R06_1_06` | pivot moved, geometry bounds changed, topology changed | (-7.310, 1.200, 17.097) | (-7.310, 1.200, 20.517) |
| `SM_Trivia_Seat_R06_1_07` | pivot moved, geometry bounds changed, topology changed | (-8.170, 1.200, 17.097) | (-8.170, 1.200, 20.517) |
| `SM_Trivia_Seat_R06_1_08` | pivot moved, geometry bounds changed, topology changed | (-9.030, 1.200, 17.097) | (-9.030, 1.200, 20.517) |
| `SM_Trivia_Seat_R06_1_09` | removed | (-9.890, 1.200, 17.097) | - |
| `SM_Trivia_Seat_R06_1_10` | removed | (-10.750, 1.200, 17.097) | - |
| `SM_Trivia_Seat_R07_-1_00` | removed | (2.150, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_-1_01` | removed | (3.010, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_-1_02` | removed | (3.870, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_-1_03` | removed | (4.730, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_-1_04` | removed | (5.590, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_-1_05` | removed | (6.450, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_-1_06` | removed | (7.310, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_-1_07` | removed | (8.170, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_-1_08` | removed | (9.030, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_-1_09` | removed | (9.890, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_-1_10` | removed | (10.750, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_1_00` | removed | (-2.150, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_1_01` | removed | (-3.010, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_1_02` | removed | (-3.870, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_1_03` | removed | (-4.730, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_1_04` | removed | (-5.590, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_1_05` | removed | (-6.450, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_1_06` | removed | (-7.310, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_1_07` | removed | (-8.170, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_1_08` | removed | (-9.030, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_1_09` | removed | (-9.890, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R07_1_10` | removed | (-10.750, 1.400, 18.527) | - |
| `SM_Trivia_Seat_R08_-1_00` | removed | (2.150, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_-1_01` | removed | (3.010, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_-1_02` | removed | (3.870, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_-1_03` | removed | (4.730, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_-1_04` | removed | (5.590, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_-1_05` | removed | (6.450, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_-1_06` | removed | (7.310, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_-1_07` | removed | (8.170, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_-1_08` | removed | (9.030, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_-1_09` | removed | (9.890, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_-1_10` | removed | (10.750, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_1_00` | removed | (-2.150, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_1_01` | removed | (-3.010, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_1_02` | removed | (-3.870, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_1_03` | removed | (-4.730, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_1_04` | removed | (-5.590, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_1_05` | removed | (-6.450, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_1_06` | removed | (-7.310, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_1_07` | removed | (-8.170, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_1_08` | removed | (-9.030, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_1_09` | removed | (-9.890, 1.600, 19.957) | - |
| `SM_Trivia_Seat_R08_1_10` | removed | (-10.750, 1.600, 19.957) | - |
| `SM_Trivia_Shield` | geometry bounds changed, topology changed | (0.000, 0.000, 2.000) | (0.000, 0.000, 2.000) |
| `SM_Trivia_ShieldBorder` | geometry bounds changed, topology changed | (0.000, 0.000, 2.000) | (0.000, 0.000, 2.000) |
| `SM_Trivia_SidePanelVertical` | geometry bounds changed | (14.320, 2.300, 23.550) | (14.320, 2.300, 23.550) |
| `SM_Trivia_Spotlight_01_Details` | pivot moved, axes rotated, geometry bounds changed, topology changed | (8.250, 8.200, -1.360) | (7.848, 7.840, -1.199) |
| `SM_Trivia_Spotlight_01_Head` | pivot moved, axes rotated, geometry bounds changed | (8.000, 8.200, -1.360) | (8.000, 7.840, -1.000) |
| `SM_Trivia_Spotlight_01_Lens` | pivot moved, axes rotated, geometry bounds changed | (8.000, 8.290, -1.550) | (7.899, 7.649, -0.922) |
| `SM_Trivia_Spotlight_01_Yoke` | pivot moved, axes rotated, geometry bounds changed | (8.230, 8.020, -1.000) | (7.860, 8.020, -1.183) |
| `SM_Trivia_Spotlight_01_Yoke.001` | pivot moved, axes rotated, geometry bounds changed | (7.770, 8.020, -1.000) | (8.140, 8.020, -0.817) |
| `SM_Trivia_Spotlight_02_Details` | pivot moved, axes rotated, geometry bounds changed, topology changed | (5.950, 8.200, -1.360) | (5.517, 7.840, -1.170) |
| `SM_Trivia_Spotlight_02_Head` | pivot moved, axes rotated, geometry bounds changed | (5.700, 8.200, -1.360) | (5.700, 7.840, -1.000) |
| `SM_Trivia_Spotlight_02_Lens` | pivot moved, axes rotated, geometry bounds changed | (5.700, 8.290, -1.550) | (5.624, 7.639, -0.918) |
| `SM_Trivia_Spotlight_02_Yoke` | pivot moved, axes rotated, geometry bounds changed | (5.930, 8.020, -1.000) | (5.532, 8.020, -1.157) |
| `SM_Trivia_Spotlight_02_Yoke.001` | pivot moved, axes rotated, geometry bounds changed | (5.470, 8.020, -1.000) | (5.868, 8.020, -0.843) |
| `SM_Trivia_Spotlight_03_Details` | pivot moved, axes rotated, geometry bounds changed, topology changed | (3.650, 8.200, -1.360) | (3.181, 7.840, -1.121) |
| `SM_Trivia_Spotlight_03_Head` | pivot moved, axes rotated, geometry bounds changed | (3.400, 8.200, -1.360) | (3.400, 7.840, -1.000) |
| `SM_Trivia_Spotlight_03_Lens` | pivot moved, axes rotated, geometry bounds changed | (3.400, 8.290, -1.550) | (3.353, 7.631, -0.915) |
| `SM_Trivia_Spotlight_03_Yoke` | pivot moved, axes rotated, geometry bounds changed | (3.630, 8.020, -1.000) | (3.199, 8.020, -1.112) |
| `SM_Trivia_Spotlight_03_Yoke.001` | pivot moved, axes rotated, geometry bounds changed | (3.170, 8.020, -1.000) | (3.601, 8.020, -0.888) |
| `SM_Trivia_Spotlight_04_Details` | pivot moved, axes rotated, geometry bounds changed, topology changed | (1.350, 8.200, -1.360) | (0.854, 7.840, -1.044) |
| `SM_Trivia_Spotlight_04_Head` | pivot moved, axes rotated, geometry bounds changed | (1.100, 8.200, -1.360) | (1.100, 7.840, -1.000) |
| `SM_Trivia_Spotlight_04_Lens` | pivot moved, axes rotated, geometry bounds changed | (1.100, 8.290, -1.550) | (1.085, 7.627, -0.914) |
| `SM_Trivia_Spotlight_04_Yoke` | pivot moved, axes rotated, geometry bounds changed | (1.330, 8.020, -1.000) | (0.874, 8.020, -1.041) |
| `SM_Trivia_Spotlight_04_Yoke.001` | pivot moved, axes rotated, geometry bounds changed | (0.870, 8.020, -1.000) | (1.326, 8.020, -0.959) |
| `SM_Trivia_Spotlight_05_Details` | pivot moved, axes rotated, geometry bounds changed, topology changed | (-0.850, 8.200, -1.360) | (-1.346, 7.840, -0.956) |
| `SM_Trivia_Spotlight_05_Head` | pivot moved, axes rotated, geometry bounds changed | (-1.100, 8.200, -1.360) | (-1.100, 7.840, -1.000) |
| `SM_Trivia_Spotlight_05_Lens` | pivot moved, axes rotated, geometry bounds changed | (-1.100, 8.290, -1.550) | (-1.085, 7.627, -0.914) |
| `SM_Trivia_Spotlight_05_Yoke` | pivot moved, axes rotated, geometry bounds changed | (-0.870, 8.020, -1.000) | (-1.326, 8.020, -0.959) |
| `SM_Trivia_Spotlight_05_Yoke.001` | pivot moved, axes rotated, geometry bounds changed | (-1.330, 8.020, -1.000) | (-0.874, 8.020, -1.041) |
| `SM_Trivia_Spotlight_06_Details` | pivot moved, axes rotated, geometry bounds changed, topology changed | (-3.150, 8.200, -1.360) | (-3.619, 7.840, -0.879) |
| `SM_Trivia_Spotlight_06_Head` | pivot moved, axes rotated, geometry bounds changed | (-3.400, 8.200, -1.360) | (-3.400, 7.840, -1.000) |
| `SM_Trivia_Spotlight_06_Lens` | pivot moved, axes rotated, geometry bounds changed | (-3.400, 8.290, -1.550) | (-3.353, 7.631, -0.915) |
| `SM_Trivia_Spotlight_06_Yoke` | pivot moved, axes rotated, geometry bounds changed | (-3.170, 8.020, -1.000) | (-3.601, 8.020, -0.888) |
| `SM_Trivia_Spotlight_06_Yoke.001` | pivot moved, axes rotated, geometry bounds changed | (-3.630, 8.020, -1.000) | (-3.199, 8.020, -1.112) |
| `SM_Trivia_Spotlight_07_Details` | pivot moved, axes rotated, geometry bounds changed, topology changed | (-5.450, 8.200, -1.360) | (-5.883, 7.840, -0.830) |
| `SM_Trivia_Spotlight_07_Head` | pivot moved, axes rotated, geometry bounds changed | (-5.700, 8.200, -1.360) | (-5.700, 7.840, -1.000) |
| `SM_Trivia_Spotlight_07_Lens` | pivot moved, axes rotated, geometry bounds changed | (-5.700, 8.290, -1.550) | (-5.624, 7.639, -0.918) |
| `SM_Trivia_Spotlight_07_Yoke` | pivot moved, axes rotated, geometry bounds changed | (-5.470, 8.020, -1.000) | (-5.868, 8.020, -0.843) |
| `SM_Trivia_Spotlight_07_Yoke.001` | pivot moved, axes rotated, geometry bounds changed | (-5.930, 8.020, -1.000) | (-5.532, 8.020, -1.157) |
| `SM_Trivia_Spotlight_08_Details` | pivot moved, axes rotated, geometry bounds changed, topology changed | (-7.750, 8.200, -1.360) | (-8.152, 7.840, -0.801) |
| `SM_Trivia_Spotlight_08_Head` | pivot moved, axes rotated, geometry bounds changed | (-8.000, 8.200, -1.360) | (-8.000, 7.840, -1.000) |
| `SM_Trivia_Spotlight_08_Lens` | pivot moved, axes rotated, geometry bounds changed | (-8.000, 8.290, -1.550) | (-7.899, 7.649, -0.922) |
| `SM_Trivia_Spotlight_08_Yoke` | pivot moved, axes rotated, geometry bounds changed | (-7.770, 8.020, -1.000) | (-8.140, 8.020, -0.817) |
| `SM_Trivia_Spotlight_08_Yoke.001` | pivot moved, axes rotated, geometry bounds changed | (-8.230, 8.020, -1.000) | (-7.860, 8.020, -1.183) |
| `SM_Trivia_Spotlight_09_Base` | geometry bounds changed | (9.000, 0.740, 2.000) | (9.000, 0.740, 2.000) |
| `SM_Trivia_Spotlight_09_Details` | pivot moved, axes rotated, geometry bounds changed, topology changed | (9.250, 0.740, 2.360) | (9.243, 1.100, 1.942) |
| `SM_Trivia_Spotlight_09_Head` | pivot moved, axes rotated, geometry bounds changed | (9.000, 0.740, 2.360) | (9.000, 1.100, 2.000) |
| `SM_Trivia_Spotlight_09_Lens` | pivot moved, axes rotated, geometry bounds changed | (9.000, 0.630, 2.550) | (8.965, 1.272, 1.851) |
| `SM_Trivia_Spotlight_09_Yoke` | pivot moved, axes rotated, geometry bounds changed | (9.230, 0.920, 2.000) | (9.224, 0.920, 1.947) |
| `SM_Trivia_Spotlight_09_Yoke.001` | pivot moved, axes rotated, geometry bounds changed | (8.770, 0.920, 2.000) | (8.776, 0.920, 2.053) |
| `SM_Trivia_Spotlight_10_Base` | geometry bounds changed | (-9.000, 0.740, 2.000) | (-9.000, 0.740, 2.000) |
| `SM_Trivia_Spotlight_10_Details` | pivot moved, axes rotated, geometry bounds changed, topology changed | (-8.750, 0.740, 2.360) | (-8.757, 1.100, 2.058) |
| `SM_Trivia_Spotlight_10_Head` | pivot moved, axes rotated, geometry bounds changed | (-9.000, 0.740, 2.360) | (-9.000, 1.100, 2.000) |
| `SM_Trivia_Spotlight_10_Lens` | pivot moved, axes rotated, geometry bounds changed | (-9.000, 0.630, 2.550) | (-8.965, 1.272, 1.851) |
| `SM_Trivia_Spotlight_10_Yoke` | pivot moved, axes rotated, geometry bounds changed | (-8.770, 0.920, 2.000) | (-8.776, 0.920, 2.053) |
| `SM_Trivia_Spotlight_10_Yoke.001` | pivot moved, axes rotated, geometry bounds changed | (-9.230, 0.920, 2.000) | (-9.224, 0.920, 1.947) |
| `SM_Trivia_Spotlight_11_Base` | geometry bounds changed | (7.000, 0.740, -3.000) | (7.000, 0.740, -3.000) |
| `SM_Trivia_Spotlight_11_Details` | pivot moved, axes rotated, geometry bounds changed, topology changed | (7.250, 0.740, -2.640) | (7.158, 1.100, -3.194) |
| `SM_Trivia_Spotlight_11_Head` | pivot moved, axes rotated, geometry bounds changed | (7.000, 0.740, -2.640) | (7.000, 1.100, -3.000) |
| `SM_Trivia_Spotlight_11_Lens` | pivot moved, axes rotated, geometry bounds changed | (7.000, 0.630, -2.450) | (6.912, 1.300, -3.072) |
| `SM_Trivia_Spotlight_11_Yoke` | pivot moved, axes rotated, geometry bounds changed | (7.230, 0.920, -3.000) | (7.145, 0.920, -3.178) |
| `SM_Trivia_Spotlight_11_Yoke.001` | pivot moved, axes rotated, geometry bounds changed | (6.770, 0.920, -3.000) | (6.855, 0.920, -2.822) |
| `SM_Trivia_Spotlight_12_Base` | geometry bounds changed | (-7.000, 0.740, -3.000) | (-7.000, 0.740, -3.000) |
| `SM_Trivia_Spotlight_12_Details` | pivot moved, axes rotated, geometry bounds changed, topology changed | (-6.750, 0.740, -2.640) | (-6.842, 1.100, -2.806) |
| `SM_Trivia_Spotlight_12_Head` | pivot moved, axes rotated, geometry bounds changed | (-7.000, 0.740, -2.640) | (-7.000, 1.100, -3.000) |
| `SM_Trivia_Spotlight_12_Lens` | pivot moved, axes rotated, geometry bounds changed | (-7.000, 0.630, -2.450) | (-6.912, 1.300, -3.072) |
| `SM_Trivia_Spotlight_12_Yoke` | pivot moved, axes rotated, geometry bounds changed | (-6.770, 0.920, -3.000) | (-6.855, 0.920, -2.822) |
| `SM_Trivia_Spotlight_12_Yoke.001` | pivot moved, axes rotated, geometry bounds changed | (-7.230, 0.920, -3.000) | (-7.145, 0.920, -3.178) |
| `SM_Trivia_StageApronEdge` | topology changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_StageLanding` | added | - | (0.000, 0.000, 0.000) |
| `SM_Trivia_StageStep` | pivot moved, geometry bounds changed | (0.000, 0.100, 6.350) | (8.900, 0.100, 5.350) |
| `SM_Trivia_StepLight` | geometry bounds changed | (0.000, 0.000, 0.000) | (0.000, 0.000, 0.000) |
| `SM_Trivia_TasselCollar` | geometry bounds changed | (0.000, 0.000, 2.000) | (0.000, 0.000, 2.000) |
| `SM_Trivia_TasselCord_L` | added | - | (0.000, 0.000, 0.000) |
| `SM_Trivia_TasselCord_R` | added | - | (0.000, 0.000, 0.000) |
| `SM_Trivia_TasselKnot` | geometry bounds changed | (0.000, 0.000, 2.000) | (0.000, 0.000, 2.000) |
| `SM_Trivia_TasselSkirt` | geometry bounds changed | (0.000, 0.000, 2.000) | (0.000, 0.000, 2.000) |
| `SM_Trivia_TasselThread` | geometry bounds changed | (0.000, 0.000, 2.000) | (0.000, 0.000, 2.000) |
| `SM_Trivia_TiebackAnchor_L` | added | - | (0.000, 0.000, 0.000) |
| `SM_Trivia_TiebackAnchor_R` | added | - | (0.000, 0.000, 0.000) |
| `SM_Trivia_TiebackBoss_L` | added | - | (10.250, 3.120, -1.730) |
| `SM_Trivia_TiebackBoss_R` | added | - | (-10.250, 3.120, -1.730) |
| `SM_Trivia_TiebackKnot_L` | added | - | (9.522, 3.120, -1.651) |
| `SM_Trivia_TiebackKnot_R` | added | - | (-9.324, 3.120, -1.651) |
| `SM_Trivia_TrussClamp` | added | - | (8.000, 8.450, -1.800) |
| `SM_Trivia_TrussHanger` | added | - | (0.000, 0.000, 0.000) |
| `SM_Trivia_UpperSeat_-1_0_0` | pivot moved, geometry bounds changed, topology changed | (2.300, 4.850, 24.217) | (1.800, 4.800, 23.317) |
| `SM_Trivia_UpperSeat_-1_0_1` | pivot moved, geometry bounds changed, topology changed | (3.150, 4.850, 24.217) | (2.660, 4.800, 23.317) |
| `SM_Trivia_UpperSeat_-1_0_10` | removed | (10.800, 4.850, 24.217) | - |
| `SM_Trivia_UpperSeat_-1_0_2` | pivot moved, geometry bounds changed, topology changed | (4.000, 4.850, 24.217) | (3.520, 4.800, 23.317) |
| `SM_Trivia_UpperSeat_-1_0_3` | pivot moved, geometry bounds changed, topology changed | (4.850, 4.850, 24.217) | (4.380, 4.800, 23.317) |
| `SM_Trivia_UpperSeat_-1_0_4` | pivot moved, geometry bounds changed, topology changed | (5.700, 4.850, 24.217) | (5.240, 4.800, 23.317) |
| `SM_Trivia_UpperSeat_-1_0_5` | pivot moved, geometry bounds changed, topology changed | (6.550, 4.850, 24.217) | (6.100, 4.800, 23.317) |
| `SM_Trivia_UpperSeat_-1_0_6` | pivot moved, geometry bounds changed, topology changed | (7.400, 4.850, 24.217) | (6.960, 4.800, 23.317) |
| `SM_Trivia_UpperSeat_-1_0_7` | pivot moved, geometry bounds changed, topology changed | (8.250, 4.850, 24.217) | (7.820, 4.800, 23.317) |
| `SM_Trivia_UpperSeat_-1_0_8` | pivot moved, geometry bounds changed, topology changed | (9.100, 4.850, 24.217) | (8.680, 4.800, 23.317) |
| `SM_Trivia_UpperSeat_-1_0_9` | removed | (9.950, 4.850, 24.217) | - |
| `SM_Trivia_UpperSeat_-1_1_0` | removed | (2.300, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_-1_1_1` | removed | (3.150, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_-1_1_10` | removed | (10.800, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_-1_1_2` | removed | (4.000, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_-1_1_3` | removed | (4.850, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_-1_1_4` | removed | (5.700, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_-1_1_5` | removed | (6.550, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_-1_1_6` | removed | (7.400, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_-1_1_7` | removed | (8.250, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_-1_1_8` | removed | (9.100, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_-1_1_9` | removed | (9.950, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_1_0_0` | pivot moved, geometry bounds changed, topology changed | (-2.300, 4.850, 24.217) | (-1.800, 4.800, 23.317) |
| `SM_Trivia_UpperSeat_1_0_1` | pivot moved, geometry bounds changed, topology changed | (-3.150, 4.850, 24.217) | (-2.660, 4.800, 23.317) |
| `SM_Trivia_UpperSeat_1_0_10` | removed | (-10.800, 4.850, 24.217) | - |
| `SM_Trivia_UpperSeat_1_0_2` | pivot moved, geometry bounds changed, topology changed | (-4.000, 4.850, 24.217) | (-3.520, 4.800, 23.317) |
| `SM_Trivia_UpperSeat_1_0_3` | pivot moved, geometry bounds changed, topology changed | (-4.850, 4.850, 24.217) | (-4.380, 4.800, 23.317) |
| `SM_Trivia_UpperSeat_1_0_4` | pivot moved, geometry bounds changed, topology changed | (-5.700, 4.850, 24.217) | (-5.240, 4.800, 23.317) |
| `SM_Trivia_UpperSeat_1_0_5` | pivot moved, geometry bounds changed, topology changed | (-6.550, 4.850, 24.217) | (-6.100, 4.800, 23.317) |
| `SM_Trivia_UpperSeat_1_0_6` | pivot moved, geometry bounds changed, topology changed | (-7.400, 4.850, 24.217) | (-6.960, 4.800, 23.317) |
| `SM_Trivia_UpperSeat_1_0_7` | pivot moved, geometry bounds changed, topology changed | (-8.250, 4.850, 24.217) | (-7.820, 4.800, 23.317) |
| `SM_Trivia_UpperSeat_1_0_8` | pivot moved, geometry bounds changed, topology changed | (-9.100, 4.850, 24.217) | (-8.680, 4.800, 23.317) |
| `SM_Trivia_UpperSeat_1_0_9` | removed | (-9.950, 4.850, 24.217) | - |
| `SM_Trivia_UpperSeat_1_1_0` | removed | (-2.300, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_1_1_1` | removed | (-3.150, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_1_1_10` | removed | (-10.800, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_1_1_2` | removed | (-4.000, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_1_1_3` | removed | (-4.850, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_1_1_4` | removed | (-5.700, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_1_1_5` | removed | (-6.550, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_1_1_6` | removed | (-7.400, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_1_1_7` | removed | (-8.250, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_1_1_8` | removed | (-9.100, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_1_1_9` | removed | (-9.950, 5.050, 25.217) | - |
| `SM_Trivia_UpperSeat_Side_-1_0_0` | removed | (13.250, 4.850, 10.217) | - |
| `SM_Trivia_UpperSeat_Side_-1_0_1` | removed | (13.250, 4.850, 9.267) | - |
| `SM_Trivia_UpperSeat_Side_-1_0_10` | removed | (13.250, 4.850, 0.717) | - |
| `SM_Trivia_UpperSeat_Side_-1_0_11` | removed | (13.250, 4.850, -0.233) | - |
| `SM_Trivia_UpperSeat_Side_-1_0_2` | removed | (13.250, 4.850, 8.317) | - |
| `SM_Trivia_UpperSeat_Side_-1_0_3` | removed | (13.250, 4.850, 7.367) | - |
| `SM_Trivia_UpperSeat_Side_-1_0_4` | removed | (13.250, 4.850, 6.417) | - |
| `SM_Trivia_UpperSeat_Side_-1_0_5` | removed | (13.250, 4.850, 5.467) | - |
| `SM_Trivia_UpperSeat_Side_-1_0_6` | removed | (13.250, 4.850, 4.517) | - |
| `SM_Trivia_UpperSeat_Side_-1_0_7` | removed | (13.250, 4.850, 3.567) | - |
| `SM_Trivia_UpperSeat_Side_-1_0_8` | removed | (13.250, 4.850, 2.617) | - |
| `SM_Trivia_UpperSeat_Side_-1_0_9` | removed | (13.250, 4.850, 1.667) | - |
| `SM_Trivia_UpperSeat_Side_-1_1_0` | removed | (13.900, 5.050, 10.217) | - |
| `SM_Trivia_UpperSeat_Side_-1_1_1` | removed | (13.900, 5.050, 9.267) | - |
| `SM_Trivia_UpperSeat_Side_-1_1_10` | removed | (13.900, 5.050, 0.717) | - |
| `SM_Trivia_UpperSeat_Side_-1_1_11` | removed | (13.900, 5.050, -0.233) | - |
| `SM_Trivia_UpperSeat_Side_-1_1_2` | removed | (13.900, 5.050, 8.317) | - |
| `SM_Trivia_UpperSeat_Side_-1_1_3` | removed | (13.900, 5.050, 7.367) | - |
| `SM_Trivia_UpperSeat_Side_-1_1_4` | removed | (13.900, 5.050, 6.417) | - |
| `SM_Trivia_UpperSeat_Side_-1_1_5` | removed | (13.900, 5.050, 5.467) | - |
| `SM_Trivia_UpperSeat_Side_-1_1_6` | removed | (13.900, 5.050, 4.517) | - |
| `SM_Trivia_UpperSeat_Side_-1_1_7` | removed | (13.900, 5.050, 3.567) | - |
| `SM_Trivia_UpperSeat_Side_-1_1_8` | removed | (13.900, 5.050, 2.617) | - |
| `SM_Trivia_UpperSeat_Side_-1_1_9` | removed | (13.900, 5.050, 1.667) | - |
| `SM_Trivia_UpperSeat_Side_1_0_0` | removed | (-13.250, 4.850, 10.217) | - |
| `SM_Trivia_UpperSeat_Side_1_0_1` | removed | (-13.250, 4.850, 9.267) | - |
| `SM_Trivia_UpperSeat_Side_1_0_10` | removed | (-13.250, 4.850, 0.717) | - |
| `SM_Trivia_UpperSeat_Side_1_0_11` | removed | (-13.250, 4.850, -0.233) | - |
| `SM_Trivia_UpperSeat_Side_1_0_2` | removed | (-13.250, 4.850, 8.317) | - |
| `SM_Trivia_UpperSeat_Side_1_0_3` | removed | (-13.250, 4.850, 7.367) | - |
| `SM_Trivia_UpperSeat_Side_1_0_4` | removed | (-13.250, 4.850, 6.417) | - |
| `SM_Trivia_UpperSeat_Side_1_0_5` | removed | (-13.250, 4.850, 5.467) | - |
| `SM_Trivia_UpperSeat_Side_1_0_6` | removed | (-13.250, 4.850, 4.517) | - |
| `SM_Trivia_UpperSeat_Side_1_0_7` | removed | (-13.250, 4.850, 3.567) | - |
| `SM_Trivia_UpperSeat_Side_1_0_8` | removed | (-13.250, 4.850, 2.617) | - |
| `SM_Trivia_UpperSeat_Side_1_0_9` | removed | (-13.250, 4.850, 1.667) | - |
| `SM_Trivia_UpperSeat_Side_1_1_0` | removed | (-13.900, 5.050, 10.217) | - |
| `SM_Trivia_UpperSeat_Side_1_1_1` | removed | (-13.900, 5.050, 9.267) | - |
| `SM_Trivia_UpperSeat_Side_1_1_10` | removed | (-13.900, 5.050, 0.717) | - |
| `SM_Trivia_UpperSeat_Side_1_1_11` | removed | (-13.900, 5.050, -0.233) | - |
| `SM_Trivia_UpperSeat_Side_1_1_2` | removed | (-13.900, 5.050, 8.317) | - |
| `SM_Trivia_UpperSeat_Side_1_1_3` | removed | (-13.900, 5.050, 7.367) | - |
| `SM_Trivia_UpperSeat_Side_1_1_4` | removed | (-13.900, 5.050, 6.417) | - |
| `SM_Trivia_UpperSeat_Side_1_1_5` | removed | (-13.900, 5.050, 5.467) | - |
| `SM_Trivia_UpperSeat_Side_1_1_6` | removed | (-13.900, 5.050, 4.517) | - |
| `SM_Trivia_UpperSeat_Side_1_1_7` | removed | (-13.900, 5.050, 3.567) | - |
| `SM_Trivia_UpperSeat_Side_1_1_8` | removed | (-13.900, 5.050, 2.617) | - |
| `SM_Trivia_UpperSeat_Side_1_1_9` | removed | (-13.900, 5.050, 1.667) | - |
| `SM_Trivia_WallLantern` | geometry bounds changed, topology changed | (13.750, 6.000, -2.160) | (13.750, 6.000, -2.160) |
