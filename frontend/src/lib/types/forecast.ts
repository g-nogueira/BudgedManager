export type ForecastType = 'ORIGINAL' | 'REFORECAST';

export interface Forecast {
  forecastId: string;
  budgetId: string;
  versionLabel: string;
  forecastType: ForecastType;
  startDay: number;
  startBalance: number;
  endOfMonthBalance: number;
  isSnapshot: boolean;
  dailyEntries: DailyEntry[];
}

export interface ForecastSummary {
  forecastId: string;
  versionLabel: string;
  forecastType: ForecastType;
  endOfMonthBalance: number;
  isSnapshot: boolean;
  createdAt: string;
}

export interface DailyEntry {
  dayNumber: number;
  remainingBalance: number;
  dailyExpenseTotal: number;
  breakdown: ExpenseItem[];
}

export interface ExpenseItem {
  name: string;
  amount: number;
}

export interface GenerateForecastResult {
  forecastId: string;
  versionLabel: string;
  endOfMonthBalance: number;
  dayCount: number;
}

export interface SaveSnapshotResult {
  forecastId: string;
  isSnapshot: boolean;
}

export interface ReforecastResult {
  forecastId: string;
  endOfMonthBalance: number;
}

export interface ComparisonResult {
  forecastAId: string;
  forecastBId: string;
  labelA: string;
  labelB: string;
  endBalanceA: number;
  endBalanceB: number;
  totalDrift: number;
  dayVariances: DayVariance[];
  expenseChanges: ExpenseChange[];
}

export interface DayVariance {
  dayNumber: number;
  balanceA: number;
  balanceB: number;
  variance: number;
}

export interface ExpenseChange {
  expenseName: string;
  changeType: 'ADDED' | 'REMOVED' | 'MODIFIED';
  amountA: number | null;
  amountB: number | null;
}

export interface GenerateForecastRequest {
  startBalance: number;
}

export interface ReforecastRequest {
  startDay: number;
  actualBalance: number;
  versionLabel: string;
}
