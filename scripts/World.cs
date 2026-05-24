using Godot;

namespace BanhKhucGame;

public partial class World : Node3D
{
    private Button _raoButton = null!;
    private Label _statusLabel = null!;
    private AudioStreamPlayer _raoPlayer = null!;
    private Player _player = null!;
    private bool _isPlaying;

    private const string RaoAudioPath = "res://assets/audio/rao/tieng_rao.mp3";

    public override void _Ready()
    {
        _raoButton = GetNode<Button>("UI/RaoButton");
        _statusLabel = GetNode<Label>("UI/StatusLabel");
        _raoPlayer = GetNode<AudioStreamPlayer>("UI/RaoPlayer");
        _player = GetNode<Player>("Player");

        var stream = GD.Load<AudioStream>(RaoAudioPath);
        if (stream != null)
        {
            _raoPlayer.Stream = stream;
            _raoButton.Pressed += OnRaoButtonPressed;
            _raoPlayer.Finished += OnRaoFinished;
        }
        else
        {
            _statusLabel.Text = "⚠️ Không tìm thấy file tiếng rao";
        }
    }

    public override void _Process(double delta)
    {
        var hSpeed = new Vector2(_player.Velocity.X, _player.Velocity.Z).Length();
        if (hSpeed > 0.5f && !_isPlaying)
            _statusLabel.Text = "🚴 Đang đạp xe...";
        else if (hSpeed <= 0.5f && !_isPlaying)
            _statusLabel.Text = "🌙 Đêm Hà Nội — kéo joystick để đi";
    }

    private void OnRaoButtonPressed()
    {
        if (_isPlaying)
        {
            _raoPlayer.Stop();
            SetIdleState();
        }
        else
        {
            _raoPlayer.Play();
            _raoButton.Text = "🔇  Tắt Loa";
            _statusLabel.Text = "🔊 \"Xôi lạc bánh khúc đây!\"";
            _isPlaying = true;
        }
    }

    private void OnRaoFinished() => SetIdleState();

    private void SetIdleState()
    {
        _raoButton.Text = "🔊  Bật Loa Rao";
        _isPlaying = false;
    }
}
