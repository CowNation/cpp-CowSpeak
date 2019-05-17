#ifndef PARSER
#define PARSER

#include <vector>
#include <string>
#include <iostream>
#include <math.h>
#include "LexedLine.h"

class Parser;
class FileLexer;

class FileLexer {
public:
	std::vector< TokenLine > Lines;
	std::vector< Variable > Vars;
private:
	std::vector < Token > ParseLine(std::string line) {
		std::vector< std::string > splitLine = SplitString(line, ' ');
		std::vector< Token > ret;
		for (int i = 0; i < splitLine.size(); i++) {
			if (splitLine[i].find(")") == splitLine[i].length() - 1 && splitLine[i].find("(") != -1)
				ret.push_back(Token(TokenType::FunctionCall, splitLine[i]));
			else if (splitLine[i] == "-")
				ret.push_back(Token(TokenType::SubtractOperator, splitLine[i]));
			else if (is_digits_only(splitLine[i]))
				ret.push_back(Token(TokenType::Number, splitLine[i]));
			else if (splitLine[i] == "+")
				ret.push_back(Token(TokenType::AddOperator, splitLine[i]));
			else if (splitLine[i] == "*")
				ret.push_back(Token(TokenType::MultiplyOperator, splitLine[i]));
			else if (splitLine[i] == "/")
				ret.push_back(Token(TokenType::DivideOperator, splitLine[i]));
			else if (splitLine[i] == "^")
				ret.push_back(Token(TokenType::PowerOperator, splitLine[i]));
			else if (splitLine[i] == "=")
				ret.push_back(Token(TokenType::EqualOperator, splitLine[i]));
			else if (is_letters_only(splitLine[i]))
				ret.push_back(Token(TokenType::VariableIdentifier, splitLine[i]));
			else {
				FATAL_ERROR("Unknown identifier: " + splitLine[i]);
			}
		}
		return ret;
	}
public:
	FileLexer(std::vector< std::string > fileLines) {
		for (int i = 0; i < fileLines.size(); i++) {
			Lines.push_back(ParseLine(fileLines[i]));
			if (Lines[i].type[0].type == VariableIdentifier && Lines[i].type[1].type == EqualOperator) {
				if (!isVarDefined(Vars, Lines[i].type[0].identifier)) {
					Vars.push_back(Variable(Lines[i].type[0].identifier, 0));
					Vars[Vars.size() - 1].Value = Lines[i].Exec(Vars);
				}
				else {
					assignDefinedVar(Vars, Lines[i].type[0].identifier, Lines[i].Exec(Vars));
				}
			}
		}
	}
};

#endif // !PARSER