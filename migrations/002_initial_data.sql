-- CodeRogue Player Data Analytics - Initial Data
-- Migration: 002_initial_data.sql
-- Created: 2024-01-01

-- Insert initial card data
INSERT INTO cards (card_id, card_name, card_type, cost, rarity, card_properties) VALUES
('skill_001', '火球术', 'attack', 2, 'common', '{"damage": 100, "element": "fire", "description": "造成火焰伤害"}'),
('skill_002', '治疗术', 'heal', 1, 'common', '{"heal_amount": 50, "target": "self", "description": "恢复生命值"}'),
('skill_003', '闪电链', 'attack', 3, 'rare', '{"damage": 80, "targets": 3, "element": "lightning", "description": "连锁闪电攻击"}'),
('skill_004', '护盾术', 'defense', 2, 'common', '{"shield_amount": 75, "duration": 3, "description": "提供护盾保护"}'),
('skill_005', '冰冻术', 'control', 2, 'uncommon', '{"damage": 60, "freeze_duration": 2, "element": "ice", "description": "冰冻敌人"}'),
('skill_006', '毒刃', 'attack', 2, 'uncommon', '{"damage": 70, "poison_duration": 3, "element": "poison", "description": "造成持续毒伤害"}'),
('skill_007', '群体治疗', 'heal', 3, 'rare', '{"heal_amount": 40, "target": "all", "description": "治疗所有友军"}'),
('skill_008', '狂暴', 'buff', 2, 'uncommon', '{"damage_boost": 50, "duration": 3, "description": "提升攻击力"}'),
('skill_009', '反击', 'defense', 1, 'common', '{"reflect_damage": 30, "duration": 2, "description": "反弹伤害"}'),
('skill_010', '终极爆发', 'attack', 5, 'legendary', '{"damage": 300, "cooldown": 5, "description": "强力终极技能"}');

-- Insert initial level data
INSERT INTO levels (level_id, level_name, difficulty, expected_duration, level_config) VALUES
('level_001', '新手教程', 1, 300, '{"enemies": 3, "boss": false, "rewards": ["skill_001"], "description": "学习基础战斗"}'),
('level_002', '森林入口', 2, 450, '{"enemies": 5, "boss": false, "rewards": ["skill_002"], "description": "探索神秘森林"}'),
('level_003', '森林深处', 3, 600, '{"enemies": 7, "boss": true, "rewards": ["skill_003"], "description": "面对森林守护者"}'),
('level_004', '山洞探索', 4, 750, '{"enemies": 8, "boss": false, "rewards": ["skill_004"], "description": "深入黑暗洞穴"}'),
('level_005', '山洞BOSS', 5, 900, '{"enemies": 10, "boss": true, "rewards": ["skill_005"], "description": "挑战洞穴之王"}'),
('level_006', '冰雪平原', 6, 1050, '{"enemies": 12, "boss": false, "rewards": ["skill_006"], "description": "穿越冰雪世界"}'),
('level_007', '火山口', 7, 1200, '{"enemies": 15, "boss": true, "rewards": ["skill_007"], "description": "征服火山之神"}'),
('level_008', '天空之城', 8, 1350, '{"enemies": 18, "boss": false, "rewards": ["skill_008"], "description": "攀登云端城堡"}'),
('level_009', '深渊入口', 9, 1500, '{"enemies": 20, "boss": true, "rewards": ["skill_009"], "description": "进入无底深渊"}'),
('level_010', '终极试炼', 10, 1800, '{"enemies": 25, "boss": true, "rewards": ["skill_010"], "description": "最终的挑战"}');

-- Create functions for data analysis
CREATE OR REPLACE FUNCTION calculate_win_rate(p_player_id VARCHAR)
RETURNS DECIMAL(5,4) AS $$
DECLARE
    total_games INTEGER;
    won_games INTEGER;
BEGIN
    SELECT COUNT(*) INTO total_games
    FROM game_sessions gs
    JOIN level_attempts la ON gs.session_id = la.session_id
    WHERE gs.player_id = p_player_id;
    
    SELECT COUNT(*) INTO won_games
    FROM game_sessions gs
    JOIN level_attempts la ON gs.session_id = la.session_id
    WHERE gs.player_id = p_player_id AND la.completed = true;
    
    IF total_games = 0 THEN
        RETURN 0.0000;
    END IF;
    
    RETURN (won_games::DECIMAL / total_games::DECIMAL);
END;
$$ LANGUAGE plpgsql;

-- Create function to update player stats
CREATE OR REPLACE FUNCTION update_player_stats(p_player_id VARCHAR, p_date DATE DEFAULT CURRENT_DATE)
RETURNS VOID AS $$
DECLARE
    v_games_played INTEGER;
    v_games_won INTEGER;
    v_win_rate DECIMAL(5,4);
    v_total_playtime INTEGER;
BEGIN
    -- Calculate daily stats
    SELECT COUNT(DISTINCT gs.session_id) INTO v_games_played
    FROM game_sessions gs
    WHERE gs.player_id = p_player_id 
    AND DATE(gs.start_time) = p_date;
    
    SELECT COUNT(DISTINCT gs.session_id) INTO v_games_won
    FROM game_sessions gs
    JOIN level_attempts la ON gs.session_id = la.session_id
    WHERE gs.player_id = p_player_id 
    AND DATE(gs.start_time) = p_date
    AND la.completed = true;
    
    SELECT COALESCE(SUM(gs.duration_seconds), 0) INTO v_total_playtime
    FROM game_sessions gs
    WHERE gs.player_id = p_player_id 
    AND DATE(gs.start_time) = p_date;
    
    v_win_rate := CASE 
        WHEN v_games_played = 0 THEN 0.0000
        ELSE (v_games_won::DECIMAL / v_games_played::DECIMAL)
    END;
    
    -- Insert or update player stats
    INSERT INTO player_stats (stat_id, player_id, stat_date, games_played, games_won, win_rate, total_playtime)
    VALUES (p_player_id || '_' || p_date, p_player_id, p_date, v_games_played, v_games_won, v_win_rate, v_total_playtime)
    ON CONFLICT (player_id, stat_date) 
    DO UPDATE SET
        games_played = EXCLUDED.games_played,
        games_won = EXCLUDED.games_won,
        win_rate = EXCLUDED.win_rate,
        total_playtime = EXCLUDED.total_playtime,
        updated_at = NOW();
END;
$$ LANGUAGE plpgsql;

-- Create view for card statistics
CREATE OR REPLACE VIEW card_statistics AS
SELECT 
    c.card_id,
    c.card_name,
    c.card_type,
    c.rarity,
    COUNT(cu.usage_id) as usage_count,
    COUNT(CASE WHEN cu.resulted_in_win THEN 1 END) as win_count,
    CASE 
        WHEN COUNT(cu.usage_id) = 0 THEN 0.0000
        ELSE ROUND((COUNT(CASE WHEN cu.resulted_in_win THEN 1 END)::DECIMAL / COUNT(cu.usage_id)::DECIMAL), 4)
    END as win_rate,
    ROUND((COUNT(cu.usage_id)::DECIMAL / NULLIF((SELECT COUNT(*) FROM card_usage), 0)::DECIMAL), 4) as selection_rate
FROM cards c
LEFT JOIN card_usage cu ON c.card_id = cu.card_id
GROUP BY c.card_id, c.card_name, c.card_type, c.rarity
ORDER BY usage_count DESC;

-- Create view for level statistics
CREATE OR REPLACE VIEW level_statistics AS
SELECT 
    l.level_id,
    l.level_name,
    l.difficulty,
    COUNT(la.attempt_id) as total_attempts,
    COUNT(CASE WHEN la.completed THEN 1 END) as completed_attempts,
    CASE 
        WHEN COUNT(la.attempt_id) = 0 THEN 0.0000
        ELSE ROUND((COUNT(CASE WHEN la.completed THEN 1 END)::DECIMAL / COUNT(la.attempt_id)::DECIMAL), 4)
    END as completion_rate,
    ROUND(AVG(EXTRACT(EPOCH FROM (la.end_time - la.start_time))), 0) as avg_duration_seconds
FROM levels l
LEFT JOIN level_attempts la ON l.level_id = la.level_id
GROUP BY l.level_id, l.level_name, l.difficulty
ORDER BY l.difficulty;

-- Grant permissions to anon and authenticated roles
GRANT SELECT ON ALL TABLES IN SCHEMA public TO anon;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO authenticated;
GRANT USAGE ON ALL SEQUENCES IN SCHEMA public TO authenticated;
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO authenticated;
GRANT SELECT ON ALL VIEWS IN SCHEMA public TO anon;
GRANT SELECT ON ALL VIEWS IN SCHEMA public TO authenticated;