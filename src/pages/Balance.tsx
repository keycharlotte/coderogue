import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { BarChart } from '@/components/charts/BarChart';
import { LineChart } from '@/components/charts/LineChart';
import { PieChart } from '@/components/charts/PieChart';
import { Scale, TrendingUp, TrendingDown, AlertTriangle, CheckCircle, Target, Activity, Zap, BarChart3, Shield, Sword } from 'lucide-react';
import { toast } from 'sonner';

interface LevelDifficulty {
  levelId: string;
  difficultyScore: number;
  averageAttempts: number;
  completionRate: number;
  averageTime: number;
}

interface CardBalance {
  cardId: string;
  powerLevel: number;
  usageRate: number;
  winRate: number;
  balanceScore: number;
}

interface ProgressionBottleneck {
  levelId: string;
  dropOffRate: number;
  averageAttempts: number;
  commonFailureReasons: string[];
}

interface WinLossDistribution {
  outcome: 'win' | 'loss';
  count: number;
  percentage: number;
  averageGameLength: number;
}

interface GameFlowAnalysis {
  stage: string;
  playerCount: number;
  averageTime: number;
  dropOffRate: number;
}

interface BalanceRecommendation {
  type: 'card' | 'level' | 'progression';
  priority: 'high' | 'medium' | 'low';
  target: string;
  issue: string;
  recommendation: string;
  expectedImpact: string;
}

interface GameBalanceResponse {
  levelDifficulty: LevelDifficulty[];
  cardBalance: CardBalance[];
  bottlenecks: ProgressionBottleneck[];
  winLossDistribution: WinLossDistribution[];
  gameFlow: GameFlowAnalysis[];
  recommendations: BalanceRecommendation[];
  overallBalance: number;
}

export default function Balance() {
  const [analytics, setAnalytics] = useState<GameBalanceResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [dateRange, setDateRange] = useState('30');
  const [selectedCategory, setSelectedCategory] = useState('all');

  useEffect(() => {
    fetchBalanceAnalytics();
  }, [dateRange]);

  const fetchBalanceAnalytics = async () => {
    try {
      setLoading(true);
      const endDate = new Date();
      const startDate = new Date();
      startDate.setDate(endDate.getDate() - parseInt(dateRange));
      
      const params = new URLSearchParams({
        startDate: startDate.toISOString(),
        endDate: endDate.toISOString()
      });
      
      const response = await fetch(`/api/v1/analytics/balance?${params}`);
      if (!response.ok) {
        throw new Error('Failed to fetch balance analytics');
      }
      const data = await response.json();
      setAnalytics(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  const getDifficultyColor = (score: number) => {
    if (score >= 0.8) return 'bg-red-100 text-red-800';
    if (score >= 0.6) return 'bg-orange-100 text-orange-800';
    if (score >= 0.4) return 'bg-yellow-100 text-yellow-800';
    return 'bg-green-100 text-green-800';
  };

  const getBalanceColor = (score: number) => {
    if (score >= 0.8) return 'bg-green-100 text-green-800';
    if (score >= 0.6) return 'bg-yellow-100 text-yellow-800';
    if (score >= 0.4) return 'bg-orange-100 text-orange-800';
    return 'bg-red-100 text-red-800';
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'high': return 'bg-red-100 text-red-800';
      case 'medium': return 'bg-yellow-100 text-yellow-800';
      case 'low': return 'bg-green-100 text-green-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  const getRecommendationIcon = (type: string) => {
    switch (type) {
      case 'card': return <Zap className="h-4 w-4" />;
      case 'level': return <Target className="h-4 w-4" />;
      case 'progression': return <TrendingUp className="h-4 w-4" />;
      default: return <BarChart3 className="h-4 w-4" />;
    }
  };

  const formatTime = (seconds: number) => {
    const minutes = Math.floor(seconds / 60);
    const secs = Math.round(seconds % 60);
    return minutes > 0 ? `${minutes}m ${secs}s` : `${secs}s`;
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-orange-50 to-red-100 p-6">
        <div className="max-w-7xl mx-auto">
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-orange-600 mx-auto"></div>
            <p className="mt-4 text-gray-600">Loading balance analytics...</p>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-orange-50 to-red-100 p-6">
        <div className="max-w-7xl mx-auto">
          <div className="text-center py-12">
            <div className="text-red-600 text-xl mb-4">Error loading data</div>
            <p className="text-gray-600 mb-4">{error}</p>
            <Button onClick={fetchBalanceAnalytics}>Retry</Button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-orange-50 to-red-100 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-4xl font-bold text-gray-900 mb-2">Game Balance Analytics</h1>
          <p className="text-gray-600">Analyze game difficulty, card balance, and progression bottlenecks</p>
        </div>

        {/* Filters */}
        <Card className="bg-white shadow-lg mb-8">
          <CardHeader>
            <CardTitle className="text-xl font-semibold text-gray-900">Analysis Controls</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <Select value={selectedCategory} onValueChange={setSelectedCategory}>
                <SelectTrigger>
                  <SelectValue placeholder="Analysis category" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Categories</SelectItem>
                  <SelectItem value="cards">Card Balance</SelectItem>
                  <SelectItem value="levels">Level Difficulty</SelectItem>
                  <SelectItem value="progression">Progression</SelectItem>
                </SelectContent>
              </Select>
              
              <Select value={dateRange} onValueChange={setDateRange}>
                <SelectTrigger>
                  <SelectValue placeholder="Date range" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="7">Last 7 days</SelectItem>
                  <SelectItem value="30">Last 30 days</SelectItem>
                  <SelectItem value="90">Last 90 days</SelectItem>
                </SelectContent>
              </Select>
              
              <Button onClick={fetchBalanceAnalytics} className="bg-orange-600 hover:bg-orange-700">
                Refresh Analysis
              </Button>
            </div>
          </CardContent>
        </Card>

        {/* Overall Balance Score */}
        <Card className="bg-white shadow-lg mb-8">
          <CardHeader>
            <CardTitle className="text-xl font-semibold text-gray-900">Overall Game Balance</CardTitle>
            <CardDescription>Comprehensive balance assessment across all game elements</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="flex items-center justify-center">
              <div className="text-center">
                <div className="text-6xl font-bold text-gray-900 mb-4">
                  {analytics?.overallBalance ? (analytics.overallBalance * 100).toFixed(0) : '0'}
                </div>
                <Badge className={getBalanceColor(analytics?.overallBalance || 0)} variant="outline">
                  {analytics?.overallBalance ? 
                    analytics.overallBalance >= 0.8 ? 'EXCELLENT' :
                    analytics.overallBalance >= 0.6 ? 'GOOD' :
                    analytics.overallBalance >= 0.4 ? 'NEEDS ATTENTION' : 'CRITICAL'
                    : 'NO DATA'
                  }
                </Badge>
                <p className="text-sm text-gray-500 mt-2">Balance Score (0-100)</p>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Win/Loss Distribution */}
        <Card className="bg-white shadow-lg mb-8">
          <CardHeader>
            <CardTitle className="text-xl font-semibold text-gray-900">Win/Loss Distribution</CardTitle>
            <CardDescription>Overall game outcome balance</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {analytics?.winLossDistribution?.map((dist, index) => (
                <div key={index} className="text-center p-6 bg-gray-50 rounded-lg">
                  <div className="flex items-center justify-center mb-4">
                    {dist.outcome === 'win' ? 
                      <Shield className="h-8 w-8 text-green-600" /> : 
                      <Sword className="h-8 w-8 text-red-600" />
                    }
                  </div>
                  <div className="text-3xl font-bold text-gray-900 mb-2">{dist.percentage.toFixed(1)}%</div>
                  <div className="text-lg font-semibold mb-2 capitalize">{dist.outcome}s</div>
                  <div className="text-sm text-gray-600">
                    {dist.count.toLocaleString()} games
                  </div>
                  <div className="text-xs text-gray-500 mt-1">
                    Avg: {formatTime(dist.averageGameLength)}
                  </div>
                </div>
              )) || (
                <div className="col-span-full text-center py-8 text-gray-500">
                  No win/loss data available
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Level Difficulty Analysis */}
        <Card className="bg-white shadow-lg mb-8">
          <CardHeader>
            <CardTitle className="text-xl font-semibold text-gray-900">Level Difficulty Analysis</CardTitle>
            <CardDescription>Difficulty assessment for each level</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-gray-200">
                    <th className="text-left py-3 px-4 font-semibold text-gray-700">Level</th>
                    <th className="text-left py-3 px-4 font-semibold text-gray-700">Difficulty</th>
                    <th className="text-left py-3 px-4 font-semibold text-gray-700">Completion Rate</th>
                    <th className="text-left py-3 px-4 font-semibold text-gray-700">Avg Attempts</th>
                    <th className="text-left py-3 px-4 font-semibold text-gray-700">Avg Time</th>
                  </tr>
                </thead>
                <tbody>
                  {analytics?.levelDifficulty?.map((level, index) => (
                    <tr key={level.levelId} className="border-b border-gray-100 hover:bg-gray-50">
                      <td className="py-3 px-4">
                        <div className="flex items-center space-x-3">
                          <div className="w-8 h-8 bg-orange-100 rounded-lg flex items-center justify-center">
                            <span className="text-sm font-semibold text-orange-600">#{index + 1}</span>
                          </div>
                          <span className="font-medium text-gray-900">{level.levelId}</span>
                        </div>
                      </td>
                      <td className="py-3 px-4">
                        <Badge className={getDifficultyColor(level.difficultyScore)}>
                          {(level.difficultyScore * 100).toFixed(0)}%
                        </Badge>
                      </td>
                      <td className="py-3 px-4">
                        <div className="flex items-center space-x-2">
                          {level.completionRate >= 0.7 ? 
                            <TrendingUp className="h-4 w-4 text-green-600" /> : 
                            <TrendingDown className="h-4 w-4 text-red-600" />
                          }
                          <span className="font-semibold text-gray-900">
                            {(level.completionRate * 100).toFixed(1)}%
                          </span>
                        </div>
                      </td>
                      <td className="py-3 px-4">
                        <span className="text-gray-900">{level.averageAttempts.toFixed(1)}</span>
                      </td>
                      <td className="py-3 px-4">
                        <span className="text-gray-900">{formatTime(level.averageTime)}</span>
                      </td>
                    </tr>
                  )) || (
                    <tr>
                      <td colSpan={5} className="text-center py-8 text-gray-500">
                        No level difficulty data available
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>

        {/* Card Balance Analysis */}
        <Card className="bg-white shadow-lg mb-8">
          <CardHeader>
            <CardTitle className="text-xl font-semibold text-gray-900">Card Balance Analysis</CardTitle>
            <CardDescription>Power level and usage balance for cards</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {analytics?.cardBalance?.slice(0, 9).map((card, index) => (
                <div key={card.cardId} className="p-4 bg-gray-50 rounded-lg">
                  <div className="flex items-center justify-between mb-2">
                    <h3 className="font-semibold text-gray-900">{card.cardId}</h3>
                    <Badge className={getBalanceColor(card.balanceScore)}>
                      {(card.balanceScore * 100).toFixed(0)}
                    </Badge>
                  </div>
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-600">Power Level:</span>
                      <span className="font-medium">{card.powerLevel.toFixed(1)}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-600">Usage Rate:</span>
                      <span className="font-medium">{(card.usageRate * 100).toFixed(1)}%</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-600">Win Rate:</span>
                      <span className="font-medium">{(card.winRate * 100).toFixed(1)}%</span>
                    </div>
                  </div>
                </div>
              )) || (
                <div className="col-span-full text-center py-8 text-gray-500">
                  No card balance data available
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Progression Bottlenecks */}
        <Card className="bg-white shadow-lg mb-8">
          <CardHeader>
            <CardTitle className="text-xl font-semibold text-gray-900">Progression Bottlenecks</CardTitle>
            <CardDescription>Levels where players commonly get stuck</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {analytics?.bottlenecks?.map((bottleneck, index) => (
                <div key={bottleneck.levelId} className="p-4 bg-red-50 border border-red-200 rounded-lg">
                  <div className="flex items-center justify-between mb-2">
                    <div className="flex items-center space-x-2">
                      <AlertTriangle className="h-5 w-5 text-red-600" />
                      <h3 className="font-semibold text-gray-900">{bottleneck.levelId}</h3>
                    </div>
                    <Badge className="bg-red-100 text-red-800">
                      {(bottleneck.dropOffRate * 100).toFixed(1)}% drop-off
                    </Badge>
                  </div>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <p className="text-sm text-gray-600 mb-1">Average Attempts: <span className="font-semibold">{bottleneck.averageAttempts.toFixed(1)}</span></p>
                    </div>
                    <div>
                      <p className="text-sm text-gray-600 mb-1">Common Issues:</p>
                      <ul className="text-xs text-gray-500 space-y-1">
                        {bottleneck.commonFailureReasons.map((reason, reasonIndex) => (
                          <li key={reasonIndex} className="flex items-center">
                            <span className="w-1 h-1 bg-red-400 rounded-full mr-2"></span>
                            {reason}
                          </li>
                        ))}
                      </ul>
                    </div>
                  </div>
                </div>
              )) || (
                <div className="text-center py-8 text-gray-500">
                  No bottlenecks detected
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Balance Recommendations */}
        <Card className="bg-white shadow-lg">
          <CardHeader>
            <CardTitle className="text-xl font-semibold text-gray-900">Balance Recommendations</CardTitle>
            <CardDescription>AI-generated suggestions to improve game balance</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {analytics?.recommendations?.map((rec, index) => (
                <div key={index} className="p-4 bg-gray-50 rounded-lg border-l-4 border-blue-500">
                  <div className="flex items-start justify-between mb-2">
                    <div className="flex items-center space-x-2">
                      {getRecommendationIcon(rec.type)}
                      <h3 className="font-semibold text-gray-900">{rec.target}</h3>
                    </div>
                    <Badge className={getPriorityColor(rec.priority)}>
                      {rec.priority.toUpperCase()}
                    </Badge>
                  </div>
                  <div className="space-y-2">
                    <div>
                      <p className="text-sm font-medium text-gray-700">Issue:</p>
                      <p className="text-sm text-gray-600">{rec.issue}</p>
                    </div>
                    <div>
                      <p className="text-sm font-medium text-gray-700">Recommendation:</p>
                      <p className="text-sm text-gray-600">{rec.recommendation}</p>
                    </div>
                    <div>
                      <p className="text-sm font-medium text-gray-700">Expected Impact:</p>
                      <p className="text-sm text-green-600">{rec.expectedImpact}</p>
                    </div>
                  </div>
                </div>
              )) || (
                <div className="text-center py-8 text-gray-500">
                  No recommendations available
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}