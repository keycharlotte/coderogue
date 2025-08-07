using Godot;
using Godot.Collections;
using CodeRogue.Achievements.Data;

namespace CodeRogue.Achievements
{
    /// <summary>
    /// 奖励处理器抽象基类
    /// 替代IRewardHandler接口以解决Godot Variant兼容性问题
    /// </summary>
    public abstract partial class RewardHandlerBase : RefCounted
    {
        /// <summary>
        /// 发放奖励
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        /// <param name="reward">奖励配置</param>
        /// <returns>奖励结果</returns>
        public abstract RewardResult GrantReward(string achievementId, RewardConfig reward);
        
        /// <summary>
        /// 获取奖励预览
        /// </summary>
        /// <param name="reward">奖励配置</param>
        /// <returns>奖励预览</returns>
        public abstract RewardPreview GetRewardPreview(RewardConfig reward);
        
        /// <summary>
        /// 检查是否可以发放奖励
        /// </summary>
        /// <param name="reward">奖励配置</param>
        /// <returns>是否可以发放</returns>
        public abstract bool CanGrantReward(RewardConfig reward);
        
        /// <summary>
        /// 验证奖励配置
        /// </summary>
        /// <param name="reward">奖励配置</param>
        /// <returns>验证结果</returns>
        protected virtual RewardValidationResult ValidateRewardConfig(RewardConfig reward)
        {
            var result = new RewardValidationResult
            {
                IsValid = true,
                ErrorMessage = string.Empty
            };
            
            if (reward == null)
            {
                result.IsValid = false;
                result.ErrorMessage = "奖励配置为空";
                return result;
            }
            
            if (reward.Amount <= 0)
            {
                result.IsValid = false;
                result.ErrorMessage = "奖励数量必须大于0";
                return result;
            }
            
            if (string.IsNullOrEmpty(reward.ItemId))
            {
                result.IsValid = false;
                result.ErrorMessage = "奖励ID不能为空";
                return result;
            }
            
            return result;
        }
        
        /// <summary>
        /// 创建成功的奖励结果
        /// </summary>
        /// <param name="message">成功消息</param>
        /// <param name="grantedAmount">实际发放数量</param>
        /// <returns>奖励结果</returns>
        protected virtual RewardResult CreateSuccessResult(string message, int grantedAmount = 0)
        {
            return new RewardResult
            {
                Success = true,
                Message = message,
                GrantedAmount = grantedAmount
            };
        }
        
        /// <summary>
        /// 创建失败的奖励结果
        /// </summary>
        /// <param name="message">失败消息</param>
        /// <returns>奖励结果</returns>
        protected virtual RewardResult CreateFailureResult(string message)
        {
            return new RewardResult
            {
                Success = false,
                Message = message,
                GrantedAmount = 0
            };
        }
        
        /// <summary>
        /// 记录奖励发放日志
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        /// <param name="reward">奖励配置</param>
        /// <param name="result">发放结果</param>
        protected virtual void LogRewardGrant(string achievementId, RewardConfig reward, RewardResult result)
        {
            var status = result.Success ? "成功" : "失败";
            GD.Print($"[{GetHandlerTypeName()}] 奖励发放{status}: 成就={achievementId}, 类型={reward.Type}, 数量={reward.Amount}, 结果={result.Message}");
        }
        
        /// <summary>
        /// 获取处理器类型名称
        /// </summary>
        /// <returns>处理器类型名称</returns>
        public virtual string GetHandlerTypeName()
        {
            return GetType().Name;
        }
        
        /// <summary>
        /// 获取支持的奖励类型
        /// </summary>
        /// <returns>支持的奖励类型</returns>
        public abstract RewardType GetSupportedRewardType();
        
        /// <summary>
        /// 获取奖励描述
        /// </summary>
        /// <param name="reward">奖励配置</param>
        /// <returns>奖励描述</returns>
        protected virtual string GetRewardDescription(RewardConfig reward)
        {
            return $"{reward.Type} x{reward.Amount}";
        }
        
        /// <summary>
        /// 检查前置条件
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        /// <param name="reward">奖励配置</param>
        /// <returns>是否满足前置条件</returns>
        protected virtual bool CheckPrerequisites(string achievementId, RewardConfig reward)
        {
            if (string.IsNullOrEmpty(achievementId))
            {
                GD.PrintErr($"[{GetHandlerTypeName()}] 成就ID为空");
                return false;
            }
            
            var validation = ValidateRewardConfig(reward);
            if (!validation.IsValid)
            {
                GD.PrintErr($"[{GetHandlerTypeName()}] 奖励配置无效: {validation.ErrorMessage}");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 执行奖励发放后的清理工作
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        /// <param name="reward">奖励配置</param>
        /// <param name="result">发放结果</param>
        protected virtual void PostGrantCleanup(string achievementId, RewardConfig reward, RewardResult result)
        {
            // 记录日志
            LogRewardGrant(achievementId, reward, result);
            
            // 可以在这里添加其他清理逻辑，如缓存清理、事件通知等
        }
    }
}