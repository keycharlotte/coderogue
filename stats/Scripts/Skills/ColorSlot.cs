using Godot;
using Godot.Collections;

/// <summary>
/// 颜色槽位数据结构
/// 表示卡牌或英雄的颜色要求
/// </summary>
[GlobalClass]
public partial class ColorSlot : Resource
{
    [Export] public Array<CardColor> Colors { get; set; } = new Array<CardColor>();
    
    public ColorSlot()
    {
        Colors = new Array<CardColor>();
    }
    
    public ColorSlot(params CardColor[] colors)
    {
        Colors = new Array<CardColor>();
        foreach (var color in colors)
        {
            Colors.Add(color);
        }
    }
    
    /// <summary>
    /// 获取颜色槽位数量
    /// </summary>
    public int SlotCount => Colors.Count;
    
    /// <summary>
    /// 检查是否包含指定颜色
    /// </summary>
    public bool ContainsColor(CardColor color)
    {
        return Colors.Contains(color);
    }
    
    /// <summary>
    /// 获取指定位置的颜色
    /// </summary>
    public CardColor GetColor(int index)
    {
        if (index >= 0 && index < Colors.Count)
            return Colors[index];
        return CardColor.Red; // 默认返回红色
    }
    
    /// <summary>
    /// 设置指定位置的颜色
    /// </summary>
    public void SetColor(int index, CardColor color)
    {
        if (index >= 0 && index < Colors.Count)
            Colors[index] = color;
    }
    
    /// <summary>
    /// 添加颜色槽位
    /// </summary>
    public void AddColor(CardColor color)
    {
        if (Colors.Count < 3) // 最多3个槽位
            Colors.Add(color);
    }
    
    /// <summary>
    /// 移除指定位置的颜色槽位
    /// </summary>
    public void RemoveColorAt(int index)
    {
        if (index >= 0 && index < Colors.Count)
            Colors.RemoveAt(index);
    }
    
    /// <summary>
    /// 清空所有颜色槽位
    /// </summary>
    public void Clear()
    {
        Colors.Clear();
    }
    
    /// <summary>
    /// 检查是否为空
    /// </summary>
    public bool IsEmpty => Colors.Count == 0;
    
    /// <summary>
    /// 检查是否已满（3个槽位）
    /// </summary>
    public bool IsFull => Colors.Count >= 3;
    
    /// <summary>
    /// 获取颜色槽位的字符串表示
    /// </summary>
    public override string ToString()
    {
        if (Colors.Count == 0)
            return "Empty";
        
        var colorNames = new string[Colors.Count];
        for (int i = 0; i < Colors.Count; i++)
        {
            colorNames[i] = Colors[i].ToString();
        }
        return string.Join(", ", colorNames);
    }
}