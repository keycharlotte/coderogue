// CodeRogue Player Data Analytics - Analytics API Routes
// This file handles analytics queries for cards, players, and game balance

import { Router, Request, Response } from 'express';
import {
  CardAnalyticsResponse,
  PlayerBehaviorResponse,
  BalanceRecommendation
} from '../../shared/types';
import { getPool } from '../config/database';
import { AnalyticsService } from '../services/analytics';
// import { ValidationUtils } from '../utils/validation'; // TODO: Implement validation utils
import { asyncHandler } from '../middleware/errorHandler';
import { validate, sanitizeRequest } from '../middleware/validation.js';

const router = Router();

// Initialize analytics service lazily
let analyticsService: AnalyticsService | null = null;

function getAnalyticsService(): AnalyticsService {
  if (!analyticsService) {
    analyticsService = new AnalyticsService(getPool());
  }
  return analyticsService;
}

// GET /api/v1/analytics/cards - Get card usage analytics
router.get('/cards',
  sanitizeRequest,
  // validateAnalytics.query, // TODO: Implement analytics validation
  asyncHandler(async (req: Request, res: Response) => {
  const query: any = {
    startDate: req.query.startDate as string,
    endDate: req.query.endDate as string,
    playerId: req.query.playerId as string,
    cardId: req.query.cardId as string,
    level: req.query.level ? parseInt(req.query.level as string) : undefined
  };

  const response = await getAnalyticsService().getCardAnalytics(query);
  res.json(response);
}));

// GET /api/v1/analytics/cards/:cardId - Get specific card analytics
router.get('/cards/:cardId',
  // validateUuidParam('cardId'), // TODO: Implement UUID validation
  sanitizeRequest,
  // validateAnalytics.query, // TODO: Implement analytics validation
  asyncHandler(async (req: Request, res: Response) => {
  const cardId = req.params.cardId;
  const query: any = {
    startDate: req.query.startDate as string,
    endDate: req.query.endDate as string,
    cardId: cardId
  };

  const response = await getAnalyticsService().getCardAnalytics(query);
  res.json(response);
}));

// GET /api/v1/analytics/players - Get player behavior analytics
router.get('/players',
  sanitizeRequest,
  // validateAnalytics.query, // TODO: Implement analytics validation
  asyncHandler(async (req: Request, res: Response) => {
  const query: any = {
    startDate: req.query.startDate as string,
    endDate: req.query.endDate as string,
    playerId: req.query.playerId as string
  };

  const response = await getAnalyticsService().getPlayerBehaviorAnalytics(query);
  res.json(response);
}));

// GET /api/v1/analytics/players/:playerId - Specific player analytics
router.get('/players/:playerId',
  // validateUuidParam('playerId'), // TODO: Implement UUID validation
  sanitizeRequest,
  // validateAnalytics.query, // TODO: Implement analytics validation
  asyncHandler(async (req: Request, res: Response) => {
    const playerId = req.params.playerId;
    const query: any = {
      startDate: req.query.startDate as string,
      endDate: req.query.endDate as string,
      playerId: playerId
    };

    const response = await getAnalyticsService().getPlayerBehaviorAnalytics(query);
    res.json(response);
}));

// GET /api/v1/analytics/balance - Get game balance analytics
router.get('/balance',
  sanitizeRequest,
  // validateAnalytics.query, // TODO: Implement analytics validation
  asyncHandler(async (req: Request, res: Response) => {
    const query: any = {
      startDate: req.query.startDate as string,
      endDate: req.query.endDate as string,
      level: req.query.level ? parseInt(req.query.level as string) : undefined
    };

    const response = await getAnalyticsService().getGameBalanceAnalytics(query);
    res.json(response);
}));

// GET /api/v1/analytics/summary - Get analytics summary
router.get('/summary',
  sanitizeRequest,
  // validateAnalytics.query, // TODO: Implement analytics validation
  asyncHandler(async (req: Request, res: Response) => {
  const query: any = {
    startDate: req.query.startDate as string,
    endDate: req.query.endDate as string
  };

  const response = await getAnalyticsService().getAnalyticsSummary(query);
  res.json(response);
}));

export default router;