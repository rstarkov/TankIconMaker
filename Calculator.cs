using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RT.Util;

namespace TankIconMaker
{
    class Calculator
    {
        protected string Description;
        protected string Input;
        protected int Pos;

        protected char? Cur { get { return Pos >= Input.Length ? null : (char?) Input[Pos]; } }

        protected void ConsumeWhitespace()
        {
            while (Pos < Input.Length && (Input[Pos] == ' ' || Input[Pos] == '\t' || Input[Pos] == '\r' || Input[Pos] == '\n'))
                Pos++;
        }

        protected Exception NewParseException(string message)
        {
            return new StyleUserError(App.Translation.Calculator.CouldNotParseExpression + ".\n\n"
                + "*{0}:* {1}\n".Fmt(EggsML.Escape(App.Translation.Calculator.ErrLabel_Error), EggsML.Escape(message))
                + "*{0}:* {1}<Red>=\"<\"{2}\">\"={3}".Fmt(
                    EggsML.Escape(App.Translation.Calculator.ErrLabel_Expression),
                    EggsML.Escape(Input.Substring(0, Pos)),
                    EggsML.Escape(App.Translation.Calculator.Err_LocationMarker),
                    EggsML.Escape(Input.Substring(Pos)))
                + Description,
                formatted: true);
        }

        public double Parse(string expression, string descriptionEggsML = null)
        {
            Description = string.IsNullOrEmpty(descriptionEggsML) ? "" : ("\n\n" + descriptionEggsML);
            Input = expression;
            Pos = 0;
            ConsumeWhitespace();
            double result = ParseExpression();
            if (Cur != null)
                throw NewParseException(App.Translation.Calculator.Err_ExpectedEndOfExpression);
            if (double.IsInfinity(result))
                throw NewParseException(App.Translation.Calculator.Err_ResultInfinite);
            if (double.IsNaN(result))
                throw NewParseException(App.Translation.Calculator.Err_ResultNaN);
            return result;
        }

        private double ParseExpression()
        {
            double left = ParseExpressionMul(); // additive expressions are left-associative, so build result as we go. First one is mandatory.
            while (true)
            {
                var op = Cur;
                if (op != '+' && op != '-')
                    return left;
                Pos++;
                ConsumeWhitespace();
                double right = ParseExpressionMul();
                if (op == '+')
                    left = left + right;
                else if (op == '-')
                    left = left - right;
                else
                    throw new Exception();
            }
        }

        private double ParseExpressionMul()
        {
            double left = ParseExpressionPwr(); // multiplicative expressions are left-associative, so build result as we go. First one is mandatory.
            while (true)
            {
                var op = Cur;
                if (op != '*' && op != '/' && op != '%')
                    return left;
                Pos++;
                ConsumeWhitespace();
                double right = ParseExpressionPwr();
                if (op == '*')
                    left = left * right;
                else if (op == '/')
                    left = left / right;
                else if (op == '%')
                    left = Math.IEEERemainder(left, right);
                else
                    throw new Exception();
            }
        }

        private double ParseExpressionPwr()
        {
            // Power expressions are right-associative, so must parse them all first
            var sequence = new List<double>();
            sequence.Add(ParseExpressionPrimary()); // the first primary is mandatory
            while (true)
            {
                if (Cur != '^')
                    break;
                Pos++;
                ConsumeWhitespace();
                sequence.Add(ParseExpressionPrimary());
            }
            sequence.Reverse();
            double result = sequence[0];
            foreach (var next in sequence.Skip(1))
                result = Math.Pow(next, result);
            return result;
        }

        private double ParseExpressionPrimary()
        {
            if (Cur == null)
                throw NewParseException(App.Translation.Calculator.Err_UnexpectedEndOfExpression);
            else if (Cur == '(') // parentheses for precedence
            {
                Pos++;
                ConsumeWhitespace();
                var result = ParseExpression();
                if (Cur != ')')
                    throw NewParseException(App.Translation.Calculator.Err_ExpectedParenthesisOrOperator);
                Pos++;
                ConsumeWhitespace();
                return result;
            }
            else if (Cur == '-') // unary minus
            {
                Pos++;
                ConsumeWhitespace();
                return -ParseExpressionPrimary();
            }
            else if (Cur == '+') // unary plus, a no-op
            {
                Pos++;
                ConsumeWhitespace();
                return ParseExpressionPrimary();
            }
            else if (Cur >= '0' && Cur <= '9') // a numeric literal
            {
                int fromPos = Pos;
                while (Cur >= '0' && Cur <= '9')
                    Pos++;
                if (Cur == '.') // fractional part
                {
                    Pos++;
                    while (Cur >= '0' && Cur <= '9')
                        Pos++;
                }
                string num = Input.Substring(fromPos, Pos - fromPos);
                double result;
                if (!double.TryParse(num, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out result))
                    throw NewParseException(App.Translation.Calculator.Err_CannotParseNumber.Fmt(num));
                ConsumeWhitespace();
                return result;
            }
            else if (char.IsLetter(Cur.Value)) // a word or a sequence of words separated by dots
            {
                int fromPos = Pos;
                while (Cur == '.' || (Cur != null && char.IsLetter(Cur.Value)))
                    Pos++;
                string word = Input.Substring(fromPos, Pos - fromPos);
                ConsumeWhitespace();
                if (Cur == '(')
                    return EvalFunction(word);
                else
                    return EvalVariable(word);
            }
            else
                throw NewParseException(App.Translation.Calculator.Err_UnexpectedCharacter.Fmt(Cur));
        }

        private List<double> ParseParameters()
        {
            if (Cur != '(')
                throw new Exception("24567837");
            Pos++;
            ConsumeWhitespace();
            var parameters = new List<double>();
            if (Cur == ')')
            {
                Pos++;
                ConsumeWhitespace();
                return parameters;
            }
            while (true)
            {
                parameters.Add(ParseExpression());
                if (Cur == ',')
                {
                    Pos++;
                    ConsumeWhitespace();
                }
                else if (Cur == ')')
                {
                    Pos++;
                    ConsumeWhitespace();
                    return parameters;
                }
                else
                    throw NewParseException(App.Translation.Calculator.Err_ExpectedCommaOrParenthesis);
            }
        }

        protected virtual double EvalVariable(string variable)
        {
            if (variable.EqualsNoCase("pi"))
                return Math.PI;
            else if (variable.EqualsNoCase("e"))
                return Math.E;
            else
                throw NewParseException(App.Translation.Calculator.Err_UnknownVariable.Fmt(variable));
        }

        protected virtual double EvalFunction(string function)
        {
            List<double> parameters = null;
            Action<int, int> parseParams = (int minRequired, int maxRequired) =>
            {
                parameters = ParseParameters();
                if (minRequired == maxRequired && minRequired != -1 && parameters.Count != minRequired)
                    throw NewParseException(App.Translation.Calculator.Err_FunctionParamCountExact.Fmt(App.Translation, function, minRequired, parameters.Count));
                else if (minRequired != -1 && parameters.Count < minRequired)
                    throw NewParseException(App.Translation.Calculator.Err_FunctionParamCountAtLeast.Fmt(App.Translation, function, minRequired, parameters.Count));
                else if (maxRequired != -1 && parameters.Count > maxRequired)
                    throw NewParseException(App.Translation.Calculator.Err_FunctionParamCountAtMost.Fmt(App.Translation, function, maxRequired, parameters.Count));
            };
            switch (function.ToLower())
            {
                case "sqrt": parseParams(1, 1); return Math.Sqrt(parameters[0]);
                case "abs": parseParams(1, 1); return Math.Abs(parameters[0]);
                case "sign": parseParams(1, 1); return Math.Sign(parameters[0]);
                case "ceil": parseParams(1, 1); return Math.Ceiling(parameters[0]);
                case "floor": parseParams(1, 1); return Math.Floor(parameters[0]);
                case "round": parseParams(1, 1); return Math.Round(parameters[0]);
                case "trunc": parseParams(1, 1); return Math.Truncate(parameters[0]);
                case "sin": parseParams(1, 1); return Math.Sin(parameters[0]);
                case "cos": parseParams(1, 1); return Math.Cos(parameters[0]);
                case "tan": parseParams(1, 1); return Math.Tan(parameters[0]);
                case "sinh": parseParams(1, 1); return Math.Sinh(parameters[0]);
                case "cosh": parseParams(1, 1); return Math.Cosh(parameters[0]);
                case "tanh": parseParams(1, 1); return Math.Tanh(parameters[0]);
                case "acos": parseParams(1, 1); return Math.Acos(parameters[0]);
                case "asin": parseParams(1, 1); return Math.Asin(parameters[0]);
                case "atan": parseParams(1, 1); return Math.Atan(parameters[0]);
                case "atan2": parseParams(2, 2); return Math.Atan2(parameters[0], parameters[1]);
                case "deg": parseParams(1, 1); return Math.PI * parameters[0] / 180.0;
                case "exp": parseParams(1, 1); return Math.Exp(parameters[0]);
                case "log10": parseParams(1, 1); return Math.Log10(parameters[0]);
                case "log2": parseParams(1, 1); return Math.Log(parameters[0], 2);
                case "ln": parseParams(1, 1); return Math.Log(parameters[0]);
                case "log":
                    parseParams(1, 2);
                    if (parameters.Count == 1)
                        return Math.Log(parameters[0]);
                    else if (parameters.Count == 2)
                        return Math.Log(parameters[1], parameters[0]); // base first, number second, like in the actual notation
                    else
                        throw new Exception("3135623"); // not reachable
                case "min": parseParams(1, -1); return parameters.Min();
                case "max": parseParams(1, -1); return parameters.Max();
                default:
                    throw NewParseException(App.Translation.Calculator.Err_UnknownFunction.Fmt(function));
            }
        }
    }
}