using Godot;
using CodeRogue.Skills;
using CodeRogue.UI;

namespace CodeRogue.Test
{
    /// <summary>
    /// SkillTrack系统测试脚本
    /// </summary>
    public partial class SkillTrackTest : Node
    {

        [Export] private SkillTrackUI _skillTrackUI;
        [Export] private SkillDeckUI _skillDeckUI;
        
        [Export] private Button _loadTestSkillsButton;
        [Export] private Button _startChargingButton;
        [Export] private Button _activateReadySkillsButton;
        [Export] private Button _clearAllTracksButton;
        
        private SkillDatabase _skillDatabase;
        private SkillDeckManager _skillDeckManager;
        private SkillTrackManager _skillTrackManager;
        
        public override void _Ready()
        {
            InitializeComponents();
            ConnectSignals();
            SetupTestEnvironment();
        }
        
        private void InitializeComponents()
        {            
            // 获取系统组件
            _skillDatabase = GetNode<SkillDatabase>("/root/SkillDatabase");
            if (_skillDatabase == null)
            {
                GD.PrintErr("SkillDatabase autoload not found!");
                return;
            }
            _skillDeckManager = GetNode<SkillDeckManager>("/root/SkillDeckManager");
            _skillTrackManager = GetNode<SkillTrackManager>("/root/SkillTrackManager");
        }
        
        private void ConnectSignals()
        {
            // 连接按钮信号
            _loadTestSkillsButton?.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnLoadTestSkillsPressed)));
            _startChargingButton?.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnStartChargingPressed)));
            _activateReadySkillsButton?.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnActivateReadySkillsPressed)));
            _clearAllTracksButton?.Connect(Button.SignalName.Pressed, new Callable(this, nameof(OnClearAllTracksPressed)));
            
            // 连接SkillTrackManager信号
            if (_skillTrackManager != null)
            {
                _skillTrackManager.Connect(SkillTrackManager.SignalName.TrackCharged, new Callable(this, nameof(OnTrackCharged)));
                _skillTrackManager.Connect(SkillTrackManager.SignalName.SkillActivated, new Callable(this, nameof(OnSkillActivated)));
                _skillTrackManager.Connect(SkillTrackManager.SignalName.ChargeUpdated, new Callable(this, nameof(OnChargeUpdated)));
            }
        }
        
        private void SetupTestEnvironment()
        {
            GD.Print("SkillTrack Test Environment Initialized");
            
            // // 初始化UI连接
            // if (_skillTrackUI != null && _skillTrackManager != null)
            // {
            //     _skillTrackUI.SetSkillTrackManager(_skillTrackManager);
            // }
        }
        
        #region Button Event Handlers
        
        private void OnLoadTestSkillsPressed()
        {
            GD.Print("Loading test skills...");
            
            // 加载一些测试技能到轨道
            if (_skillDatabase != null && _skillTrackManager != null)
            {
                var skills = _skillDatabase.GetAllSkills();
                
                for (int i = 0; i < Mathf.Min(skills.Count, _skillTrackManager.MaxTracks); i++)
                {
                    _skillTrackManager.EquipSkillToTrack(skills[i], i);
                    GD.Print($"Equipped {skills[i].Name} to track {i}");
                }
            }
        }
        
        private void OnStartChargingPressed()
        {
            GD.Print("Starting charge process...");
            
            if (_skillTrackManager != null)
            {
                _skillTrackManager.StartCharging();
            }
        }
        
        private void OnActivateReadySkillsPressed()
        {
            GD.Print("Activating ready skills...");
            
            if (_skillTrackManager != null)
            {
                var tracks = _skillTrackManager.GetTracks();
                
                for (int i = 0; i < tracks.Count; i++)
                {
                    if (tracks[i].State == TrackState.Ready)
                    {
                        _skillTrackManager.ActivateSkill(i);
                        GD.Print($"Activated skill in track {i}");
                    }
                }
            }
        }
        
        private void OnClearAllTracksPressed()
        {
            GD.Print("Clearing all tracks...");
            
            if (_skillTrackManager != null)
            {
                for (int i = 0; i < _skillTrackManager.MaxTracks; i++)
                {
                    _skillTrackManager.ClearTrack(i);
                }
            }
        }
        
        #endregion
        
        #region SkillTrackManager Event Handlers
        
        private void OnTrackCharged(int trackIndex, SkillCard skill)
        {
            GD.Print($"Track {trackIndex} charged: {skill.Name}");
        }
        
        private void OnSkillActivated(SkillCard skill, int trackIndex)
        {
            GD.Print($"Skill activated: {skill.Name} from track {trackIndex}");
        }
        
        private void OnChargeUpdated(int trackIndex, float currentCharge, float maxCharge)
        {
            var percentage = (currentCharge / maxCharge) * 100f;
            GD.Print($"Track {trackIndex} charge: {percentage:F1}%");
        }
        
        #endregion
        
        public override void _Input(InputEvent @event)
        {
            // 添加一些快捷键用于测试
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                switch (keyEvent.Keycode)
                {
                    case Key.Key1:
                        OnLoadTestSkillsPressed();
                        break;
                    case Key.Key2:
                        OnStartChargingPressed();
                        break;
                    case Key.Key3:
                        OnActivateReadySkillsPressed();
                        break;
                    case Key.Key4:
                        OnClearAllTracksPressed();
                        break;
                }
            }
        }
    }
}