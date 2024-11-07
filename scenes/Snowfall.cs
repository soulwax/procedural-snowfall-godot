using Godot;

public partial class Snowfall : Node2D
{
    private GpuParticles2D _snowParticles;
    private FastNoiseLite _noise;
    
    public override void _Ready()
    {
        // Create the particles node
        _snowParticles = new GpuParticles2D();
        AddChild(_snowParticles);
        
        // Create the particle material
        var particleMaterial = new ParticleProcessMaterial();
        
        // Get screen size
        Vector2 viewPort = GetViewport().GetVisibleRect().Size;
        GD.Print(viewPort);
        // Set up basic particle properties
        particleMaterial.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box;
        particleMaterial.EmissionBoxExtents = new Vector3(viewPort.X, viewPort.Y, 0);
        
        // Particle movement
        particleMaterial.Direction = new Vector3(0, 1, 0);
        particleMaterial.InitialVelocityMin = 20.0f;
        particleMaterial.InitialVelocityMax = 40.0f;
        particleMaterial.Gravity = new Vector3(0, 5.0f, 0);
        
        // Particle appearance
        particleMaterial.Scale = new Vector2(0.5f, 0.5f);
        particleMaterial.ScaleMin = 0.3f;
        particleMaterial.ScaleMax = 0.7f;
        
        // Add some randomness to particle movement
        _noise = new FastNoiseLite();
        _noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
        particleMaterial.TurbulenceEnabled = true;
        particleMaterial.TurbulenceNoiseStrength = 2.0f;
        particleMaterial.TurbulenceNoiseScale = 2.0f;
        
        // Create the snowflake texture
        var texture = CreateSnowflakeTexture();
        
        // Set up the particles
        _snowParticles.ProcessMaterial = particleMaterial;
        _snowParticles.Texture = texture;
        _snowParticles.Amount = 1000;
        _snowParticles.Lifetime = 8.0f;
        _snowParticles.Preprocess = 2.0f; // Start with some particles already visible
        
        // Position the emitter at the top of the screen
        _snowParticles.Position = new Vector2(GetViewportRect().Size.X / 2, -10);
        
        // Start the particles
        _snowParticles.Emitting = true;
    }
    
    public override void _Process(double delta)
    {
        // Update emitter position if the window is resized
        _snowParticles.Position = new Vector2(GetViewportRect().Size.X / 2, -10);
    }
    
    private Texture2D CreateSnowflakeTexture()
    {
        // Create a simple circular snowflake texture
        var image = Image.CreateEmpty(8, 8, false, Image.Format.Rgba8);
        image.Fill(Colors.Transparent);
        
        // Draw a simple white circle
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                float dx = x - 3.5f;
                float dy = y - 3.5f;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                
                if (distance < 3.0f)
                {
                    // Create a soft edge
                    float alpha = Mathf.Clamp(1.0f - (distance / 3.0f), 0, 1);
                    var color = new Color(1, 1, 1, alpha);
                    image.SetPixel(x, y, color);
                }
            }
        }
        
        return ImageTexture.CreateFromImage(image);
    }
}