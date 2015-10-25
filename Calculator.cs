using System;
using System.Collections.Generic;

namespace TankIconMaker
{
    class Calculator
    {
        public readonly string[] ReservedVariables = new string[] { "PI", "e" };
        public readonly string[] Functions = new string[]{"sqrt","sin","cos","tan","log","ln","abs","exp","deg","log10",
		"acos","asin","atan","ceil","floor","round","trunc","sihn","cosh","tanh","sign"};
        protected Dictionary<string, double> Variables = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        public readonly string[] Operators = new string[] { "(", ")", "+", "-", "*", "%", "/", "^" };

        public Calculator()
        {
            Variables.Add("PI", Math.PI);
            Variables.Add("e", Math.E);
        }

        public virtual bool GetVariable(string varName, out double value)
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

        private int GetClosingBracket(string expression, int start, ref int bracketLevel)
        {
            int Position = start;
            for (int i = start; i < expression.Length; ++i)
            {
                switch (expression[i])
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
        /// <param name="expression">Expression to evaluate</param>
        public double Parse(string expression)
        {
            expression = expression.Replace(" ", "");
            int bracketLevel = 0;
            char symbol = expression[0];
            if (symbol == '(' &&
                GetClosingBracket(expression, 0, ref bracketLevel) == expression.Length - 1)
                expression = expression.Substring(1, expression.Length - 2);

            double varValue = 0;
            int pos;
            if (GetVariable(expression, out varValue))
                return varValue;
            string expressionInBracket;
            if (expression.Length > 4)
            {
                int pos2 = expression.IndexOf('(');
                if (pos2 != -1 && pos2 <= 5)
                {
                    pos = GetClosingBracket(expression, pos2, ref bracketLevel);
                    if (pos == expression.Length - 1)
                    {
                        expressionInBracket = expression.Substring(pos2 + 1, pos - pos2 - 1);
                        string function = expression.Substring(0, pos2);
                        switch (function)
                        {
                            case "sqrt":
                                return Math.Sqrt(Parse(expressionInBracket));
                            case "sin":
                                return Math.Sin(Parse(expressionInBracket));
                            case "cos":
                                return Math.Cos(Parse(expressionInBracket));
                            case "tan":
                                return Math.Tan(Parse(expressionInBracket));
                            case "log":
                            case "ln":
                                return Math.Log(Parse(expressionInBracket));
                            case "abs":
                                return Math.Abs(Parse(expressionInBracket));
                            case "exp":
                                return Math.Exp(Parse(expressionInBracket));
                            case "deg":
                                return Math.PI * Parse(expressionInBracket) / 180.0;
                            case "log10":
                                return Math.Log10(Parse(expressionInBracket));
                            case "acos":
                                return Math.Acos(Parse(expressionInBracket));
                            case "asin":
                                return Math.Asin(Parse(expressionInBracket));
                            case "atan":
                                return Math.Atan(Parse(expressionInBracket));
                            case "ceil":
                                return Math.Ceiling(Parse(expressionInBracket));
                            case "floor":
                                return Math.Floor(Parse(expressionInBracket));
                            case "round":
                                return Math.Round(Parse(expressionInBracket));
                            case "trunc":
                                return Math.Truncate(Parse(expressionInBracket));
                            case "sinh":
                                return Math.Sinh(Parse(expressionInBracket));
                            case "cosh":
                                return Math.Cosh(Parse(expressionInBracket));
                            case "tanh":
                                return Math.Tanh(Parse(expressionInBracket));
                            case "sign":
                                return Math.Sign(Parse(expressionInBracket));
                        }
                    }
                }
            }
            pos = 0; int level = 6; bracketLevel = 0;
            for (int i = expression.Length - 1; i > -1; --i)
            {
                symbol = expression[i];
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
            if (pos == 0 || pos == expression.Length - 1)
            {
                if (expression[0] >= '0' && expression[0] <= '9')
                    return double.Parse(expression.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
                throw new Exception("Parse expression: unexpected character in " + expression);
            }
            leftExpression = expression.Substring(0, pos);
            rightExpression = expression.Substring(pos + 1, expression.Length - (pos + 1));
            symbol = expression[pos];
            switch (symbol)
            {
                case '+':
                    return Parse(leftExpression) + Parse(rightExpression);
                case '-':
                    return Parse(leftExpression) - Parse(rightExpression);
                case '*':
                    return Parse(leftExpression) * Parse(rightExpression);
                case '/':
                    return Parse(leftExpression) / Parse(rightExpression);
                case '%':
                    return Math.IEEERemainder(Parse(leftExpression), Parse(rightExpression));
                case '^':
                    return Math.Pow(Parse(leftExpression), Parse(rightExpression));
                default:
                    throw new Exception("Parse expression: unexpected operator at pos " + pos + " in " + expression);
            }
        }
    }
}