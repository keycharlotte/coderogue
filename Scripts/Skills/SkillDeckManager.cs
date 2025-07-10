using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class SkillDeckManager : Node
{
    private static SkillDeckManager _instance;
    public static SkillDeckManager Instance => _instance;
    
    [Signal] public delegate void DeckChangedEventHandler(SkillDeck newDeck);
    [Signal] public delegate void CardAddedEventHandler(SkillCard card);
    [Signal] public delegate void CardRemovedEventHandler(SkillCard card);
    
    private SkillDeck _currentDeck;
    private List<SkillDeck> _savedDecks;
    
    public override void _Ready()
    {
        _instance = this;
        _savedDecks = new List<SkillDeck>();
        InitializeDefaultDeck();
    }
    
    private void InitializeDefaultDeck()
    {
        _currentDeck = new SkillDeck();
        _currentDeck.MaxDeckSize = 20;
        _currentDeck.Cards = new Godot.Collections.Array<SkillCard>();
        
        // 添加一些基础技能
        var database = SkillDatabase.Instance;
        if (database != null)
        {
            var basicSkills = database.GetSkillsByRarity(SkillRarity.Common);
            foreach (var skill in basicSkills.Take(4)) // 添加前4个基础技能
            {
                _currentDeck.AddCard(skill);
            }
        }
        
        _currentDeck.Initialize();
        EmitSignal(SignalName.DeckChanged, _currentDeck);
    }
    
    public SkillDeck GetCurrentDeck()
    {
        return _currentDeck;
    }
    
    public void SetCurrentDeck(SkillDeck deck)
    {
        _currentDeck = deck;
        _currentDeck.Initialize();
        EmitSignal(SignalName.DeckChanged, _currentDeck);
    }
    
    public void AddCardToDeck(SkillCard card)
    {
        if (_currentDeck.AddCard(card))
        {
            EmitSignal(SignalName.CardAdded, card);
            EmitSignal(SignalName.DeckChanged, _currentDeck);
        }
    }
    
    public void RemoveCardFromDeck(SkillCard card)
    {
        if (_currentDeck.RemoveCard(card))
        {
            EmitSignal(SignalName.CardRemoved, card);
            EmitSignal(SignalName.DeckChanged, _currentDeck);
        }
    }
    
    public void SaveDeck(string name)
    {
        var deckCopy = (SkillDeck)_currentDeck.Duplicate();
        // 这里可以添加保存到文件的逻辑
        _savedDecks.Add(deckCopy);
    }
    
    public List<SkillDeck> GetSavedDecks()
    {
        return new List<SkillDeck>(_savedDecks);
    }

    internal float GetChargeEfficiency()
    {
        throw new NotImplementedException();
    }
}