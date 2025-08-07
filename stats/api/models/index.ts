// CodeRogue Player Data Analytics - Database Models
// This file contains database entity classes and data access interfaces

import { Pool } from 'pg';
import {
  Player,
  GameSession,
  GameEvent,
  Card,
  CardUsage,
  Level,
  LevelAttempt,
  PlayerStats,
  EventType,
  CardType,
  CardRarity
} from '../../shared/types';

// Database connection interface
export interface DatabaseConfig {
  host: string;
  port: number;
  database: string;
  user: string;
  password: string;
  ssl?: boolean;
}

// Base repository interface
export interface BaseRepository<T> {
  findById(id: string): Promise<T | null>;
  findAll(limit?: number, offset?: number): Promise<T[]>;
  create(entity: Omit<T, 'created_at' | 'updated_at'>): Promise<T>;
  update(id: string, updates: Partial<T>): Promise<T | null>;
  delete(id: string): Promise<boolean>;
}

// Player repository interface
export interface PlayerRepository extends BaseRepository<Player> {
  findByDeviceId(deviceId: string): Promise<Player | null>;
  updateLastSeen(playerId: string): Promise<void>;
  getPlayerSegmentCounts(): Promise<Record<string, number>>;
  getActivePlayersCount(days: number): Promise<number>;
}

// Game session repository interface
export interface GameSessionRepository extends BaseRepository<GameSession> {
  findByPlayerId(playerId: string, limit?: number): Promise<GameSession[]>;
  getAverageSessionLength(): Promise<number>;
  getSessionCountByDate(startDate: string, endDate: string): Promise<Array<{ date: string; count: number }>>;
  endSession(sessionId: string): Promise<void>;
}

// Game event repository interface
export interface GameEventRepository extends BaseRepository<GameEvent> {
  findBySessionId(sessionId: string): Promise<GameEvent[]>;
  findByEventType(eventType: EventType, limit?: number): Promise<GameEvent[]>;
  getEventCountsByType(startDate: string, endDate: string): Promise<Record<EventType, number>>;
  createBatch(events: Omit<GameEvent, 'event_id' | 'created_at'>[]): Promise<GameEvent[]>;
}

// Card repository interface
export interface CardRepository extends BaseRepository<Card> {
  findByType(cardType: CardType): Promise<Card[]>;
  findByRarity(rarity: CardRarity): Promise<Card[]>;
  getCardStatistics(startDate?: string, endDate?: string): Promise<Array<{
    cardId: string;
    cardName: string;
    cardType: CardType;
    rarity: CardRarity;
    selectionRate: number;
    winRate: number;
    usageCount: number;
  }>>;
}

// Card usage repository interface
export interface CardUsageRepository extends BaseRepository<CardUsage> {
  findByCardId(cardId: string, limit?: number): Promise<CardUsage[]>;
  findBySessionId(sessionId: string): Promise<CardUsage[]>;
  getWinRateByCard(cardId: string): Promise<number>;
  getUsageCountByCard(cardId: string, startDate?: string, endDate?: string): Promise<number>;
}

// Level repository interface
export interface LevelRepository extends BaseRepository<Level> {
  findByDifficulty(difficulty: number): Promise<Level[]>;
  getLevelCompletionRates(): Promise<Array<{
    levelId: string;
    levelName: string;
    completionRate: number;
    averageAttempts: number;
  }>>;
}

// Level attempt repository interface
export interface LevelAttemptRepository extends BaseRepository<LevelAttempt> {
  findByLevelId(levelId: string, limit?: number): Promise<LevelAttempt[]>;
  findBySessionId(sessionId: string): Promise<LevelAttempt[]>;
  getCompletionRate(levelId: string): Promise<number>;
  getAverageAttempts(levelId: string): Promise<number>;
}

// Player stats repository interface
export interface PlayerStatsRepository extends BaseRepository<PlayerStats> {
  findByPlayerId(playerId: string, limit?: number): Promise<PlayerStats[]>;
  getRetentionRates(): Promise<{
    day1: number;
    day7: number;
    day30: number;
  }>;
  updatePlayerStats(playerId: string, date: string): Promise<void>;
}

// Analytics service interface
export interface AnalyticsService {
  getCardAnalytics(query: {
    startDate?: string;
    endDate?: string;
    cardType?: CardType;
    rarity?: CardRarity;
    limit?: number;
    offset?: number;
  }): Promise<{
    cards: Array<{
      cardId: string;
      cardName: string;
      cardType: CardType;
      rarity: CardRarity;
      selectionRate: number;
      winRate: number;
      usageCount: number;
    }>;
    totalCount: number;
  }>;

  getPlayerBehaviorAnalytics(query: {
    metric: 'retention' | 'session_length' | 'progression' | 'engagement';
    startDate?: string;
    endDate?: string;
    timeframe?: '7d' | '30d' | '90d';
    segment?: string;
  }): Promise<{
    metric: string;
    data: Array<{
      metric: string;
      value: number;
      date: string;
      segment?: string;
    }>;
    summary: {
      average: number;
      trend: 'up' | 'down' | 'stable';
      changePercent: number;
    };
  }>;

  getBalanceAnalytics(query: {
    analysisType: 'winrate' | 'difficulty' | 'completion';
    startDate?: string;
    endDate?: string;
    levelRange?: string;
    playerSegment?: string;
  }): Promise<{
    analysisType: string;
    results: Array<{
      id: string;
      name: string;
      value: number;
      status: 'balanced' | 'overpowered' | 'underpowered';
    }>;
    recommendations: Array<{
      type: 'buff' | 'nerf' | 'rework';
      target: string;
      reason: string;
      priority: 'low' | 'medium' | 'high';
    }>;
  }>;

  getDashboardMetrics(): Promise<{
    dailyActiveUsers: number;
    totalSessions: number;
    averageSessionLength: number;
    retentionRate: {
      day1: number;
      day7: number;
      day30: number;
    };
    topCards: Array<{
      cardId: string;
      cardName: string;
      cardType: CardType;
      rarity: CardRarity;
      selectionRate: number;
      winRate: number;
      usageCount: number;
    }>;
    levelCompletionRates: Array<{
      levelId: string;
      levelName: string;
      completionRate: number;
    }>;
  }>;
}

// Database connection manager
export class DatabaseManager {
  private pool: Pool;

  constructor(config: DatabaseConfig) {
    this.pool = new Pool({
      host: config.host,
      port: config.port,
      database: config.database,
      user: config.user,
      password: config.password,
      ssl: config.ssl,
      max: 20,
      idleTimeoutMillis: 30000,
      connectionTimeoutMillis: 2000,
    });
  }

  getPool(): Pool {
    return this.pool;
  }

  async query(text: string, params?: any[]): Promise<any> {
    const client = await this.pool.connect();
    try {
      const result = await client.query(text, params);
      return result;
    } finally {
      client.release();
    }
  }

  async close(): Promise<void> {
    await this.pool.end();
  }
}

// Validation utilities
export class ValidationUtils {
  static isValidEventType(eventType: string): eventType is EventType {
    const validTypes: EventType[] = [
      'game_start', 'game_end', 'level_start', 'level_complete', 'level_failed',
      'card_selected', 'card_used', 'battle_start', 'battle_end', 'player_death',
      'item_acquired', 'achievement_unlocked'
    ];
    return validTypes.includes(eventType as EventType);
  }

  static isValidCardType(cardType: string): cardType is CardType {
    const validTypes: CardType[] = [
      'attack', 'defense', 'heal', 'control', 'buff', 'debuff', 'utility'
    ];
    return validTypes.includes(cardType as CardType);
  }

  static isValidCardRarity(rarity: string): rarity is CardRarity {
    const validRarities: CardRarity[] = [
      'common', 'uncommon', 'rare', 'epic', 'legendary'
    ];
    return validRarities.includes(rarity as CardRarity);
  }

  static validateEventData(eventData: any): boolean {
    return typeof eventData === 'object' && eventData !== null;
  }

  static validatePlayerId(playerId: string): boolean {
    return typeof playerId === 'string' && playerId.length > 0 && playerId.length <= 255;
  }

  static validateSessionId(sessionId: string): boolean {
    return typeof sessionId === 'string' && sessionId.length > 0 && sessionId.length <= 255;
  }
}

// Export all types and interfaces
export * from '../../shared/types';