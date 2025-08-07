// CodeRogue Player Data Analytics - Shared Type Definitions
// This file contains TypeScript interfaces and types shared between frontend and backend

// Base entity interface
export interface BaseEntity {
  created_at: string;
  updated_at?: string;
}

// Player related types
export interface Player extends BaseEntity {
  player_id: string;
  device_id?: string;
  first_seen: string;
  last_seen: string;
  player_segment: 'new' | 'active' | 'returning' | 'churned';
  metadata: Record<string, any>;
}

// Game session types
export interface GameSession extends BaseEntity {
  session_id: string;
  player_id: string;
  start_time: string;
  end_time?: string;
  duration_seconds?: number;
  game_version?: string;
  session_data: Record<string, any>;
}

// Game event types
export interface GameEvent extends BaseEntity {
  event_id: string;
  session_id: string;
  event_type: EventType;
  timestamp: string;
  event_data: Record<string, any>;
  event_category?: EventCategory;
}

export type EventType = 
  | 'game_start'
  | 'game_end'
  | 'level_start'
  | 'level_complete'
  | 'level_failed'
  | 'card_selected'
  | 'card_used'
  | 'battle_start'
  | 'battle_end'
  | 'player_death'
  | 'item_acquired'
  | 'achievement_unlocked';

export type EventCategory = 
  | 'gameplay'
  | 'progression'
  | 'monetization'
  | 'social'
  | 'technical';

// Card related types
export interface Card extends BaseEntity {
  card_id: string;
  card_name: string;
  card_type: CardType;
  cost: number;
  rarity: CardRarity;
  card_properties: Record<string, any>;
}

export type CardType = 
  | 'attack'
  | 'defense'
  | 'heal'
  | 'control'
  | 'buff'
  | 'debuff'
  | 'utility';

export type CardRarity = 
  | 'common'
  | 'uncommon'
  | 'rare'
  | 'epic'
  | 'legendary';

// Card usage tracking
export interface CardUsage extends BaseEntity {
  usage_id: string;
  card_id: string;
  session_id: string;
  used_at: string;
  context?: string;
  resulted_in_win: boolean;
}

// Level related types
export interface Level extends BaseEntity {
  level_id: string;
  level_name: string;
  difficulty: number;
  expected_duration: number;
  level_config: Record<string, any>;
}

// Level attempt tracking
export interface LevelAttempt extends BaseEntity {
  attempt_id: string;
  level_id: string;
  session_id: string;
  start_time: string;
  end_time?: string;
  completed: boolean;
  attempts_count: number;
}

// Player statistics
export interface PlayerStats extends BaseEntity {
  stat_id: string;
  player_id: string;
  stat_date: string;
  games_played: number;
  games_won: number;
  win_rate: number;
  total_playtime: number;
}

// API Request/Response types
export interface EventReportRequest {
  playerId: string;
  eventType: EventType;
  eventData: Record<string, any>;
  timestamp: number;
  sessionId: string;
}

export interface EventReportResponse {
  success: boolean;
  message: string;
  eventId?: string;
}

// Analytics query parameters
export interface AnalyticsQuery {
  startDate?: string;
  endDate?: string;
  limit?: number;
  offset?: number;
}

export interface CardAnalyticsQuery extends AnalyticsQuery {
  cardType?: CardType;
  rarity?: CardRarity;
}

export interface PlayerBehaviorQuery extends AnalyticsQuery {
  metric: 'retention' | 'session_length' | 'progression' | 'engagement';
  timeframe?: '7d' | '30d' | '90d';
  segment?: string;
}

export interface BalanceAnalyticsQuery extends AnalyticsQuery {
  analysisType: 'winrate' | 'difficulty' | 'completion';
  levelRange?: string;
  playerSegment?: string;
}

// Analytics response types
export interface CardStatistics {
  cardId: string;
  cardName: string;
  cardType: CardType;
  rarity: CardRarity;
  selectionRate: number;
  winRate: number;
  usageCount: number;
}

export interface CardAnalyticsResponse {
  cards: CardStatistics[];
  totalCount: number;
  dateRange: {
    start: string;
    end: string;
  };
}

export interface PlayerBehaviorData {
  metric: string;
  value: number;
  date: string;
  segment?: string;
}

export interface PlayerBehaviorResponse {
  metric: string;
  data: PlayerBehaviorData[];
  summary: {
    average: number;
    trend: 'up' | 'down' | 'stable';
    changePercent: number;
  };
}

export interface BalanceRecommendation {
  type: 'buff' | 'nerf' | 'rework';
  target: string;
  reason: string;
  priority: 'low' | 'medium' | 'high';
}

export interface BalanceAnalyticsResponse {
  analysisType: string;
  results: Array<{
    id: string;
    name: string;
    value: number;
    status: 'balanced' | 'overpowered' | 'underpowered';
  }>;
  recommendations: BalanceRecommendation[];
}

// Dashboard data types
export interface DashboardMetrics {
  dailyActiveUsers: number;
  totalSessions: number;
  averageSessionLength: number;
  retentionRate: {
    day1: number;
    day7: number;
    day30: number;
  };
  topCards: CardStatistics[];
  levelCompletionRates: Array<{
    levelId: string;
    levelName: string;
    completionRate: number;
  }>;
}

// Error response type
export interface ErrorResponse {
  error: string;
  message: string;
  statusCode: number;
  timestamp: string;
}