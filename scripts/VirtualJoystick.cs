using Godot;

namespace BanhKhucGame;

public partial class VirtualJoystick : Control
{
    [Export] public float BaseRadius { get; set; } = 100f;
    [Export] public float StickRadius { get; set; } = 45f;
    [Export] public float MaxOffset { get; set; } = 80f;
    [Export] public float DeadZone { get; set; } = 8f;
    [Export] public Color BaseColor { get; set; } = new(1, 1, 1, 0.15f);
    [Export] public Color BaseRingColor { get; set; } = new(1, 1, 1, 0.35f);
    [Export] public Color StickColor { get; set; } = new(1, 1, 1, 0.45f);

    public Vector2 Direction { get; private set; } = Vector2.Zero;
    public float Intensity { get; private set; }

    private const int MouseTouchIndex = -2;
    private int _activeTouchIndex = -1;
    private Vector2 _stickOffset = Vector2.Zero;

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(BaseRadius * 2, BaseRadius * 2);
        MouseFilter = MouseFilterEnum.Stop;
    }

    public override void _Draw()
    {
        var center = Size / 2;
        DrawCircle(center, BaseRadius, BaseColor);
        DrawArc(center, BaseRadius, 0, Mathf.Tau, 64, BaseRingColor, 3f, antialiased: true);
        DrawCircle(center + _stickOffset, StickRadius, StickColor);
    }

    public override void _Input(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventScreenTouch touch:
                if (touch.Pressed) TryGrabTouch(touch.Index, touch.Position);
                else ReleaseTouch(touch.Index);
                break;

            case InputEventScreenDrag drag:
                if (drag.Index == _activeTouchIndex) UpdateStick(drag.Position);
                break;

            case InputEventMouseButton mouseBtn when mouseBtn.ButtonIndex == MouseButton.Left:
                if (mouseBtn.Pressed) TryGrabTouch(MouseTouchIndex, mouseBtn.Position);
                else ReleaseTouch(MouseTouchIndex);
                break;

            case InputEventMouseMotion motion when _activeTouchIndex == MouseTouchIndex:
                UpdateStick(motion.Position);
                break;
        }
    }

    private void TryGrabTouch(int index, Vector2 globalPosition)
    {
        if (_activeTouchIndex != -1) return;
        if (!GetGlobalRect().HasPoint(globalPosition)) return;
        _activeTouchIndex = index;
        UpdateStick(globalPosition);
    }

    private void ReleaseTouch(int index)
    {
        if (index != _activeTouchIndex) return;
        _activeTouchIndex = -1;
        _stickOffset = Vector2.Zero;
        Direction = Vector2.Zero;
        Intensity = 0f;
        QueueRedraw();
    }

    private void UpdateStick(Vector2 globalPosition)
    {
        var center = GlobalPosition + Size / 2;
        var offset = globalPosition - center;
        var distance = offset.Length();

        if (distance < DeadZone)
        {
            _stickOffset = Vector2.Zero;
            Direction = Vector2.Zero;
            Intensity = 0f;
        }
        else
        {
            if (distance > MaxOffset)
                offset = offset.Normalized() * MaxOffset;

            _stickOffset = offset;
            Direction = offset.Normalized();
            Intensity = Mathf.Clamp((distance - DeadZone) / (MaxOffset - DeadZone), 0f, 1f);
        }

        QueueRedraw();
    }
}
