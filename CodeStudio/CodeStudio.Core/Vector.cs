using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Metrino.Development.Core
{
    [DataContract]
    public class F64Array : IEnumerable<double>
    {
        [DataMember]
        public double[] ArrayData { get; set; }

        public F64Array(IEnumerable<double> _values)
        {
            ArrayData = _values.ToArray();
        }

        public F64Array(int size)
        {
            ArrayData = new double[size];
        }

        public F64Array(int size, double value)
        {
            ArrayData = new double[size];
            Fill(value);
        }

        IEnumerator<double> IEnumerable<double>.GetEnumerator()
        {
            return ArrayData.GetEnumerator() as IEnumerator<double>;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ArrayData.GetEnumerator();
        }

        public static F64Array ARange(int begin, int end)
        {
            return new F64Array(Enumerable.Range(begin, end - begin).Select(x => Convert.ToDouble(x)));
        }

        public static F64Array operator -(F64Array c1)
        {
            return new F64Array(c1.Select(x => -x));
        }

        public static F64Array operator -(F64Array c1, F64Array c2)
        {
            return new F64Array(c1.Zip(c2, (x, y) => x - y));
        }

        public static F64Array operator -(F64Array c1, double c2)
        {
            return new F64Array(c1.Select(x => x - c2));
        }

        public static F64Array operator -(double c1, F64Array c2)
        {
            return new F64Array(c2.Select(x => c1 - x));
        }

        public static F64Array operator +(F64Array c1)
        {
            return c1;
        }

        public static F64Array operator +(F64Array c1, F64Array c2)
        {
            return new F64Array(c1.Zip(c2, (x, y) => x + y));
        }

        public static F64Array operator +(F64Array c1, double c2)
        {
            return new F64Array(c1.Select(x => x + c2));
        }

        public static F64Array operator +(double c1, F64Array c2)
        {
            return new F64Array(c2.Select(x => c1 * x));
        }

        public static F64Array operator *(F64Array c1, F64Array c2)
        {
            return new F64Array(c1.Zip(c2, (x, y) => x * y));
        }

        public static F64Array operator *(F64Array c1, double c2)
        {
            return new F64Array(c1.Select(x => x * c2));
        }

        public static F64Array operator *(double c1, F64Array c2)
        {
            return new F64Array(c2.Select(x => c1 * x));
        }

        public F64Array FillNA()
        {
            return Fill(double.NaN);
        }

        public double this[int i]
        {
            get { return ArrayData[i]; }
            set { ArrayData[i] = value; }
        }

        public F64Array Fill(double value)
        {
            for (int i = 0; i < ArrayData.Length; i++)
                ArrayData[i] = value;

            return this;
        }

        public F64Array LeftShift(int shift)
        {
            var newArray = new F64Array(Length);
            shift = shift % ArrayData.Length;

            Array.Copy(ArrayData, shift, newArray.ArrayData, 0, ArrayData.Length - shift);

            return newArray;
        }

        public F64Array RightShift(int shift)
        {
            shift = shift % ArrayData.Length;

            var newArray = new F64Array(Length);

            Array.Copy(ArrayData, 0, newArray.ArrayData, shift, ArrayData.Length - shift);
            
            return newArray;
        }

        public F64Array Shift(int shift)
        {
            if(shift > 0)
                return RightShift(shift);
                
            if(shift < 0)
                return LeftShift(-shift);

            return this;
        }

        public double[] ToArray()
        {
            return ArrayData;
        }

        public int Length => ArrayData.Length;
    }
}