using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;

namespace SnowLib.Scripting
{
    #region Исключения
    public class SimpleExpressionException : Exception
    {
        public int Position { get; protected set; }
        public int Length { get; protected set; }
        public SimpleExpressionException(string message, int position)
            : base(message)
        {
            this.Position = position;
            this.Length = 1;
        }
        public SimpleExpressionException(string message, int position, int length)
            : base(message)
        {
            this.Position = position;
            this.Length = Math.Max(length, 1);
        }
    }
    #endregion

    #region Основные лексемы
    internal enum SimpleExpressionToken : int
    {
        Begin = 0,  // начало формулы
        End,        // конец формулы
        Name,       // имя (переменной, функции)
        Float,      // floating-point number ordinal
        Double,     // floating-point number double
        Decimal,    // fixed point number
        Int,        // integer number
        Bool,       // logical
        Plus,       // + 
        Minus,      // - 
        Mul,        // *
        Div,        // /
        LP,         // (
        RP,         // )
        LSB,        // [
        RSB,        // ]
        LFB,        // {
        RFB,        // }
        Semi,       // ;
        Colon,      // :
        Comma,      // ,
        Point,      // .
        Equal,      // =
        Less,       // <
        More,       // >
        LessOrEqual,// <=
        MoreOrEqual,// >=
        NotEqual,   // <>
        LeftShift,  // <<
        RightShift, // >>
        BitOr,      // |
        BitAnd,     // &
        BitXor,     // ^
        BitNot,     // ~
        CondOr,     // ||
        CondAnd,    // &&
        CondNot,    // ?
        Text        // текстовая константа в ""
    }
    #endregion

    #region Класс для разбора формулы на лексемы
    internal class SimpleExpressionTokenizer
    {
        #region Закрытые поля
        // разбираемая формула
        private string formula;
        // текущая позиция
        private int pos;
        // значение, если лексема - целое число
        private int currentTokenIntValue;
        // значение, если лексема - число с плавающей точкой
        private float currentTokenFloatValue;
        private double currentTokenDoubleValue;
        private decimal currentTokenDecimalValue;
        private bool currentTokenBoolValue;
        // значение, если лексема - имя
        private string currentTokenNameValue;
        // значение, если лексема - текст
        private string currentTokenTextValue;
        #endregion

        #region Открытые поля
        // разбираемая формула
        public string Formula
        {
            get { return this.formula; }
            set
            {
                this.formula = (value == null) ? String.Empty : value;
                this.Pos = 0;
                clearValues();
                this.CurrentToken = SimpleExpressionToken.Begin;
            }
        }
        // текущая позиция
        public int Pos
        {
            get
            {
                return this.pos;
            }
            set
            {
                this.Start = this.pos;
                this.pos = value;
            }
        }
        // предыдущая позиция
        public int Start
        {
            get;
            protected set;
        }
        // длина лексемы
        public int Length { get { return this.Pos - this.Start; } }

        // текущая лексема
        public SimpleExpressionToken CurrentToken
        {
            get;
            protected set;
        }

        // значение, если лексема - целое число
        public int CurrentTokenIntValue
        {
            get
            {
                if (this.CurrentToken != SimpleExpressionToken.Int)
                    throw new InvalidOperationException("Целое числовое значение только для Token.Int");
                return this.currentTokenIntValue;
            }
        }

        // значение, если лексема - число с плавающей точкой
        public float CurrentTokenFloatValue
        {
            get
            {
                if (this.CurrentToken != SimpleExpressionToken.Float)
                    throw new InvalidOperationException("Числовое значение только для Token.Float");
                return this.currentTokenFloatValue;
            }
        }

        // значение, если лексема - число с плавающей точкой
        public double CurrentTokenDoubleValue
        {
            get
            {
                if (this.CurrentToken != SimpleExpressionToken.Double)
                    throw new InvalidOperationException("Числовое значение только для Token.Double");
                return this.currentTokenDoubleValue;
            }
        }

        // значение, если лексема - число с фиксированной точкой
        public decimal CurrentTokenDecimalValue
        {
            get
            {
                if (this.CurrentToken != SimpleExpressionToken.Decimal)
                    throw new InvalidOperationException("Числовое значение только для Token.Decimal");
                return this.currentTokenDecimalValue;
            }
        }

        // значение, если лексема - логическая
        public bool CurrentTokenBoolValue
        {
            get
            {
                if (this.CurrentToken != SimpleExpressionToken.Bool)
                    throw new InvalidOperationException("Числовое значение только для Token.Bool");
                return this.currentTokenBoolValue;
            }
        }

        // значение, если лексема - имя
        public string CurrentTokenNameValue
        {
            get
            {
                if (this.CurrentToken != SimpleExpressionToken.Name)
                    throw new InvalidOperationException("Значение имени только для Token.Name");
                return this.currentTokenNameValue;
            }
        }

        // значение, если лексема - текст
        public string CurrentTokenTextValue
        {
            get
            {
                if (this.CurrentToken != SimpleExpressionToken.Text)
                    throw new InvalidOperationException("Значение текста только для Token.Text");
                return this.currentTokenTextValue;
            }
        }
        #endregion

        #region Конструкторы и иницилизация
        public SimpleExpressionTokenizer() : this(null) { }

        public SimpleExpressionTokenizer(string initFormula)
        {
            this.Formula = initFormula;
        }

        private void clearValues()
        {
            this.currentTokenFloatValue = 0.0f;
            this.currentTokenDoubleValue = 0.0;
            this.currentTokenDecimalValue = 0.0m;
            this.currentTokenIntValue = 0;
            this.currentTokenBoolValue = false;
            this.currentTokenNameValue = String.Empty;
            this.currentTokenTextValue = String.Empty;
        }
        #endregion

        #region Открытые методы
        // получить текущую лексему
        public SimpleExpressionToken GetToken()
        {
            clearValues();
            // пропускаем пробелы
            while (this.Pos < this.formula.Length && char.IsWhiteSpace(this.formula[this.Pos]))
                this.Pos++;
            if (this.Pos < this.formula.Length)
            {
                char cs = this.formula[this.Pos];
                switch (cs)
                {
                    case '+':
                        this.CurrentToken = SimpleExpressionToken.Plus;
                        this.Pos++;
                        break;
                    case '-':
                        this.CurrentToken = SimpleExpressionToken.Minus;
                        this.Pos++;
                        break;
                    case '^':
                        this.CurrentToken = SimpleExpressionToken.BitXor;
                        this.Pos++;
                        break;
                    case '~':
                        this.CurrentToken = SimpleExpressionToken.BitNot;
                        this.Pos++;
                        break;
                    case '!':
                        this.CurrentToken = SimpleExpressionToken.CondNot;
                        this.Pos++;
                        break;
                    case '*':
                        this.CurrentToken = SimpleExpressionToken.Mul;
                        this.Pos++;
                        break;
                    case '/':
                        this.CurrentToken = SimpleExpressionToken.Div;
                        this.Pos++;
                        break;
                    case '(':
                        this.CurrentToken = SimpleExpressionToken.LP;
                        this.Pos++;
                        break;
                    case ')':
                        this.CurrentToken = SimpleExpressionToken.RP;
                        this.Pos++;
                        break;
                    case '[':
                        this.CurrentToken = SimpleExpressionToken.LSB;
                        this.Pos++;
                        break;
                    case ']':
                        this.CurrentToken = SimpleExpressionToken.RSB;
                        this.Pos++;
                        break;
                    case '{':
                        this.CurrentToken = SimpleExpressionToken.LFB;
                        this.Pos++;
                        break;
                    case '}':
                        this.CurrentToken = SimpleExpressionToken.RFB;
                        this.Pos++;
                        break;
                    case ';':
                        this.CurrentToken = SimpleExpressionToken.Semi;
                        this.Pos++;
                        break;
                    case ':':
                        this.CurrentToken = SimpleExpressionToken.Colon;
                        this.Pos++;
                        break;
                    case ',':
                        this.CurrentToken = SimpleExpressionToken.Comma;
                        this.Pos++;
                        break;
                    case '.':
                        this.CurrentToken = SimpleExpressionToken.Point;
                        this.Pos++;
                        break;
                    default:
                        if (this.GetNumber())
                            break;
                        if (this.GetName())
                            break;
                        if (this.GetCompareShift())
                            break;
                        if (this.GetLogical())
                            break;
                        if (this.GetText())
                            break;
                        throw new SimpleExpressionException("Недопустимый символ.", this.Pos);
                }
            }
            else
            {
                this.CurrentToken = SimpleExpressionToken.End;
            }
            return this.CurrentToken;
        }

        // вспомогательная функция получения числа
        private bool GetNumber()
        {
            bool res = false;
            int tpos = this.Pos;
            while (tpos < this.formula.Length && char.IsDigit(this.formula[tpos])) tpos++;
            bool isPower = false;
            if (tpos < this.formula.Length && (this.formula[tpos] == '.' || (isPower = Char.ToLower(this.formula[tpos])=='e')))
            {
                // число с плавающей точкой
                tpos++;
                while (tpos < this.formula.Length && char.IsDigit(this.formula[tpos])) tpos++;
                if (tpos < this.formula.Length && !isPower && Char.ToLower(this.formula[tpos]) == 'e')
                {
                    tpos++;
                    while (tpos < this.formula.Length && char.IsDigit(this.formula[tpos])) tpos++;
                }
                try
                {
                    char postfix = ' ';
                    if (tpos > this.formula.Length)
                        postfix = this.formula[tpos];
                    switch(postfix)
                    {
                        case 'm':
                            this.currentTokenDecimalValue =
                                decimal.Parse(this.formula.Substring(this.Pos, tpos - this.Pos), System.Globalization.CultureInfo.InvariantCulture);
                            this.CurrentToken = SimpleExpressionToken.Decimal;
                            tpos++;
                            break;
                        case 'f':
                            this.currentTokenFloatValue =
                                float.Parse(this.formula.Substring(this.Pos, tpos - this.Pos), System.Globalization.CultureInfo.InvariantCulture);
                            this.CurrentToken = SimpleExpressionToken.Float;
                            tpos++;
                            break;
                        default:
                            this.currentTokenDoubleValue =
                                double.Parse(this.formula.Substring(this.Pos, tpos - this.Pos), System.Globalization.CultureInfo.InvariantCulture);
                            this.CurrentToken = SimpleExpressionToken.Double;
                            break;
                    }
                    this.Pos = tpos;
                    res = true;
                }
                catch (FormatException)
                {
                    throw new SimpleExpressionException("Неверный формат числа.", this.Pos, tpos - this.Pos);
                }
                catch (OverflowException)
                {
                    throw new SimpleExpressionException("Число слишком велико или слишком мало.", this.Pos, tpos - this.Pos);
                }
            }
            else
            {
                if (tpos > this.Pos)
                {
                    // целое число
                    try
                    {
                        this.currentTokenIntValue =
                            int.Parse(this.formula.Substring(this.Pos, tpos - this.Pos));
                        this.CurrentToken = SimpleExpressionToken.Int;
                        this.Pos = tpos;
                        res = true;
                    }
                    catch (FormatException)
                    {
                        throw new SimpleExpressionException("Неверный формат числа.", this.Pos, tpos - this.Pos);
                    }
                    catch (OverflowException)
                    {
                        throw new SimpleExpressionException("Число слишком велико или слишком мало.", this.Pos, tpos - this.Pos);
                    }
                    res = true;
                }
            }
            return res;
        }

        // вспомогательная функция получения имени
        private bool GetName()
        {
            int tpos = this.Pos;
            if (tpos < this.formula.Length && (Char.IsLetter(this.formula[tpos]) || this.formula[tpos] == '_'))
            {
                tpos++;
                while (tpos < this.formula.Length && (Char.IsLetterOrDigit(this.formula[tpos])
                    || this.formula[tpos] == '_' || this.formula[tpos] == '.')) tpos++;
                string name = this.formula.Substring(this.Pos, tpos - this.Pos);
                if (name.EndsWith("."))
                    throw new SimpleExpressionException("Наименование не должно закачиваться на \".\"", this.Pos, tpos - this.Pos);
                bool bres;
                if (bool.TryParse(name, out bres))
                {
                    this.CurrentToken = SimpleExpressionToken.Bool;
                    this.currentTokenBoolValue = bres;
                }
                else
                {
                    this.currentTokenNameValue = name;
                    this.CurrentToken = SimpleExpressionToken.Name;
                }
                this.Pos = tpos;
                return true;
            }
            else
                return false;
        }

        // вспомогательная функция получения знака сравнения чисел
        private bool GetCompareShift()
        {
            char[] compareSymbols = new char[] { '=', '<', '>' };
            string compareSeq = null;
            int tpos = this.Pos;
            if (tpos < this.formula.Length && 
                (this.formula[tpos]=='=' || this.formula[tpos]=='<' || this.formula[tpos]=='>'))
            {
                compareSeq += this.formula[tpos++];
                if (tpos < this.formula.Length && 
                    (this.formula[tpos]=='=' || this.formula[tpos]=='<' || this.formula[tpos]=='>'))
                    compareSeq += this.formula[tpos++];
                switch (compareSeq)
                {
                    case "=":
                        this.CurrentToken = SimpleExpressionToken.Equal;
                        break;
                    case "<":
                        this.CurrentToken = SimpleExpressionToken.Less;
                        break;
                    case ">":
                        this.CurrentToken = SimpleExpressionToken.More;
                        break;
                    case "<=":
                        this.CurrentToken = SimpleExpressionToken.LessOrEqual;
                        break;
                    case ">=":
                        this.CurrentToken = SimpleExpressionToken.MoreOrEqual;
                        break;
                    case "!=":
                        this.CurrentToken = SimpleExpressionToken.NotEqual;
                        break;
                    case ">>":
                        this.CurrentToken = SimpleExpressionToken.RightShift;
                        break;
                    case "<<":
                        this.CurrentToken = SimpleExpressionToken.LeftShift;
                        break;
                    default:
                        throw new SimpleExpressionException("Неверный формат символов сравнения, допускаются только комбинации: =, <, >, <=, >= и <>.",
                            this.Pos, tpos - this.Pos);
                }
                this.Pos = tpos;
                return true;
            }
            else
                return false;
        }

        private bool GetLogical()
        {
            if (this.Pos >= this.formula.Length)
                return false;
            int tpos = this.Pos;
            if (this.formula[tpos] == '|')
            {
                tpos++;
                if (tpos < this.formula.Length && this.formula[tpos] == '|')
                {
                    this.CurrentToken = SimpleExpressionToken.CondOr;
                    tpos++;
                }
                else
                    this.CurrentToken = SimpleExpressionToken.BitOr;
            }
            else if (this.formula[tpos] == '&')
            {
                tpos++;
                if (tpos < this.formula.Length && this.formula[tpos] == '&')
                {
                    this.CurrentToken = SimpleExpressionToken.CondAnd;
                    tpos++;
                }
                else
                    this.CurrentToken = SimpleExpressionToken.BitAnd;
            }
            else
                return false;
            this.Pos = tpos;
            return true;
        }

        private bool GetText()
        {
            int tpos = this.Pos;
            if (tpos < this.formula.Length && this.formula[tpos] == '"')
            {
                tpos++;
                if (tpos >= this.formula.Length)
                    throw new SimpleExpressionException("Текстовая константа или формула не завершены.", tpos);
                StringBuilder sb = new StringBuilder();
                bool screen = false;
                bool quote = false;
                while (quote = this.formula[tpos] != '"' || screen)
                {
                    if (screen && quote)
                        sb[sb.Length - 1] = '"';
                    else
                        sb.Append(this.formula[tpos]);
                    screen = this.formula[tpos] == '\\';
                    if (++tpos >= this.formula.Length)
                        throw new SimpleExpressionException("Обнаружен конец формулы, но не обнаружено завершающих кавычек для текстовой константы",
                            this.Pos, tpos - this.Pos);
                }
                this.currentTokenTextValue = sb.ToString();
                this.CurrentToken = SimpleExpressionToken.Text;
                this.Pos = ++tpos;
                return true;
            }
            else
                return false;
        }
        #endregion
    }
    #endregion

    #region Парсер выражений
    public class SimpleExpressionParser
    {
        #region Открытые методы и свойства
        // состояние разбора формулы
        public bool IsParsing { get { return this.tokz.CurrentToken != SimpleExpressionToken.End; } }
        // связанный объект
        public object Tag { get; set; }
        // положение парсера
        public int CurTokStart { get { return this.tokz.Start; } }
        // длина лексемы
        public int CurTokLength { get { return this.tokz.Length; } }
        #endregion

        #region Private classes
        private struct MethodDesc
        {
            public readonly object Instance;
            public readonly MethodInfo Info;
            public readonly Type[] Parameters;

            public MethodDesc(object instance, MethodInfo info)
            {
                this.Instance = instance;
                this.Info = info;
                this.Parameters = this.Info.GetParameters().Select(m => m.ParameterType).ToArray();
            }

            public MethodDesc(MethodInfo info) : this(null, info)
            {
            }
        }
        #endregion

        #region Закрытые поля
        // разборщик формул
        private SimpleExpressionTokenizer tokz;
        #endregion

        #region Конструкторы и иницилизация
        public SimpleExpressionParser()
            : this(null)
        {
        }

        public SimpleExpressionParser(SimpleExpressionParser parser)
        {
            if (parser != null)
            {
               // this.ExternalFunction = parser.ExternalFunction;
            }
            this.tokz = new SimpleExpressionTokenizer();
            
            /*this.extFunc = new Dictionary<string, List<MethodDesc>>(100);
            foreach (MethodInfo mi in typeof(System.Math).GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                List<MethodDesc> list;
                if (!this.extFunc.TryGetValue(mi.Name, out list))
                {
                    list = new List<MethodDesc>(3);
                    this.extFunc.Add(mi.Name, list);
                }
                list.Add(new MethodDesc(mi));
            }*/
        }
        #endregion

        #region Основные методы
        public Expression GetExpression(string formula)
        {
            this.tokz.Formula = formula;
            return Expr(true, false);
        }
        #endregion

        #region Разбор и вычисление основных компонент формулы (+/-/*//)
        private Expression Expr(bool get, bool part)
        {
            Expression left = CondAnd(get);
            for (; ; )
            {
                switch (this.tokz.CurrentToken)
                {
                    case SimpleExpressionToken.CondOr:
                        left = Expression.Or(left, CondAnd(true));
                        break;
                    case SimpleExpressionToken.End:
                        return left;
                    default:
                        if (part)
                            return left;
                        else
                            throw new SimpleExpressionException("Ожидался || или конец формулы", this.CurTokStart);
                }
            }
        }

        private Expression CondAnd(bool get)
        {
            Expression left = BitOr(get);
            for (; ; )
            {
                if (this.tokz.CurrentToken == SimpleExpressionToken.CondAnd)
                    left = Expression.And(left, BitOr(true));
                else
                    return left;
            }
        }

        private Expression BitOr(bool get)
        {
            Expression left = BitXor(get);
            for (; ; )
            {
                if (this.tokz.CurrentToken == SimpleExpressionToken.BitOr)
                    left = Expression.Or(left, BitXor(true));
                else
                    return left;
            }
        }

        private Expression BitXor(bool get)
        {
            Expression left = BitAnd(get);
            for (; ; )
            {
                if (this.tokz.CurrentToken == SimpleExpressionToken.BitXor)
                    left = Expression.ExclusiveOr(left, BitAnd(true));
                else
                    return left;
            }
        }

        private Expression BitAnd(bool get)
        {
            Expression left = Equality(get);
            for (; ; )
            {
                if (this.tokz.CurrentToken == SimpleExpressionToken.BitAnd)
                    left = Expression.And(left, Equality(true));
                else
                    return left;
            }
        }

        private Expression Equality(bool get)
        {
            Expression left = Inequality(get);
            for (; ; )
            {
                switch (this.tokz.CurrentToken)
                {
                    case SimpleExpressionToken.Equal:
                        left = Expression.Equal(left, Inequality(true));
                        break;
                    case SimpleExpressionToken.NotEqual:
                        left = Expression.NotEqual(left, Inequality(true));
                        break;
                    default:
                        return left;
                }
            }
        }


        private Expression Inequality(bool get)
        {
            Expression left = Shift(get);
            for (; ; )
            {
                switch (this.tokz.CurrentToken)
                {
                    case SimpleExpressionToken.Less:
                        left = Expression.LessThan(left, Shift(true));
                        break;
                    case SimpleExpressionToken.LessOrEqual:
                        left = Expression.LessThanOrEqual(left, Shift(true));
                        break;
                    case SimpleExpressionToken.More:
                        left = Expression.GreaterThan(left, Shift(true));
                        break;
                    case SimpleExpressionToken.MoreOrEqual:
                        left = Expression.GreaterThanOrEqual(left, Shift(true));
                        break;
                    default:
                        return left;
                }
            }
        }

        private Expression Shift(bool get)
        {
            Expression left = AddSub(get);
            for (; ; )
            {
                switch (this.tokz.CurrentToken)
                {
                    case SimpleExpressionToken.LeftShift:
                        left = Expression.LeftShift(left, AddSub(true));
                        break;
                    case SimpleExpressionToken.RightShift:
                        left = Expression.RightShift(left, AddSub(true));
                        break;
                    default:
                        return left;
                }
            }
        }

        private Expression AddSub(bool get)
        {
            Expression left = MulDiv(get);
            for (; ; )
            {
                switch (this.tokz.CurrentToken)
                {
                    case SimpleExpressionToken.Plus:
                        left = Expression.Add(left, MulDiv(true));
                        break;
                    case SimpleExpressionToken.Minus:
                        left = Expression.Subtract(left, MulDiv(true));
                        break;
                    default:
                        return left;
                }
            }
        }

        // вычисляет * и /
        private Expression MulDiv(bool get)
        {
            Expression left = CallMethod(get);
            for (; ; )
            {
                switch (this.tokz.CurrentToken)
                {
                    case SimpleExpressionToken.Mul:
                        left = Expression.Multiply(left, CallMethod(true));
                        break;
                    case SimpleExpressionToken.Div:
                        left = Expression.Divide(left, CallMethod(true));
                        break;

                    default:
                        return left;
                }
            }
        }

        // вызывает методы
        private Expression CallMethod(bool get)
        {
            return getCallSequence(Prim(get));
        }


        // первичные выражения
        private Expression Prim(bool get)
        {
            Expression v;
            if (get)
                this.tokz.GetToken();
            switch (this.tokz.CurrentToken)
            {
                case SimpleExpressionToken.LP: // выражение в скобках
                    v = Expr(true, true);
                    if (this.tokz.CurrentToken != SimpleExpressionToken.RP)
                        throw new SimpleExpressionException("Ожидались закрывающие круглые скобки.", this.CurTokStart);
                    this.tokz.GetToken();
                    return getCallSequence(v);

                case SimpleExpressionToken.CondNot: // cond not
                case SimpleExpressionToken.BitNot: // bitset not
                    return Expression.Not(Prim(true));


                case SimpleExpressionToken.Minus: // унарный минус
                    return Expression.Negate(Prim(true));

                case SimpleExpressionToken.Float:
                    v = Expression.Constant(this.tokz.CurrentTokenFloatValue);
                    this.tokz.GetToken();
                    return v;

                case SimpleExpressionToken.Double:
                    v = Expression.Constant(this.tokz.CurrentTokenDoubleValue);
                    this.tokz.GetToken();
                    return v;

                case SimpleExpressionToken.Decimal:
                    v = Expression.Constant(this.tokz.CurrentTokenDecimalValue);
                    this.tokz.GetToken();
                    return v;

                case SimpleExpressionToken.Int:
                    v = Expression.Constant(this.tokz.CurrentTokenIntValue);
                    this.tokz.GetToken();
                    return v;

                case SimpleExpressionToken.Bool:
                    v = Expression.Constant(this.tokz.CurrentTokenBoolValue);
                    this.tokz.GetToken();
                    return v;

                case SimpleExpressionToken.Name: // имя
                    v = GetNameValue(this.tokz.CurrentTokenNameValue, null);
                    return v;

                case SimpleExpressionToken.Text: // текстовая константа
                    v = Expression.Constant(this.tokz.CurrentTokenTextValue);
                    this.tokz.GetToken();
                    return v;


                default:
                    throw new SimpleExpressionException("Ожидалось первичное выражение (выражение в скобках, унарный минус, число или функция).", this.tokz.Start);
            }
        }
        #endregion

        #region Работа с функциями и константами
        private Expression getCallSequence(Expression expr)
        {
            while (this.tokz.CurrentToken == SimpleExpressionToken.Point)
            {
                if (this.tokz.GetToken() != SimpleExpressionToken.Name)
                    throw new SimpleExpressionException("Ожидалось имя метода или свойства", this.CurTokStart, this.CurTokLength);
                expr = GetNameValue(this.tokz.CurrentTokenNameValue, expr);
            }
            return expr;
        }

        // получение значения по имени
        private Expression GetNameValue(string name, Expression expr)
        {
            SimpleExpressionToken next = this.tokz.GetToken();
            switch(next)
            {
                case SimpleExpressionToken.LP: // function
                    return GetFunctionValue(name, expr);







            }
            return null;
        }

        private Expression GetFunctionValue(string name, Expression expr)
        {
            Type type;
            string methodName;
            int index = name.LastIndexOf('.');
            if (index > 0)
            {
                if (expr != null)
                    throw new SimpleExpressionException("Неверное имя метода name", this.CurTokStart);
                string typeName = name.Substring(0, index);
                type = Type.GetType(typeName);
                if (type == null)
                    throw new SimpleExpressionException("Неизвестное имя типа \"" + typeName + "\"", this.CurTokStart);
                methodName = name.Substring(index + 1, name.Length - index - 1);
            }
            else
            {
                if (expr != null)
                    type = expr.Type;
                else
                    type = null;
                methodName = name;
            }
            List<Expression> callParameters = new List<Expression>(3);
            bool getToken = false;
            this.tokz.GetToken();
            while (this.tokz.CurrentToken != SimpleExpressionToken.RP)
            {
                callParameters.Add(this.Expr(getToken, true));
                if (this.tokz.CurrentToken != SimpleExpressionToken.RP && this.tokz.CurrentToken != SimpleExpressionToken.Comma)
                    throw new SimpleExpressionException("Ожидалась закрывающая скобка для завершения параметров или запятая для их продолжения", this.CurTokStart);                    
                getToken = true;
            }
            Type[] callTypes = callParameters.Select( m => m.Type ).ToArray();
            MethodInfo methodInfo = type.GetMethod(methodName, callTypes);
            if (methodInfo == null)
                throw new SimpleExpressionException("Не найден подходящий метод \""+name+"\" с параметрами: "+
                    String.Join(", ",  (object[])callTypes), this.CurTokStart);
            this.tokz.GetToken();
            return Expression.Call(expr, methodInfo, callParameters);
        }
        #endregion
    }
    #endregion
}
