echo Converting LOD_0...

if exist ..\..\Assets\Terrains\SwissAlps\lod_0\ rmdir /s /q ..\..\Assets\Terrains\SwissAlps\lod_0\
mkdir ..\..\Assets\Terrains\SwissAlps\lod_0\

convert .\lod_0\terrain_h.png -crop 8x8+1+1@ +repage -endian LSB gray:..\..\Assets\Terrains\SwissAlps\lod_0\terrain_h_%%02d.raw
convert .\lod_0\terrain_c.png -crop 8x8@ +repage	..\..\Assets\Terrains\SwissAlps\lod_0\terrain_c_%%02d.png
convert .\lod_0\terrain_d.png -crop 8x8@ +repage	..\..\Assets\Terrains\SwissAlps\lod_0\terrain_d_%%02d.png
convert .\lod_0\terrain_n.png -crop 8x8@ +repage 	..\..\Assets\Terrains\SwissAlps\lod_0\terrain_n_%%02d.png
convert .\lod_0\terrain_s.png -crop 8x8@ +repage 	..\..\Assets\Terrains\SwissAlps\lod_0\terrain_s_%%02d.png
convert .\lod_0\terrain_t.png -crop 8x8@ +repage 	..\..\Assets\Terrains\SwissAlps\lod_0\terrain_t_%%02d.png

pause