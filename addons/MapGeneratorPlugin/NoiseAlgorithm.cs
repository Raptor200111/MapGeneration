using Godot;
using System;
using System.Collections.Generic;

[Tool]
public class NoiseAlgorithm
{
    private FastNoiseLite tempNoise;
    private FastNoiseLite moistNoise;
    private FastNoiseLite heightMap;

    public NoiseAlgorithm(uint seed, float freq2D, float freq3D)
    {
        tempNoise = new FastNoiseLite();
        tempNoise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.SimplexSmooth);
        tempNoise.SetSeed((int)seed);
        tempNoise.SetFrequency(freq2D);
        tempNoise.SetFractalType(FastNoiseLite.FractalTypeEnum.None);
        tempNoise.DomainWarpEnabled = false;
        tempNoise.Offset = new Vector3(0, 0, 0);

        moistNoise = new FastNoiseLite();
        moistNoise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.SimplexSmooth);
        moistNoise.SetSeed((int)seed + 1); //seed + 1
        moistNoise.SetFrequency(freq2D);
        moistNoise.SetFractalType(FastNoiseLite.FractalTypeEnum.None);
        moistNoise.DomainWarpEnabled = false;
        moistNoise.Offset = new Vector3(0, 0, 0);

        heightMap = new FastNoiseLite();
        heightMap.SetNoiseType(FastNoiseLite.NoiseTypeEnum.Perlin);
        heightMap.SetSeed((int)seed + 2); // seed + 2
        heightMap.SetFrequency(freq3D);
        heightMap.SetFractalType(FastNoiseLite.FractalTypeEnum.None);
        heightMap.DomainWarpEnabled = false;
        heightMap.Offset = new Vector3(0, 0, 0);
        
    }
    public Dictionary<int, Dictionary<int, List<Vector3>>> GenerateNoise(Vector3 offset, int width, int length, Godot.Collections.Array<Godot.Collections.Array<int>> coherenceTable, Zone[] zones, Godot.Collections.Array<int> heightOverride, bool useHeight = false)
    {
        var zonesUsed = new Dictionary<int, Dictionary<int, List<Vector3>>>();

        for (int j = 0; j < length; j++)
        {
            for (int i = 0; i < width; i++)
            {
                int indiTempNoise = 9 - Mathf.Clamp((int)((tempNoise.GetNoise2D(j + offset.Z, i + offset.X) + 1) * 5), 0, 9);
                int indiMoistNoise = Mathf.Clamp((int)((moistNoise.GetNoise2D(j + offset.Z, i + offset.X) + 1) * 5), 0, 9);
                int resourceNoise = (indiTempNoise + indiMoistNoise) * 5;

                float height = (heightMap.GetNoise2D(j + offset.Z, i + offset.X) + 1f) * 5f;
                int zoneIdHeight = heightOverride[9 - Mathf.Clamp((int)height, 0, 9)];

                int zoneId = coherenceTable[indiTempNoise][indiMoistNoise];
                if (zoneId != -1 || zoneIdHeight != -1)
                {
                    if (zoneIdHeight != -1)
                        zoneId = zoneIdHeight;

                    int resourceId = zones[zoneId].GetResources().GetResourceIndexByProbability(resourceNoise);

                    if (!zonesUsed.ContainsKey(zoneId))
                        zonesUsed[zoneId] = new Dictionary<int, List<Vector3>>();
                    if (!zonesUsed[zoneId].ContainsKey(resourceId))
                        zonesUsed[zoneId][resourceId] = new List<Vector3>();

                    zonesUsed[zoneId][resourceId].Add(new Vector3(i, useHeight ? height : 0, j));
                }
            }
        }

        return zonesUsed;
    }
}