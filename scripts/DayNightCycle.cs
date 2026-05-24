using Godot;
using System.Collections.Generic;

namespace BanhKhucGame;

public partial class DayNightCycle : Node
{
    [Export] public float DayLengthSeconds { get; set; } = 180f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float StartTime { get; set; } = 0.25f;
    [Export] public NodePath SunPath { get; set; } = "";
    [Export] public NodePath EnvPath { get; set; } = "";
    [Export] public NodePath TimeLabelPath { get; set; } = "";
    [Export] public string LampGroupName { get; set; } = "lamps";
    [Export] public float LampNightEnergy { get; set; } = 2.5f;

    public float TimeOfDay { get; private set; }
    public float Daylight { get; private set; }

    private DirectionalLight3D? _sun;
    private WorldEnvironment? _env;
    private Label? _label;
    private readonly List<OmniLight3D> _lamps = new();

    public override void _Ready()
    {
        TimeOfDay = StartTime;
        _sun = GetNodeOrNull<DirectionalLight3D>(SunPath);
        _env = GetNodeOrNull<WorldEnvironment>(EnvPath);
        _label = GetNodeOrNull<Label>(TimeLabelPath);

        foreach (var node in GetTree().GetNodesInGroup(LampGroupName))
        {
            if (node is OmniLight3D lamp)
                _lamps.Add(lamp);
        }
    }

    public override void _Process(double delta)
    {
        TimeOfDay = (TimeOfDay + (float)delta / DayLengthSeconds) % 1f;
        UpdateDaylight();
        UpdateSun();
        UpdateSky();
        UpdateLamps();
        UpdateLabel();
    }

    private void UpdateDaylight()
    {
        // Realistic light curve:
        //   5h-7h30: dawn ramp 0 → 1
        //   7h30-17h30: full daylight plateau
        //   17h30-20h: dusk ramp 1 → 0
        //   20h-5h: night
        float hour = TimeOfDay * 24f;

        if (hour < 5f || hour >= 20f)
            Daylight = 0f;
        else if (hour < 7.5f)
            Daylight = (hour - 5f) / 2.5f;
        else if (hour < 17.5f)
            Daylight = 1f;
        else
            Daylight = 1f - (hour - 17.5f) / 2.5f;
    }

    private void UpdateSun()
    {
        if (_sun == null) return;

        // Smooth blend between night-moon config and day-sun config.
        // No hard threshold → no visible jerk at sunrise/sunset.
        float blend = Mathf.SmoothStep(0f, 0.3f, Daylight);

        // Pitch: moon at -60°, sun goes from -10° (horizon) to -80° (overhead)
        float nightPitch = -60f;
        float dayPitch = -10f - Daylight * 70f;
        float pitchDeg = Mathf.Lerp(nightPitch, dayPitch, blend);

        var basis = Basis.FromEuler(new Vector3(Mathf.DegToRad(pitchDeg), Mathf.DegToRad(35), 0));
        _sun.Transform = new Transform3D(basis, _sun.Position);

        // Energy: 0.35 (moon) → up to 1.45 (full noon sun)
        float nightEnergy = 0.35f;
        float dayEnergy = Daylight * 1.3f + 0.15f;
        _sun.LightEnergy = Mathf.Lerp(nightEnergy, dayEnergy, blend);

        // Color: moon-blue → sun-warm
        Color moonColor = new(0.6f, 0.75f, 1f);
        float warmth = 1f - Daylight;
        Color dayColor = new(
            1f,
            0.95f - warmth * 0.3f,
            0.85f - warmth * 0.5f);
        _sun.LightColor = moonColor.Lerp(dayColor, blend);
    }

    private void UpdateSky()
    {
        if (_env?.Environment?.Sky?.SkyMaterial is not ProceduralSkyMaterial sky) return;

        // 3-stop continuous gradient: night → dusk/dawn → day
        // Blended smoothly with Daylight (no hard thresholds → no flicker)
        Color nightTop = new(0.04f, 0.04f, 0.12f);
        Color nightHorizon = new(0.12f, 0.08f, 0.18f);
        Color duskTop = new(0.22f, 0.16f, 0.32f);
        Color duskHorizon = new(0.90f, 0.50f, 0.28f);
        Color dayTop = new(0.30f, 0.60f, 0.90f);
        Color dayHorizon = new(0.72f, 0.85f, 0.95f);

        Color topTarget, horizonTarget;
        if (Daylight < 0.5f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Daylight * 2f);
            topTarget = nightTop.Lerp(duskTop, t);
            horizonTarget = nightHorizon.Lerp(duskHorizon, t);
        }
        else
        {
            float t = Mathf.SmoothStep(0f, 1f, (Daylight - 0.5f) * 2f);
            topTarget = duskTop.Lerp(dayTop, t);
            horizonTarget = duskHorizon.Lerp(dayHorizon, t);
        }

        sky.SkyTopColor = sky.SkyTopColor.Lerp(topTarget, 0.1f);
        sky.SkyHorizonColor = sky.SkyHorizonColor.Lerp(horizonTarget, 0.1f);
    }

    private void UpdateLamps()
    {
        float target;
        if (Daylight < 0.15f)
            target = LampNightEnergy;
        else if (Daylight < 0.45f)
            target = LampNightEnergy * (1f - (Daylight - 0.15f) / 0.30f);
        else
            target = 0f;

        foreach (var lamp in _lamps)
            lamp.LightEnergy = Mathf.Lerp(lamp.LightEnergy, target, 0.06f);
    }

    private void UpdateLabel()
    {
        if (_label == null) return;

        float hourFloat = TimeOfDay * 24f;
        int hour = (int)hourFloat;
        int min = (int)((hourFloat - hour) * 60f);

        string phase;
        if (hour >= 5 && hour < 11) phase = "🌅 Sáng";
        else if (hour >= 11 && hour < 14) phase = "☀️ Trưa";
        else if (hour >= 14 && hour < 18) phase = "🌇 Chiều";
        else phase = "🌙 Đêm";

        _label.Text = $"{phase}  {hour:D2}:{min:D2}";
    }
}
