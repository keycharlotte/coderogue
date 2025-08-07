using Godot;
using CodeRogue.Data;
using CodeRogue.Utils;

namespace CodeRogue.Core
{
	/// <summary>
	/// 音频管理器 - 处理游戏音效和音乐
	/// </summary>
	public partial class AudioManager : Node
	{
		[Export]
		public AudioStream MenuMusic { get; set; }

		[Export]
		public AudioStream GameMusic { get; set; }

		[Export]
		public AudioStream[] SoundEffects { get; set; }

		private AudioStreamPlayer _musicPlayer;
		private AudioStreamPlayer _sfxPlayer;

		// 添加音量属性
		public float MasterVolume { get; private set; } = 1.0f;
		public float MusicVolume { get; private set; } = 0.8f;
		public float SfxVolume { get; private set; } = 1.0f;

		public override void _Ready()
		{
			InitializeAudio();
		}


		private void InitializeAudio()
		{
			_musicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");
			_sfxPlayer = GetNode<AudioStreamPlayer>("SFXPlayer");

			// 加载音频设置
			LoadAudioSettings();
		}

		private void LoadAudioSettings()
		{
			var GameData = NodeUtils.GetGameData(this);
			// 从游戏数据加载音量设置
			MasterVolume = GameData.MasterVolume;
			MusicVolume = GameData.MusicVolume;
			SfxVolume = GameData.SfxVolume;

			// 应用音量设置
			UpdateAudioVolumes();
		}

		private void UpdateAudioVolumes()
		{
			if (_musicPlayer != null)
			{
				_musicPlayer.VolumeDb = Mathf.LinearToDb(MasterVolume * MusicVolume);
			}

			if (_sfxPlayer != null)
			{
				_sfxPlayer.VolumeDb = Mathf.LinearToDb(MasterVolume * SfxVolume);
			}
		}

		public void PlayMusic(AudioStream music, bool loop = true)
		{
			if (_musicPlayer != null && music != null)
			{
				_musicPlayer.Stream = music;
				_musicPlayer.Play();
			}
		}

		public void PlaySFX(AudioStream sfx)
		{
			if (_sfxPlayer != null && sfx != null)
			{
				_sfxPlayer.Stream = sfx;
				_sfxPlayer.Play();
			}
		}

		public void StopMusic()
		{
			_musicPlayer?.Stop();
		}

		// 添加主音量控制方法
		public void SetMasterVolume(float volume)
		{
			MasterVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
			UpdateAudioVolumes();

			// 保存设置
			SaveAudioSettings();
		}

		public void SetMusicVolume(float volume)
		{
			MusicVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
			UpdateAudioVolumes();

			// 保存设置
			SaveAudioSettings();
		}

		public void SetSFXVolume(float volume)
		{
			SfxVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
			UpdateAudioVolumes();

			// 保存设置
			SaveAudioSettings();
		}

		private void SaveAudioSettings()
		{
			var GameData = NodeUtils.GetGameData(this);

			GameData.MasterVolume = MasterVolume;
			GameData.MusicVolume = MusicVolume;
			GameData.SfxVolume = SfxVolume;
			GameData.SaveGameData();
		}
	}
}
