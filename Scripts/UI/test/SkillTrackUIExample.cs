using Godot;
using CodeRogue.Skills;
using CodeRogue.UI;
using CodeRogue.Utils;

namespace CodeRogue.Examples
{
    /// <summary>
    /// SkillTrackUI状态监听使用示例
    /// 演示如何在其他系统中集成SkillTrackUI的状态监听功能
    /// </summary>
    public partial class SkillTrackUIExample : Node
    {
        private SkillTrackUI _skillTrackUI;
        private SkillTrackManager _trackManager;
        private DeckManager _deckManager;
        
        public override void _Ready()
        {
            SetupReferences();
            ConnectToManagers();
        }
        
        private void SetupReferences()
        {
            // 获取UI组件引用
            _skillTrackUI = GetNode<SkillTrackUI>("../SkillTrackUI");
            
            // 获取管理器引用
            _trackManager = NodeUtils.GetSkillTrackManager(this);
            _deckManager = NodeUtils.GetDeckManager(this);
            
            if (_skillTrackUI == null)
            {
                GD.PrintErr("SkillTrackUIExample: 无法找到SkillTrackUI组件");
                return;
            }
        }
        
        private void ConnectToManagers()
        {
            // 连接到SkillTrackManager的信号
            if (_trackManager != null)
            {
                _trackManager.TrackCharged += OnTrackCharged;
                _trackManager.SkillActivated += OnSkillActivated;
                _trackManager.ChargeUpdated += OnChargeUpdated;
                
                GD.Print("SkillTrackUIExample: 已连接到SkillTrackManager信号");
            }
            
            // 如果DeckManager有卡组变化信号，可以在这里连接
        // 注意：这里假设DeckManager有DeckChanged信号
            // if (_deckManager != null && _deckManager.HasSignal("DeckChanged"))
            // {
            //     _deckManager.Connect("DeckChanged", new Callable(this, nameof(OnDeckChanged)));
            // }
        }
        
        private void OnTrackCharged(int trackIndex, SkillCard skill)
        {
            GD.Print($"Example: 监听到轨道 {trackIndex} 充能完成: {skill.Name}");
            
            // 可以在这里添加额外的逻辑，比如:
            // - 播放音效
            // - 更新其他UI组件
            // - 触发特殊效果
        }
        
        private void OnSkillActivated(SkillCard skill, int trackIndex)
        {
            GD.Print($"Example: 监听到技能激活: {skill.Name} (轨道 {trackIndex})");
            
            // 可以在这里添加额外的逻辑，比如:
            // - 更新战斗统计
            // - 触发连击系统
            // - 更新成就进度
        }
        
        private void OnChargeUpdated(int trackIndex, float currentCharge, float maxCharge)
        {
            float progress = maxCharge > 0 ? (currentCharge / maxCharge) * 100 : 0;
            
            // 只在特定进度点输出日志，避免过多输出
            if (progress >= 100 || (progress % 25 == 0 && progress > 0))
            {
                GD.Print($"Example: 轨道 {trackIndex} 充能进度: {progress:F1}%");
            }
        }
        
        /// <summary>
        /// 模拟卡组切换事件
        /// </summary>
        public void SimulateDeckChange(UnifiedDeck newDeck)
        {
            if (_skillTrackUI != null)
            {
                _skillTrackUI.OnDeckChanged(newDeck);
                GD.Print($"Example: 模拟卡组切换到: {newDeck?.DeckName ?? "无"}");
            }
        }
        
        /// <summary>
        /// 模拟系统错误事件
        /// </summary>
        public void SimulateSystemError(string errorMessage)
        {
            if (_skillTrackUI != null)
            {
                _skillTrackUI.OnSkillSystemError(errorMessage);
                GD.Print($"Example: 模拟系统错误: {errorMessage}");
            }
        }
        
        /// <summary>
        /// 模拟系统重置事件
        /// </summary>
        public void SimulateSystemReset()
        {
            if (_skillTrackUI != null)
            {
                _skillTrackUI.OnSkillSystemReset();
                GD.Print("Example: 模拟系统重置");
            }
        }
        
        /// <summary>
        /// 手动刷新UI - 用于测试
        /// </summary>
        public void RefreshUI()
        {
            if (_skillTrackUI != null)
            {
                _skillTrackUI.RefreshUI();
                GD.Print("Example: 手动刷新UI");
            }
        }
        
        // 测试用的输入处理
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                switch (keyEvent.Keycode)
                {
                    case Key.F1:
                        RefreshUI();
                        break;
                    case Key.F2:
                        SimulateSystemError("测试错误消息");
                        break;
                    case Key.F3:
                        SimulateSystemReset();
                        break;
                    case Key.F4:
                        // 模拟卡组切换（需要实际的卡组实例）
                        if (_deckManager != null)
                        {
                            var deck = _deckManager.GetDeck("Basic");
                            SimulateDeckChange(deck);
                        }
                        break;
                }
            }
        }
        
        public override void _ExitTree()
        {
            // 清理信号连接
            if (_trackManager != null)
            {
                _trackManager.TrackCharged -= OnTrackCharged;
                _trackManager.SkillActivated -= OnSkillActivated;
                _trackManager.ChargeUpdated -= OnChargeUpdated;
            }
        }
    }
}