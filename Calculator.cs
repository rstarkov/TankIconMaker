using System;
using System.Collections.Generic;

class Calculator
{
    public readonly string[] ReservedVariables = new string[] { "PI", "e" };
    public readonly string[] Functions = new string[]{"sqrt","sin","cos","tan","log","ln","abs","exp","deg","log10",
		"acos","asin","atan","ceil","floor","round","trunc","sihn","cosh","tanh","sign"};
    protected Dictionary<string, double> Variables = new Dictionary<string, double>();
    public readonly string[] Operators = new string[] { "(", ")", "+", "-", "*", "%", "/", "^" };

    public Calculator()
    {
        Variables.Add("PI", Math.PI);
        Variables.Add("e", Math.E);
    }

    public virtual bool SetVariable(string varName, double value)
    {
        foreach (string opertr in Operators)
        {
            if (varName.Contains(opertr))
            {
                return false;
            }
        }
        foreach (string переменная in ReservedVariables)
        {
            if (varName == переменная)
            {
                return false;
            }
        }
        Variables[varName] = value;
        return true;
    }

    public virtual bool GetVariable(string varName, ref double value)
    {
        if (Variables.ContainsKey(varName))
        {
            value = Variables[varName];
            return true;
        }
        else
        {
            value = 0;
            return false;
        }
    }

    private int GetClosingBracket(string Expression, int Start, ref int bracketLevel)
    {
        int Position = Start;
        for (int i = Start; i < Expression.Length; ++i)
        {
            switch (Expression[i])
            {
                case '(': ++bracketLevel; break;
                case ')': --bracketLevel; break;
            }
            if (bracketLevel == 0)
            {
                Position = i;
                break;
            }
        }
        return Position;
    }

    /// <summary>
    /// Evaluates the expression
    /// </summary>
    /// <param name="Expression">Expression to evaluate</param>
    /// <returns></returns>
    public double Parse(string Expression)
    {
        Expression = Expression.Replace(" ", "");
        int bracketLevel = 0;
        char symbol = Expression[0];
        if (symbol == '(' &&
            GetClosingBracket(Expression, 0, ref bracketLevel) == Expression.Length - 1)
            Expression = Expression.Substring(1, Expression.Length - 2);

        double varValue = 0;
        double result = 0;
        int pos;
        if (GetVariable(Expression, ref varValue))
        {
            result = varValue;
            return result;
        }
        string expressionInBracket;
        if (Expression.Length > 4)
        {
            int pos2 = Expression.IndexOf('(');
            if (pos2 != -1 && pos2 <= 5)
            {
                pos = GetClosingBracket(Expression, pos2, ref bracketLevel);
                if (pos == Expression.Length - 1)
                {
                    expressionInBracket = Expression.Substring(pos2 + 1, pos - pos2 - 1);
                    string function = Expression.Substring(0, pos2);
                    switch (function)
                    {
                        case "sqrt":
                            result = Math.Sqrt(Parse(expressionInBracket));
                            return result;
                        case "sin":
                            result = Math.Sin(Parse(expressionInBracket));
                            return result;
                        case "cos":
                            result = Math.Cos(Parse(expressionInBracket));
                            return result;
                        case "tan":
                            result = Math.Tan(Parse(expressionInBracket));
                            return result;
                        case "log":
                        case "ln":
                            result = Math.Log(Parse(expressionInBracket));
                            return result;
                        case "abs":
                            result = Math.Abs(Parse(expressionInBracket));
                            return result;
                        case "exp":
                            result = Math.Exp(Parse(expressionInBracket));
                            return result;
                        case "deg":
                            result = Math.PI * Parse(expressionInBracket) / 180.0;
                            return result;
                        case "log10":
                            result = Math.Log10(Parse(expressionInBracket));
                            return result;
                        case "acos":
                            result = Math.Acos(Parse(expressionInBracket));
                            return result;
                        case "asin":
                            result = Math.Asin(Parse(expressionInBracket));
                            return result;
                        case "atan":
                            result = Math.Atan(Parse(expressionInBracket));
                            return result;
                        case "ceil":
                            result = Math.Ceiling(Parse(expressionInBracket));
                            return result;
                        case "floor":
                            result = Math.Floor(Parse(expressionInBracket));
                            return result;
                        case "round":
                            result = Math.Round(Parse(expressionInBracket));
                            return result;
                        case "trunc":
                            result = Math.Truncate(Parse(expressionInBracket));
                            return result;
                        case "sihn":
                            result = Math.Sinh(Parse(expressionInBracket));
                            return result;
                        case "cosh":
                            result = Math.Cosh(Parse(expressionInBracket));
                            return result;
                        case "tanh":
                            result = Math.Tanh(Parse(expressionInBracket));
                            return result;
                        case "sign":
                            result = Math.Sign(Parse(expressionInBracket));
                            return result;
                    }
                }
            }
        }
        pos = 0; int level = 6; bracketLevel = 0;
        for (int i = Expression.Length - 1; i > -1; --i)
        {
            symbol = Expression[i];
            switch (symbol)
            {
                case '(': ++bracketLevel; break;
                case ')': --bracketLevel; break;
                case '+': if (bracketLevel == 0 && level > 0) { pos = i; level = 0; } break;
                case '-': if (bracketLevel == 0 && level > 1) { pos = i; level = 1; } break;
                case '*': if (bracketLevel == 0 && level > 2) { pos = i; level = 2; } break;
                case '%': if (bracketLevel == 0 && level > 3) { pos = i; level = 3; } break;
                case '/': if (bracketLevel == 0 && level > 4) { pos = i; level = 4; } break;
                case '^': if (bracketLevel == 0 && level > 5) { pos = i; level = 5; } break;
            }
        }
        string leftExpression, rightExpression;
        if (pos == 0 || pos == Expression.Length - 1)
        {
            if (Expression[0] >= '0' && Expression[0] <= '9')
            {
                result = double.Parse(Expression.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
            }
            return result;
        }
        leftExpression = Expression.Substring(0, pos);
        rightExpression = Expression.Substring(pos + 1, Expression.Length - (pos + 1));
        symbol = Expression[pos];
        switch (symbol)
        {
            case '+':
                result = Parse(leftExpression) + Parse(rightExpression);
                return result;
            case '-':
                result = Parse(leftExpression) - Parse(rightExpression);
                return result;
            case '*':
                result = Parse(leftExpression) * Parse(rightExpression);
                return result;
            case '/':
                result = Parse(leftExpression) / Parse(rightExpression);
                return result;
            case '%':
                result = Math.IEEERemainder(Parse(leftExpression), Parse(rightExpression));
                return result;
            case '^':
                result = Math.Pow(Parse(leftExpression), Parse(rightExpression));
                return result;
        }
        return result;
    }
}