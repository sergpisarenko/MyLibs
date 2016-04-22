using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace SnowLib.Items
{
    /// <summary>
    /// Класс описывающий временной промежуток - период
    /// </summary>
    [DataContract]
    [Serializable]
    public struct DateTimePeriod : IComparable<DateTimePeriod>, IEquatable<DateTimePeriod>
    {
        private const string partDelimiter = " - ";
        [DataMember]
        private readonly DateTime begin;
        [DataMember]
        private readonly DateTime end;

        public DateTime Begin { get { return this.begin; } }
        public DateTime End { get { return this.end; } }
        public TimeSpan Duration
        {
            get { return this.end - this.begin; }
        }

        public DateTimePeriod(DateTime periodBegin, DateTime periodEnd)
        {
            if (periodBegin > periodEnd)
                throw new ArgumentException("periodBegin>periodEnd");
            this.begin = periodBegin;
            this.end = periodEnd;
        }

        public DateTimePeriod(DateTime periodBegin, TimeSpan periodDuration)
        {
            if (periodDuration.Ticks<0)
                throw new ArgumentException("periodDuration<0");
            this.begin = periodBegin;
            this.end = periodBegin.Add(periodDuration);
        }

        public int CompareTo(DateTimePeriod other)
        {
            int res = this.end.CompareTo(other.end);
            return res == 0 ? other.begin.CompareTo(this.begin) : res;
        }

        public override string ToString()
        {
            return String.Concat(this.Begin.ToString(), partDelimiter, this.End.ToString());
        }

        public string ToString(IFormatProvider provider)
        {
            return String.Concat(this.Begin.ToString(provider), partDelimiter, this.End.ToString(provider));
        }

        public string ToString(string format)
        {
            return String.Concat(this.Begin.ToString(format), partDelimiter, this.End.ToString(format));
        }

        public string ToString(string format, IFormatProvider provider)
        {
            return String.Concat(this.Begin.ToString(format, provider), partDelimiter, this.End.ToString(format, provider));
        }

        public bool Equals(DateTimePeriod other)
        {
            return this.begin == other.begin && this.end == other.end;
        }
    }
}
