using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CodeRogue.Skills;

[GlobalClass]
public partial class SkillTrackManager : Node
{
	[Signal] public delegate void TrackChargedEventHandler(int trackIndex, SkillCard skill);
	[Signal] public delegate void SkillActivatedEventHandler(SkillCard skill, int trackIndex);
	[Signal] public delegate void ChargeUpdatedEventHandler(int trackIndex, float currentCharge, float maxCharge);
	[Signal] public delegate void SkillEquippedEventHandler(int trackIndex, SkillCard skill);
	[Signal] public delegate void TemporaryTrackAddedEventHandler(int trackIndex, SkillCard skill);
	[Signal] public delegate void TemporaryTrackRemovedEventHandler(int trackIndex);

	[Export] public int MaxTracks { get; set; } = 4;
	[Export] public float BaseChargeRate { get; set; } = 1f; // 每秒充能速度
	[Export] public float MaxChargePerTrack { get; set; } = 2000f;

	private CardDatabase _database;
	private List<SkillTrack> _tracks;
	private UnifiedDeck _currentDeck;

	public override void _Ready()
	{
		_database = GetNode<CardDatabase>("/root/CardDatabase");
		if (_database == null)
		{
			GD.PrintErr("CardDatabase autoload not found!");
		}
		InitializeTracks();
	}

	public override void _Process(double delta)
	{
		UpdateTrackCharging((float)delta);
		CheckAutoActivation();
	}

	private void InitializeTracks()
	{
		_tracks = new List<SkillTrack>();
		for (int i = 0; i < MaxTracks; i++)
		{
			_tracks.Add(new SkillTrack
			{
				Index = i,
				CurrentCharge = 0f,
				MaxCharge = MaxChargePerTrack,
				State = TrackState.Empty
			});
		}
	}

	private void UpdateTrackCharging(float delta)
	{
		foreach (var track in _tracks)
		{
			if (track.State == TrackState.Charging && track.EquippedSkill != null)
			{
				float chargeGain = BaseChargeRate * delta;

				// 应用充能速度加成
				chargeGain *= GetChargeRateMultiplier();

				track.CurrentCharge = Mathf.Min(track.CurrentCharge + chargeGain, track.MaxCharge);

				EmitSignal(SignalName.ChargeUpdated, track.Index, track.CurrentCharge, track.MaxCharge);

				// 检查是否充能完成
				if (track.CurrentCharge >= track.EquippedSkill.Cost)
				{
					track.State = TrackState.Ready;
					EmitSignal(SignalName.TrackCharged, track.Index, track.EquippedSkill);
				}
			}
		}
	}

	private void CheckAutoActivation()
	{
		foreach (var track in _tracks)
		{
			if (track.State == TrackState.Ready && track.EquippedSkill != null)
			{
				// 自动释放技能
				ActivateSkill(track.Index);
			}
		}
	}

	public void ActivateSkill(int trackIndex)
	{
		if (trackIndex < 0 || trackIndex >= _tracks.Count) return;

		var track = _tracks[trackIndex];
		if (track.State != TrackState.Ready || track.EquippedSkill == null) return;

		// 保存技能引用用于后续检查
		var activatedSkill = track.EquippedSkill;

		// 执行技能效果
		ExecuteSkillEffect(activatedSkill);

		// 重置轨道
		track.CurrentCharge = 0f;
		track.State = TrackState.Empty;
		track.EquippedSkill = null;

		// 自动装填新技能
		AutoEquipNextSkill(trackIndex);

		EmitSignal(SignalName.SkillActivated, activatedSkill, trackIndex);

	}

	private void AutoEquipNextSkill(int trackIndex)
	{
		if (_currentDeck == null) return;

		var nextSkill = _currentDeck.DrawRandomSkill();
		if (nextSkill != null)
		{
			EquipSkillToTrack(nextSkill, trackIndex);
		}
	}

	public void EquipSkillToTrack(SkillCard skill, int trackIndex)
	{
		if (trackIndex < 0 || trackIndex >= _tracks.Count)
		{
			GD.Print($"Invalid track index {trackIndex}");
			return;
		}

		var track = _tracks[trackIndex];
		track.EquippedSkill = skill;
		track.State = TrackState.Charging;
		track.CurrentCharge = 0f;

		GD.Print($"Equipped skill {skill.Name} to track {trackIndex}");

		// 发射技能装备信号通知UI更新
		EmitSignal(SignalName.SkillEquipped, trackIndex, skill);

	}

	public void AddCharge(int amount, string source = "")
	{
		foreach (var track in _tracks)
		{
			if (track.State == TrackState.Charging)
			{
				track.CurrentCharge = Mathf.Min(track.CurrentCharge + amount, track.MaxCharge);
				EmitSignal(SignalName.ChargeUpdated, track.Index, track.CurrentCharge, track.MaxCharge);
			}
		}
	}

	private float GetChargeRateMultiplier()
	{
		float multiplier = 1f;

		// // 英雄加成
		// var hero = HeroManager.Instance?.GetActiveHero();
		// if (hero != null)
		// {
		//     multiplier *= hero.GetChargeRateBonus();
		// }

		// // 遗物加成
		// var relics = RelicManager.Instance?.GetActiveRelics();
		// if (relics != null)
		// {
		//     multiplier *= relics.Sum(r => r.GetChargeRateBonus());
		// }

		// // Buff加成
		// var buffs = BuffManager.Instance?.GetActiveBuffs();
		// if (buffs != null)
		// {
		//     multiplier *= buffs.Where(b => b.HasTag("ChargeRate")).Sum(b => b.GetEffectValue());
		// }

		return multiplier;
	}

	private void ExecuteSkillEffect(SkillCard skill)
	{
		foreach (var effect in skill.Effects)
		{
			switch (effect.Type)
			{
				case SkillEffectType.Damage:
					ExecuteDamageEffect(effect);
					break;
				case SkillEffectType.Heal:
					ExecuteHealEffect(effect);
					break;
				case SkillEffectType.Shield:
					ExecuteShieldEffect(effect);
					break;
				case SkillEffectType.Buff:
					ExecuteBuffEffect(effect);
					break;
				case SkillEffectType.TypingModifier:
					ExecuteTypingModifierEffect(effect);
					break;
				case SkillEffectType.TemporaryTrack:
					ExecuteTemporaryTrackEffect(effect);
					break;
					// ... 其他效果类型
			}
		}
	}

	private void ExecuteHealEffect(SkillEffect effect)
	{
		throw new NotImplementedException();
	}

	private void ExecuteTemporaryTrackEffect(SkillEffect effect)
		{
			if (effect is TemporaryTrackSkillEffect tempTrackEffect)
			{
				tempTrackEffect.Execute();
			}
			else
			{
				GD.PrintErr("ExecuteTemporaryTrackEffect: 效果类型不匹配");
			}
		}

		private void ExecuteShieldEffect(SkillEffect effect)
		{
			throw new NotImplementedException();
		}

		private void ExecuteBuffEffect(SkillEffect effect)
		{
			throw new NotImplementedException();
		}

		private void ExecuteDamageEffect(SkillEffect effect)
		{
			// 对所有敌人造成伤害
			var enemies = GetTree().GetNodesInGroup("enemies");
			foreach (Node enemy in enemies)
			{
				if (enemy.HasMethod("TakeDamage"))
				{
					enemy.Call("TakeDamage", effect.Value);
				}
			}
		}

		private void ExecuteTypingModifierEffect(SkillEffect effect)
		{
			// 影响打字体验的效果
			var typingSystem = TypingCombatSystem.Instance;
			if (typingSystem != null)
			{
				switch (effect.TargetProperty)
				{
					case "WordLength":
						// 减少敌人单词长度
						var enemies = GetTree().GetNodesInGroup("enemies");
						foreach (Node enemy in enemies)
						{
							if (enemy.HasMethod("ModifyWordLength"))
							{
								enemy.Call("ModifyWordLength", -effect.Value, effect.Duration);
							}
						}
						break;
					case "ChargeMultiplier":
						// 双倍充能获取
						typingSystem.ApplyChargeMultiplier(effect.Value, effect.Duration);
						break;
				}
			}
		}

		public void SetDeck(UnifiedDeck deck)
		{
			_currentDeck = deck;
			// 重新装填所有轨道
			for (int i = 0; i < _tracks.Count; i++)
			{
				AutoEquipNextSkill(i);
			}
		}

		public List<SkillTrack> GetTracks() => _tracks;
		public SkillTrack GetTrack(int index) => index >= 0 && index < _tracks.Count ? _tracks[index] : null;

		internal void TryActivateSkill()
		{
			throw new NotImplementedException();
		}

		internal bool CanActivateAnySkill()
		{
			throw new NotImplementedException();
		}

		internal void ClearTrack(int i)
		{
			throw new NotImplementedException();
		}

		internal void StartCharging()
		{
			throw new NotImplementedException();
		}

		// 临时轨道底层支持方法
		public int AddTemporaryTrack(SkillCard skill, TemporaryTrackDestroyCondition destroyCondition, float duration = 0f, string createdByBuffId = "")
		{
			// 创建新的临时轨道
			var tempTrack = new SkillTrack
			{
				Index = _tracks.Count,
				EquippedSkill = skill,
				CurrentCharge = 0f,
				MaxCharge = MaxChargePerTrack,
				State = skill != null ? TrackState.Charging : TrackState.Empty,
				Source = TrackSource.Skill,
			};

			_tracks.Add(tempTrack);

			GD.Print($"添加临时轨道 {tempTrack.Index}，销毁条件: {destroyCondition}，持续时间: {duration}秒");

			EmitSignal(SignalName.TemporaryTrackAdded, tempTrack.Index, skill);

			return tempTrack.Index;
		}

		public void RemoveTemporaryTrack(int trackIndex)
		{
			if (trackIndex < 0 || trackIndex >= _tracks.Count) return;

			var track = _tracks[trackIndex];

			GD.Print($"移除临时轨道 {trackIndex}");

			_tracks.RemoveAt(trackIndex);

			// 重新索引剩余轨道
			for (int i = trackIndex; i < _tracks.Count; i++)
			{
				_tracks[i].Index = i;
			}

			EmitSignal(SignalName.TemporaryTrackRemoved, trackIndex);
		}
	}
