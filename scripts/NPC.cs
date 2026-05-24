using Godot;

namespace BanhKhucGame;

public partial class NPC : CharacterBody3D
{
    public enum AttentionState { Idle, Curious, Interested }

    [Export] public float WalkSpeed { get; set; } = 1.5f;
    [Export] public Vector3 PatrolStart { get; set; } = Vector3.Zero;
    [Export] public Vector3 PatrolEnd { get; set; } = new Vector3(0, 1, 10);
    [Export] public float NoticeDuration { get; set; } = 2.5f;
    [Export] public float Gravity { get; set; } = 20f;
    [Export] public Color BodyColor { get; set; } = new Color(0.6f, 0.3f, 0.4f, 1f);

    public AttentionState State { get; private set; } = AttentionState.Idle;

    private float _stateTimer;
    private Vector3 _currentTarget;
    private Label3D _indicator = null!;
    private Node3D _visual = null!;
    private MeshInstance3D? _body;

    public override void _Ready()
    {
        AddToGroup("npcs");
        _indicator = GetNode<Label3D>("Indicator");
        _visual = GetNode<Node3D>("Visual");
        _body = GetNodeOrNull<MeshInstance3D>("Visual/Body");
        _indicator.Visible = false;

        // Apply per-NPC body color
        if (_body != null && BodyColor != Colors.White)
        {
            var mat = new StandardMaterial3D
            {
                AlbedoColor = BodyColor,
                Roughness = 0.7f
            };
            _body.MaterialOverride = mat;
        }

        if (PatrolStart != Vector3.Zero)
            GlobalPosition = PatrolStart;
        _currentTarget = PatrolEnd;
    }

    public override void _PhysicsProcess(double delta)
    {
        var fdelta = (float)delta;
        Vector3 horizontalVel = Vector3.Zero;

        if (State == AttentionState.Idle)
        {
            var toTarget = _currentTarget - GlobalPosition;
            toTarget.Y = 0;

            if (toTarget.Length() < 0.4f)
            {
                _currentTarget = _currentTarget.DistanceTo(PatrolEnd) < 0.5f
                    ? PatrolStart
                    : PatrolEnd;
            }
            else
            {
                var dir = toTarget.Normalized();
                horizontalVel = dir * WalkSpeed;

                var angle = Mathf.Atan2(dir.X, dir.Z);
                var currentY = _visual.Rotation.Y;
                var newY = Mathf.LerpAngle(currentY, angle, fdelta * 8f);
                _visual.Rotation = new Vector3(_visual.Rotation.X, newY, _visual.Rotation.Z);
            }
        }
        else
        {
            _stateTimer -= fdelta;
            if (_stateTimer <= 0)
                SetAttention(AttentionState.Idle);
        }

        var velY = IsOnFloor() ? 0f : Velocity.Y - Gravity * fdelta;
        Velocity = new Vector3(horizontalVel.X, velY, horizontalVel.Z);
        MoveAndSlide();
    }

    public void OnRaoHeard()
    {
        if (State != AttentionState.Idle) return;

        float roll = (float)GD.RandRange(0.0, 1.0);
        if (roll < 0.7f) return;                                   // 70% ignore
        else if (roll < 0.9f) SetAttention(AttentionState.Curious); // 20% "?"
        else SetAttention(AttentionState.Interested);              // 10% "!"
    }

    private void SetAttention(AttentionState s)
    {
        State = s;
        _stateTimer = NoticeDuration;

        switch (s)
        {
            case AttentionState.Idle:
                _indicator.Visible = false;
                break;
            case AttentionState.Curious:
                _indicator.Text = "?";
                _indicator.Modulate = new Color(1f, 1f, 0.5f);
                _indicator.Visible = true;
                break;
            case AttentionState.Interested:
                _indicator.Text = "!";
                _indicator.Modulate = new Color(1f, 0.7f, 0.2f);
                _indicator.Visible = true;
                GD.Print($"[Rao notice] {Name} → interested in bánh khúc");
                break;
        }
    }
}
