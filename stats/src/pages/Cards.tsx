import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { BarChart } from '@/components/charts/BarChart';
import { PieChart } from '@/components/charts/PieChart';
import { LineChart } from '@/components/charts/LineChart';
import { Search, Filter, TrendingUp, TrendingDown, Users, Target, Zap, BarChart3 } from 'lucide-react';
import { toast } from 'sonner';

interface CardStat {
  cardId: string;
  totalUsage: number;
  winRate: number;
  wins: number;
  losses: number;
}

interface CardCombination {
  cards: string[];
  frequency: number;
  winRate: number;
}

interface CardTrend {
  cardId: string;
  date: string;
  usage: number;
}

interface CardAnalyticsResponse {
  cardStats: CardStat[];
  combinations: CardCombination[];
  trends: CardTrend[];
  totalCards: number;
}

export default function Cards() {
  const [analytics, setAnalytics] = useState<CardAnalyticsResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [sortBy, setSortBy] = useState('usage');
  const [dateRange, setDateRange] = useState('30');

  useEffect(() => {
    fetchCardAnalytics();
  }, [dateRange]);

  const fetchCardAnalytics = async () => {
    try {
      setLoading(true);
      const endDate = new Date();
      const startDate = new Date();
      startDate.setDate(endDate.getDate() - parseInt(dateRange));
      
      const params = new URLSearchParams({
        startDate: startDate.toISOString(),
        endDate: endDate.toISOString()
      });
      
      const response = await fetch(`/api/v1/analytics/cards?${params}`);
      if (!response.ok) {
        throw new Error('Failed to fetch card analytics');
      }
      const data = await response.json();
      setAnalytics(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  const filteredCards = analytics?.cardStats?.filter(card =>
    card.cardId.toLowerCase().includes(searchTerm.toLowerCase())
  ) || [];

  const sortedCards = [...filteredCards].sort((a, b) => {
    switch (sortBy) {
      case 'usage':
        return b.totalUsage - a.totalUsage;
      case 'winRate':
        return b.winRate - a.winRate;
      case 'name':
        return a.cardId.localeCompare(b.cardId);
      default:
        return 0;
    }
  });

  const getWinRateColor = (winRate: number) => {
    if (winRate >= 0.7) return 'bg-green-100 text-green-800';
    if (winRate >= 0.5) return 'bg-yellow-100 text-yellow-800';
    return 'bg-red-100 text-red-800';
  };

  const getUsageIcon = (usage: number) => {
    const maxUsage = Math.max(...(analytics?.cardStats?.map(c => c.totalUsage) || [1]));
    const percentage = usage / maxUsage;
    
    if (percentage >= 0.8) return <TrendingUp className="h-4 w-4 text-green-600" />;
    if (percentage >= 0.4) return <BarChart3 className="h-4 w-4 text-yellow-600" />;
    return <TrendingDown className="h-4 w-4 text-red-600" />;
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-purple-50 to-pink-100 p-6">
        <div className="max-w-7xl mx-auto">
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-purple-600 mx-auto"></div>
            <p className="mt-4 text-gray-600">Loading card analytics...</p>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-purple-50 to-pink-100 p-6">
        <div className="max-w-7xl mx-auto">
          <div className="text-center py-12">
            <div className="text-red-600 text-xl mb-4">Error loading data</div>
            <p className="text-gray-600 mb-4">{error}</p>
            <Button onClick={fetchCardAnalytics}>Retry</Button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-purple-50 to-pink-100 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-4xl font-bold text-gray-900 mb-2">Card Analytics</h1>
          <p className="text-gray-600">Analyze card usage patterns, win rates, and combinations</p>
        </div>

        {/* Filters */}
        <Card className="bg-white shadow-lg mb-8">
          <CardHeader>
            <CardTitle className="text-xl font-semibold text-gray-900">Filters &amp; Search</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
                <Input
                  placeholder="Search cards..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-10"
                />
              </div>
              
              <Select value={sortBy} onValueChange={setSortBy}>
                <SelectTrigger>
                  <SelectValue placeholder="Sort by" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="usage">Usage Count</SelectItem>
                  <SelectItem value="winRate">Win Rate</SelectItem>
                  <SelectItem value="name">Card Name</SelectItem>
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
              
              <Button onClick={fetchCardAnalytics} className="bg-purple-600 hover:bg-purple-700">
                Refresh Data
              </Button>
            </div>
          </CardContent>
        </Card>

        {/* Summary Stats */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <Card className="bg-white shadow-lg">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium text-gray-600">Total Cards</CardTitle>
              <Zap className="h-4 w-4 text-purple-600" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-gray-900">{analytics?.totalCards || 0}</div>
              <p className="text-xs text-gray-500 mt-1">Unique cards analyzed</p>
            </CardContent>
          </Card>

          <Card className="bg-white shadow-lg">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium text-gray-600">Total Usage</CardTitle>
              <BarChart3 className="h-4 w-4 text-blue-600" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-gray-900">
                {analytics?.cardStats?.reduce((sum, card) => sum + card.totalUsage, 0)?.toLocaleString() || 0}
              </div>
              <p className="text-xs text-gray-500 mt-1">Total card plays</p>
            </CardContent>
          </Card>

          <Card className="bg-white shadow-lg">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium text-gray-600">Avg Win Rate</CardTitle>
              <TrendingUp className="h-4 w-4 text-green-600" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-gray-900">
                {analytics?.cardStats?.length ? 
                  ((analytics.cardStats.reduce((sum, card) => sum + card.winRate, 0) / analytics.cardStats.length) * 100).toFixed(1) + '%' 
                  : '0%'
                }
              </div>
              <p className="text-xs text-gray-500 mt-1">Average across all cards</p>
            </CardContent>
          </Card>
        </div>

        {/* 卡牌使用统计图表 */}
        <Card className="bg-white shadow-lg mb-8">
          <CardHeader>
            <CardTitle className="text-xl font-semibold text-gray-900">卡牌使用统计</CardTitle>
          </CardHeader>
          <CardContent>
            <BarChart
              data={analytics?.cardStats || []}
              xKey="cardId"
              yKey="totalUsage"
              height={320}
              color="#8884d8"
            />
          </CardContent>
        </Card>

        {/* Card Statistics Table */}
        <Card className="bg-white shadow-lg mb-8">
          <CardHeader>
            <CardTitle className="text-xl font-semibold text-gray-900">Card Performance</CardTitle>
            <CardDescription>Detailed statistics for each card</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-gray-200">
                    <th className="text-left py-3 px-4 font-semibold text-gray-700">Card</th>
                    <th className="text-left py-3 px-4 font-semibold text-gray-700">Usage</th>
                    <th className="text-left py-3 px-4 font-semibold text-gray-700">Win Rate</th>
                    <th className="text-left py-3 px-4 font-semibold text-gray-700">Wins/Losses</th>
                    <th className="text-left py-3 px-4 font-semibold text-gray-700">Trend</th>
                  </tr>
                </thead>
                <tbody>
                  {sortedCards.map((card, index) => (
                    <tr key={card.cardId} className="border-b border-gray-100 hover:bg-gray-50">
                      <td className="py-3 px-4">
                        <div className="flex items-center space-x-3">
                          <div className="w-8 h-8 bg-purple-100 rounded-lg flex items-center justify-center">
                            <span className="text-sm font-semibold text-purple-600">#{index + 1}</span>
                          </div>
                          <span className="font-medium text-gray-900">{card.cardId}</span>
                        </div>
                      </td>
                      <td className="py-3 px-4">
                        <div className="flex items-center space-x-2">
                          {getUsageIcon(card.totalUsage)}
                          <span className="font-semibold text-gray-900">{card.totalUsage.toLocaleString()}</span>
                        </div>
                      </td>
                      <td className="py-3 px-4">
                        <Badge className={getWinRateColor(card.winRate)}>
                          {(card.winRate * 100).toFixed(1)}%
                        </Badge>
                      </td>
                      <td className="py-3 px-4">
                        <span className="text-green-600 font-medium">{card.wins}</span>
                        <span className="text-gray-400 mx-1">/</span>
                        <span className="text-red-600 font-medium">{card.losses}</span>
                      </td>
                      <td className="py-3 px-4">
                        <div className="w-16 h-8 bg-gray-100 rounded flex items-center justify-center">
                          <div className="w-12 h-2 bg-gradient-to-r from-blue-200 to-blue-500 rounded"></div>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              
              {sortedCards.length === 0 && (
                <div className="text-center py-8 text-gray-500">
                  No cards found matching your criteria
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Card Combinations */}
        <Card className="bg-white shadow-lg">
          <CardHeader>
            <CardTitle className="text-xl font-semibold text-gray-900">Popular Combinations</CardTitle>
            <CardDescription>Most frequently used card combinations</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {analytics?.combinations?.slice(0, 6).map((combo, index) => (
                <div key={index} className="p-4 bg-gray-50 rounded-lg">
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-sm font-semibold text-gray-600">Combo #{index + 1}</span>
                    <Badge className={getWinRateColor(combo.winRate)}>
                      {(combo.winRate * 100).toFixed(1)}%
                    </Badge>
                  </div>
                  <div className="space-y-1 mb-2">
                    {combo.cards.map((cardId, cardIndex) => (
                      <div key={cardIndex} className="text-sm text-gray-700 bg-white px-2 py-1 rounded">
                        {cardId}
                      </div>
                    ))}
                  </div>
                  <p className="text-xs text-gray-500">{combo.frequency} times used</p>
                </div>
              )) || (
                <div className="col-span-full text-center py-8 text-gray-500">
                  No combination data available
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}