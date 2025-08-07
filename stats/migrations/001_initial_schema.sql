-- CodeRogue Player Data Analytics - Initial Database Schema
-- Migration: 001_initial_schema.sql
-- Created: 2024-01-01

-- Create players table
CREATE TABLE players (
    player_id VARCHAR(255) PRIMARY KEY,
    device_id VARCHAR(255),
    first_seen TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    last_seen TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    player_segment VARCHAR(50) DEFAULT 'new',
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes for players table
CREATE INDEX idx_players_device_id ON players(device_id);
CREATE INDEX idx_players_last_seen ON players(last_seen DESC);
CREATE INDEX idx_players_segment ON players(player_segment);

-- Create game_sessions table
CREATE TABLE game_sessions (
    session_id VARCHAR(255) PRIMARY KEY,
    player_id VARCHAR(255) REFERENCES players(player_id),
    start_time TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    end_time TIMESTAMP WITH TIME ZONE,
    duration_seconds INTEGER,
    game_version VARCHAR(50),
    session_data JSONB DEFAULT '{}',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes for game_sessions table
CREATE INDEX idx_sessions_player_id ON game_sessions(player_id);
CREATE INDEX idx_sessions_start_time ON game_sessions(start_time DESC);
CREATE INDEX idx_sessions_duration ON game_sessions(duration_seconds);

-- Create game_events table
CREATE TABLE game_events (
    event_id VARCHAR(255) PRIMARY KEY,
    session_id VARCHAR(255) REFERENCES game_sessions(session_id),
    event_type VARCHAR(100) NOT NULL,
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    event_data JSONB DEFAULT '{}',
    event_category VARCHAR(50),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes for game_events table
CREATE INDEX idx_events_session_id ON game_events(session_id);
CREATE INDEX idx_events_type ON game_events(event_type);
CREATE INDEX idx_events_timestamp ON game_events(timestamp DESC);
CREATE INDEX idx_events_category ON game_events(event_category);

-- Create cards table
CREATE TABLE cards (
    card_id VARCHAR(255) PRIMARY KEY,
    card_name VARCHAR(255) NOT NULL,
    card_type VARCHAR(100) NOT NULL,
    cost INTEGER DEFAULT 0,
    rarity VARCHAR(50) DEFAULT 'common',
    card_properties JSONB DEFAULT '{}',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes for cards table
CREATE INDEX idx_cards_type ON cards(card_type);
CREATE INDEX idx_cards_rarity ON cards(rarity);
CREATE INDEX idx_cards_cost ON cards(cost);

-- Create card_usage table
CREATE TABLE card_usage (
    usage_id VARCHAR(255) PRIMARY KEY,
    card_id VARCHAR(255) REFERENCES cards(card_id),
    session_id VARCHAR(255) REFERENCES game_sessions(session_id),
    used_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    context VARCHAR(100),
    resulted_in_win BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes for card_usage table
CREATE INDEX idx_card_usage_card_id ON card_usage(card_id);
CREATE INDEX idx_card_usage_session_id ON card_usage(session_id);
CREATE INDEX idx_card_usage_used_at ON card_usage(used_at DESC);
CREATE INDEX idx_card_usage_win ON card_usage(resulted_in_win);

-- Create levels table
CREATE TABLE levels (
    level_id VARCHAR(255) PRIMARY KEY,
    level_name VARCHAR(255) NOT NULL,
    difficulty INTEGER DEFAULT 1,
    expected_duration INTEGER DEFAULT 300,
    level_config JSONB DEFAULT '{}',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes for levels table
CREATE INDEX idx_levels_difficulty ON levels(difficulty);
CREATE INDEX idx_levels_duration ON levels(expected_duration);

-- Create level_attempts table
CREATE TABLE level_attempts (
    attempt_id VARCHAR(255) PRIMARY KEY,
    level_id VARCHAR(255) REFERENCES levels(level_id),
    session_id VARCHAR(255) REFERENCES game_sessions(session_id),
    start_time TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    end_time TIMESTAMP WITH TIME ZONE,
    completed BOOLEAN DEFAULT FALSE,
    attempts_count INTEGER DEFAULT 1,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes for level_attempts table
CREATE INDEX idx_level_attempts_level_id ON level_attempts(level_id);
CREATE INDEX idx_level_attempts_session_id ON level_attempts(session_id);
CREATE INDEX idx_level_attempts_completed ON level_attempts(completed);
CREATE INDEX idx_level_attempts_start_time ON level_attempts(start_time DESC);

-- Create player_stats table
CREATE TABLE player_stats (
    stat_id VARCHAR(255) PRIMARY KEY,
    player_id VARCHAR(255) REFERENCES players(player_id),
    stat_date DATE DEFAULT CURRENT_DATE,
    games_played INTEGER DEFAULT 0,
    games_won INTEGER DEFAULT 0,
    win_rate DECIMAL(5,4) DEFAULT 0.0000,
    total_playtime INTEGER DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes for player_stats table
CREATE INDEX idx_player_stats_player_id ON player_stats(player_id);
CREATE INDEX idx_player_stats_date ON player_stats(stat_date DESC);
CREATE INDEX idx_player_stats_win_rate ON player_stats(win_rate DESC);

-- Create unique constraint for player_stats (one record per player per day)
CREATE UNIQUE INDEX idx_player_stats_unique ON player_stats(player_id, stat_date);