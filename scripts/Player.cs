using Godot;

namespace BanhKhucGame;

public partial class Player : CharacterBody3D
{
    [Export] public float MaxSpeed { get; set; } = 6f;
    [Export] public float Acceleration { get; set; } = 30f;
    [Export] public float Friction { get; set; } = 40f;
    [Export] public float Gravity { get; set; } = 20f;
    [Export] public NodePath JoystickPath { get; set; } = "";

    private VirtualJoystick? _joystick;
    private Node3D _visual = null!;
    private float _wobbleTime;
    private float _baseVisualY;

    public override void _Ready()
    {
        _visual = GetNode<Node3D>("Visual");
        _baseVisualY = _visual.Position.Y;
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

        var kbd = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        if (kbd != Vector2.Zero)
        {
            inputDir = kbd.Normalized();
            intensity = kbd.Length();
        }

        // Joystick X -> world X, Joystick Y (screen down +) -> world Z
        // Joystick UP (negative Y) -> forward into scene (negative Z)
        var moveDir = new Vector3(inputDir.X, 0, inputDir.Y);

        var targetVelocity = moveDir * MaxSpeed * intensity;
        var accel = intensity > 0f ? Acceleration : Friction;

        var hVel = new Vector3(Velocity.X, 0, Velocity.Z);
        hVel = hVel.MoveToward(targetVelocity, accel * fdelta);

        var newVelY = IsOnFloor() ? 0f : Velocity.Y - Gravity * fdelta;
        Velocity = new Vector3(hVel.X, newVelY, hVel.Z);

        MoveAndSlide();
        UpdateVisual(fdelta);
    }

    private void UpdateVisual(float delta)
    {
        var horizontalSpeed = new Vector2(Velocity.X, Velocity.Z).Length();
        var moving = horizontalSpeed > 0.5f;

        if (moving)
        {
            _wobbleTime += delta * 14f;
            var bob = Mathf.Abs(Mathf.Sin(_wobbleTime)) * 0.2f;
            _visual.Position = new Vector3(_visual.Position.X, _baseVisualY + bob, _visual.Position.Z);

            // Face movement direction (rotate around Y)
            var targetAngle = Mathf.Atan2(Velocity.X, Velocity.Z);
            var currentY = _visual.Rotation.Y;
            var newY = Mathf.LerpAngle(currentY, targetAngle, delta * 12f);
            _visual.Rotation = new Vector3(_visual.Rotation.X, newY, _visual.Rotation.Z);
        }
        else
        {
            _wobbleTime = 0f;
            var newY = Mathf.Lerp(_visual.Position.Y, _baseVisualY, delta * 10f);
            _visual.Position = new Vector3(_visual.Position.X, newY, _visual.Position.Z);
        }
    }
}
