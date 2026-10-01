public enum MenuType
{
    None,
    Main,
    Credit,
    Gameplay,
    Exit,
    CompleteStage,
    GameOver,
    Pause,
    Setting,
    Revive,

    // Thêm vào CUỐI enum: _type được serialize theo số thứ tự,
    // chèn vào giữa sẽ làm lệch các menu đã lưu trong scene.
    Shop
}