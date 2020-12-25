using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RamjetAnvil.Unity.Landmass;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

// Todo: You forget the extra S in the name. :/
public class LandmasImporter {
    private static LandmasImporter _instance;

    public static LandmasImporter Instance {
        get { return _instance ?? (_instance = new LandmasImporter()); }
    }

    #region Heightmaps

    public static float[,] GetHeights(TerrainData terrainData, int res) {
        float[,] heights = new float[res, res];
        for (int x = 0; x < res; x++) {
            for (int z = 0; z < res; z++) {
                float xUnit = x / (float)res;
                float zUnit = z / (float)res;
                heights[x, z] = terrainData.GetInterpolatedHeight(xUnit, zUnit);
            }
        }
        return heights;
    }

    public static Vector3[,] GetNormals(TerrainData terrainData, int res) {
        Vector3[,] normals = new Vector3[res, res];
        for (int x = 0; x < res; x++) {
            for (int z = 0; z < res; z++) {
                float xUnit = x / (float)res;
                float zUnit = z / (float)res;
                normals[x, z] = terrainData.GetInterpolatedNormal(xUnit, zUnit);
            }
        }
        return normals;
    }

    public static void LoadRawFileAsFloats(string fileName, ref float[,] output, HeightfileFormat format, bool flipX, bool flipY) {
        if (!File.Exists(fileName)) {
            throw new FileNotFoundException("Heightmap file does not exist: " + fileName);
        }

        // Todo: we allocate a byte array, and then a float array. Can we do allocation for just one, and then reinterpret?

        byte[] input = File.ReadAllBytes(fileName);
        int resolution = GetHeightmapResolution(input, format);
        ValidateHeightmapRaw(input, output.GetLength(0), format);
        
        // Parse bytebuffer into floats, taking care of mirroring and byte format

        // Todo: replace in-loop switch with a Func<> lookup to avoid branching

        int i = 0;
        for (int x = 0; x < resolution; x++) {
            for (int y = 0; y < resolution; y++) {
                int iX = flipY ? resolution - 1 - x : x;
                int iY = flipX ? resolution - 1 - y : y;

                switch (format) {
                    case HeightfileFormat.R8:
                        output[iX, iY] = input[i++] / 256f;
                        break;

                    case HeightfileFormat.Windows:
                        output[iX, iY] = (input[i++] + input[i++] * 256f) / 65535f;
                        break;

                    case HeightfileFormat.Mac:
                        output[iX, iY] = (input[i++] * 256.0f + input[i++]) / 65535f;
                        break;
                }
            }
        }
    }

    private static void ValidateHeightmapRaw(byte[] input, int outputResolution, HeightfileFormat format) {
        int bytesPerPixel = (format == HeightfileFormat.R8 ? 1 : 2);
        int inputResolution = GetHeightmapResolution(input, format);
        int inputPixels = inputResolution*inputResolution*bytesPerPixel;

        if (inputResolution != outputResolution) {
            throw new ArgumentException(String.Format("The input file resolution {0} does not match the output resolution {1}", inputResolution, outputResolution));
        }

        if (input.Length != inputPixels) {
            throw new ArgumentException("The specified HeightFileFormat does not match the file. Expected size does not match.");
        }
    }

    public static int GetHeightmapResolution(byte[] bytes, HeightfileFormat format) {
        int bytesPerPixel = (format == HeightfileFormat.R8 ? 1 : 2);
        return (int)Math.Sqrt(bytes.Length / bytesPerPixel);
    }

    public static void LoadRawFileAsTexture(string rawFile, Texture2D output, HeightfileFormat format) {
        if (!File.Exists(rawFile)) {
            throw new FileNotFoundException("Raw file does not exist: " + rawFile);
        }

        // Open filestream and read into bytebuffer
        byte[] bytes = File.ReadAllBytes(rawFile);
        int resolution = GetHeightmapResolution(bytes, format);
        ValidateHeightmapRaw(bytes, output.width, format);

        //Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.ARGB32, true);

        // Prepare colorbuffer
        Color[] pixels = new Color[resolution * resolution];

        // Parse bytebuffer into colors, taking care of mirroring and byte format
        int i = 0;
        int j = 0;
        for (int x = 0; x < resolution; x++) {
            for (int y = 0; y < resolution; y++) {
                float gray;
                switch (format) {
                    case HeightfileFormat.R8:
                        gray = bytes[i++] / 256f;
                        pixels[j++] = new Color(gray, gray, gray);
                        break;

                    case HeightfileFormat.Windows:
                        gray = (bytes[i++] + bytes[i++] * 256f) / 65535f;
                        pixels[j++] = new Color(gray, gray, gray);
                        break;

                    case HeightfileFormat.Mac:
                        gray = (bytes[i++] * 256.0f + bytes[i++]) / 65535f;
                        pixels[j++] = new Color(gray, gray, gray);
                        break;
                }
            }
        }

        output.SetPixels(pixels);
    }

    // Given the path to a raw heightmap file, create a TerrainData asset file
    public static void ParseHeightTextureToTerrainData(Texture2D heightmap, TerrainData terrainData, bool flipX, bool flipY) {
        // Get pixels from texture
        Color[] pixels = heightmap.GetPixels();

        // Prepare floatbuffer
        float[,] heights = new float[heightmap.width, heightmap.height];

        // Parse bytebuffer into floats, taking care of mirroring and byte format
        int i = 0;
        for (int x = 0; x < heightmap.width; x++) {
            for (int y = 0; y < heightmap.height; y++) {
                int iX = x;//flipX ? heightmap.width - 1 - x : x;
                int iY = x;//flipY ? heightmap.height - 1 - y : y;

                heights[iX, iY] = pixels[i++].grayscale;
            }
        }

        terrainData.heightmapResolution = heightmap.width - 1;
        terrainData.SetHeights(0, 0, heights);
    }

    public static void ParseHeightmapFileToTerrain(string path, TerrainData terrainData, HeightfileFormat format, bool flipX, bool flipY) {
        if (path == "") {
            Debug.LogError("Path is empty");
            return;
        }

        if (terrainData == null) {
            Debug.LogError("Terraindata is null");
            return;
        }

        var heightData = new float[terrainData.heightmapWidth, terrainData.heightmapWidth];
        LoadRawFileAsFloats(path, ref heightData, format, flipX, flipY);

        if (heightData != null) {
            terrainData.SetHeights(0,0,heightData);
        }
    }

    // Generate mipmap. Note: current implementation doesn't have any filtering whatsoever, so expect noise
    // Todo: interpolation/filtering
    public float[,] DownsampleHeightData(float[,] original, int mipLevel) {
        int xLength = original.GetLength(0);
        int yLength = original.GetLength(1);

        int mipPower = (int)Mathf.Pow(2, mipLevel);

        int xMipLength = Mathf.CeilToInt((float)xLength / (float)mipPower);
        int yMipLength = Mathf.CeilToInt((float)yLength / (float)mipPower);

        if (xMipLength < 2 || yMipLength < 2) {
            Debug.LogWarning("mipLevel " + mipLevel + " is too high for this map");
            return null;
        }

        float[,] result = new float[xMipLength, yMipLength];
        for (int x = 0; x < xMipLength; x++) {
            for (int y = 0; y < yMipLength; y++)
                result[x, y] = original[Mathf.Clamp(x * mipPower, 0, xLength), Mathf.Clamp(y * mipPower, 0, yLength)];
        }

        return result;
    }

    #endregion

    #region Splatmaps

    /* Todo: add method that loads directly to float[,,] instead of using Texture2D as a medium */

    public static Texture2D LoadTexture(string fileName, int resolution) {
        FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        byte[] imageData = new byte[fileStream.Length];
        fileStream.Read(imageData, 0, (int)fileStream.Length);
        Texture2D texture = new Texture2D(resolution, resolution);
        texture.LoadImage(imageData);
        return texture;
    }

    public static void TextureToHeightMap(Texture2D texture, ref float[,] output, bool flipX, bool flipY) {
        if (texture.width != output.GetLength(0) || texture.height != output.GetLength(1)) {
            throw new ArgumentException("The source splatmap does not match the output dimensions");
        }
        
        Color[] splatmapColors = texture.GetPixels();
        int resolution = texture.width;

        for (int x = 0; x < resolution; x++) {
            for (int y = 0; y < resolution; y++) {
                // Generate 2D index, mirror read order according to settings
                int indexX = x;//flipY ? resolution - 1 - x : x;
                int indexY = y;//flipX ? resolution - 1 - y : y;

                // Assign values to splatmapData
                Color pixelColor = splatmapColors[x * resolution + y];

                output[indexX, indexY] = pixelColor.grayscale;
            }
        }
    }

    public static void TextureToSplatMap(Texture2D texture, ref float[,,] output, bool flipX, bool flipY) {
        if (texture.width != output.GetLength(0) || texture.height != output.GetLength(1)) {
            throw new ArgumentException("The source splatmap does not match the output dimensions");
        }
        
        const int numChannels = 4;

        Color[] splatmapColors = texture.GetPixels();
        int resolution = texture.width;

        for (int x = 0; x < resolution; x++) {
            for (int y = 0; y < resolution; y++) {
                // Generate 2D index, mirror read order according to settings
                int indexX = x;//flipY ? resolution - 1 - x : x;
                int indexY = y;//flipX ? resolution - 1 - y : y;

                // Assign values to splatmapData
                Color pixelColor = splatmapColors[x * resolution + y];

                for (int i = 0; i < numChannels; i++)
                    output[indexX, indexY, i] = pixelColor[i];
            }
        }
    }

    public static void TextureToDetailMap(Texture2D texture, ref int[][,] output, bool flipX, bool flipY) {
        int numChannels = output.Length;

        // Get color values from the textures
        Color[] splatmapColors = texture.GetPixels();
        int resolution = texture.width;

        //if (texture.width != output.GetLength(0) || texture.height != output.GetLength(1)) {
        //    throw new ArgumentException("The source splatmap does not match the output dimensions");
        //}

        for (int x = 0; x < resolution; x++) {
            for (int y = 0; y < resolution; y++) {
                // Generate 2D index, mirror read order according to settings
                int indexX = x;//flipY ? resolution - 1 - x : x;
                int indexY = y;//flipX ? resolution - 1 - y : y;

                // Assign values to splatmapData
                Color pixelColor = splatmapColors[x * resolution + y];

                for (int i = 0; i < numChannels; i++)
                    output[i][indexX, indexY] = (int)(pixelColor[i] * 64);
            }
        }
    }

    public void NormalizeSplatmap(ref float[,,] splatmap, NormalizationMode normalizationMode) {
        int width = splatmap.GetLength(0);
        int height = splatmap.GetLength(1);
        int numChannels = splatmap.GetLength(2);

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                // Calculate total luminance of this pixel
                
                float totalColor = 0f;
                for (int i = 0; i < numChannels; i++) {
                    totalColor += splatmap[x, y, i];
                }

                // Normalize pixel luminance to 1 using chosen strategy
                // Todo: inject these strategies using Func<> chosen from a library
                
                switch (normalizationMode) {
                    case NormalizationMode.Equal:
                        if (totalColor < 0.01f) {
                            splatmap[x, y, 0] += 1f - totalColor;
                        }
                        else {
                            for (int i = 0; i < numChannels; i++) {
                                splatmap[x, y, i] /= totalColor;
                            }
                        }
                        break;
                    case NormalizationMode.Strongest:
                        float strongestValue = 0f;
                        int strongestChannel = 0;
                        for (int i = 0; i < numChannels; i++) {
                            float channelValue = splatmap[x, y, i];
                            if (channelValue > strongestValue) {
                                strongestValue = channelValue;
                                strongestChannel = i;
                            }
                        }
                        splatmap[x, y, strongestChannel] += 1f - totalColor;
                        break;
                    case NormalizationMode.First:
                        splatmap[x, y, 0] += 1f - totalColor;
                        break;
                    case NormalizationMode.Last:
                        splatmap[x, y, numChannels - 1] += 1f - totalColor;
                        break;
                }
            }
        }
    }

    public void ParseSplatmapToTerrain(float[,,] splatmap, TerrainData terrainData) {
        int size = splatmap.GetLength(0);
        int numPrototypes = terrainData.splatPrototypes.Length;

        if (numPrototypes == 0) {
            Debug.Log("TerrainData '" + terrainData.name + "' has no channels. Skipping...");
            return;
        }

        /* If we have *more* prototypes than we have texture channels, remove the ones 
         * we don't have data for This potentially saves some space in the resulting
         * terrainData asset. */
        if (numPrototypes > 4) {
            Debug.Log("TerrainData '" + terrainData.name + "' has too many prototypes. Trimming..");
            TrimPrototypes(terrainData, 4);
        }

        terrainData.alphamapResolution = size;
        terrainData.SetAlphamaps(0, 0, splatmap);
        terrainData.RefreshPrototypes();
    }

    public void ParseDetailmapToTerrain(int[][,] map, TerrainData terrainData) {
        int numPrototypes = terrainData.detailPrototypes.Length;

        if (numPrototypes == 0) {
            Debug.Log("TerrainData '" + terrainData.name + "' has no detail prototypes. Skipping...");
            return;
        }

        /* If we have *more* prototypes than we have texture channels, remove the ones 
         * we don't have data for This potentially saves some space in the resulting
         * terrainData asset. */
        if (numPrototypes > 4) {
            Debug.Log("TerrainData '" + terrainData.name + "' has too many prototypes. Skipping..");
            return;
        }

        for (int i = 0; i < numPrototypes; i++) {
            terrainData.SetDetailLayer(0,0,i,map[i]);
        }
        
        terrainData.RefreshPrototypes();
    }

    private static void TrimPrototypes(TerrainData terrainData, int numTextureChannels) {
        var oldSplats = terrainData.splatPrototypes;
        var newSplats = new SplatPrototype[numTextureChannels];

        for (int i = 0; i < numTextureChannels; i++)
            newSplats[i] = oldSplats[i];

        terrainData.splatPrototypes = newSplats;
    }

    //private float[,,] TrimEmptySplatmapChannels(TerrainData terrainData, float[,,] splatmapChannels, int size,
    //                                            int numTextureChannels, bool[] channelsEmpty) {
    //    List<int> emptyChannels = GetEmptyChannels(ref splatmapChannels, size);

    //    if (emptyChannels.Count != 0) {
    //        int newSize = numTextureChannels - emptyChannels.Count;
    //        float[,,] newSplatmapData = new float[size,size,newSize];

    //        // Move through channels, see if one is empty
    //        int j = 0;
    //        for (int i = 0; i < newSize; i++) {
    //            if (emptyChannels.Contains(j)) {
    //                // Find next channel that is not empty, if any
    //                for (; j < numTextureChannels; j++) {
    //                    if (!channelsEmpty[j])
    //                        break;
    //                }
    //            }

    //            CopyChannel(ref splatmapChannels, ref newSplatmapData, size, j, i);
    //            j++;
    //        }

    //        // Create new list of splatPrototypes
    //        List<SplatPrototype> newSplats = new List<SplatPrototype>();
    //        for (int i = 0; i < numTextureChannels; i++) {
    //            if (!emptyChannels.Contains(i))
    //                newSplats.Add(terrainData.splatPrototypes[i]);
    //        }

    //        terrainData.splatPrototypes = newSplats.ToArray();
    //        splatmapChannels = newSplatmapData;
    //    }
    //    return splatmapChannels;
    //}

    //private List<int> GetEmptyChannels(ref float[, ,] mapData, int size) {
    //    int numChannels = mapData.GetLength(2);

    //    List<int> channels = new List<int>();
    //    for (int i = 0; i < numChannels; i++)
    //        channels.Add(i);

    //    for (int x = 0; x < size; x++) {
    //        for (int y = 0; y < size; y++) {
    //            // Check if any channels are not empty
    //            List<int> rmv = new List<int>();
    //            foreach (int i in channels) {
    //                if (mapData[x, y, i] > 0f)
    //                    rmv.Add(i);
    //            }

    //            // If so, remove from list of channels
    //            foreach (int i in rmv)
    //                channels.Remove(i);
    //        }
    //    }

    //    return channels;
    //}

    //private void CopyChannel(ref float[, ,] oldSplatmapData, ref float[, ,] newSplatmapData, int size, int fromChannel,
    //                         int toChannel) {
    //    for (int x = 0; x < size; x++) {
    //        for (int y = 0; y < size; y++) {
    //            newSplatmapData[x, y, toChannel] = oldSplatmapData[x, y, fromChannel];
    //        }
    //    }
    //}

    public void ApplySplatPrototypes(TerrainData terrainData, IList<SplatPrototype> prototypes) {
        Debug.Log("applying splat prototypes");

        if (prototypes.Count > 8)
            Debug.LogError("A terrain cannot hold more than 8 splat textures");

        if (prototypes.Count == 0)
            Debug.LogError("Please add one or more splat prototypes to apply");

        foreach (SplatPrototype prototype in prototypes) {
            if (prototype == null || prototype.texture == null)
                Debug.LogError("A terrain splat texture can not be null. Remove any empty ones.");
        }

        terrainData.splatPrototypes = prototypes.ToArray();
        terrainData.RefreshPrototypes();
    }

    public void ApplyDetailPrototypes(TerrainData terrainData, IList<DetailPrototype> prototypes) {
        Debug.Log("applying detail prototypes");

        if (prototypes.Count > 4)
            Debug.LogError("A terrain cannot hold more than 8 detail meshes");

        if (prototypes.Count < 1)
            Debug.LogError("Please add one or more splat prototypes to apply");

        foreach (DetailPrototype prototype in prototypes) {
            if (prototype == null || (prototype.prototype == null && prototype.prototypeTexture == null))
                Debug.LogError("A tree prototype's prefab can not be null. Remove any empty ones.");
        }

        terrainData.detailPrototypes = prototypes.ToArray();
        terrainData.RefreshPrototypes();
    }

    public void ApplyTreePrototypes(TerrainData terrainData, IList<TreePrototype> prototypes) {
        Debug.Log("applying tree prototypes");

        if (prototypes.Count > 4)
            Debug.LogError("A terrain cannot hold more than 8 tree prototypes");

        if (prototypes.Count < 1)
            Debug.LogError("Please add one or more tree prototypes to apply");

        foreach (TreePrototype prototype in prototypes) {
            if (prototype == null || prototype.prefab == null)
                Debug.LogError("A tree prototype's prefab can not be null. Remove any empty ones.");
        }

        terrainData.treePrototypes = prototypes.ToArray();
        terrainData.RefreshPrototypes();
    }

    public static void ValidateSplatPrototypes(IList<SplatPrototype> splatPrototypes) {
        if (splatPrototypes.Count < 1) {
            throw new ArgumentException("Please assign at least one splat prototype in the Splatmap Section");
        }

        foreach (SplatPrototype splatPrototype in splatPrototypes) {
            if (splatPrototype.texture == null) {
                throw new ArgumentException("Please make sure there are no empty splat prototypes in the Splatmap Section");
            }
        }
    }

    public static void ValidateTextureMap(Texture2D splatmap) {
        if (splatmap == null)
            throw new ArgumentException("The specified splat texture is null");

        if (splatmap.format != TextureFormat.ARGB32) {
            throw new ArgumentException("Splatmap should be of type ARGB32. Use the TerrainImporter to fix this.");
        }

        if (splatmap.height != splatmap.width) {
            throw new ArgumentException("Splatmap should be square");
        }

        if (Mathf.ClosestPowerOfTwo(splatmap.width) != splatmap.width) {
            throw new ArgumentException("Splatmap size should be a power of two");
        }
    }

    #endregion

    #region Treemaps

    /// <summary>
    /// Populates a terrain with trees, using random patterns guided by the specified tree maps.
    /// </summary>
    /// <param name="treemaps"></param>
    /// <param name="terrainData"></param>
    /// <returns></returns>
    public static TerrainData ParseTreemapTexturesToTerrain(Texture2D treemap, TerrainData terrainData) {
        const float samplingPrecision = 0.1f; // How many map samples per meter to perform
        const float treemapDensityGain = 1f;
        const float treemapDensityMax = 1f;
        const float minThreshold = 0.1f;
        const float treeSize = 1f;
        const float sizeVariation = 0.3f;
        

        float terrainSize = terrainData.size.x;
        int numSteps = Mathf.FloorToInt(terrainSize * samplingPrecision); // Number of steps across texture map
        float stepSize = 1f / terrainSize / samplingPrecision; // Stepsize in texture space 0 < x < 1
        float positionVariation = 1f / (float)numSteps / samplingPrecision * 0.33f; // Factor of distance between tree sample points
        
        var treeInstances = new List<TreeInstance>();

        for (int x = 0; x < numSteps; x++) {
            for (int y = 0; y < numSteps; y++) {
                float u = x * stepSize;
                float v = y * stepSize;

                // place the chosen tree, if the colours are right
                int prototypeIndex = -1;
                Color pixelColor = treemap.GetPixelBilinear(u, v);

                // Todo: These two range changes are actually things that need to be applied in world machine, or at least exposed in the editor
                pixelColor *= treemapDensityGain;
                pixelColor = Clamp(pixelColor, treemapDensityMax);

                if (Mathf.Pow(pixelColor.r, 1.75f) > GetJitteredThreshold(minThreshold)) {
                    prototypeIndex = 0;
                } else if (pixelColor.g > GetJitteredThreshold(minThreshold)) {
                    prototypeIndex = 1;
                } else if (pixelColor.b > GetJitteredThreshold(minThreshold)) {
                    prototypeIndex = 2;
                } else if (pixelColor.a > GetJitteredThreshold(minThreshold)) {
                    prototypeIndex = 3;
                }
                
                if (prototypeIndex >= 0) {
                    TreeInstance treeInstance = new TreeInstance();

                    // random placement offset for a more natural look
                    float positionX = Mathf.Clamp01(u + Random.Range(-positionVariation, positionVariation));
                    float positionZ = Mathf.Clamp01(v + Random.Range(-positionVariation, positionVariation));
                    float positionY = terrainData.GetInterpolatedHeight(positionX, positionZ) / terrainData.size.y;

                    treeInstance.position = new Vector3(positionX, positionY, positionZ);

                    treeInstance.color = Color.white;
                    treeInstance.lightmapColor = Color.white;
                    treeInstance.prototypeIndex = prototypeIndex;

                    float baseScale = treeSize * (1f + Random.Range(-sizeVariation, sizeVariation));
                    treeInstance.heightScale = baseScale;
                    treeInstance.widthScale = baseScale * (1f + Random.Range(-sizeVariation, sizeVariation));
                    
                    treeInstances.Add(treeInstance);
                }
            }
        }

        terrainData.treeInstances = treeInstances.ToArray();

        Debug.Log("Placed " + terrainData.treeInstances.Length + " trees");

        return terrainData;
    }

    private static Color Clamp(Color color, float max) {
        color.r = Mathf.Clamp(color.r, 0f, max);
        color.g = Mathf.Clamp(color.g, 0f, max);
        color.b = Mathf.Clamp(color.b, 0f, max);
        color.a = Mathf.Clamp(color.a, 0f, max);
        return color;
    }

    private static float GetJitteredThreshold(float threshold) {
        return (threshold + Random.Range(0f, 1f - threshold));
    }

    #endregion

    /// <summary>
    /// Sets neighbours of the target region with one level of recursion.
    ///
    /// Neighbours of this region might not have a reference to to this
    /// newly streamed terrain yet, so we set their respective neighbour
    /// references again just in case.
    /// </summary>
    /// <param name="region"></param>
    /// <param name="terrains"></param>
    public static void SetNeighboursOfNeighbours(IntVector3 region, IDictionary<IntVector3, TerrainTile> terrains) {
        int x = region.X;
        int z = region.Z;

        var leftRegion = new IntVector3(x - 1, 0, z + 0);
        var topRegion = new IntVector3(x + 0, 0, z + 1);
        var rightRegion = new IntVector3(x + 1, 0, z + 0);
        var bottomRegion = new IntVector3(x + 0, 0, z - 1);

        // Self
        SetNeighbours(region, terrains);

        // Neighbours
        SetNeighbours(leftRegion, terrains);
        SetNeighbours(topRegion, terrains);
        SetNeighbours(rightRegion, terrains);
        SetNeighbours(bottomRegion, terrains);
    }

    /// <summary>
    /// Sets neighbour references for one loaded region.
    /// </summary>
    /// <param name="region"></param>
    /// <param name="terrains"></param>
    public static void SetNeighbours(IntVector3 region, IDictionary<IntVector3, TerrainTile> terrains) {
        Terrain center = terrains.ContainsKey(region) ? terrains[region].Terrain : null;
        if (center == null) {
            return;
        }

        int x = region.X;
        int z = region.Z;

        var leftRegion = new IntVector3(x - 1, 0, z + 0);
        Terrain left = terrains.ContainsKey(leftRegion) ? terrains[leftRegion].Terrain : null;
        var topRegion = new IntVector3(x + 0, 0, z + 1);
        Terrain top = terrains.ContainsKey(topRegion) ? terrains[topRegion].Terrain : null;
        var rightRegion = new IntVector3(x + 1, 0, z + 0);
        Terrain right = terrains.ContainsKey(rightRegion) ? terrains[rightRegion].Terrain : null;
        var bottomRegion = new IntVector3(x + 0, 0, z - 1);
        Terrain bottom = terrains.ContainsKey(bottomRegion) ? terrains[bottomRegion].Terrain : null;

        center.SetNeighbors(left, top, right, bottom);
    }



    public static void ApplyTerrainQualitySettings(Terrain terrain, TerrainConfiguration configuration) {
        terrain.drawTreesAndFoliage = configuration.DrawTreesAndFoliage;
        terrain.treeDistance = configuration.TreeDistance;
        terrain.treeBillboardDistance = configuration.TreeBillboardDistance;
        terrain.treeCrossFadeLength = configuration.TreeCrossFadeLength;
        terrain.treeMaximumFullLODCount = configuration.TreeMaximumFullLODCount;
        terrain.detailObjectDistance = configuration.DetailObjectDistance;
        terrain.detailObjectDensity = configuration.DetailObjectDensity;
        terrain.heightmapPixelError = configuration.HeightmapPixelError;
        terrain.heightmapMaximumLOD = configuration.HeightmapMaximumLOD;
        terrain.castShadows = configuration.CastShadows;
    }

    public static void ApplyTerrainWeatherSettings(Terrain terrain, TerrainConfiguration configuration) {
        terrain.materialTemplate.SetVector("_SnowAltitude", new Vector4(configuration.SnowAltitude - 250f, configuration.SnowAltitude + 250f));
    }

    public static IntVector3 GetTerrainRegionFromName(string name, int gridSize) {
        string[] separators = new[] {
            "_",
        };

        string[] parts = name.Split(separators, StringSplitOptions.None);
        int index = Int32.Parse(parts[parts.Length - 1]);

        // Ordering of tiles produced by image magick starts top-left, goes right, then down.

        return new IntVector3(
            index % gridSize,
            0,
            gridSize-1-(int)(index/gridSize)
        );
    }

    public static int GetTerrainAssetIndexFromRegion(IntVector3 region, int gridSize) {
        return (int)(region.X + region.Z*gridSize);
    }

    public static Vector3 GetTerrainPosition(IntVector3 region, float terrainWidth) {
        return new Vector3(region.X * terrainWidth, region.Y * terrainWidth, region.Z * terrainWidth);
    }

    public static Dictionary<int, IDictionary<IntVector3, TerrainTile>> FindTerrainTilesInScene() {
        var tiles = new Dictionary<int, IDictionary<IntVector3, TerrainTile>>();

        var terrainsInScene = GameObject.FindObjectsOfType<TerrainTile>();

        for (int i = 0; i < terrainsInScene.Length; i++) {
            var tile = terrainsInScene[i];

            if (tile.Terrain == null) {
                Debug.LogError(tile.Region);
            }

            if (!tiles.ContainsKey(tile.LodLevel)) {
                tiles.Add(tile.LodLevel, new Dictionary<IntVector3, TerrainTile>());
            }

            tiles[tile.LodLevel].Add(tile.Region, tile);
        }

        return tiles;
    }

    public static void MakeNormalMap(TerrainData data, ref Color[] normals, int res) {
        float stepSize = 1f / res;
        for (int x = 0; x < res; x++) {
            for (int z = 0; z < res; z++) {
                int index = x + z * res;

//                float heightL = data.GetInterpolatedHeight((x - 1) * stepSize, z * stepSize);
//                float heightR = data.GetInterpolatedHeight((x + 1) * stepSize, z * stepSize);
//                float heightB = data.GetInterpolatedHeight(x * stepSize, (z - 1) * stepSize);
//                float heightT = data.GetInterpolatedHeight(x * stepSize, (z + 1) * stepSize);
//
//                Vector3 lr = new Vector3(2f * stepSize * data.heightmapScale.x, (heightR - heightL) * data.heightmapScale.y, 0f);
//                Vector3 bt = new Vector3(0f, (heightB - heightT) * data.heightmapScale.y, 2f * stepSize * data.heightmapScale.x);
//                Vector3 normal = Vector3.Cross(bt, lr).normalized;

                Vector3 normal = data.GetInterpolatedNormal(x * stepSize, z * stepSize);
                
                normals[index] = new Color(
                    0.5f + normal.x * 0.5f,
                    0.5f + normal.z * 0.5f,
                    0.5f + normal.y * 0.5f,
                    1f);
            }
        }
    }
}

#region Enums

[Serializable]
public enum HeightfileFormat {
    Windows,
    Mac,
    R8
}

[Serializable]
public enum NormalizationMode {
    Equal,
    Strongest,
    First,
    Last
}

#endregion