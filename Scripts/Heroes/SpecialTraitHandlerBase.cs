using Godot;
using System;

namespace CodeRogue.Heroes
{
    /// <summary>
    /// 特性处理器抽象基类
    /// 用于替代ISpecialTraitHandler接口，解决Godot Variant兼容性问题
    /// </summary>
    public abstract partial class SpecialTraitHandlerBase : RefCounted
    {
        /// <summary>
        /// 执行特性效果
        /// </summary>
        /// <param name="hero">英雄实例</param>
        /// <param name="trait">特性配置</param>
        /// <param name="context">上下文数据</param>
        public abstract void Execute(HeroInstance hero, SpecialTraitConfig trait, Variant context);
        
        /// <summary>
        /// 获取特性处理器类型名称
        /// </summary>
        /// <returns>处理器类型名称</returns>
        public virtual string GetHandlerTypeName()
        {
            return GetType().Name;
        }
        
        /// <summary>
        /// 验证特性配置是否有效
        /// </summary>
        /// <param name="trait">特性配置</param>
        /// <returns>是否有效</returns>
        public virtual bool ValidateTraitConfig(SpecialTraitConfig trait)
        {
            if (trait == null)
            {
                LogError("特性配置为空");
                return false;
            }
            
            if (string.IsNullOrEmpty(trait.Name))
            {
                LogError("特性名称为空");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 验证英雄实例是否有效
        /// </summary>
        /// <param name="hero">英雄实例</param>
        /// <returns>是否有效</returns>
        public virtual bool ValidateHeroInstance(HeroInstance hero)
        {
            if (hero == null)
            {
                LogError("英雄实例为空");
                return false;
            }
            
            if (hero.Config == null)
            {
                LogError("英雄配置为空");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 检查特性执行前置条件
        /// </summary>
        /// <param name="hero">英雄实例</param>
        /// <param name="trait">特性配置</param>
        /// <param name="context">上下文数据</param>
        /// <returns>是否满足前置条件</returns>
        public virtual bool CheckPrerequisites(HeroInstance hero, SpecialTraitConfig trait, Variant context)
        {
            return ValidateHeroInstance(hero) && ValidateTraitConfig(trait);
        }
        
        /// <summary>
        /// 获取特性支持的触发器类型
        /// </summary>
        /// <returns>触发器类型</returns>
        public virtual SpecialTraitTrigger GetSupportedTrigger()
        {
            return SpecialTraitTrigger.OnCombatStart; // 默认值
        }
        
        /// <summary>
        /// 获取特性描述
        /// </summary>
        /// <param name="trait">特性配置</param>
        /// <param name="heroLevel">英雄等级</param>
        /// <returns>特性描述</returns>
        public virtual string GetTraitDescription(SpecialTraitConfig trait, int heroLevel)
        {
            if (trait == null) return "未知特性";
            
            var description = trait.Description;
            
            // 替换参数占位符
            if (trait.Parameters != null)
            {
                foreach (var param in trait.Parameters)
                {
                    var value = param.Value;
                    if (trait.ScalesWithLevel)
                    {
                        value = CalculateScaledValue(value, heroLevel, trait.LevelScaling);
                    }
                    description = description.Replace($"{{{param.Key}}}", value.ToString());
                }
            }
            
            return description;
        }
        
        /// <summary>
        /// 计算等级缩放值
        /// </summary>
        /// <param name="baseValue">基础值</param>
        /// <param name="level">等级</param>
        /// <param name="scaling">缩放系数</param>
        /// <returns>缩放后的值</returns>
        protected virtual float CalculateScaledValue(Variant baseValue, int level, float scaling)
        {
            var floatValue = baseValue.AsSingle();
            return floatValue + (level - 1) * scaling;
        }
        
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">错误信息</param>
        protected virtual void LogError(string message)
        {
            GD.PrintErr($"[{GetHandlerTypeName()}] {message}");
        }
        
        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">信息内容</param>
        protected virtual void LogInfo(string message)
        {
            GD.Print($"[{GetHandlerTypeName()}] {message}");
        }
        
        /// <summary>
        /// 执行特性效果后的清理工作
        /// </summary>
        /// <param name="hero">英雄实例</param>
        /// <param name="trait">特性配置</param>
        /// <param name="context">上下文数据</param>
        protected virtual void PostExecuteCleanup(HeroInstance hero, SpecialTraitConfig trait, Variant context)
        {
            // 默认不执行任何清理操作
            // 子类可以重写此方法来实现特定的清理逻辑
        }
    }
}