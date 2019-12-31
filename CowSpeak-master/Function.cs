using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CowSpeak{
	public class Parameter{
		public string Name;
		public Type Type;

		public Parameter(Type Type, string Name){
			this.Name = Name;
			this.Type = Type;
		}
	};

	public enum DefinitionType {
		Language,
		User
	}

	public abstract class FunctionBase {
		public string Name;
		public Type type;
		public string ProperUsage;
		public bool isMethod = false;
		public Parameter[] Parameters; // defined parameters
		public DefinitionType DefinitionType;
		public string Usage{
			get{
				string def = Name + "(";
				for (int i = 0; i < Parameters.Length; i++){
					def += Parameters[i].Type.Name + " " + Parameters[i].Name;
					if (i < Parameters.Length - 1)
						def += ", ";
				}
				return def + ")";
			}
		}

		public bool isVoid(){
			return type == Type.Void;
		}

		public static Any[] ParseParameters(string s_parameters){
			if (s_parameters == "()")
				return new Any[0]; // no parameters

			List< Any > parameters = new List< Any >();
			s_parameters = s_parameters.Substring(1, s_parameters.Length - 2); // remove parentheses
			
			s_parameters = Utils.SubstituteBetween(Utils.SubstituteBetween(s_parameters, ',', '\"', '\"', (char)0x1a), ',', '(', ')', (char)0x1a).Replace(((char)0x1D).ToString(), " "); // prevent splitting of commas in nested functions & strings

			string[] splitParams = s_parameters.Split(","); // split by each comma (each item is a parameter)

			for (int i = 0; i < splitParams.Length; i++){
				splitParams[i] = splitParams[i].Replace(((char)0x1a).ToString(), ",");

				if (splitParams[i][0] == ',')
					splitParams[i] = splitParams[i].Substring(1, splitParams[i].Length - 1);
			} // splitting has been done so we can revert placeholders back

			foreach (string parameter in splitParams){
				string cleanedUp = "";
				if (parameter != "\"\"" && (parameter[0] == '\"' || parameter[0] == '\'') && (parameter[parameter.Length - 1] == '\"' || parameter[parameter.Length - 1] == '\''))
					cleanedUp = parameter.Substring(1, parameter.Length - 2);
				else
					cleanedUp = parameter;

				cleanedUp = cleanedUp.Replace(((char)0x1f).ToString(), " ").Replace(((char)0x1E).ToString(), ","); // remove quotes/apostrophes & remove string space placeholders
				Token token = null;

				if (parameter.Split('\"').Length - 1 <= 2 && parameter.IndexOf(" ") == -1){
					token = Lexer.ParseToken(parameter, false); // a flaw in the parsing function for strings would take a string chain if it starts and ends with a string as 1 string (this is a janky workaround)
				}

				Type vtype = null;

				if (token == null){
					TokenLine tl = new TokenLine(Lexer.ParseLine(parameter));
					parameters.Add(tl.Exec());
					continue;
				} // unknown identifier, could be an equation waiting to be solved
				else if (token.type == TokenType.VariableIdentifier){
					Variable _var = CowSpeak.GetVariable(token.identifier);
					parameters.Add(new Any(_var.vType, _var.Get()));
					continue;
				}
				else if (token.type == TokenType.FunctionCall){
					while ((int)token.identifier[0] < 'A' || (int)token.identifier[0] > 'z'){
						token.identifier = token.identifier.Remove(0, 1);
					}
					FunctionBase func = CowSpeak.GetFunction(token.identifier);
					if (func.type == Type.Void)
						throw new Exception("Cannot pass void function as a parameter");
					parameters.Add(new Any(func.type, func.Execute(token.identifier).Get()));
					continue;
				}
				else if (token.type == TokenType.String)
					vtype = Type.String;
				else if (token.type == TokenType.Character)
					vtype = Type.Character;
				else if (token.type == TokenType.Number){
					if (token.identifier.IndexOf(".") != -1)
						vtype = Type.Decimal;
					else
						vtype = Type.Integer;
				}

				if (vtype == null)
					throw new Exception("Unknown type passed as parameter: " + parameter);


				parameters.Add(new Any(vtype, System.Convert.ChangeType(cleanedUp, vtype.rep)));
			}

			return parameters.ToArray();
		}

		public void CheckParameters(List< Any > usedParams){
			if ((isMethod && Parameters.Length != usedParams.Count - 1) || (!isMethod && Parameters.Length != usedParams.Count))
				throw new Exception("Invalid number of parameters passed in FunctionCall: '" + Name + "'");

			for (int i = 0; i < Parameters.Length; i++){
				int usedIndex = isMethod ? i + 1 : i; // first object of usedParams in a method call is the object the method is being called on

				if (!Conversion.IsCompatible(usedParams[usedIndex].vType, Parameters[i].Type))
					throw new Exception("Cannot call '" + Name + "', parameter '" + Parameters[i].Type.Name + " " + Parameters[i].Name + "' is incompatible with '" + usedParams[usedIndex].vType.Name + "'");
			}
		}

		// To be overrode
		public abstract Any Execute(string usage);
	};
}