using Godot;

public partial class PerformanceMonitor : Node2D
{
    private Label _statsLabel;
    private int updateCount = 0;
    private float elapsedTime = 0.0f;
    private const float UPDATE_INTERVAL = 0.5f; // Update display every 0.5 seconds


    public override void _Ready()
    {
        // Create a new Label node
        _statsLabel = new Label();
        AddChild(_statsLabel);

        // Position the label in the top-left corner with some padding
        _statsLabel.Position = new Vector2(10, 10);

        // Set some basic styling
        _statsLabel.AddThemeColorOverride("font_color", Colors.White);
        _statsLabel.AddThemeFontSizeOverride("font_size", 16);

        // Optional: Add a shadow for better visibility
        _statsLabel.AddThemeConstantOverride("shadow_offset_x", 1);
        _statsLabel.AddThemeConstantOverride("shadow_offset_y", 1);
        _statsLabel.AddThemeColorOverride("font_shadow_color", Colors.Black);
    }

    public override void _Process(double delta)
    {
        updateCount++;
        elapsedTime += (float)delta;

        if (elapsedTime >= UPDATE_INTERVAL)
        {
            int fps = (int)Engine.GetFramesPerSecond();
            int updates = (int)(updateCount / elapsedTime);

            // Get physics updates per second (fixed updates)
            float physics_fps = Engine.PhysicsTicksPerSecond;

            // Get .NET version
            string dotnetVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;

            // Update the label text
            _statsLabel.Text = $"FPS: {fps}\n" +
                              $"Normal Updates: {updates}/s\n" +
                              $"Physics Updates: {physics_fps}/s\n" +
                              $"dotnet Version: {dotnetVersion}";
            // Calculate the average FPS
            updateCount = 0;
            elapsedTime = 0.0f;
        }

    }
}