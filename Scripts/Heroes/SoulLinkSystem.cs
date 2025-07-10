using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class SoulLinkSystem : Node
{
    [Signal] public delegate void SoulLinkEstablishedEventHandler(HeroInstance activeHero, HeroInstance linkedHero);
    [Signal] public delegate void SoulLinkBrokenEventHandler(HeroInstance activeHero, HeroInstance linkedHero);
    
    private Godot.Collections.Dictionary<string, Array<string>> _activeSoulLinks; // 活跃链接 <活跃英雄ID, 链接英雄ID列表>
    private Godot.Collections.Dictionary<string, Array<PassiveEffect>> _linkEffects; // 链接效果缓存
    
    public override void _Ready()
    {
        _activeSoulLinks = new Godot.Collections.Dictionary<string, Array<string>>();
        _linkEffects = new Godot.Collections.Dictionary<string, Array<PassiveEffect>>();
    }
    
    // 建立灵魂链接
    public bool EstablishSoulLink(HeroInstance activeHero, HeroInstance linkedHero)
    {
        // 检查链接条件
        if (!CanEstablishLink(activeHero, linkedHero))
            return false;
        
        // 检查链接数量限制
        if (!CheckLinkLimit(activeHero))
            return false;
        
        // 建立链接
        if (!_activeSoulLinks.ContainsKey(activeHero.InstanceId))
            _activeSoulLinks[activeHero.InstanceId] = new Array<string>();
        
        _activeSoulLinks[activeHero.InstanceId].Add(linkedHero.InstanceId);
        linkedHero.IsSoulLinked = true;
        
        // 更新效果缓存
        UpdateLinkEffects(activeHero);
        
        EmitSignal(SignalName.SoulLinkEstablished, activeHero, linkedHero);
        return true;
    }
    
    // 断开灵魂链接
    public bool BreakSoulLink(HeroInstance activeHero, HeroInstance linkedHero)
    {
        if (!_activeSoulLinks.ContainsKey(activeHero.InstanceId))
            return false;
        
        var links = _activeSoulLinks[activeHero.InstanceId];
        if (!links.Contains(linkedHero.InstanceId))
            return false;
        
        links.Remove(linkedHero.InstanceId);
        linkedHero.IsSoulLinked = false;
        
        // 更新效果缓存
        UpdateLinkEffects(activeHero);
        
        EmitSignal(SignalName.SoulLinkBroken, activeHero, linkedHero);
        return true;
    }
    
    // 获取链接效果
    public Array<PassiveEffect> GetLinkEffects(HeroInstance activeHero)
    {
        return _linkEffects.GetValueOrDefault(activeHero.InstanceId, new Array<PassiveEffect>());
    }
    
    // 检查是否可以建立链接
    public bool CanEstablishLink(HeroInstance activeHero, HeroInstance linkedHero)
    {
        var linkConfig = linkedHero.Config.SoulLink;
        if (linkConfig == null) return false;
        
        // 检查所有链接条件
        foreach (var condition in linkConfig.Conditions)
        {
            if (!CheckCondition(activeHero, linkedHero, condition))
                return false;
        }
        
        return true;
    }
    
    // 检查链接条件
    private bool CheckCondition(HeroInstance activeHero, HeroInstance linkedHero, SoulLinkCondition condition)
    {
        return condition.Type switch
        {
            SoulLinkConditionType.HeroClass => (int)activeHero.Config.Class == condition.Value.AsInt32(),
            SoulLinkConditionType.HeroRarity => (int)activeHero.Config.Rarity >= condition.Value.AsInt32(),
            SoulLinkConditionType.HeroLevel => activeHero.Level >= condition.Value.AsInt32(),
            SoulLinkConditionType.HeroStar => activeHero.Star >= condition.Value.AsInt32(),
            SoulLinkConditionType.SpecificHero => activeHero.ConfigId == condition.Value.AsInt32(),
            _ => true
        };
    }
    
    // 检查链接数量限制
    private bool CheckLinkLimit(HeroInstance activeHero)
    {
        if (!_activeSoulLinks.ContainsKey(activeHero.InstanceId))
            return true;
        
        var currentLinks = _activeSoulLinks[activeHero.InstanceId].Count;
        // TODO: 从配置中获取最大链接数
        var maxLinks = 2; // 默认最大2个链接
        
        return currentLinks < maxLinks;
    }
    
    // 更新链接效果缓存
    private void UpdateLinkEffects(HeroInstance activeHero)
    {
        var effects = new Array<PassiveEffect>();
        
        if (_activeSoulLinks.TryGetValue(activeHero.InstanceId, out var linkedHeroIds))
        {
            foreach (var linkedHeroId in linkedHeroIds)
            {
                var linkedHero = HeroManager.Instance.GetHeroInstance(linkedHeroId);
                if (linkedHero?.Config.SoulLink != null)
                {
                    foreach (var effect in linkedHero.Config.SoulLink.PassiveEffects)
                    {
                        effects.Add(effect);
                    }
                }
            }
        }
        
        _linkEffects[activeHero.InstanceId] = effects;
    }
}