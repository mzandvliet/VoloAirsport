echo Converting LOD_0...

if exist ..\..\Assets\Terrains\SwissAlps\lod_1\ rmdir /s /q ..\..\Assets\Terrains\SwissAlps\lod_1\
mkdir ..\..\Assets\Terrains\SwissAlps\lod_1\

convert .\lod_1\terrain_h.png -crop 3x3+1+1@ +repage -endian LSB gray:..\..\Assets\Terrains\SwissAlps\lod_1\terrain_h_%%02d.raw
convert .\lod_1\terrain_c.png -crop 3x3@ +repage	..\..\Assets\Terrains\SwissAlps\lod_1\terrain_c_%%02d.png
convert .\lod_1\terrain_n.png -crop 3x3@ +repage 	..\..\Assets\Terrains\SwissAlps\lod_1\terrain_n_%%02d.png
convert .\lod_1\terrain_s.png -crop 3x3@ +repage 	..\..\Assets\Terrains\SwissAlps\lod_1\terrain_s_%%02d.png

del ..\..\Assets\Terrains\SwissAlps\lod_1\terrain_*_04.*

pause