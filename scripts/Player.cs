using Godot;

namespace BanhKhucGame;

public partial class Player : CharacterBody2D
{
    [Export] public float MaxSpeed { get; set; } = 280f;
    [Export] public float Acceleration { get; set; } = 1400f;
    [Export] public float Friction { get; set; } = 1800f;
    [Export] public NodePath JoystickPath { get; set; } = "";

    private VirtualJoystick? _joystick;
    private Node2D _visual = null!;
    private float _wobbleTime;

    public override void _Ready()
    {
        _visual = GetNode<Node2D>("Visual");
        if (!string.IsNullOrEmpty(JoystickPath))
            _joystick = GetNodeOrNull<VirtualJoystick>(JoystickPath);
    }

    public override void _PhysicsProcess(double delta)
    {
        var fdelta = (float)delta;

        Vector2 inputDir = Vector2.Zero;
        float intensity = 0f;

        if (_joystick != null)
        {
            inputDir = _joystick.Direction;
            intensity = _joystick.Intensity;
        }

        // Keyboard fallback for desktop testing
        var kbd = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        if (kbd != Vector2.Zero)
        {
            inputDir = kbd.Normalized();
            intensity = kbd.Length();
        }

        var targetVelocity = inputDir * MaxSpeed * intensity;
        var accel = intensity > 0f ? Acceleration : Friction;
        Velocity = Velocity.MoveToward(targetVelocity, accel * fdelta);

        MoveAndSlide();
        UpdateVisual(fdelta);
    }

    private void UpdateVisual(float delta)
    {
        var speed = Velocity.Length();
        var moving = speed > 10f;

        if (moving)
        {
            _wobbleTime += delta * 14f;
            var wobbleAmount = Mathf.Clamp(speed / MaxSpeed, 0.3f, 1f);
            _visual.Rotation = Mathf.Sin(_wobbleTime) * 0.12f * wobbleAmount;

            if (Mathf.Abs(Velocity.X) > 1f)
                _visual.Scale = new Vector2(Velocity.X >= 0 ? 1 : -1, 1);
        }
        else
        {
            _wobbleTime = 0f;
            _visual.Rotation = Mathf.Lerp(_visual.Rotation, 0f, delta * 10f);
        }
    }
}
