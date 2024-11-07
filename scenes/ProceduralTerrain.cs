using Godot;
using System;
using System.Collections.Generic;

public partial class ProceduralTerrain : Node2D
{
    [Export]
    public int TerrainChunkSize = 200;
    
    [Export]
    public float NoiseScale = 100.0f;
    
    [Export]
    public float TerrainHeight = 100.0f;
    
    [Export]
    public float SnowThreshold = 0.6f;

    private int TerrainWidth;
    private Vector2 ViewportSize;
    private float BaseTerrainHeight;
    
    private FastNoiseLite _terrainNoise;
    private FastNoiseLite _biomeNoise;
    private List<StaticBody2D> _terrainChunks = new List<StaticBody2D>();
    
    public override void _Ready()
    {
        // Get screen dimensions
        ViewportSize = GetViewport().GetVisibleRect().Size;
        
        // Set terrain width to be wider than screen for scrolling
        TerrainWidth = (int)(ViewportSize.X * 1.5f);
        
        // Set base terrain height relative to screen height
        BaseTerrainHeight = ViewportSize.Y * 0.7f; // Terrain starts at 70% of screen height
        
        SetupNoise();
        GenerateTerrain();
    }
    
    private void SetupNoise()
    {
        _terrainNoise = new FastNoiseLite();
        _terrainNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
        _terrainNoise.Seed = (int)GD.Randi();
        _terrainNoise.Frequency = 0.01f;
        
        _biomeNoise = new FastNoiseLite();
        _biomeNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
        _biomeNoise.Seed = (int)GD.Randi();
        _biomeNoise.Frequency = 0.005f;
    }
    
    private void GenerateTerrain()
    {
        int numChunks = TerrainWidth / TerrainChunkSize;
        
        // Center the terrain horizontally
        float startOffsetX = -TerrainWidth / 4f;
        
        for (int chunk = 0; chunk < numChunks; chunk++)
        {
            GenerateTerrainChunk(startOffsetX + (chunk * TerrainChunkSize));
        }
    }
    
    private void GenerateTerrainChunk(float startX)
    {
        var chunk = new StaticBody2D();
        var collision = new CollisionPolygon2D();
        var terrainPolygon = new Polygon2D();
        var snowPolygon = new Polygon2D();
        
        var points = new List<Vector2>();
        var snowPoints = new List<Vector2>();
        bool isGeneratingSnow = false;
        Vector2 lastPoint = Vector2.Zero;
        
        // Generate points for the terrain profile
        for (int x = 0; x <= TerrainChunkSize; x++)
        {
            float worldX = startX + x;
            float height = GetTerrainHeight(worldX);
            float snowVariation = GetSnowVariation(worldX, height);
            
            Vector2 point = new Vector2(worldX, height);
            points.Add(point);
            
            // Handle snow coverage
            if (snowVariation > SnowThreshold && !isGeneratingSnow)
            {
                isGeneratingSnow = true;
                snowPoints.Add(lastPoint);
            }
            else if (snowVariation <= SnowThreshold && isGeneratingSnow)
            {
                isGeneratingSnow = false;
                snowPoints.Add(lastPoint);
                GenerateSnowPatch(snowPolygon, snowPoints);
                snowPoints.Clear();
            }
            
            if (isGeneratingSnow)
            {
                snowPoints.Add(point);
            }
            
            lastPoint = point;
        }
        
        // Close the terrain polygon at the bottom of the screen
        points.Add(new Vector2(startX + TerrainChunkSize, ViewportSize.Y));
        points.Add(new Vector2(startX, ViewportSize.Y));
        
        terrainPolygon.Polygon = points.ToArray();
        terrainPolygon.Color = new Color(0.4f, 0.3f, 0.2f);
        
        collision.Polygon = points.ToArray();
        
        chunk.AddChild(terrainPolygon);
        chunk.AddChild(collision);
        chunk.AddChild(snowPolygon);
        
        AddChild(chunk);
        _terrainChunks.Add(chunk);
    }
    
    private void GenerateSnowPatch(Polygon2D snowPolygon, List<Vector2> points)
    {
        if (points.Count < 2) return;
        
        var snowPatchPoints = new List<Vector2>(points);
        
        // Add snow depth relative to screen size
        float snowDepth = ViewportSize.Y * 0.01f; // 1% of screen height
        
        for (int i = points.Count - 1; i >= 0; i--)
        {
            snowPatchPoints.Add(points[i] + new Vector2(0, snowDepth));
        }
        
        snowPolygon.Color = Colors.White;
        snowPolygon.Polygon = snowPatchPoints.ToArray();
    }
    
    private float GetTerrainHeight(float x)
    {
        float noise = _terrainNoise.GetNoise1D(x);
        // Scale terrain variations relative to screen height
        float variationHeight = ViewportSize.Y * 0.15f; // 15% of screen height
        return BaseTerrainHeight + (noise * variationHeight);
    }
    
    private float GetSnowVariation(float x, float height)
    {
        // Adjust threshold based on screen height
        float heightThresholdMin = ViewportSize.Y * 0.6f; // Snow starts at 60% screen height
        float heightThresholdMax = ViewportSize.Y * 0.8f; // Full snow potential at 80% screen height
        
        float heightFactor = Mathf.InverseLerp(heightThresholdMin, heightThresholdMax, height);
        float noiseFactor = (_biomeNoise.GetNoise1D(x) + 1) * 0.5f;
        return (heightFactor * 0.7f) + (noiseFactor * 0.3f);
    }
}