export type BudgetStatus = 'DRAFT' | 'ACTIVE' | 'CLOSED';
export type ExpenseCategory = 'FIXED' | 'SUBSCRIPTION' | 'VARIABLE';

export interface Budget {
  budgetId: string;
  householdId: string;
  yearMonth: string;
  status: BudgetStatus;
  incomeSources: IncomeSource[];
  expenses: Expense[];
  totalIncome: number;
  createdAt: string;
  updatedAt: string;
}

export interface IncomeSource {
  incomeId: string;
  name: string;
  amount: number;
}

export interface Expense {
  expenseId: string;
  name: string;
  category: string;
  dayOfMonth: number | null;
  isSpread: boolean;
  amount: number;
  isExcluded: boolean;
}

export interface CreateBudgetResult {
  budgetId: string;
  status: string;
}

export interface AddIncomeResult {
  incomeId: string;
}

export interface AddExpenseResult {
  expenseId: string;
}

export interface ActivateBudgetResult {
  budgetId: string;
  status: string;
}

export interface RolloverMonthResult {
  newBudgetId: string;
}

export interface CreateBudgetRequest {
  yearMonth: string;
}

export interface AddIncomeRequest {
  name: string;
  amount: number;
}

export interface UpdateIncomeRequest {
  name: string;
  amount: number;
}

export interface AddExpenseRequest {
  name: string;
  category: ExpenseCategory;
  dayOfMonth: number | null;
  isSpread: boolean;
  amount: number;
}

export interface UpdateExpenseRequest {
  name: string;
  category: ExpenseCategory;
  dayOfMonth: number | null;
  isSpread: boolean;
  amount: number;
}

export interface ToggleExclusionRequest {
  isExcluded: boolean;
}

export interface RolloverMonthRequest {
  targetYearMonth: string;
}
