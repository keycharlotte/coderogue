import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { cn } from '@/lib/utils';
import { 
  BarChart3, 
  Users, 
  CreditCard, 
  Scale, 
  Download, 
  FileText,
  Home
} from 'lucide-react';

const navigationItems = [
  {
    name: '首页',
    href: '/',
    icon: Home
  },
  {
    name: '数据仪表板',
    href: '/dashboard',
    icon: BarChart3
  },
  {
    name: '卡牌分析',
    href: '/cards',
    icon: CreditCard
  },
  {
    name: '玩家行为',
    href: '/players',
    icon: Users
  },
  {
    name: '游戏平衡',
    href: '/balance',
    icon: Scale
  },
  {
    name: '数据导出',
    href: '/export',
    icon: Download
  },
  {
    name: 'API文档',
    href: '/api-docs',
    icon: FileText
  }
];

export const Navigation: React.FC = () => {
  const location = useLocation();

  return (
    <nav className="bg-white shadow-sm border-b">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between h-16">
          <div className="flex">
            <div className="flex-shrink-0 flex items-center">
              <h1 className="text-xl font-bold text-gray-900">CodeRogue Analytics</h1>
            </div>
            <div className="hidden sm:ml-6 sm:flex sm:space-x-8">
              {navigationItems.map((item) => {
                const Icon = item.icon;
                const isActive = location.pathname === item.href;
                
                return (
                  <Link
                    key={item.name}
                    to={item.href}
                    className={cn(
                      'inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium',
                      isActive
                        ? 'border-indigo-500 text-gray-900'
                        : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700'
                    )}
                  >
                    <Icon className="h-4 w-4 mr-2" />
                    {item.name}
                  </Link>
                );
              })}
            </div>
          </div>
        </div>
      </div>
    </nav>
  );
};