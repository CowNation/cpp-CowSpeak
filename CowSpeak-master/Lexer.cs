using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace CowSpeak{
	public class Lexer {
		public List< Line > Lines = new List< Line >();

		public int GetClosingBracket(int start){
			int nextSkips = 0; // skip bracket(s) after next Conditional (for nested conditionals)
			for (int j = start + 1; j < Lines.Count; j++){
				if (Lines[j].Count < 1)
					continue;

				if (Lines[j][0].type.ToString().IndexOf("Conditional") != -1 || (Lines[j].Count > 1 && Lines[j][1].type.ToString() == "FunctionDefinition"))
					nextSkips++; // there is a nested conditional, skip the next bracket

				if (Lines[j][0].type == TokenType.EndBracket){
					if (nextSkips > 0)
						nextSkips--;
					else
						return j;
				}
			}

			throw new Exception("Conditional or function is missing an ending curly bracket");
		}

		public static Token ParseToken(string token, bool _throw = true){
			token = Utils.FixBoolean(token);

			if (Utils.IsHexadecimal(token))
				token = int.Parse(token.Substring(2), System.Globalization.NumberStyles.HexNumber).ToString(); // determine if it's a hexadecimal number

			Type type = Utils.GetType(token, false);
			if (type != null)
				return new Token(TokenType.TypeIdentifier, token);

			if (token[0] == '\"' && token[token.Length - 1] == '\"')
				return new Token(TokenType.String, token.Replace(((char)0x1f).ToString(), " ").Substring(1, token.Replace(((char)0x1f).ToString(), " ").Length - 2));
			else if (token[0] == '\'' && token[token.Length - 1] == '\'' && token.Length == 3)
				return new Token(TokenType.Character, token[token.Length - 2].ToString());
			else if (token == "}")
				return new Token(TokenType.EndBracket, token);
			else if (token.IndexOf(Syntax.Conditionals.If + "(") == 0 && token[token.Length - 1] == '{' && token[token.Length - 2] == ')')
				return new Token(TokenType.IfConditional, token);
			else if (token.IndexOf(Syntax.Conditionals.Else) == 0 && token[token.Length - 1] == '{')
				return new Token(TokenType.ElseConditional, token);
			else if (token.IndexOf(Syntax.Conditionals.While + "(") == 0 && token[token.Length - 1] == '{' && token[token.Length - 2] == ')')
				return new Token(TokenType.WhileConditional, token);
			else if (token.IndexOf(Syntax.Conditionals.Loop + "(") == 0 && token[token.Length - 1] == '{' && token[token.Length - 2] == ')')
				return new Token(TokenType.LoopConditional, token);
			else if (token == Syntax.Statements.Delete)
				return new Token(TokenType.DeleteIdentifier, token);
			else if (Utils.IsDigitsOnly(token))
				return new Token(TokenType.Number, token);
			else if (token == Syntax.Statements.Return)
				return new Token(TokenType.ReturnStatement, token);
			else if (token == Syntax.Comparators.IsEqual)
				return new Token(TokenType.IsEqualOperator, token);
			else if (token == Syntax.Comparators.IsNotEqual)
				return new Token(TokenType.IsNotEqualOperator, token);
			else if (token == Syntax.Comparators.IsGreaterThan)
				return new Token(TokenType.IsGreaterThanOperator, token);
			else if (token == Syntax.Comparators.IsLessThan)
				return new Token(TokenType.IsLessThanOperator, token);
			else if (token == Syntax.Comparators.IsGreaterThanOrEqual)
				return new Token(TokenType.IsGreaterThanOrEqualOperator, token);
			else if (token == Syntax.Comparators.IsLessThanOrEqual)
				return new Token(TokenType.IsLessThanOrEqualOperator, token);
			else if (token == Syntax.Operators.Add)
				return new Token(TokenType.AddOperator, token);
			else if (token == Syntax.Operators.And)
				return new Token(TokenType.AndOperator, token);
			else if (token == Syntax.Operators.Subtract)
				return new Token(TokenType.SubtractOperator, token);
			else if (token == Syntax.Operators.Multiply)
				return new Token(TokenType.MultiplyOperator, token);
			else if (token == Syntax.Operators.Divide)
				return new Token(TokenType.DivideOperator, token);
			else if (token == Syntax.Operators.Power)
				return new Token(TokenType.PowerOperator, token);
			else if (token == Syntax.Operators.Modulo)
				return new Token(TokenType.ModuloOperator, token);
			else if (token == Syntax.Operators.Equal)
				return new Token(TokenType.EqualOperator, token);
			else if (token.IndexOf("(") != -1 && token[token.Length - 1] == ')')
				return new Token(TokenType.FunctionCall, token);
			else if (token.IndexOf("(") != -1 && token[token.Length - 2] == ')' && token[token.Length - 1] == '{')
				return new Token(TokenType.FunctionDefinition, token);
			else if (Utils.IsLettersOnly(token))
				return new Token(TokenType.VariableIdentifier, token);

			if (_throw)
				throw new Exception("Unknown token: " + token.Replace(((char)0x1f).ToString(), " "));

			return null;
		}

		public static List < Token > ParseLine(string line, bool _throw = true) {
			if (string.IsNullOrWhiteSpace(line))
				return new List< Token >(); // don't parse empty line

			line = Utils.SubstituteBetween(line, ' ', '\"', '\"', (char)0x1f);
			line = Utils.SubstituteBetween(line, ' ', '(', ')', (char)0x1D);

			List< string > splitLine = line.Split(' ').ToList();
			List< Token > ret = new List< Token >();
			List< int > skipList = new List< int >(); // lines to be skipped
			for (int i = 0; i < splitLine.Count; i++) {
				if (string.IsNullOrWhiteSpace(splitLine[i]))
					continue;

				foreach (int skip in skipList){
					if (skip == i)
						continue;
				}

				ret.Add(ParseToken(splitLine[i], _throw));
			}

			return ret;
		}

		public Lexer(List< string > fileLines, int CurrentLineOffset = 0, bool isNestedInFunction = false, bool isNestedInConditional = false) {
			for (int i = 0; i < fileLines.Count; i++) {
				CowSpeak.CurrentLine = i + 1 + CurrentLineOffset;

				fileLines[i] = Utils.FixBoolean(fileLines[i].Replace(@"\n", System.Environment.NewLine)); // interpret \n as a newline in strings & support for setting booleans using true and false

				while (fileLines[i].IndexOf("	") == 0 || fileLines[i].IndexOf(" ") == 0)
					fileLines[i] = fileLines[i].Remove(0, 1);

				foreach (string[] Definition in CowSpeak.Definitions)
					fileLines[i] = fileLines[i].Replace(Definition[0], Definition[1]);

				while (fileLines[i].IndexOf(Syntax.Identifiers.Comment) != -1){
					int pos = fileLines[i].IndexOf(Syntax.Identifiers.Comment);
					if (Utils.IsBetween(fileLines[i], pos, '"', '"') || Utils.IsBetween(fileLines[i], pos, '\'', '\'')){
						StringBuilder fileLine = new StringBuilder(fileLines[i]);
						fileLine[pos] = (char)0x1f;
						fileLines[i] = fileLine.ToString(); 
						continue;
						// replace with placeholder temporarily to prevent while loop forever
					}

					fileLines[i] = fileLines[i].Remove(pos, fileLines[i].Length - pos);
				} // get rid of all Comments and anything after it (but it cannot be enclosed in quotes or apostrophes)

				fileLines[i] = fileLines[i].Replace(((char)0x1f).ToString(), Syntax.Identifiers.Comment); // replace placeholders back with comment token

				if (string.IsNullOrWhiteSpace(fileLines[i])){
					Lines.Add(new Line(new List<Token>()));
					continue;
				} // no need to parse or evaluate empty line

				Lines.Add(new Line(ParseLine(fileLines[i])));
				Line recentLine = Lines[Lines.Count - 1];

				if (!isNestedInFunction && CowSpeak.Debug && recentLine.Count > 0){
					System.Console.WriteLine("\n(" + CowSpeak.CurrentFile + ") Line " + (i + 1) + ": ");
					foreach (var token in recentLine){
						System.Console.WriteLine(token.type.ToString() + " - " + token.identifier.Replace(System.Environment.NewLine, @"\n").Replace(((char)0x1f).ToString(), " ").Replace(((char)0x1D).ToString(), " "));
					}
				}

				if (recentLine.Count > 0 && recentLine[0].type == TokenType.FunctionCall && recentLine[0].identifier.IndexOf("define(") == 0){
					CowSpeak.GetFunction("define(").Execute(recentLine[0].identifier);
					Lines[Lines.Count - 1] = new Line(new List<Token>()); // line was already handled, clear line
				} // must handle this function before the other lines are compiled to avoid errors
			} // COMPILATION

			CowSpeak.Debug = false; // only debug tokens on compilation, needed because many things recurse back to Lexer

			for (int i = 0; i < fileLines.Count; i++){
				CowSpeak.CurrentLine = i + 1 + CurrentLineOffset;

				if (Lines[i].Count >= 1 && Lines[i][0].type == TokenType.ReturnStatement){
					if (isNestedInFunction)
						return; // ReturnStatement to be handled by UserFunction.ExecuteLines
					else
						throw new Exception("ReturnStatement must be located inside of a FunctionDefinition");
				}


				if (Lines[i].Count > 1 && Lines[i][0].type == TokenType.TypeIdentifier && Lines[i][1].type == TokenType.FunctionDefinition){
					if (isNestedInFunction || isNestedInConditional)
						throw new Exception("Function cannot be defined inside of a function or conditional");

					string usage = Lines[i][1].identifier.Replace(((char)0x1D).ToString(), " ");

					usage = usage.Substring(0, usage.Length - 2); // remove StartBracket
					string dName = usage.Substring(0, usage.IndexOf("(")); // text before first '('

					CowSpeak.CreateFunction(new UserFunction(dName, Utils.GetContainedLines(fileLines, GetClosingBracket(i), i), UserFunction.ParseDefinitionParams(usage.Substring(usage.IndexOf("("))), Utils.GetType(Lines[i][0].identifier), Lines[i][0].identifier + " " + usage + ")", i));

					i = GetClosingBracket(i); // skip to end of definition
				}
				else if (Lines[i].Count > 0){
					if (Lines[i][0].type == TokenType.IfConditional){
						int endingBracket = GetClosingBracket(i);

						if (new Conditional(Lines[i][0].identifier).EvaluateBoolean()){
							Scope scope = new Scope();

							new Lexer(Utils.GetContainedLines(Lines, endingBracket, i), i + 1, isNestedInFunction, true);

							scope.End();
						}

						i = endingBracket; // IfConditional is over, skip to end of brackets to prevent continedLines to be executed again
					}
					else if (Lines[i][0].type == TokenType.ElseConditional){
						int parentIf = -1;

						if (i == 0 || (Lines[i - 1].Count > 0 && Lines[i - 1][0].type != TokenType.EndBracket))
							throw new Exception("ElseConditional isn't immediately preceding an EndBracket");


						for (int j = 0; j < i; j++){
							if (Lines[j].Count > 0 && Lines[j][0].type == TokenType.IfConditional && GetClosingBracket(j) == i - 1){
								parentIf = j;
								break;
							}
						}

						if (parentIf == -1)
							throw new Exception("ElseConditional isn't immediately preceding an EndBracket");


						int endingBracket = GetClosingBracket(i);

						if (!new Conditional(Lines[parentIf][0].identifier).EvaluateBoolean()){
							Scope scope = new Scope();

							new Lexer(Utils.GetContainedLines(Lines, endingBracket, i), i + 1, isNestedInFunction, true);

							scope.End();
						}

						i = endingBracket; // ElseConditional is over, skip to end of brackets to prevent continedLines to be executed again
					}
					else if (Lines[i][0].type == TokenType.WhileConditional){
						int endingBracket = GetClosingBracket(i);

						Conditional whileStatement = new Conditional(Lines[i][0].identifier);
						
						while (whileStatement.EvaluateBoolean()){
							Scope scope = new Scope();

							new Lexer(Utils.GetContainedLines(Lines, endingBracket, i), i + 1, isNestedInFunction, true);

							scope.End();
						}

						i = endingBracket; // while loop is over, skip to end of brackets to prevent continedLines to be executed again
					}
					else if (Lines[i][0].type == TokenType.LoopConditional){
						int endingBracket = GetClosingBracket(i);

						string usage = Lines[i][0].identifier;
						Any[] loopParams = StaticFunction.ParseParameters(usage.Substring(usage.IndexOf("("), usage.LastIndexOf(")") - usage.IndexOf("(") + 1));

						StaticFunction Loop = new StaticFunction("Loop", null, Type.Void, new Parameter[]{ new Parameter(Type.String, "IndexVariableName"), new Parameter(Type.Integer, "StartAt"), new Parameter(Type.Integer, "EndAt") });
						Loop.CheckParameters(loopParams.ToList()); // throws errors if given parameters are bad

						string varName = loopParams[0].Get().ToString();
						int start = (int)loopParams[1].Get();
						int end = (int)loopParams[2].Get();

						CowSpeak.Vars.Add(new Variable(Type.Integer, varName));

						for (int p = start; p < end; p++){
							Scope scope = new Scope();

							CowSpeak.GetVariable(varName).Set(p);

							new Lexer(Utils.GetContainedLines(Lines, endingBracket, i), i + 1, isNestedInFunction, true);

							scope.End();
						}

						for (int p = 0; p < CowSpeak.Vars.Count; p++){
							if (CowSpeak.Vars[p].Name == varName){
								CowSpeak.Vars.RemoveAt(p);
								break;
							}
						} // delete the variable after loop is done

						i = endingBracket; // loop is over, skip to end of brackets to prevent continedLines getting executed again
					}
				}

				if (i >= fileLines.Count)
					break;

				if (Lines[i].Count == 2 && Lines[i][0].type == TokenType.DeleteIdentifier && Lines[i][1].type == TokenType.VariableIdentifier) {
					Variable target = CowSpeak.GetVariable(Lines[i][1].identifier);
					for (int z = 0; z < CowSpeak.Vars.Count; z++){
						if (CowSpeak.Vars[z].Name == target.Name){
							CowSpeak.Vars.RemoveAt(z);
							break;
						}	
					}
					continue; // prevent execution
				} // must handle this before the other lines are evaluated to avoid wrong exceptions

				bool shouldBeSet = false; // topmost variable in list should be set after exec
				if (Lines[i].Count >= 3 && Lines[i][0].type == TokenType.TypeIdentifier && Lines[i][1].type == TokenType.VariableIdentifier && Lines[i][2].type == TokenType.EqualOperator){
					CowSpeak.CreateVariable(new Variable(Type.GetType(Lines[i][0].identifier), Lines[i][1].identifier));

					shouldBeSet = true;
				} // variable must be created before exec is called so that it may be accessed

				Any retVal = Lines[i].Exec(); // Execute line

				if (Lines[i].Count >= 3 && Lines[i][1].type == TokenType.VariableIdentifier && Lines[i][2].type == TokenType.EqualOperator && !Conversion.IsCompatible(CowSpeak.GetVariable(Lines[i][1].identifier).vType, retVal.vType))
					throw new Exception("Cannot set '" + Lines[i][1].identifier + "', type '" + CowSpeak.GetVariable(Lines[i][1].identifier).vType.Name + "' is incompatible with '" + retVal.vType.Name + "'"); // check if types are compatible
				else if (Lines[i].Count >= 2 && Lines[i][0].type == TokenType.VariableIdentifier && Lines[i][1].type == TokenType.EqualOperator && !Conversion.IsCompatible(CowSpeak.GetVariable(Lines[i][0].identifier).vType, retVal.vType))
					throw new Exception("Cannot set '" + Lines[i][0].identifier + "', type '" + CowSpeak.GetVariable(Lines[i][0].identifier).vType.Name + "' is incompatible with '" + retVal.vType.Name + "'"); // check if types are compatible

				if (shouldBeSet)
					CowSpeak.Vars[CowSpeak.Vars.Count - 1].byteArr = retVal.byteArr;
				else if (Lines[i].Count >= 2 && Lines[i][0].type == TokenType.VariableIdentifier && Lines[i][1].type == TokenType.EqualOperator){
					if (CowSpeak.GetVariable(Lines[i][0].identifier, false) == null){
						throw new Exception("Variable '" + Lines[i][0].identifier + "' must be defined before it can be set");
					} // var not found

					for (int v = 0; v < CowSpeak.Vars.Count; v++){
						if (Lines[i][0].identifier == CowSpeak.Vars[v].Name)
							CowSpeak.Vars[v].byteArr = retVal.byteArr;
					} // using GetVariable does not work for this
				} // type is not specified, var must already be defined
			} // EXECUTION
		}
	}
}