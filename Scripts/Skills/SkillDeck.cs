using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class SkillDeck : Resource
{
	[Export] public Array<SkillCard> Cards { get; set; }
	[Export] public int MaxDeckSize { get; set; } = 20;
	
	public List<SkillCard> _drawPile;
	public List<SkillCard> _discardPile;
	private Random _random;
	
	public void Initialize()
	{
		_drawPile = new List<SkillCard>(Cards.ToList());
		_discardPile = new List<SkillCard>();
		_random = new Random();
		ShuffleDeck();
	}
	
	public SkillCard DrawRandomSkill()
	{
		if (_drawPile.Count == 0)
		{
			// 重新洗牌
			ReshuffleDiscardPile();
		}
		
		if (_drawPile.Count == 0) return null;
		
		var card = _drawPile[0];
		_drawPile.RemoveAt(0);
		return card;
	}
	
	public void DiscardSkill(SkillCard skill)
	{
		_discardPile.Add(skill);
	}
	
	private void ShuffleDeck()
	{
		for (int i = _drawPile.Count - 1; i > 0; i--)
		{
			int randomIndex = _random.Next(i + 1);
			var temp = _drawPile[i];
			_drawPile[i] = _drawPile[randomIndex];
			_drawPile[randomIndex] = temp;
		}
	}
	
	private void ReshuffleDiscardPile()
	{
		_drawPile.AddRange(_discardPile);
		_discardPile.Clear();
		ShuffleDeck();
	}
	
	public bool AddCard(SkillCard card)
	{
		if (Cards.Count < MaxDeckSize)
		{
			Cards.Add(card);
			_drawPile.Add(card);
			return true;
		}
		return false;
	}
	
	public bool RemoveCard(SkillCard card)
	{
		bool removed = Cards.Remove(card);
		if (removed)
		{
			_drawPile.Remove(card);
			_discardPile.Remove(card);
		}
		return removed;
	}
	
	// 计算卡组效率
	public float GetChargeEfficiency()
	{
		if (Cards.Count == 0) return 1f;
		
		float totalEfficiency = 0f;
		foreach (var card in Cards)
		{
			// 低消耗技能提供更高的充能效率
			float cardEfficiency = 1f + (10f - card.ChargeCost) * 0.1f;
			totalEfficiency += cardEfficiency;
		}
		
		return totalEfficiency / Cards.Count;
	}
	
	public float GetSkillRatio(SkillType type)
	{
		if (Cards.Count == 0) return 0f;
		return (float)Cards.Count(c => c.Type == type) / Cards.Count;
	}
	
	public float GetAverageDamageMultiplier()
	{
		var attackCards = Cards.Where(c => c.Type == SkillType.Attack);
		if (!attackCards.Any()) return 1f;
		
		return attackCards.Average(c => c.Effects
			.Where(e => e.Type == SkillEffectType.Damage)
			.Sum(e => e.Value));
	}

	internal float GetAverageDefenseValue()
	{
		throw new NotImplementedException();
	}

	internal float GetCostEfficiency()
	{
		throw new NotImplementedException();
	}

	internal int CountSkillsOfType(SkillType typingEnhancement)
	{
		throw new NotImplementedException();
	}
}
