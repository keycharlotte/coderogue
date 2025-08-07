import React, { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { MetricCard } from '@/components/dashboard/MetricCard';
import { LineChart } from '@/components/charts/LineChart';
import { BarChart } from '@/components/charts/BarChart';
import { PieChart } from '@/components/charts/PieChart';
import { Users, GamepadIcon, Activity, Clock, TrendingUp, Download } from 'lucide-react';
import { toast } from 'sonner';

interface AnalyticsSummary {
  totalPlayers: number;
  totalSessions: number;
  totalEvents: number;
  avgSessionDuration: number;
  popularCards: Array<{
    cardId: string;
    usageCount: number;
    winRate: number;
  }>;
  activityTrends: Array<{
    date: string;
    events: number;
    players: number;
  }>;
}

export default function Dashboard() {
  const [summary, setSummary] = useState<AnalyticsSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchAnalyticsSummary();
  }, []);

  const fetchAnalyticsSummary = async () => {
    try {
      setLoading(true);
      const response = await fetch('/api/v1/analytics/summary');
      if (!response.ok) {
        throw new Error('Failed to fetch analytics summary');
      }
      const data = await response.json();
      setSummary(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  const formatDuration = (seconds: number) => {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes}m ${remainingSeconds}s`;
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-6">
        <div className="max-w-7xl mx-auto">
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto"></div>
            <p className="mt-4 text-gray-600">Loading analytics data...</p>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-6">
        <div className="max-w-7xl mx-auto">
          <div className="text-center py-12">
            <div className="text-red-600 text-xl mb-4">Error loading data</div>
            <p className="text-gray-600 mb-4">{error}</p>
            <Button onClick={fetchAnalyticsSummary}>Retry</Button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-4xl font-bold text-gray-900 mb-2">Analytics Dashboard</h1>
          <p className="text-gray-600">Game data insights and performance metrics</p>
        </div>

        {/* 关键指标 */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <MetricCard
            title="总玩家数"
            value={summary?.totalPlayers || 0}
            change={12}
            changeType="increase"
            icon={<Users className="h-4 w-4" />}
            description="较上月"
          />
          
          <MetricCard
            title="总会话数"
            value={summary?.totalSessions || 0}
            change={8}
            changeType="increase"
            icon={<GamepadIcon className="h-4 w-4" />}
            description="较上月"
          />
          
          <MetricCard
            title="总事件数"
            value={summary?.totalEvents || 0}
            change={15}
            changeType="increase"
            icon={<Activity className="h-4 w-4" />}
            description="较上月"
          />
          
          <MetricCard
            title="平均会话时长"
            value={`${summary?.avgSessionDuration || 0}分钟`}
            change={5}
            changeType="increase"
            icon={<Clock className="h-4 w-4" />}
            description="较上月"
          />
        </div>

        {/* 热门卡牌 */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-8">
          <Card className="bg-white shadow-lg">
            <CardHeader>
              <CardTitle className="text-lg font-semibold text-gray-900">热门卡牌</CardTitle>
            </CardHeader>
            <CardContent>
              <BarChart
                data={summary?.popularCards?.slice(0, 5) || []}
                xKey="cardName"
                yKey="usageCount"
                height={320}
                color="#10b981"
              />
            </CardContent>
          </Card>

          {/* 活动趋势图表 */}
          <Card className="bg-white shadow-lg">
            <CardHeader>
              <CardTitle className="text-lg font-semibold text-gray-900">玩家活动趋势</CardTitle>
            </CardHeader>
            <CardContent>
              <LineChart
                data={summary?.activityTrends || []}
                xKey="date"
                yKey="players"
                height={320}
                color="#3b82f6"
              />
            </CardContent>
          </Card>
        </div>

        {/* 快速操作 */}
        <Card className="bg-white shadow-lg">
          <CardHeader>
            <CardTitle className="text-lg font-semibold text-gray-900">快速操作</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <Button 
                className="h-12 bg-blue-600 hover:bg-blue-700 text-white"
                onClick={() => {
                  window.location.href = '/cards';
                  toast.success('正在跳转到卡牌分析页面');
                }}
              >
                <TrendingUp className="mr-2 h-4 w-4" />
                查看分析
              </Button>
              <Button 
                variant="outline" 
                className="h-12 border-green-600 text-green-600 hover:bg-green-50"
                onClick={() => {
                  window.location.href = '/export';
                  toast.success('正在跳转到数据导出页面');
                }}
              >
                <Download className="mr-2 h-4 w-4" />
                导出数据
              </Button>
              <Button 
                variant="outline" 
                className="h-12 border-purple-600 text-purple-600 hover:bg-purple-50"
                onClick={() => {
                  window.location.href = '/players';
                  toast.success('正在跳转到玩家报告页面');
                }}
              >
                <Users className="mr-2 h-4 w-4" />
                玩家报告
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}