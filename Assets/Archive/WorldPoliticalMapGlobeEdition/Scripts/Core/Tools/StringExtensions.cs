using UnityEngine;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

namespace WPM {

    public struct StringSpan {
        public int start;
        public int length;
    }

    public static class StringTools {

        //public static int Split(this string input, char separator, ref int[] splitIndices) {
        //    return input.Split(separator, ref splitIndices, 0, input.Length);
        //}

        //public static int Split(this string input, char separator, ref int[] splitIndices, int start, int end) {

        //    if (splitIndices == null || splitIndices.Length < 4) {
        //        splitIndices = new int[4];
        //    }
        //    int count = 0;
        //    splitIndices[count++] = start;

        //    int index = start;
        //    while (index < end) {
        //        index = input.IndexOf(separator, index);
        //        if (index < 0 || index > end) {
        //            index = end;
        //        }
        //        if (count == splitIndices.Length) {
        //            System.Array.Resize(ref splitIndices, count * 2);
        //        }
        //        index++;
        //        splitIndices[count++] = index;
        //    }
        //    return count - 1;
        //}

        public static string Substring(this string input, StringSpan span) {
            return input.Substring(span.start, span.length);
        }

        public static IEnumerable<StringSpan> Split(this string input, char separator, int start, int length) {
            if (string.IsNullOrEmpty(input))
                yield break;
            int end = start + length;
            int index = start;
            int prev = start;
            StringSpan seg = new StringSpan();
            while (index < end) {
                index = input.IndexOf(separator, prev);
                if (index < 0 || index > end) {
                    index = end;
                }
                seg.start = prev;
                seg.length = index - prev;
                yield return seg;
                prev = index + 1;
            }
        }


        public static int Count(this string input, char character, int start, int length) {
            int count = 0;
            for (int k=start; k< start + length;k++) { 
                if (input[k] == character) {
                    count++;
                }
            }
            return count;
        }

        public static string ReadString(this string input, IEnumerator<StringSpan> reader) {
            if (!reader.MoveNext()) return null;
            StringSpan span = reader.Current;
            return input.Substring(span.start, span.length);
        }

        public static int ReadInt(this string input, IEnumerator<StringSpan> reader) {
            return (int)input.ReadFloat(reader);
        }

        public static float ReadFloat(this string input, IEnumerator<StringSpan> reader) {
            if (!reader.MoveNext()) return 0;
            StringSpan span = reader.Current;

            int d = 1;
            float v = 0;

            for (int k = span.start + span.length - 1; k >= span.start; k--) {
                char ch = input[k];
                if (ch >= '0' && ch <= '9') {
                    v += (ch - '0') * d;
                    d *= 10;
                } else if (ch == '.') {
                    v = v / d;
                    d = 1;
                } else if (ch == '-') {
                    v = -v;
                } 
            }
            return v;
        }


    }


}