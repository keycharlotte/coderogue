// CodeRogue Player Data Analytics - Extended Repository Implementations
// This file contains the remaining repository implementations

import { Pool } from 'pg';
import {
  Card,
  CardUsage,
  Level,
  LevelAttempt,
  PlayerStats,
  CardType,
  CardRarity
} from '../../shared/types';
import {
  CardRepository,
  CardUsageRepository,
  LevelRepository,
  LevelAttemptRepository,
  PlayerStatsRepository
} from '../models';
import { getPool } from '../config/database';

// Base repository implementation (copied from index.ts)
abstract class BaseRepositoryImpl<T> {
  protected pool: Pool;
  protected tableName: string;

  constructor(tableName: string) {
    this.pool = getPool();
    this.tableName = tableName;
  }

  async findById(id: string): Promise<T | null> {
    const query = `SELECT * FROM ${this.tableName} WHERE ${this.getIdColumn()} = $1`;
    const result = await this.pool.query(query, [id]);
    return result.rows[0] || null;
  }

  async findAll(limit = 100, offset = 0): Promise<T[]> {
    const query = `SELECT * FROM ${this.tableName} ORDER BY created_at DESC LIMIT $1 OFFSET $2`;
    const result = await this.pool.query(query, [limit, offset]);
    return result.rows;
  }

  async delete(id: string): Promise<boolean> {
    const query = `DELETE FROM ${this.tableName} WHERE ${this.getIdColumn()} = $1`;
    const result = await this.pool.query(query, [id]);
    return result.rowCount > 0;
  }

  protected abstract getIdColumn(): string;
  abstract create(entity: Omit<T, 'created_at' | 'updated_at'>): Promise<T>;
  abstract update(id: string, updates: Partial<T>): Promise<T | null>;
}

// Card repository implementation
export class CardRepositoryImpl extends BaseRepositoryImpl<Card> implements CardRepository {
  constructor() {
    super('cards');
  }

  protected getIdColumn(): string {
    return 'card_id';
  }

  async create(entity: Omit<Card, 'created_at' | 'updated_at'>): Promise<Card> {
    const query = `
      INSERT INTO cards (card_id, card_name, card_type, cost, rarity, card_properties)
      VALUES ($1, $2, $3, $4, $5, $6)
      RETURNING *
    `;
    const values = [
      entity.card_id,
      entity.card_name,
      entity.card_type,
      entity.cost,
      entity.rarity,
      JSON.stringify(entity.card_properties)
    ];
    const result = await this.pool.query(query, values);
    return result.rows[0];
  }

  async update(id: string, updates: Partial<Card>): Promise<Card | null> {
    const setClause = Object.keys(updates)
      .map((key, index) => `${key} = $${index + 2}`)
      .join(', ');
    
    const query = `
      UPDATE cards 
      SET ${setClause}, updated_at = CURRENT_TIMESTAMP 
      WHERE card_id = $1 
      RETURNING *
    `;
    
    const values = [id, ...Object.values(updates)];
    const result = await this.pool.query(query, values);
    return result.rows[0] || null;
  }

  async findByType(cardType: CardType): Promise<Card[]> {
    const query = 'SELECT * FROM cards WHERE card_type = $1 ORDER BY card_name';
    const result = await this.pool.query(query, [cardType]);
    return result.rows;
  }

  async findByRarity(rarity: CardRarity): Promise<Card[]> {
    const query = 'SELECT * FROM cards WHERE rarity = $1 ORDER BY card_name';
    const result = await this.pool.query(query, [rarity]);
    return result.rows;
  }

  async getCardStatistics(startDate?: string, endDate?: string): Promise<Array<{
    cardId: string;
    cardName: string;
    cardType: CardType;
    rarity: CardRarity;
    selectionRate: number;
    winRate: number;
    usageCount: number;
  }>> {
    let dateFilter = '';
    const params: any[] = [];
    
    if (startDate && endDate) {
      dateFilter = 'AND cu.used_at >= $1 AND cu.used_at <= $2';
      params.push(startDate, endDate);
    }

    const query = `
      SELECT 
        c.card_id,
        c.card_name,
        c.card_type,
        c.rarity,
        COUNT(cu.usage_id) as usage_count,
        COALESCE(AVG(CASE WHEN cu.resulted_in_win THEN 1.0 ELSE 0.0 END), 0) as win_rate,
        COALESCE(COUNT(cu.usage_id) * 100.0 / NULLIF(total_usage.total, 0), 0) as selection_rate
      FROM cards c
      LEFT JOIN card_usage cu ON c.card_id = cu.card_id ${dateFilter}
      CROSS JOIN (
        SELECT COUNT(*) as total 
        FROM card_usage 
        WHERE 1=1 ${dateFilter}
      ) total_usage
      GROUP BY c.card_id, c.card_name, c.card_type, c.rarity, total_usage.total
      ORDER BY usage_count DESC
    `;
    
    const result = await this.pool.query(query, params);
    return result.rows.map(row => ({
      cardId: row.card_id,
      cardName: row.card_name,
      cardType: row.card_type,
      rarity: row.rarity,
      selectionRate: parseFloat(row.selection_rate),
      winRate: parseFloat(row.win_rate),
      usageCount: parseInt(row.usage_count)
    }));
  }
}

// Card usage repository implementation
export class CardUsageRepositoryImpl extends BaseRepositoryImpl<CardUsage> implements CardUsageRepository {
  constructor() {
    super('card_usage');
  }

  protected getIdColumn(): string {
    return 'usage_id';
  }

  async create(entity: Omit<CardUsage, 'created_at' | 'updated_at'>): Promise<CardUsage> {
    const query = `
      INSERT INTO card_usage (usage_id, card_id, session_id, used_at, context, resulted_in_win)
      VALUES ($1, $2, $3, $4, $5, $6)
      RETURNING *
    `;
    const values = [
      entity.usage_id,
      entity.card_id,
      entity.session_id,
      entity.used_at,
      entity.context,
      entity.resulted_in_win
    ];
    const result = await this.pool.query(query, values);
    return result.rows[0];
  }

  async update(id: string, updates: Partial<CardUsage>): Promise<CardUsage | null> {
    const setClause = Object.keys(updates)
      .map((key, index) => `${key} = $${index + 2}`)
      .join(', ');
    
    const query = `
      UPDATE card_usage 
      SET ${setClause}, updated_at = CURRENT_TIMESTAMP 
      WHERE usage_id = $1 
      RETURNING *
    `;
    
    const values = [id, ...Object.values(updates)];
    const result = await this.pool.query(query, values);
    return result.rows[0] || null;
  }

  async findByCardId(cardId: string, limit = 100): Promise<CardUsage[]> {
    const query = `
      SELECT * FROM card_usage 
      WHERE card_id = $1 
      ORDER BY used_at DESC 
      LIMIT $2
    `;
    const result = await this.pool.query(query, [cardId, limit]);
    return result.rows;
  }

  async findBySessionId(sessionId: string): Promise<CardUsage[]> {
    const query = `
      SELECT * FROM card_usage 
      WHERE session_id = $1 
      ORDER BY used_at ASC
    `;
    const result = await this.pool.query(query, [sessionId]);
    return result.rows;
  }

  async getWinRateByCard(cardId: string): Promise<number> {
    const query = `
      SELECT AVG(CASE WHEN resulted_in_win THEN 1.0 ELSE 0.0 END) as win_rate
      FROM card_usage
      WHERE card_id = $1
    `;
    const result = await this.pool.query(query, [cardId]);
    return parseFloat(result.rows[0].win_rate) || 0;
  }

  async getUsageCountByCard(cardId: string, startDate?: string, endDate?: string): Promise<number> {
    let query = 'SELECT COUNT(*) as count FROM card_usage WHERE card_id = $1';
    const params = [cardId];
    
    if (startDate && endDate) {
      query += ' AND used_at >= $2 AND used_at <= $3';
      params.push(startDate, endDate);
    }
    
    const result = await this.pool.query(query, params);
    return parseInt(result.rows[0].count);
  }

  async getTopCards(limit: number, startDate?: Date, endDate?: Date): Promise<Array<{
    cardId: string;
    cardName: string;
    usageCount: number;
    winRate: number;
  }>> {
    let dateFilter = '';
    const params: any[] = [limit];
    
    if (startDate && endDate) {
      dateFilter = 'AND cu.used_at >= $2 AND cu.used_at <= $3';
      params.push(startDate.toISOString(), endDate.toISOString());
    }

    const query = `
      SELECT 
        c.card_id,
        c.card_name,
        COUNT(cu.usage_id) as usage_count,
        COALESCE(AVG(CASE WHEN cu.resulted_in_win THEN 1.0 ELSE 0.0 END), 0) as win_rate
      FROM cards c
      LEFT JOIN card_usage cu ON c.card_id = cu.card_id
      WHERE 1=1 ${dateFilter}
      GROUP BY c.card_id, c.card_name
      ORDER BY usage_count DESC
      LIMIT $1
    `;
    
    const result = await this.pool.query(query, params);
    return result.rows.map(row => ({
      cardId: row.card_id,
      cardName: row.card_name,
      usageCount: parseInt(row.usage_count),
      winRate: parseFloat(row.win_rate)
    }));
  }
}

// Level repository implementation
export class LevelRepositoryImpl extends BaseRepositoryImpl<Level> implements LevelRepository {
  constructor() {
    super('levels');
  }

  protected getIdColumn(): string {
    return 'level_id';
  }

  async create(entity: Omit<Level, 'created_at' | 'updated_at'>): Promise<Level> {
    const query = `
      INSERT INTO levels (level_id, level_name, difficulty, expected_duration, level_config)
      VALUES ($1, $2, $3, $4, $5)
      RETURNING *
    `;
    const values = [
      entity.level_id,
      entity.level_name,
      entity.difficulty,
      entity.expected_duration,
      JSON.stringify(entity.level_config)
    ];
    const result = await this.pool.query(query, values);
    return result.rows[0];
  }

  async update(id: string, updates: Partial<Level>): Promise<Level | null> {
    const setClause = Object.keys(updates)
      .map((key, index) => `${key} = $${index + 2}`)
      .join(', ');
    
    const query = `
      UPDATE levels 
      SET ${setClause}, updated_at = CURRENT_TIMESTAMP 
      WHERE level_id = $1 
      RETURNING *
    `;
    
    const values = [id, ...Object.values(updates)];
    const result = await this.pool.query(query, values);
    return result.rows[0] || null;
  }

  async findByDifficulty(difficulty: number): Promise<Level[]> {
    const query = 'SELECT * FROM levels WHERE difficulty = $1 ORDER BY level_name';
    const result = await this.pool.query(query, [difficulty]);
    return result.rows;
  }

  async getLevelCompletionRates(): Promise<Array<{
    levelId: string;
    levelName: string;
    completionRate: number;
    averageAttempts: number;
  }>> {
    const query = `
      SELECT 
        l.level_id,
        l.level_name,
        COALESCE(AVG(CASE WHEN la.completed THEN 1.0 ELSE 0.0 END), 0) as completion_rate,
        COALESCE(AVG(la.attempts_count), 0) as average_attempts
      FROM levels l
      LEFT JOIN level_attempts la ON l.level_id = la.level_id
      GROUP BY l.level_id, l.level_name
      ORDER BY l.level_name
    `;
    
    const result = await this.pool.query(query);
    return result.rows.map(row => ({
      levelId: row.level_id,
      levelName: row.level_name,
      completionRate: parseFloat(row.completion_rate),
      averageAttempts: parseFloat(row.average_attempts)
    }));
  }
}

// Level attempt repository implementation
export class LevelAttemptRepositoryImpl extends BaseRepositoryImpl<LevelAttempt> implements LevelAttemptRepository {
  constructor() {
    super('level_attempts');
  }

  protected getIdColumn(): string {
    return 'attempt_id';
  }

  async create(entity: Omit<LevelAttempt, 'created_at' | 'updated_at'>): Promise<LevelAttempt> {
    const query = `
      INSERT INTO level_attempts (attempt_id, level_id, session_id, start_time, end_time, completed, attempts_count)
      VALUES ($1, $2, $3, $4, $5, $6, $7)
      RETURNING *
    `;
    const values = [
      entity.attempt_id,
      entity.level_id,
      entity.session_id,
      entity.start_time,
      entity.end_time,
      entity.completed,
      entity.attempts_count
    ];
    const result = await this.pool.query(query, values);
    return result.rows[0];
  }

  async update(id: string, updates: Partial<LevelAttempt>): Promise<LevelAttempt | null> {
    const setClause = Object.keys(updates)
      .map((key, index) => `${key} = $${index + 2}`)
      .join(', ');
    
    const query = `
      UPDATE level_attempts 
      SET ${setClause}, updated_at = CURRENT_TIMESTAMP 
      WHERE attempt_id = $1 
      RETURNING *
    `;
    
    const values = [id, ...Object.values(updates)];
    const result = await this.pool.query(query, values);
    return result.rows[0] || null;
  }

  async findByLevelId(levelId: string, limit = 100): Promise<LevelAttempt[]> {
    const query = `
      SELECT * FROM level_attempts 
      WHERE level_id = $1 
      ORDER BY start_time DESC 
      LIMIT $2
    `;
    const result = await this.pool.query(query, [levelId, limit]);
    return result.rows;
  }

  async findBySessionId(sessionId: string): Promise<LevelAttempt[]> {
    const query = `
      SELECT * FROM level_attempts 
      WHERE session_id = $1 
      ORDER BY start_time ASC
    `;
    const result = await this.pool.query(query, [sessionId]);
    return result.rows;
  }

  async getCompletionRate(levelId: string): Promise<number> {
    const query = `
      SELECT AVG(CASE WHEN completed THEN 1.0 ELSE 0.0 END) as completion_rate
      FROM level_attempts
      WHERE level_id = $1
    `;
    const result = await this.pool.query(query, [levelId]);
    return parseFloat(result.rows[0].completion_rate) || 0;
  }

  async getAverageAttempts(levelId: string, startDate?: Date, endDate?: Date): Promise<number> {
    let query = `
      SELECT AVG(attempts_count) as average_attempts
      FROM level_attempts
      WHERE level_id = $1
    `;
    const params = [levelId];
    
    if (startDate && endDate) {
      query += ' AND start_time >= $2 AND start_time <= $3';
      params.push(startDate.toISOString(), endDate.toISOString());
    }
    
    const result = await this.pool.query(query, params);
    return parseFloat(result.rows[0].average_attempts) || 0;
  }

  async getLevelCompletionRate(levelId: string, startDate?: Date, endDate?: Date): Promise<number> {
    let query = `
      SELECT AVG(CASE WHEN completed THEN 1.0 ELSE 0.0 END) as completion_rate
      FROM level_attempts
      WHERE level_id = $1
    `;
    const params = [levelId];
    
    
    if (startDate && endDate) {
      query += ' AND start_time >= $2 AND start_time <= $3';
      params.push(startDate.toISOString(), endDate.toISOString());
    }
    
    const result = await this.pool.query(query, params);
    return parseFloat(result.rows[0].completion_rate) || 0;
  }
}

// Player stats repository implementation
export class PlayerStatsRepositoryImpl extends BaseRepositoryImpl<PlayerStats> implements PlayerStatsRepository {
  constructor() {
    super('player_stats');
  }

  protected getIdColumn(): string {
    return 'stat_id';
  }

  async create(entity: Omit<PlayerStats, 'created_at' | 'updated_at'>): Promise<PlayerStats> {
    const query = `
      INSERT INTO player_stats (stat_id, player_id, stat_date, games_played, games_won, win_rate, total_playtime)
      VALUES ($1, $2, $3, $4, $5, $6, $7)
      RETURNING *
    `;
    const values = [
      entity.stat_id,
      entity.player_id,
      entity.stat_date,
      entity.games_played,
      entity.games_won,
      entity.win_rate,
      entity.total_playtime
    ];
    const result = await this.pool.query(query, values);
    return result.rows[0];
  }

  async update(id: string, updates: Partial<PlayerStats>): Promise<PlayerStats | null> {
    const setClause = Object.keys(updates)
      .map((key, index) => `${key} = $${index + 2}`)
      .join(', ');
    
    const query = `
      UPDATE player_stats 
      SET ${setClause}, updated_at = CURRENT_TIMESTAMP 
      WHERE stat_id = $1 
      RETURNING *
    `;
    
    const values = [id, ...Object.values(updates)];
    const result = await this.pool.query(query, values);
    return result.rows[0] || null;
  }

  async findByPlayerId(playerId: string, limit = 30): Promise<PlayerStats[]> {
    const query = `
      SELECT * FROM player_stats 
      WHERE player_id = $1 
      ORDER BY stat_date DESC 
      LIMIT $2
    `;
    const result = await this.pool.query(query, [playerId, limit]);
    return result.rows;
  }

  async getRetentionRates(): Promise<{
    day1: number;
    day7: number;
    day30: number;
  }> {
    const query = `
      WITH player_first_seen AS (
        SELECT player_id, MIN(first_seen) as first_date
        FROM players
        GROUP BY player_id
      ),
      retention_data AS (
        SELECT 
          pfs.player_id,
          pfs.first_date,
          MAX(CASE WHEN p.last_seen >= pfs.first_date + INTERVAL '1 day' THEN 1 ELSE 0 END) as day1_retained,
          MAX(CASE WHEN p.last_seen >= pfs.first_date + INTERVAL '7 days' THEN 1 ELSE 0 END) as day7_retained,
          MAX(CASE WHEN p.last_seen >= pfs.first_date + INTERVAL '30 days' THEN 1 ELSE 0 END) as day30_retained
        FROM player_first_seen pfs
        JOIN players p ON pfs.player_id = p.player_id
        WHERE pfs.first_date <= CURRENT_DATE - INTERVAL '30 days'
        GROUP BY pfs.player_id, pfs.first_date
      )
      SELECT 
        AVG(day1_retained) as day1_retention,
        AVG(day7_retained) as day7_retention,
        AVG(day30_retained) as day30_retention
      FROM retention_data
    `;
    
    const result = await this.pool.query(query);
    const row = result.rows[0];
    
    return {
      day1: parseFloat(row.day1_retention) || 0,
      day7: parseFloat(row.day7_retention) || 0,
      day30: parseFloat(row.day30_retention) || 0
    };
  }

  async updatePlayerStats(playerId: string, date: string): Promise<void> {
    const query = `
      INSERT INTO player_stats (stat_id, player_id, stat_date, games_played, games_won, win_rate, total_playtime)
      SELECT 
        CONCAT($1, '_', $2) as stat_id,
        $1 as player_id,
        $2 as stat_date,
        COUNT(DISTINCT gs.session_id) as games_played,
        COUNT(DISTINCT CASE WHEN ge.event_type = 'level_complete' THEN gs.session_id END) as games_won,
        COALESCE(
          COUNT(DISTINCT CASE WHEN ge.event_type = 'level_complete' THEN gs.session_id END) * 100.0 / 
          NULLIF(COUNT(DISTINCT gs.session_id), 0), 0
        ) as win_rate,
        COALESCE(SUM(gs.duration_seconds), 0) as total_playtime
      FROM game_sessions gs
      LEFT JOIN game_events ge ON gs.session_id = ge.session_id
      WHERE gs.player_id = $1 AND DATE(gs.start_time) = $2
      GROUP BY gs.player_id
      ON CONFLICT (stat_id) DO UPDATE SET
        games_played = EXCLUDED.games_played,
        games_won = EXCLUDED.games_won,
        win_rate = EXCLUDED.win_rate,
        total_playtime = EXCLUDED.total_playtime,
        updated_at = CURRENT_TIMESTAMP
    `;
    
    await this.pool.query(query, [playerId, date]);
  }

  async getRetentionRate(days: number, startDate?: Date, endDate?: Date): Promise<number> {
    let query = `
      WITH player_cohort AS (
        SELECT DISTINCT player_id, DATE(first_seen) as cohort_date
        FROM players
        WHERE 1=1
    `;
    const params: any[] = [days];
    
    if (startDate && endDate) {
      query += ' AND first_seen >= $2 AND first_seen <= $3';
      params.push(startDate.toISOString(), endDate.toISOString());
    }
    
    query += `
      ),
      retention_check AS (
        SELECT 
          pc.player_id,
          pc.cohort_date,
          CASE WHEN p.last_seen >= pc.cohort_date + INTERVAL '${days} days' THEN 1 ELSE 0 END as retained
        FROM player_cohort pc
        JOIN players p ON pc.player_id = p.player_id
        WHERE pc.cohort_date <= CURRENT_DATE - INTERVAL '${days} days'
      )
      SELECT AVG(retained) as retention_rate
      FROM retention_check
    `;
    
    const result = await this.pool.query(query, params);
    return parseFloat(result.rows[0].retention_rate) || 0;
  }
}

// Export repository factory functions
let cardRepositoryInstance: CardRepositoryImpl | null = null;
let cardUsageRepositoryInstance: CardUsageRepositoryImpl | null = null;
let levelRepositoryInstance: LevelRepositoryImpl | null = null;
let levelAttemptRepositoryInstance: LevelAttemptRepositoryImpl | null = null;
let playerStatsRepositoryInstance: PlayerStatsRepositoryImpl | null = null;

export function getCardRepository(): CardRepositoryImpl {
  if (!cardRepositoryInstance) {
    cardRepositoryInstance = new CardRepositoryImpl();
  }
  return cardRepositoryInstance;
}

export function getCardUsageRepository(): CardUsageRepositoryImpl {
  if (!cardUsageRepositoryInstance) {
    cardUsageRepositoryInstance = new CardUsageRepositoryImpl();
  }
  return cardUsageRepositoryInstance;
}

export function getLevelRepository(): LevelRepositoryImpl {
  if (!levelRepositoryInstance) {
    levelRepositoryInstance = new LevelRepositoryImpl();
  }
  return levelRepositoryInstance;
}

export function getLevelAttemptRepository(): LevelAttemptRepositoryImpl {
  if (!levelAttemptRepositoryInstance) {
    levelAttemptRepositoryInstance = new LevelAttemptRepositoryImpl();
  }
  return levelAttemptRepositoryInstance;
}

export function getPlayerStatsRepository(): PlayerStatsRepositoryImpl {
  if (!playerStatsRepositoryInstance) {
    playerStatsRepositoryInstance = new PlayerStatsRepositoryImpl();
  }
  return playerStatsRepositoryInstance;
}