using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeRogue.Core
{
	/// <summary>
	/// 单词管理器 - 管理游戏中的单词库
	/// </summary>
	public partial class WordManager : Node
	{
		private List<string> _wordBank = new List<string>
	{
		"cat", "dog", "bird", "fish", "tree", "book", "fire", "water",
		"stone", "wind", "light", "dark", "sword", "magic", "power",
		"speed", "jump", "run", "walk", "fly", "swim", "fight", "win",
		"lose", "game", "play", "fun", "cool", "hot", "cold", "warm"
	};
	
	private Random _random = new Random();
	
	public override void _Ready()
	{
		// 初始化逻辑
	}
		
		/// <summary>
		/// 获取随机单词
		/// </summary>
		public string GetRandomWord()
		{
			if (_wordBank.Count == 0) return "word";
			return _wordBank[_random.Next(_wordBank.Count)];
		}
		
		/// <summary>
		/// 验证输入的单词是否正确
		/// </summary>
		public bool ValidateWord(string inputWord, string targetWord)
		{
			return string.Equals(inputWord.Trim().ToLower(), targetWord.ToLower(), StringComparison.OrdinalIgnoreCase);
		}
	}
}
