interface ExpenseValidationInput {
  name: string;
  amountInput: string;
  isSpread: boolean;
  dayInput: string;
}

export const validateExpenseInput = ({
  name,
  amountInput,
  isSpread,
  dayInput
}: ExpenseValidationInput): string | null => {
  if (name.trim().length === 0) {
    return 'Expense name is required.';
  }

  const parsedAmount = Number(amountInput);
  if (!Number.isFinite(parsedAmount) || parsedAmount <= 0) {
    return 'Amount must be greater than zero.';
  }

  if (!isSpread) {
    const parsedDay = Number(dayInput);
    if (!Number.isFinite(parsedDay) || !Number.isInteger(parsedDay) || parsedDay < 1 || parsedDay > 31) {
      return 'Day of month must be between 1 and 31.';
    }
  }

  return null;
};
