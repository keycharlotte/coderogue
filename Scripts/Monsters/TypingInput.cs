using System;

namespace CodeRogue.Monsters
{
    /// <summary>
    /// 打字输入数据
    /// </summary>
    [Serializable]
    public class TypingInput
    {
        public string Text { get; set; }
        public float Timestamp { get; set; }
        public int CharacterCount { get; set; }
        public bool IsCorrect { get; set; } = true;
    }
}