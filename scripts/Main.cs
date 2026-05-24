using Godot;

namespace BanhKhucGame;

public partial class Main : Node2D
{
    private Button _raoButton = null!;
    private Label _statusLabel = null!;
    private AudioStreamPlayer _raoPlayer = null!;
    private bool _isPlaying;

    private const string RaoAudioPath = "res://assets/audio/rao/tieng_rao.mp3";

    public override void _Ready()
    {
        _raoButton = GetNode<Button>("VBox/RaoButton");
        _statusLabel = GetNode<Label>("VBox/StatusLabel");
        _raoPlayer = GetNode<AudioStreamPlayer>("RaoPlayer");

        var stream = GD.Load<AudioStream>(RaoAudioPath);
        if (stream == null)
        {
            _statusLabel.Text = "⚠️ Không tìm thấy file tiếng rao";
            GD.PushError($"Không load được audio: {RaoAudioPath}");
            return;
        }

        _raoPlayer.Stream = stream;
        _raoButton.Pressed += OnRaoButtonPressed;
        _raoPlayer.Finished += OnRaoFinished;

        GD.Print("M1 ready. Audio loaded: " + RaoAudioPath);
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
            _statusLabel.Text = "🌙 Đang rao... \"Xôi lạc bánh khúc đây!\"";
            _isPlaying = true;
        }
    }

    private void OnRaoFinished()
    {
        SetIdleState();
    }

    private void SetIdleState()
    {
        _raoButton.Text = "🔊  Bật Loa Rao";
        _statusLabel.Text = "Nhấn nút để nghe tiếng rao";
        _isPlaying = false;
    }
}
