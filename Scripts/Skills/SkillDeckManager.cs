using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CodeRogue.UI;

[GlobalClass]
public partial class SkillDeckManager : Node
{
    [Signal] public delegate void DeckChangedEventHandler(SkillDeck newDeck);
    [Signal] public delegate void CardAddedEventHandler(SkillCard card);
    [Signal] public delegate void CardRemovedEventHandler(SkillCard card);
    
    [Export]
    public SkillDeck _currentDeck;
    private List<SkillDeck> _savedDecks;
    
    public override void _Ready()
    {
        _savedDecks = new List<SkillDeck>();
        InitializeDefaultDeck();
    }
    
    private void InitializeDefaultDeck()
    {



        // 添加一些基础技能
        var database = GetNode<SkillDatabase>("/root/SkillDatabase");
        if (database != null)
        {
            var deck = database.GetDeckByName("Basic");
            if (deck != null)
            {
                _currentDeck = deck;
                _currentDeck.Initialize();
            }
            else
            {
                GD.PrintErr("Initial deck not found");
            }
        }
        else
        {
            GD.PrintErr("SkillDatabase is null");
        }
        
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

    internal void OpenDeck()
    {
        // 创建技能卡组UI场景
        var deckUIScene = GD.Load<PackedScene>("res://Scenes/UI/SkillDeckUI.tscn");
        if (deckUIScene != null)
        {
            var deckUI = deckUIScene.Instantiate<SkillDeckUI>();
            GetTree().CurrentScene.AddChild(deckUI);
        }
        else
        {
            // 如果场景文件不存在，直接创建UI
            var deckUI = new SkillDeckUI();
            GetTree().CurrentScene.AddChild(deckUI);
        }
    }
}