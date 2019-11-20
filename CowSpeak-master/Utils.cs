using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace CowSpeak{
	public static class Utils {
		public static Random rand = new Random();

		public static bool IsOperator(TokenType type){
			return type.ToString().IndexOf("Operator") != -1;
		}

		public static T TryParse<T>(string inValue)
		{
			TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
			return (T)converter.ConvertFromString(null, CultureInfo.InvariantCulture, inValue);
		}

		public static Type GetType(string usage, bool _throw = true){
			foreach (Type type in Type.GetTypes()){
				if (type.Name == usage){
					return type;
				}
			}

			if (_throw)
				CowSpeak.FatalError("Type '" + usage + "' does not exist");

			return null;
		}

		public static List< string > GetContainedLines(List< string > Lines, int endingBracket, int i){
			return Lines.GetRange(i + 1, endingBracket - (i + 1));
		}

		public static List< string > GetContainedLines(List< TokenLine > Lines, int endingBracket, int i){
			List< TokenLine > _containedLines = new List< TokenLine >();
			_containedLines = Lines.GetRange(i + 1, endingBracket - (i + 1));
			List< string > containedLines = new List< string >();

			foreach (TokenLine line in _containedLines){
				string built = "";
				foreach (Token pToken in line.tokens){
					if (pToken.type == TokenType.String)
						built += "\"";

					built += pToken.identifier.Replace(Environment.NewLine, @"\n").Replace(((char)0x1f).ToString(), " ").Replace(((char)0x1D).ToString(), " ") + " ";

					if (pToken.type == TokenType.String)
						built += "\"";
				}
				containedLines.Add(built);
			}

			return containedLines;
		}

		public static bool IsBetween(string str, int index, char start, char end){
			string leftOf = str.Substring(0, index);
			int starts = leftOf.Split(start).Length - 1;

			if (start != end){
				int lastStart = leftOf.LastIndexOf(start);
				int lastEnd = leftOf.LastIndexOf(end);
				int ends = leftOf.Split(end).Length - 1;
				return lastStart != -1 && (lastStart > lastEnd || starts > ends);
			}
			else
				return starts % 2 != 0;
		}

		public static string FixBoolean(string str){
			if (str.IndexOf("True") == -1 && str.IndexOf("False") == -1)
				return str; 

			for (int i = 0; i < str.Length; i++){
				string trueSub = i + 4 <= str.Length ? str.Substring(i, 4) : "";
				string falseSub = i + 5 <= str.Length ? str.Substring(i, 5) : "";

				if (trueSub == "True" && !IsBetween(str, i, '\"', '\"'))
					str = str.Remove(i, 4).Insert(i, "1");
				else if (falseSub == "False" && !IsBetween(str, i, '\"', '\"'))
					str = str.Remove(i, 5).Insert(i, "0");
			}

			return str;
		}

		public static string SubstituteBetween(string str, char toSub, char start, char end, char substitution = (char)0x1a){
			int i = 0;
			string _str = str;
			foreach (char letter in str){
				if (letter == toSub && IsBetween(str, i, start, end)){
					if (substitution == (char)0x0){
						_str = _str.Remove(i, 1);
					}
					else{
						StringBuilder fileLine = new StringBuilder(_str);
						fileLine[i] = substitution;
						_str = fileLine.ToString(); 
					}
				}

				i++;
			}
			return _str;
		} // so you don't have to do it very inefficently with IsBetween on each char

		public static string AddStrings(List< string > toAdd){
			// example input is "hello ", "world", "!"
			string result = "";
			foreach (string _add in toAdd){
				result += _add;
			}
			return result;
		}

		public static bool IsIndexValid<T>(int index, List< T > container){
			return index >= 0 && index < container.Count && container.Count > 0;
		}

		public static bool IsLettersOnly(string s)
		{
			foreach (char c in s)
			{
				if (!Char.IsLetter(c) && c != '_')
					return false;
			}
			return true;
		}

		public static bool IsDigitsOnly(string str)
		{
			foreach (char c in str)
			{
				if ((c < '0' || c > '9') && c != '.' && c != '-')
					return false;
			}

			return true;
		}
	}
}